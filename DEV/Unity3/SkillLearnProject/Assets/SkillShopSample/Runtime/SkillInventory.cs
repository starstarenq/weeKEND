using System;
using System.Collections.Generic;
using UnityEngine;

namespace KDH_SkillSystem.ShopSample
{
    /// <summary>
    /// 구매한 스킬(SkillBase)을 보관하는 샘플 인벤토리.
    /// 상점/UI를 참조하지 않고, 아이템 추가 시 이벤트만 발행한다.
    /// </summary>
    public sealed class SkillInventory : MonoBehaviour
    {
        [SerializeField, Min(1)] private int capacity = 8;

        private readonly List<SkillBase> items = new List<SkillBase>();

        /// <summary>아이템 추가 시 (추가된 스킬, 슬롯 인덱스) 전달</summary>
        public event Action<SkillBase, int> ItemAdded;

        public int Capacity => capacity;
        public int Count => items.Count;
        public bool IsFull => items.Count >= capacity;
        public IReadOnlyList<SkillBase> Items => items;

        /// <summary>에디터 샘플 생성기에서 칸 수를 주입한다.</summary>
        public void Configure(int slotCapacity) => capacity = Mathf.Max(1, slotCapacity);

        public bool Contains(SkillBase skill) => skill != null && items.Contains(skill);

        /// <summary>빈 칸이 있고 중복이 아니면 추가한다.</summary>
        public bool TryAdd(SkillBase skill)
        {
            if (skill == null || IsFull || Contains(skill)) return false;
            items.Add(skill);
            ItemAdded?.Invoke(skill, items.Count - 1);
            return true;
        }
    }
}
