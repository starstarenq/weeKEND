using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KDH_SkillSystem.ShopSample
{
    /// <summary>인벤토리 한 칸의 표시(아이콘/이름)만 담당하는 뷰.</summary>
    public sealed class SkillInventorySlotView : MonoBehaviour
    {
        [SerializeField] private Image frame;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameLabel;

        private static readonly Color EmptyColor = new Color(0.1f, 0.13f, 0.18f, 1f);
        private static readonly Color FilledColor = new Color(0.2f, 0.32f, 0.45f, 1f);

        /// <summary>에디터 UI 빌더에서 하위 요소를 연결한다.</summary>
        public void Configure(Image frameImage, Image iconImage, TextMeshProUGUI label)
        {
            frame = frameImage; icon = iconImage; nameLabel = label;
        }

        public void Clear()
        {
            frame.color = EmptyColor;
            icon.sprite = null;
            icon.enabled = false;
            nameLabel.text = "비어 있음";
            nameLabel.color = new Color(1f, 1f, 1f, 0.3f);
        }

        public void Show(SkillBase skill)
        {
            frame.color = FilledColor;
            icon.sprite = skill.Icon;
            icon.enabled = skill.Icon != null;
            nameLabel.text = skill.SkillName;
            nameLabel.color = Color.white;
        }
    }
}
