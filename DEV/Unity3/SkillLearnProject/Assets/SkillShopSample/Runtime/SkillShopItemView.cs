using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KDH_SkillSystem.ShopSample
{
    /// <summary>
    /// 상점 상품 한 줄(아이콘/이름/설명/가격/구매 버튼)을 표시하는 뷰.
    /// 구매 판단은 하지 않고 클릭 사실만 콜백으로 알린다.
    /// </summary>
    public sealed class SkillShopItemView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private TextMeshProUGUI descriptionLabel;
        [SerializeField] private TextMeshProUGUI priceLabel;
        [SerializeField] private Button buyButton;
        [SerializeField] private TextMeshProUGUI buyLabel;

        private SkillBase skill;
        private Action<SkillBase> onBuy;

        public SkillBase Skill => skill;

        /// <summary>에디터 UI 빌더에서 하위 요소를 연결한다.</summary>
        public void Configure(Image iconImage, TextMeshProUGUI title, TextMeshProUGUI description,
            TextMeshProUGUI price, Button button, TextMeshProUGUI buttonLabel)
        {
            icon = iconImage; nameLabel = title; descriptionLabel = description;
            priceLabel = price; buyButton = button; buyLabel = buttonLabel;
        }

        public void Bind(SkillBase data, int price, Action<SkillBase> buyCallback)
        {
            skill = data;
            onBuy = buyCallback;
            icon.sprite = data.Icon;
            icon.enabled = data.Icon != null;
            nameLabel.text = $"{data.SkillName}  <size=70%><color=#9AA4B2>{data.Element} · CD {data.CostInfo.cooldown:0.#}s · MP {data.CostInfo.mpCost}</color></size>";
            descriptionLabel.text = data.Description;
            priceLabel.text = $"{price:N0} G";
            buyButton.onClick.RemoveListener(HandleClick);
            buyButton.onClick.AddListener(HandleClick);
        }

        /// <summary>보유 여부/구매 가능 여부에 따라 버튼 상태를 갱신한다.</summary>
        public void Refresh(bool owned, bool affordable)
        {
            buyButton.interactable = !owned;
            buyLabel.text = owned ? "보유중" : "구매";
            priceLabel.color = owned ? new Color(0.55f, 0.6f, 0.68f)
                : affordable ? new Color(1f, 0.84f, 0.35f) : new Color(1f, 0.4f, 0.4f);
        }

        private void HandleClick() => onBuy?.Invoke(skill);

        private void OnDestroy()
        {
            if (buyButton != null) buyButton.onClick.RemoveListener(HandleClick);
        }
    }
}
