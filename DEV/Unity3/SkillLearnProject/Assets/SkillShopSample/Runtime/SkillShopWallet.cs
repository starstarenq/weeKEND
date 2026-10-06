using System;
using UnityEngine;

namespace KDH_SkillSystem.ShopSample
{
    /// <summary>
    /// 플레이어의 재화(골드)만 관리하는 지갑 컴포넌트.
    /// 상점/인벤토리를 알지 못하며, 값이 바뀌면 이벤트로만 알린다. (단일 책임)
    /// </summary>
    public sealed class SkillShopWallet : MonoBehaviour
    {
        [SerializeField, Min(0)] private int startingGold = 500;

        private int gold;

        /// <summary>골드 변경 시 (현재 골드) 전달</summary>
        public event Action<int> GoldChanged;

        public int Gold => gold;

        private void Awake() => gold = startingGold;

        /// <summary>에디터 샘플 생성기에서 초기값을 주입한다.</summary>
        public void Configure(int startGold) => startingGold = Mathf.Max(0, startGold);

        public bool CanAfford(int amount) => amount >= 0 && gold >= amount;

        /// <summary>잔액이 충분하면 차감하고 true를 반환한다.</summary>
        public bool TrySpend(int amount)
        {
            if (!CanAfford(amount)) return false;
            gold -= amount;
            GoldChanged?.Invoke(gold);
            return true;
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;
            gold += amount;
            GoldChanged?.Invoke(gold);
        }
    }
}
