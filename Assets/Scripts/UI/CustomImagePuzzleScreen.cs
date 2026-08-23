using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArrowsPuzzle.CustomPuzzle;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.UI
{
    public class CustomImagePuzzleScreen : MonoBehaviour
    {
        [Header("Pick Image State")]
        [SerializeField] private GameObject _pickerPanel;
        [SerializeField] private Button _pickGalleryButton;
        [SerializeField] private Button _pickCameraButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private RawImage _previewImage;

        [Header("Difficulty Select State")]
        [SerializeField] private GameObject _difficultyPanel;
        [SerializeField] private Button _easyButton;    // 6x6
        [SerializeField] private Button _mediumButton;  // 7x7
        [SerializeField] private Button _hardButton;    // 8x8
        [SerializeField] private Button _generateButton;

        [Header("Gallery of User Puzzles")]
        [SerializeField] private Transform _myPuzzlesContainer;
        [SerializeField] private GameObject _puzzleTilePrefab;

        private Texture2D _selectedTexture;
        private int _selectedGridSize = 6;
        private int _scrambleDepth = 15;

        private void Start()
        {
            _backButton.onClick.AddListener(() => SceneLoader.LoadScene(SceneLoader.SceneMainMenu));
            
            _pickGalleryButton.onClick.AddListener(() => ImageCropper.PickFromGallery(tex => SetSelectedImage(tex)));
            _pickCameraButton.onClick.AddListener(() => ImageCropper.PickFromCamera(tex => SetSelectedImage(tex)));

            _easyButton?.onClick.AddListener(() => SetDifficulty(6, 12));
            _mediumButton?.onClick.AddListener(() => SetDifficulty(7, 20));
            _hardButton?.onClick.AddListener(() => SetDifficulty(8, 30));

            _generateButton?.onClick.AddListener(GenerateAndPlay);

            LoadMyPuzzlesGallery();
        }

        private void SetSelectedImage(Texture2D tex)
        {
            if (tex == null) return;
            _selectedTexture = ImageCropper.CropToSquare(tex, 512);
            if (_previewImage != null) _previewImage.texture = _selectedTexture;
            _difficultyPanel?.SetActive(true);
        }

        private void SetDifficulty(int gridSize, int scrambleDepth)
        {
            _selectedGridSize = gridSize;
            _scrambleDepth = scrambleDepth;
        }

        private void GenerateAndPlay()
        {
            if (_selectedTexture == null) return;

            var customPuzzle = ImagePuzzleGenerator.Generate(_selectedTexture, _selectedGridSize, _scrambleDepth);
            int puzzleId = PhotoPuzzleSave.SaveCustomPuzzle(customPuzzle, _selectedTexture);

            AnalyticsManager.TrackCustomPuzzleCreated(_selectedGridSize, $"{_selectedGridSize}x{_selectedGridSize}");
            AchievementSystem.Increment("custom_puzzle");

            SceneLoader.LoadGameplay(0, puzzleId, isCustom: true);
        }

        private void LoadMyPuzzlesGallery()
        {
            if (_myPuzzlesContainer == null) return;
            var list = PhotoPuzzleSave.GetAllSavedPuzzles();
            foreach (var meta in list)
            {
                var go = new GameObject($"CustomPuz_{meta.puzzleId}", typeof(RectTransform), typeof(RawImage), typeof(Button));
                go.transform.SetParent(_myPuzzlesContainer, false);
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(100, 100);

                var raw = go.GetComponent<RawImage>();
                raw.texture = PhotoPuzzleSave.LoadTexture(meta.puzzleId);

                var btn = go.GetComponent<Button>();
                int pid = meta.puzzleId;
                btn.onClick.AddListener(() => SceneLoader.LoadGameplay(0, pid, isCustom: true));
            }
        }
    }
}
