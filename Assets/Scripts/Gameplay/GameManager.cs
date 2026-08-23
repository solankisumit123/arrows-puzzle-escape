using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArrowsPuzzle.Engine;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.Gameplay
{
    /// <summary>
    /// Central gameplay coordinator. Singleton per gameplay scene.
    /// Owns PuzzleEngine, wires input → engine → view, manages game state.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // ── State ─────────────────────────────────────────────────────────────
        public PuzzleEngine Engine { get; private set; }
        public LevelData CurrentLevel { get; private set; }
        public int MoveCount => Engine?.MoveCount ?? 0;
        public float ElapsedTime { get; private set; }
        public bool IsPaused { get; private set; }
        public bool IsLevelComplete { get; private set; }
        public bool IsTutorial => CurrentLevel?.isTutorial ?? false;
        public int TutorialStep => CurrentLevel?.tutorialStep ?? 0;

        // Tutorial: which block is currently forced
        public string TutorialForcedBlockId { get; private set; }
        public int TutorialForcedOffset { get; private set; }

        // ── Events ────────────────────────────────────────────────────────────
        public event Action<string, int>    OnBlockMoved;      // (blockId, offset)
        public event Action<string>         OnBlockExited;     // (blockId)
        public event Action<string>         OnBlockUnlocked;
        public event Action                 OnLevelComplete;
        public event Action<int, int>       OnMoveCountChanged; // (old, new)
        public event Action<float>          OnTimeUpdate;
        public event Action                 OnUndoPerformed;
        public event Action                 OnResetPerformed;
        public event Action<string, (int,int)> OnBlockTeleported;
        public event Action<string>         OnIceBlockSlid;
        public event Action                 OnPauseChanged;

        // ── References ───────────────────────────────────────────────────────
        [SerializeField] private GridView _gridView;
        [SerializeField] private UI.GameplayHUD _hud;
        [SerializeField] private UI.LevelCompletePanel _levelCompletePanel;
        [SerializeField] private UI.PauseMenuPanel _pausePanel;

        // Ghost replay recording
        private List<Move> _playerMoveHistory = new List<Move>();
        private bool _usedHint = false;
        private bool _usedUndo = false;
        private int _undoCount = 0;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            LoadLevel();
        }

        private void Update()
        {
            if (!IsPaused && !IsLevelComplete && Engine != null)
            {
                ElapsedTime += Time.deltaTime;
                OnTimeUpdate?.Invoke(ElapsedTime);
            }
        }

        // ── Level Loading ─────────────────────────────────────────────────────

        private void LoadLevel()
        {
            int chapterId = SceneLoader.SelectedChapterId;
            int levelId = SceneLoader.SelectedLevelId;

            LevelData data;
            if (SceneLoader.IsCustomPuzzle)
            {
                data = CustomPuzzle.PhotoPuzzleSave.LoadPuzzle(levelId);
            }
            else if (SceneLoader.IsDailyChallenge)
            {
                data = LevelLoader.LoadFromResources(levelId, 0); // chapter 0 = daily
            }
            else
            {
                data = LevelLoader.LoadFromResources(levelId, chapterId);
            }

            if (data == null)
            {
                Debug.LogError("[GameManager] Failed to load level data.");
                return;
            }

            CurrentLevel = data;
            Engine = LevelLoader.BuildEngine(data);

            // Wire engine events
            Engine.OnBlockMoved   = (id, off) => {
                var move = new Move(id, off);
                _playerMoveHistory.Add(move);
                OnBlockMoved?.Invoke(id, off);
                OnMoveCountChanged?.Invoke(Engine.MoveCount - 1, Engine.MoveCount);
                AudioManager.Instance?.PlaySlide();
            };
            Engine.OnBlockExited  = id => OnBlockExited?.Invoke(id);
            Engine.OnBlockUnlocked = id => {
                OnBlockUnlocked?.Invoke(id);
                AudioManager.Instance?.PlayUnlock();
            };
            Engine.OnLevelComplete = HandleLevelComplete;
            Engine.OnBlockTeleported = (id, dest) => {
                OnBlockTeleported?.Invoke(id, dest);
                AudioManager.Instance?.PlayTeleport();
            };

            // Build visual grid
            _gridView?.BuildGrid(Engine, data);

            // Tutorial setup
            if (IsTutorial) SetupTutorialStep();

            ElapsedTime = 0;
            IsLevelComplete = false;
            _playerMoveHistory.Clear();
            _usedHint = false;
            _usedUndo = false;

            AnalyticsManager.TrackLevelStart(levelId, chapterId);
            AudioManager.Instance?.PlayChapterMusic(chapterId);
        }

        // ── Tutorial ─────────────────────────────────────────────────────────

        private void SetupTutorialStep()
        {
            // Tutorial levels 1-3: force specific moves
            // In level JSON, blocks have a "tutorialOrder" field (see JSON schema)
            // We highlight them via GridView; other blocks are not draggable
            _gridView?.SetTutorialMode(true);
        }

        // ── Input from InputHandler ───────────────────────────────────────────

        public bool TryMoveBlock(string blockId, int offset)
        {
            if (IsPaused || IsLevelComplete || Engine == null) return false;
            if (IsTutorial && blockId != TutorialForcedBlockId) return false;

            var outcome = Engine.TryMove(blockId, offset);

            if (outcome.Result == MoveResult.Blocked)
                AudioManager.Instance?.PlayThud();

            return outcome.Result == MoveResult.Success || outcome.Result == MoveResult.ExitedGate;
        }

        // ── Undo / Reset ──────────────────────────────────────────────────────

        public void Undo()
        {
            if (!Engine.CanUndo || IsLevelComplete) return;
            _usedUndo = true;
            _undoCount++;
            Engine.Undo();
            _gridView?.RefreshAllBlocks(Engine);
            OnUndoPerformed?.Invoke();
            AudioManager.Instance?.PlayButton();
        }

        public void Reset()
        {
            Engine.Reset();
            ElapsedTime = 0;
            IsLevelComplete = false;
            _playerMoveHistory.Clear();
            _gridView?.RefreshAllBlocks(Engine);
            OnResetPerformed?.Invoke();
        }

        // ── Pause ─────────────────────────────────────────────────────────────

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
            _pausePanel?.gameObject.SetActive(paused);
            OnPauseChanged?.Invoke();
        }

        // ── Level Complete ─────────────────────────────────────────────────────

        private void HandleLevelComplete()
        {
            if (IsLevelComplete) return;
            IsLevelComplete = true;

            int stars = Engine.GetStarRating(SaveSystem.Data.adaptiveFactor);
            int coins = CoinEconomy.CalcLevelReward(stars, SceneLoader.SelectedChapterId);

            // Record progress
            SaveSystem.RecordLevelComplete(CurrentLevel.levelId, stars, Engine.MoveCount, ElapsedTime);
            CoinEconomy.AddCoins(coins);

            // Achievement tracking
            AchievementSystem.Increment("first_level");
            AchievementSystem.Increment("total_1000", Engine.MoveCount);
            if (stars == 3) AchievementSystem.Increment("ten_3stars");
            if (!_usedHint) AchievementSystem.Increment("no_hint_5");
            if (!_usedUndo) AchievementSystem.Increment("undo_never");
            if (ElapsedTime < 30f) AchievementSystem.Increment("speed_run");

            // Save ghost replay
            var solutionJson = JsonUtility.ToJson(new MoveListWrapper { moves = _playerMoveHistory });
            SaveSystem.Data.lastSolutionJson[CurrentLevel.levelId] = solutionJson;
            SaveSystem.Save();

            AnalyticsManager.TrackLevelComplete(CurrentLevel.levelId, stars, Engine.MoveCount, ElapsedTime);
            AdManager.Instance?.TryShowInterstitial();

            OnLevelComplete?.Invoke();

            // Trigger the full Level Complete Flow
            StartCoroutine(LevelCompleteSequence(stars, coins));
        }

        private IEnumerator LevelCompleteSequence(int stars, int coins)
        {
            // Step 1: 150ms freeze for anticipation
            yield return new WaitForSeconds(0.15f);

            // Step 2: Confetti + board zoom-out
            Effects.ConfettiController.Instance?.Burst();
            _gridView?.PlayCelebrationZoom();

            // Step 3-8: Level complete panel handles its own animation sequence
            _levelCompletePanel?.Show(stars, coins, Engine.MoveCount, ElapsedTime,
                isNewBest: IsNewBest());

            AudioManager.Instance?.PlayWin();
        }

        private bool IsNewBest()
        {
            var prog = SaveSystem.GetLevelProgress(CurrentLevel.levelId);
            return prog.bestMoves == Engine.MoveCount || Engine.MoveCount < prog.bestMoves;
        }

        public void GoToNextLevel()
        {
            int nextLevel = CurrentLevel.levelId + 1;
            int chapter = SceneLoader.SelectedChapterId;

            // Check chapter boundary
            bool chapterComplete = nextLevel > (chapter * 20);
            if (chapterComplete)
            {
                AchievementSystem.Increment($"chapter{chapter}_done");
                // Show chapter complete screen
                FindObjectOfType<UI.ChapterCompleteScreen>()?.Show(chapter);
                return;
            }

            SceneLoader.LoadGameplay(chapter, nextLevel);
        }

        // ── Power-ups (delegated from PowerUpManager) ─────────────────────────

        public void OnHintUsed() => _usedHint = true;

        public bool ApplyBomb(string blockId)
        {
            bool ok = Engine.ApplyBomb(blockId);
            if (ok) _gridView?.RemoveBlockView(blockId);
            return ok;
        }

        public void ApplyShuffle() => Engine.ApplyShuffle();

        [Serializable]
        private class MoveListWrapper { public List<Move> moves; }
    }
}
