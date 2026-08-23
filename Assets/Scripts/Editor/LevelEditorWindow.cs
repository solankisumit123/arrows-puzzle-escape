#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ArrowsPuzzle.Engine;

namespace ArrowsPuzzle.Editor
{
    public class LevelEditorWindow : EditorWindow
    {
        private int _gridSize = 6;
        private int _levelId = 1;
        private int _chapterId = 1;

        [MenuItem("Arrows Puzzle/Level Editor")]
        public static void ShowWindow()
        {
            GetWindow<LevelEditorWindow>("Arrows Level Editor");
        }

        private void OnGUI()
        {
            GUILayout.Label("Arrows – Level Editor & Sandbox", EditorStyles.boldLabel);
            _chapterId = EditorGUILayout.IntField("Chapter ID", _chapterId);
            _levelId = EditorGUILayout.IntField("Level ID", _levelId);
            _gridSize = EditorGUILayout.IntSlider("Grid Size (NxN)", _gridSize, 6, 9);

            if (GUILayout.Button("Create New Blank Level JSON"))
            {
                var level = new LevelData
                {
                    levelId = _levelId,
                    chapterId = _chapterId,
                    gridSize = _gridSize,
                    optimalMoves = 5,
                    isTutorial = _levelId <= 3,
                    tutorialStep = _levelId <= 3 ? _levelId : 0,
                    chapterTheme = $"Chapter{_chapterId:D2}",
                    blocks = new BlockData[0],
                    gates = new GateData[0]
                };

                string json = JsonUtility.ToJson(level, true);
                string path = EditorUtility.SaveFilePanel("Save Level JSON", "Assets/Levels", $"level_{_levelId:D3}.json", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    System.IO.File.WriteAllText(path, json);
                    AssetDatabase.Refresh();
                    Debug.Log($"[LevelEditor] Saved: {path}");
                }
            }
        }
    }
}
#endif
