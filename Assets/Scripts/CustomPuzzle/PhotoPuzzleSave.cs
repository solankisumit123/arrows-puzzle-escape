using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ArrowsPuzzle.Engine;

namespace ArrowsPuzzle.CustomPuzzle
{
    [Serializable]
    public class CustomPuzzleMeta
    {
        public int puzzleId;
        public string title;
        public string dateCreated;
        public int gridSize;
    }

    [Serializable]
    public class CustomGalleryWrapper
    {
        public List<CustomPuzzleMeta> puzzles = new List<CustomPuzzleMeta>();
    }

    public static class PhotoPuzzleSave
    {
        private static string BaseDir => Path.Combine(Application.persistentDataPath, "CustomPuzzles");
        private static string MetaFile => Path.Combine(BaseDir, "gallery.json");

        public static int SaveCustomPuzzle(LevelData data, Texture2D photo)
        {
            if (!Directory.Exists(BaseDir)) Directory.CreateDirectory(BaseDir);

            var gallery = LoadGalleryMeta();
            int newId = 9001 + gallery.puzzles.Count;
            data.levelId = newId;

            // Save JSON level data
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(Path.Combine(BaseDir, $"puzzle_{newId}.json"), json);

            // Save PNG texture
            if (photo != null)
            {
                byte[] bytes = photo.EncodeToPNG();
                File.WriteAllText(Path.Combine(BaseDir, $"puzzle_{newId}.png"), Convert.ToBase64String(bytes));
            }

            gallery.puzzles.Add(new CustomPuzzleMeta
            {
                puzzleId = newId,
                title = $"Photo Puzzle #{gallery.puzzles.Count + 1}",
                dateCreated = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                gridSize = data.gridSize
            });

            // Keep max 10
            if (gallery.puzzles.Count > 10) gallery.puzzles.RemoveAt(0);

            File.WriteAllText(MetaFile, JsonUtility.ToJson(gallery, true));
            return newId;
        }

        public static LevelData LoadPuzzle(int puzzleId)
        {
            string path = Path.Combine(BaseDir, $"puzzle_{puzzleId}.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return LevelLoader.LoadFromJson(json);
            }
            return null;
        }

        public static Texture2D LoadTexture(int puzzleId)
        {
            string path = Path.Combine(BaseDir, $"puzzle_{puzzleId}.png");
            if (File.Exists(path))
            {
                string b64 = File.ReadAllText(path);
                byte[] bytes = Convert.FromBase64String(b64);
                var tex = new Texture2D(2, 2);
                tex.LoadImage(bytes);
                return tex;
            }
            return null;
        }

        public static List<CustomPuzzleMeta> GetAllSavedPuzzles() => LoadGalleryMeta().puzzles;

        private static CustomGalleryWrapper LoadGalleryMeta()
        {
            if (File.Exists(MetaFile))
            {
                string json = File.ReadAllText(MetaFile);
                return JsonUtility.FromJson<CustomGalleryWrapper>(json);
            }
            return new CustomGalleryWrapper();
        }
    }
}
