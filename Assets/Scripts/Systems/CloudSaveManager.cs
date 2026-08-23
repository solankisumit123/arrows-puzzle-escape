using System;
using UnityEngine;

namespace ArrowsPuzzle.Systems
{
    /// <summary>
    /// Cloud save stub. Integrate Unity Gaming Services or Firebase.
    /// </summary>
    public static class CloudSaveManager
    {
        public static void UploadSave(Action onComplete = null)
        {
            // TODO: Upload SaveSystem.Data JSON to cloud (UGS Cloud Save or Firebase Realtime DB)
            Debug.Log("[CloudSave] STUB: Upload save to cloud");
            onComplete?.Invoke();
        }

        public static void DownloadSave(Action<bool> onComplete = null)
        {
            // TODO: Fetch cloud save, merge with local, resolve conflicts by timestamp
            Debug.Log("[CloudSave] STUB: Download save from cloud");
            onComplete?.Invoke(false);
        }
    }
}
