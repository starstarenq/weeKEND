using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KDH_SkillSystem.ShopSample
{
    /// <summary>구매 시도 결과</summary>
    public enum PurchaseResult
    {
        Success,
        AlreadyOwned,
        NotEnoughGold,
        InventoryFull,
        InvalidItem
    }

    /// <summary>스킬 ID별 판매 가격</summary>
    [Serializable]
    public struct SkillPriceEntry
    {
        public int skillId;
        [Min(0)] public int price;

        public SkillPriceEntry(int skillId, int price)
        {
            this.skillId = skillId;
            this.price = price;
        }
    }

    /// <summary>
    /// 상점 로직(카탈로그 로드, 가격 조회, 구매 검증)만 담당한다.
    /// UI를 알지 못하며 구매 결과는 이벤트로 전파한다.
    /// 카탈로그는 Resources 폴더의 SkillBase 에셋에서 로드한다.
    /// </summary>
    public sealed class SkillShop : MonoBehaviour
    {
        [Tooltip("Resources 기준 스킬 에셋 폴더 경로")]
        [SerializeField] private string resourcesPath = "Skills";
        [SerializeField, Min(0)] private int defaultPrice = 100;
        [SerializeField] private List<SkillPriceEntry> prices = new List<SkillPriceEntry>();
        [SerializeField] private SkillShopWallet wallet;
        [SerializeField] private SkillInventory inventory;

        private readonly List<SkillBase> catalog = new List<SkillBase>();

        /// <summary>구매 성공 시 (스킬, 가격) 전달</summary>
        public event Action<SkillBase, int> Purchased;
        /// <summary>구매 실패 시 (스킬, 실패 사유) 전달</summary>
        public event Action<SkillBase, PurchaseResult> PurchaseFailed;

        public IReadOnlyList<SkillBase> Catalog => catalog;
        public SkillShopWallet Wallet => wallet;
        public SkillInventory Inventory => inventory;

        private void Awake() => LoadCatalog();

        /// <summary>에디터 샘플 생성기에서 의존성과 가격표를 주입한다.</summary>
        public void Configure(string path, int fallbackPrice, IEnumerable<SkillPriceEntry> priceTable,
            SkillShopWallet targetWallet, SkillInventory targetInventory)
        {
            resourcesPath = path;
            defaultPrice = Mathf.Max(0, fallbackPrice);
            prices = priceTable != null ? priceTable.ToList() : new List<SkillPriceEntry>();
            wallet = targetWallet;
            inventory = targetInventory;
        }

        /// <summary>Resources 폴더에서 스킬 데이터를 읽어 ID 순으로 정렬한다.</summary>
        public void LoadCatalog()
        {
            catalog.Clear();
            catalog.AddRange(Resources.LoadAll<SkillBase>(resourcesPath)
                .Where(skill => skill != null)
                .OrderBy(skill => skill.SkillId));
            if (catalog.Count == 0)
                Debug.LogWarning($"[SkillShop] Resources/{resourcesPath} 에서 SkillBase 에셋을 찾지 못했습니다.");
        }

        public int GetPrice(SkillBase skill)
        {
            if (skill == null) return defaultPrice;
            foreach (var entry in prices)
                if (entry.skillId == skill.SkillId) return entry.price;
            return defaultPrice;
        }

        public bool IsOwned(SkillBase skill) => inventory != null && inventory.Contains(skill);

        /// <summary>구매 가능 여부를 검증한 뒤 골드 차감 → 인벤토리 추가 순으로 처리한다.</summary>
        public PurchaseResult TryPurchase(SkillBase skill)
        {
            PurchaseResult result = Validate(skill);
            if (result == PurchaseResult.Success)
            {
                int price = GetPrice(skill);
                wallet.TrySpend(price);
                inventory.TryAdd(skill);
                Purchased?.Invoke(skill, price);
            }
            else
            {
                PurchaseFailed?.Invoke(skill, result);
            }
            return result;
        }

        private PurchaseResult Validate(SkillBase skill)
        {
            if (skill == null || wallet == null || inventory == null || !catalog.Contains(skill))
                return PurchaseResult.InvalidItem;
            if (inventory.Contains(skill)) return PurchaseResult.AlreadyOwned;
            if (inventory.IsFull) return PurchaseResult.InventoryFull;
            if (!wallet.CanAfford(GetPrice(skill))) return PurchaseResult.NotEnoughGold;
            return PurchaseResult.Success;
        }
    }
}
