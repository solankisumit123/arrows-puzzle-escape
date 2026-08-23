#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using ArrowsPuzzle.Engine;

namespace ArrowsPuzzle.Editor
{
    public static class LevelValidator
    {
        [MenuItem("Arrows Puzzle/Validate All Levels (BFS)")]
        public static void ValidateAllLevels()
        {
            string levelsRoot = Path.Combine(Application.dataPath, "Levels");
            if (!Directory.Exists(levelsRoot))
            {
                Debug.LogWarning("No Levels directory found.");
                return;
            }

            var jsonFiles = Directory.GetFiles(levelsRoot, "*.json", SearchOption.AllDirectories);
            int validCount = 0;
            int total = jsonFiles.Length;

            for (int i = 0; i < total; i++)
            {
                string f = jsonFiles[i];
                string json = File.ReadAllText(f);
                var data = LevelLoader.LoadFromJson(json);
                if (data != null && data.blocks != null)
                {
                    var engine = LevelLoader.BuildEngine(data);
                    var sol = BFSSolver.Solve(engine, 20_000);
                    if (sol.IsSolvable)
                    {
                        validCount++;
                    }
                    else
                    {
                        Debug.LogWarning($"[Validator] Level {Path.GetFileName(f)} marked unsolvable or exceeded BFS depth limit.");
                    }
                }
            }

            Debug.Log($"[Validator Complete] Checked {total} levels: {validCount}/{total} solvable.");
        }
    }
}
#endif
