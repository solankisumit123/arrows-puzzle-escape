using System;
using UnityEngine;

namespace ArrowsPuzzle.Systems
{
    /// <summary>
    /// Ad integration stub. Replace the TODO sections with Unity Ads SDK calls.
    /// See: https://docs.unity.com/ads/en-us/manual/MonetizationIntro
    /// </summary>
    public class AdManager : MonoBehaviour
    {
        public static AdManager Instance { get; private set; }

        private const int InterstitialInterval = 5; // show after every N levels
        private int _levelsSinceAd = 0;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void Initialize()
        {
            // TODO: Initialize Unity Ads SDK
            // Advertisement.Initialize("YOUR_GAME_ID", testMode: false);
            Debug.Log("[AdManager] STUB: Initialize ads SDK here");
        }

        public void ShowRewardedAd(Action onRewarded, Action onFailed = null)
        {
            if (SaveSystem.Data.adsRemoved) { onRewarded?.Invoke(); return; }
            // TODO: Show rewarded video ad
            // Advertisement.Show("rewardedVideo", new ShowOptions { resultCallback = result => {...} });
            Debug.Log("[AdManager] STUB: Show rewarded ad");
            // Simulate success in editor
            #if UNITY_EDITOR
            onRewarded?.Invoke();
            #endif
        }

        public void TryShowInterstitial()
        {
            if (SaveSystem.Data.adsRemoved) return;
            _levelsSinceAd++;
            if (_levelsSinceAd < InterstitialInterval) return;
            _levelsSinceAd = 0;
            // TODO: Show interstitial ad
            Debug.Log("[AdManager] STUB: Show interstitial ad");
        }
    }
}
