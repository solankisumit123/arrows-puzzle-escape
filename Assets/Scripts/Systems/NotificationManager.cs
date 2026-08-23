using UnityEngine;
#if UNITY_ANDROID || UNITY_IOS
using Unity.Notifications.Android;
using Unity.Notifications.iOS;
#endif

namespace ArrowsPuzzle.Systems
{
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start() => RequestPermissions();

        private void RequestPermissions()
        {
#if UNITY_IOS
            StartCoroutine(RequestIOSPermissions());
#endif
#if UNITY_ANDROID
            var channel = new AndroidNotificationChannel
            {
                Id = "arrows_puzzle",
                Name = "Arrows Puzzle",
                Importance = Importance.Default,
                Description = "Game reminders"
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif
        }

#if UNITY_IOS
        private System.Collections.IEnumerator RequestIOSPermissions()
        {
            var req = new AuthorizationRequest(AuthorizationOption.Alert | AuthorizationOption.Badge, true);
            while (!req.IsFinished) yield return null;
        }
#endif

        public void ScheduleDailyChallengeReminder()
        {
            // Schedule for next day midnight
            // TODO: Use Unity Mobile Notifications API
            Debug.Log("[Notifications] STUB: Schedule daily challenge reminder");
        }

        public void ScheduleStreakReminder()
        {
            Debug.Log("[Notifications] STUB: Schedule streak reminder");
        }

        public void CancelAll()
        {
#if UNITY_ANDROID
            AndroidNotificationCenter.CancelAllNotifications();
#endif
            Debug.Log("[Notifications] All notifications cancelled");
        }
    }
}
