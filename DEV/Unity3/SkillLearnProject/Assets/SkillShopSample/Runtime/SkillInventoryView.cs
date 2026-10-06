using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace KDH_SkillSystem.ShopSample
{
    /// <summary>
    /// 인벤토리 패널 뷰. 칸 수만큼 슬롯을 만들고 ItemAdded 이벤트에만 반응한다.
    /// 상점을 참조하지 않으므로 다른 획득 경로(보상 등)에도 그대로 재사용 가능하다.
    /// </summary>
    public sealed class SkillInventoryView : MonoBehaviour
    {
        [SerializeField] private SkillInventory inventory;
        [SerializeField] private SkillInventorySlotView slotTemplate;
        [SerializeField] private RectTransform grid;
        [SerializeField] private TextMeshProUGUI countLabel;

        private readonly List<SkillInventorySlotView> slots = new List<SkillInventorySlotView>();

        /// <summary>에디터 UI 빌더에서 참조를 연결한다.</summary>
        public void Configure(SkillInventory target, SkillInventorySlotView template, RectTransform slotGrid,
            TextMeshProUGUI count)
        {
            inventory = target; slotTemplate = template; grid = slotGrid; countLabel = count;
        }

        private void OnEnable()
        {
            if (inventory != null) inventory.ItemAdded += HandleItemAdded;
        }

        private void OnDisable()
        {
            if (inventory != null) inventory.ItemAdded -= HandleItemAdded;
        }

        private void Start()
        {
            slotTemplate.gameObject.SetActive(false);
            for (int i = 0; i < inventory.Capacity; i++)
            {
                var slot = Instantiate(slotTemplate, grid);
                slot.name = $"Slot {i + 1}";
                slot.gameObject.SetActive(true);
                slots.Add(slot);
            }
            // 시작 시점에 이미 들어있는 아이템까지 반영
            for (int i = 0; i < slots.Count; i++)
            {
                if (i < inventory.Count) slots[i].Show(inventory.Items[i]);
                else slots[i].Clear();
            }
            RefreshCount();
        }

        private void HandleItemAdded(SkillBase skill, int index)
        {
            if (index >= 0 && index < slots.Count) slots[index].Show(skill);
            RefreshCount();
            Debug.Log($"[SkillInventory] {skill.SkillName} 이(가) 슬롯 {index + 1}에 추가되었습니다.");
        }

        private void RefreshCount()
        {
            if (countLabel != null) countLabel.text = $"{inventory.Count} / {inventory.Capacity}";
        }
    }
}
