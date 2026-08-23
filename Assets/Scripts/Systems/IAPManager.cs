using System;
using UnityEngine;

namespace ArrowsPuzzle.Systems
{
    /// <summary>
    /// In-app purchase stub. Integrate Unity IAP package and replace TODOs.
    /// See: https://docs.unity3d.com/Manual/UnityIAP.html
    /// </summary>
    public class IAPManager : MonoBehaviour
    {
        public static IAPManager Instance { get; private set; }

        // Product IDs – match your app store listing
        public const string ProductRemoveAds  = "com.arrowspuzzle.removeads";
        public const string ProductSmallPack  = "com.arrowspuzzle.coins.small";
        public const string ProductMediumPack = "com.arrowspuzzle.coins.medium";
        public const string ProductLargePack  = "com.arrowspuzzle.coins.large";

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PurchaseRemoveAds(Action onSuccess, Action onFail = null)
        {
            // TODO: Initiate Unity IAP purchase for ProductRemoveAds
            // IAPButton or UnityPurchasing.BuyProduct(ProductRemoveAds, ...);
            Debug.Log("[IAPManager] STUB: Purchase Remove Ads");
            #if UNITY_EDITOR
            SaveSystem.Data.adsRemoved = true;
            SaveSystem.Save();
            onSuccess?.Invoke();
            #endif
        }

        public void PurchaseCoinPack(string packId, Action<int> onSuccess, Action onFail = null)
        {
            int coins = packId switch {
                ProductSmallPack  => CoinEconomy.SmallPackCoins,
                ProductMediumPack => CoinEconomy.MediumPackCoins,
                ProductLargePack  => CoinEconomy.LargePackCoins,
                _ => 0
            };
            // TODO: Initiate Unity IAP purchase
            Debug.Log($"[IAPManager] STUB: Purchase coin pack {packId} ({coins} coins)");
            #if UNITY_EDITOR
            CoinEconomy.AddCoins(coins);
            onSuccess?.Invoke(coins);
            #endif
        }

        public void RestorePurchases()
        {
            // TODO: Call Unity IAP restore
            Debug.Log("[IAPManager] STUB: Restore purchases");
        }
    }
}
