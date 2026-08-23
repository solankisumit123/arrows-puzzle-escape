# Arrows – Puzzle Escape

A complete, premium mobile sliding-puzzle game for iOS & Android built in **Unity (C#)**.

---

## 🎮 Game Overview

- **Genre**: Grid-based sliding escape puzzle (Unblock Me / Traffic Jam / Block Puzzle Escape).
- **Core Mechanics**:
  - NxN grid (6x6 scaling to 9x9 across chapters).
  - Colored arrow blocks (1x1, 1x2, 1x3) locked to fixed axes (Horizontal or Vertical).
  - Exit Gates placed on grid edges matching block colors.
  - Multi-special mechanics: Ice blocks, Locked/Key pairs, One-way blocks, Teleporters, Bomb blocks.
- **Custom Image Puzzle (Photo-to-Puzzle)**:
  - User can pick/crop any image from gallery or camera.
  - Procedural **reverse-construction** puzzle generator guarantees solvability.
  - Image-tiled block rendering with reveal animation upon solving.
  - Local puzzle gallery for replaying up to 10 saved photo puzzles.
- **Live-Service Systems**:
  - 200+ levels across 10 themed chapters + 30 Daily Challenges.
  - Full animated Level Complete Flow (150ms anticipation, confetti burst, staggered star pop, coin count-up, auto-advance).
  - Power-ups: Hint (BFS auto-solver), Bomb, Shuffle, Extra Move with coin economy.
  - Achievements, Login Streak rewards, Adaptive Difficulty, Ghost replay recording.
  - Accessibility: Colorblind shapes mode, dark mode, reduce motion toggle.

---

## 📁 Project Structure

```
ArrowsPuzzleEscape/
├── Packages/
│   └── manifest.json             # Unity package dependencies
├── Assets/
│   ├── Scripts/
│   │   ├── Engine/               # Pure C# puzzle engine (framework-agnostic)
│   │   │   ├── Block.cs
│   │   │   ├── Gate.cs
│   │   │   ├── GridState.cs
│   │   │   ├── MoveValidator.cs
│   │   │   ├── PuzzleEngine.cs
│   │   │   ├── BFSSolver.cs
│   │   │   └── LevelLoader.cs
│   │   ├── Gameplay/             # Unity gameplay MonoBehaviours
│   │   │   ├── GameManager.cs
│   │   │   ├── BlockView.cs
│   │   │   ├── GridView.cs
│   │   │   ├── GateView.cs
│   │   │   ├── InputHandler.cs
│   │   │   ├── HintSystem.cs
│   │   │   └── PowerUpManager.cs
│   │   ├── UI/                   # All 10 UI screens & panels
│   │   │   ├── SplashScreen.cs
│   │   │   ├── MainMenuScreen.cs
│   │   │   ├── WorldMapScreen.cs
│   │   │   ├── LevelSelectScreen.cs
│   │   │   ├── GameplayHUD.cs
│   │   │   ├── LevelCompletePanel.cs
│   │   │   ├── ChapterCompleteScreen.cs
│   │   │   ├── ShopScreen.cs
│   │   │   ├── SettingsScreen.cs
│   │   │   ├── AchievementsScreen.cs
│   │   │   ├── DailyChallengeScreen.cs
│   │   │   ├── CustomImagePuzzleScreen.cs
│   │   │   └── PauseMenuPanel.cs
│   │   ├── Systems/              # Persistence, Audio, Economy & Stubs
│   │   │   ├── SaveSystem.cs
│   │   │   ├── AudioManager.cs
│   │   │   ├── CoinEconomy.cs
│   │   │   ├── AchievementSystem.cs
│   │   │   ├── AdManager.cs
│   │   │   ├── IAPManager.cs
│   │   │   ├── NotificationManager.cs
│   │   │   ├── CloudSaveManager.cs
│   │   │   ├── AnalyticsManager.cs
│   │   │   └── SceneLoader.cs
│   │   ├── Effects/              # Particles, tweens & transitions
│   │   │   ├── ConfettiController.cs
│   │   │   ├── ScreenTransitionManager.cs
│   │   │   ├── BlockAnimator.cs
│   │   │   ├── StarPopAnimation.cs
│   │   │   └── ParticleThemeManager.cs
│   │   ├── CustomPuzzle/         # Image-to-puzzle reverse solver & generator
│   │   │   ├── ImagePuzzleGenerator.cs
│   │   │   ├── ImageCropper.cs
│   │   │   ├── ReverseSolver.cs
│   │   │   └── PhotoPuzzleSave.cs
│   │   └── Editor/               # Unity Editor sandbox & level validator
│   │       ├── LevelEditorWindow.cs
│   │       └── LevelValidator.cs
│   ├── Tests/
│   │   └── EditMode/             # NUnit tests for engine, solver & move validator
│   │       ├── PuzzleEngineTests.cs
│   │       ├── BFSSolverTests.cs
│   │       └── MoveValidatorTests.cs
│   ├── Levels/                   # 200+ Level JSON files across 10 chapters
│   │   ├── Chapter01 ... Chapter10
│   │   └── DailyChallenge
│   └── Resources/
│       └── Data/                 # chapters.json, achievements.json, skins.json
└── README.md
```

---

## 🚀 Getting Started in Unity Editor

1. Open **Unity Hub** and click **Add Project from Disk**.
2. Select the folder `C:\Users\solan\Downloads\image 2\ArrowsPuzzleEscape`.
3. Recommended Unity Version: **2022.3 LTS** (or 2023.x / Unity 6).
4. Install recommended packages via Package Manager:
   - **DOTween** (Demigiant) via Unity Asset Store / Package Manager.
   - **TextMeshPro** (Import TMP Essentials when prompted).
   - **NativeGallery** & **NativeCamera** (for mobile photo picking).
5. Open `Assets/Tests/EditMode` in the **Test Runner** (`Window > General > Test Runner`) and click **Run All** to verify the core engine and BFS solver.
6. Open `Arrows Puzzle > Level Editor` in the top Unity menu to inspect or create new levels.

---

## 🕹️ Architecture Highlights

- **Pure C# Engine**: `PuzzleEngine`, `BFSSolver`, `MoveValidator`, `GridState`, and `Block` are 100% decoupled from Unity UnityEngine APIs, making them lightning-fast and independently testable.
- **Reverse Construction**: `ReverseSolver.ScrambleFromSolved()` legally reverses moves from the solved grid, guaranteeing that any custom user photo puzzle is mathematically solvable before playing.
