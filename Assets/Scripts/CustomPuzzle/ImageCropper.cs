using System;
using UnityEngine;

namespace ArrowsPuzzle.CustomPuzzle
{
    public static class ImageCropper
    {
        public static void PickFromGallery(Action<Texture2D> onPicked)
        {
            // Integration with NativeGallery package or fallback mock in Editor
#if UNITY_EDITOR || !UNITY_ANDROID && !UNITY_IOS
            Debug.Log("[ImageCropper] Editor mode: Generating sample photo texture");
            var tex = CreateSamplePhotoTexture();
            onPicked?.Invoke(tex);
#else
            // TODO: Call NativeGallery.GetImageFromGallery(...)
            Debug.Log("[ImageCropper] Mobile gallery picker stub");
#endif
        }

        public static void PickFromCamera(Action<Texture2D> onPicked)
        {
#if UNITY_EDITOR || !UNITY_ANDROID && !UNITY_IOS
            Debug.Log("[ImageCropper] Editor mode: Generating sample camera texture");
            var tex = CreateSamplePhotoTexture();
            onPicked?.Invoke(tex);
#else
            // TODO: Call NativeCamera.TakePicture(...)
            Debug.Log("[ImageCropper] Mobile camera picker stub");
#endif
        }

        public static Texture2D CropToSquare(Texture2D source, int targetSize = 512)
        {
            if (source == null) return null;

            int minDim = Mathf.Min(source.width, source.height);
            int startX = (source.width - minDim) / 2;
            int startY = (source.height - minDim) / 2;

            Color[] pixels = source.GetPixels(startX, startY, minDim, minDim);
            var squareTex = new Texture2D(minDim, minDim, TextureFormat.RGBA32, false);
            squareTex.SetPixels(pixels);
            squareTex.Apply();

            // Resize to targetSize
            var resized = new Texture2D(targetSize, targetSize, TextureFormat.RGBA32, false);
            for (int y = 0; y < targetSize; y++)
            {
                for (int x = 0; x < targetSize; x++)
                {
                    float u = (float)x / targetSize;
                    float v = (float)y / targetSize;
                    resized.SetPixel(x, y, squareTex.GetPixelBilinear(u, v));
                }
            }
            resized.Apply();
            return resized;
        }

        public static Texture2D ExtractBlockTile(Texture2D fullSquarePhoto, Engine.Block block, int gridSize)
        {
            if (fullSquarePhoto == null) return null;

            int fullSize = fullSquarePhoto.width;
            int cellPx = fullSize / gridSize;

            int blockW = block.Orientation == Engine.BlockOrientation.Horizontal ? block.Length * cellPx : cellPx;
            int blockH = block.Orientation == Engine.BlockOrientation.Vertical   ? block.Length * cellPx : cellPx;

            // In Texture2D coordinate system, (0,0) is bottom-left
            int pixelX = block.Col * cellPx;
            int pixelY = (gridSize - 1 - block.Row - (block.Orientation == Engine.BlockOrientation.Vertical ? block.Length - 1 : 0)) * cellPx;

            Color[] pixels = fullSquarePhoto.GetPixels(pixelX, pixelY, blockW, blockH);
            var tile = new Texture2D(blockW, blockH, TextureFormat.RGBA32, false);
            tile.SetPixels(pixels);
            tile.Apply();
            return tile;
        }

        private static Texture2D CreateSamplePhotoTexture()
        {
            int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float r = (float)x / size;
                    float g = (float)y / size;
                    float b = 0.5f;
                    tex.SetPixel(x, y, new Color(r, g, b, 1f));
                }
            }
            tex.Apply();
            return tex;
        }
    }
}
