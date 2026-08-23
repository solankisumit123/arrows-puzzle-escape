using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using ArrowsPuzzle.Engine;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.Gameplay
{
    public class GridView : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private RectTransform _gridContainer;
        [SerializeField] private RectTransform _cellsContainer;
        [SerializeField] private RectTransform _gatesContainer;
        [SerializeField] private RectTransform _blocksContainer;

        [Header("Prefabs")]
        [SerializeField] private GameObject _cellBackgroundPrefab;
        [SerializeField] private GameObject _gatePrefab;
        [SerializeField] private GameObject _blockPrefab;

        private Dictionary<string, BlockView> _blockViews = new Dictionary<string, BlockView>();
        private List<GateView> _gateViews = new List<GateView>();
        private float _cellSize;
        private int _gridSize;
        private bool _tutorialMode;

        public float CellSize => _cellSize;
        public RectTransform GridContainer => _gridContainer;

        public void BuildGrid(PuzzleEngine engine, LevelData levelData, Texture2D photoTexture = null)
        {
            ClearGrid();
            _gridSize = engine.GridSize;

            float containerWidth = _gridContainer.rect.width > 0 ? _gridContainer.rect.width : 540f;
            _cellSize = containerWidth / _gridSize;

            // Spawn cell background tiles
            for (int r = 0; r < _gridSize; r++)
            {
                for (int c = 0; c < _gridSize; c++)
                {
                    var cellGo = _cellBackgroundPrefab != null 
                        ? Instantiate(_cellBackgroundPrefab, _cellsContainer)
                        : CreateDefaultCellGo(_cellsContainer);
                    
                    var rt = cellGo.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(_cellSize - 2, _cellSize - 2);
                    rt.anchoredPosition = new Vector2(c * _cellSize, -r * _cellSize);

                    // Check if teleporter cell
                    if (engine.Grid.IsTeleporter(r, c))
                    {
                        var img = cellGo.GetComponent<Image>();
                        if (img != null) img.color = new Color(0.7f, 0.3f, 0.9f, 0.35f);
                    }
                }
            }

            // Spawn gates
            foreach (var gate in engine.Gates)
            {
                var gateGo = _gatePrefab != null 
                    ? Instantiate(_gatePrefab, _gatesContainer)
                    : CreateDefaultGateGo(_gatesContainer);

                var gView = gateGo.GetComponent<GateView>() ?? gateGo.AddComponent<GateView>();
                gView.Initialize(gate, _cellSize, _gridSize);
                _gateViews.Add(gView);
            }

            // Spawn block views
            foreach (var block in engine.Blocks)
            {
                if (block.HasExited) continue;

                var blockGo = _blockPrefab != null 
                    ? Instantiate(_blockPrefab, _blocksContainer)
                    : CreateDefaultBlockGo(_blocksContainer);

                var bView = blockGo.GetComponent<BlockView>() ?? blockGo.AddComponent<BlockView>();
                Texture2D tileTex = null;
                if (photoTexture != null)
                {
                    tileTex = CustomPuzzle.ImageCropper.ExtractBlockTile(photoTexture, block, _gridSize);
                }

                bView.Initialize(block, _cellSize, photoTexture != null, tileTex);
                bView.SetGridPosition(block.Row, block.Col, _cellSize);
                _blockViews[block.Id] = bView;
            }

            PlayEntranceAnimation();
        }

        public BlockView GetBlockView(string blockId) => _blockViews.TryGetValue(blockId, out var v) ? v : null;

        public void RefreshAllBlocks(PuzzleEngine engine)
        {
            foreach (var block in engine.Blocks)
            {
                if (_blockViews.TryGetValue(block.Id, out var view))
                {
                    if (block.HasExited)
                    {
                        view.gameObject.SetActive(false);
                    }
                    else
                    {
                        view.gameObject.SetActive(true);
                        view.SetGridPosition(block.Row, block.Col, _cellSize);
                        view.UpdateLockedState(block.IsLocked);
                    }
                }
            }
        }

        public void RemoveBlockView(string blockId)
        {
            if (_blockViews.TryGetValue(blockId, out var view))
            {
                Destroy(view.gameObject);
                _blockViews.Remove(blockId);
            }
        }

        public void SetTutorialMode(bool isTutorial)
        {
            _tutorialMode = isTutorial;
        }

        public void PlayEntranceAnimation()
        {
            if (SaveSystem.Data.reduceMotion) return;

            int i = 0;
            foreach (var kvp in _blockViews)
            {
                kvp.Value.transform.localScale = Vector3.zero;
                kvp.Value.transform.DOScale(1f, 0.35f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(i * 0.03f);
                i++;
            }
        }

        public void PlayCelebrationZoom()
        {
            if (SaveSystem.Data.reduceMotion) return;
            _gridContainer.DOScale(0.95f, 0.2f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                _gridContainer.DOScale(1f, 0.4f).SetEase(Ease.OutBounce);
            });
        }

        private void ClearGrid()
        {
            foreach (Transform t in _cellsContainer) Destroy(t.gameObject);
            foreach (Transform t in _gatesContainer) Destroy(t.gameObject);
            foreach (Transform t in _blocksContainer) Destroy(t.gameObject);
            _blockViews.Clear();
            _gateViews.Clear();
        }

        private GameObject CreateDefaultCellGo(Transform parent)
        {
            var go = new GameObject("Cell_BG", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.18f, 0.22f, 0.28f, 0.6f);
            return go;
        }

        private GameObject CreateDefaultGateGo(Transform parent)
        {
            var go = new GameObject("Gate", typeof(RectTransform), typeof(Image), typeof(GateView));
            go.transform.SetParent(parent, false);
            return go;
        }

        private GameObject CreateDefaultBlockGo(Transform parent)
        {
            var go = new GameObject("Block", typeof(RectTransform), typeof(Image), typeof(BlockView));
            go.transform.SetParent(parent, false);
            return go;
        }
    }
}
