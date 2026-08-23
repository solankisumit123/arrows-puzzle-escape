using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.UI
{
    public class ShopScreen : MonoBehaviour
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _buyRemoveAdsButton;
        [SerializeField] private Button _buySmallPackButton;
        [SerializeField] private Button _buyMediumPackButton;
        [SerializeField] private Button _buyLargePackButton;
        [SerializeField] private Button _watchAdForCoinsButton;
        [SerializeField] private TextMeshProUGUI _currentCoinsText;

        private void Awake()
        {
            gameObject.SetActive(false);
            _closeButton.onClick.AddListener(Hide);

            _buyRemoveAdsButton?.onClick.AddListener(() => IAPManager.Instance?.PurchaseRemoveAds(() => UpdateUI()));
            _buySmallPackButton?.onClick.AddListener(() => IAPManager.Instance?.PurchaseCoinPack(IAPManager.ProductSmallPack, _ => UpdateUI()));
            _buyMediumPackButton?.onClick.AddListener(() => IAPManager.Instance?.PurchaseCoinPack(IAPManager.ProductMediumPack, _ => UpdateUI()));
            _buyLargePackButton?.onClick.AddListener(() => IAPManager.Instance?.PurchaseCoinPack(IAPManager.ProductLargePack, _ => UpdateUI()));

            _watchAdForCoinsButton?.onClick.AddListener(() =>
            {
                AdManager.Instance?.ShowRewardedAd(() =>
                {
                    CoinEconomy.AddCoins(50);
                    UpdateUI();
                });
            });
        }

        public void Show()
        {
            gameObject.SetActive(true);
            UpdateUI();
        }

        public void Hide() => gameObject.SetActive(false);

        private void UpdateUI()
        {
            if (_currentCoinsText != null)
                _currentCoinsText.text = SaveSystem.Data.totalCoins.ToString();
        }
    }
}
