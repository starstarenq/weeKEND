using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace KDH_SkillSystem.ShopSample
{
    /// <summary>
    /// 상점 패널 뷰. 카탈로그를 템플릿으로 복제해 상품 목록을 만들고,
    /// 상점/지갑/인벤토리 이벤트를 구독해 표시만 갱신한다.
    /// </summary>
    public sealed class SkillShopView : MonoBehaviour
    {
        [SerializeField] private SkillShop shop;
        [SerializeField] private SkillShopItemView itemTemplate;
        [SerializeField] private RectTransform content;
        [SerializeField] private TextMeshProUGUI goldLabel;
        [SerializeField] private TextMeshProUGUI messageLabel;

        private readonly List<SkillShopItemView> rows = new List<SkillShopItemView>();

        /// <summary>에디터 UI 빌더에서 참조를 연결한다.</summary>
        public void Configure(SkillShop targetShop, SkillShopItemView template, RectTransform listContent,
            TextMeshProUGUI gold, TextMeshProUGUI message)
        {
            shop = targetShop; itemTemplate = template; content = listContent;
            goldLabel = gold; messageLabel = message;
        }

        private void OnEnable()
        {
            if (shop == null) return;
            shop.Purchased += HandlePurchased;
            shop.PurchaseFailed += HandleFailed;
            if (shop.Wallet != null) shop.Wallet.GoldChanged += HandleGoldChanged;
        }

        private void OnDisable()
        {
            if (shop == null) return;
            shop.Purchased -= HandlePurchased;
            shop.PurchaseFailed -= HandleFailed;
            if (shop.Wallet != null) shop.Wallet.GoldChanged -= HandleGoldChanged;
        }

        private void Start()
        {
            itemTemplate.gameObject.SetActive(false);
            foreach (var skill in shop.Catalog)
            {
                var row = Instantiate(itemTemplate, content);
                row.name = $"Shop Item - {skill.SkillName}";
                row.Bind(skill, shop.GetPrice(skill), OnBuyClicked);
                row.gameObject.SetActive(true);
                rows.Add(row);
            }
            ShowMessage(shop.Catalog.Count > 0 ? "구매할 스킬을 선택하세요." : "Resources/Skills 에 스킬 데이터가 없습니다.");
            RefreshAll();
        }

        private void OnBuyClicked(SkillBase skill) => shop.TryPurchase(skill);

        private void HandlePurchased(SkillBase skill, int price)
        {
            ShowMessage($"<color=#7CFC9A>{skill.SkillName}</color> 구매 완료! (-{price:N0} G)");
            RefreshAll();
        }

        private void HandleFailed(SkillBase skill, PurchaseResult result)
        {
            string skillName = skill != null ? skill.SkillName : "알 수 없음";
            string reason = result switch
            {
                PurchaseResult.AlreadyOwned => "이미 보유한 스킬입니다.",
                PurchaseResult.NotEnoughGold => "골드가 부족합니다.",
                PurchaseResult.InventoryFull => "인벤토리가 가득 찼습니다.",
                _ => "구매할 수 없는 상품입니다."
            };
            ShowMessage($"<color=#FF6B6B>{skillName}</color> 구매 실패: {reason}");
        }

        private void HandleGoldChanged(int gold) => RefreshAll();

        private void RefreshAll()
        {
            int gold = shop.Wallet != null ? shop.Wallet.Gold : 0;
            goldLabel.text = $"보유 골드  <color=#FFD659>{gold:N0} G</color>";
            foreach (var row in rows)
                row.Refresh(shop.IsOwned(row.Skill), shop.Wallet != null && shop.Wallet.CanAfford(shop.GetPrice(row.Skill)));
        }

        private void ShowMessage(string text)
        {
            if (messageLabel != null) messageLabel.text = text;
        }
    }
}
