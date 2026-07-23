using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace GameSystem
{
    // ==========================================
    // [1] 데이터 도메인 영역 (Item Data)
    // ==========================================
    public class Item
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public int Value { get; set; }

        public override string ToString() => $"[{Name} ({Type}) - {Description}]";
    }

    // ==========================================
    // [2] 컴포넌트 영역 (Inventory Component)
    // ==========================================
    public class Inventory
    {
        private readonly List<Item> _slots = new List<Item>();
        private readonly int _maxSlots;

        public IReadOnlyList<Item> Slots => _slots;
        public int MaxSlots => _maxSlots;

        public Inventory(int maxSlots = 20)
        {
            _maxSlots = maxSlots;
        }

        // 아이템 추가 로직
        public bool AddItem(Item item)
        {
            if (item == null) return false;
            if (_slots.Count >= _maxSlots)
            {
                Console.WriteLine("❌ 인벤토리 슬롯이 가득 찼습니다.");
                return false;
            }

            _slots.Add(item);
            Console.WriteLine($"인벤토리에 아이템 추가 완료: {item.Name}");
            return true;
        }

        // 아이템 제거/사용 로직
        public bool RemoveItem(string itemId)
        {
            var item = _slots.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
            {
                Console.WriteLine("❌ 인벤토리에 해당 아이템이 없습니다.");
                return false;
            }

            _slots.Remove(item);
            return true;
        }

        // 인벤토리 UI 렌더링 시뮬레이션
        public void DisplayInventory()
        {
            Console.WriteLine($"\n=====  인벤토리 상태 ({_slots.Count}/{_maxSlots}) =====");
            if (!_slots.Any())
            {
                Console.WriteLine("비어 있음");
                return;
            }

            for (int i = 0; i < _slots.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {_slots[i].Name} | {_slots[i].Description}");
            }
            Console.WriteLine("===========================================\n");
        }
    }

    // ==========================================
    // [3] 엔티티 도메인 영역 (Player Entity)
    // ==========================================

    // 플레이어가 인벤토리 기능 구현
    // 인벤토리 기능 클래스 인벤토리에서 만들어

    public class Player
    {
        public string Name { get; private set; }
        public int CurrentHp { get; private set; }
        public int MaxHp { get; private set; }
        public int UltimateCount { get; private set; }
        public Inventory Inventory { get; private set; }

        public Player(string name, int maxHp = 1250)
        {
            Name = name;
            MaxHp = maxHp;
            CurrentHp = maxHp / 2; // 데모용 반피 설정
            UltimateCount = 0;
            Inventory = new Inventory(10); // 슬롯 10개 할당
        }

        // 플레이어 동작: 인벤토리 열기
        public void OpenInventory()
        {
            Console.WriteLine($"\n {Name}님이 인벤토리를 열었습니다.");
            Inventory.DisplayInventory();
        }

        // 플레이어 동작: 아이템 획득
        public void PickupItem(Item item)
        {
            Inventory.AddItem(item);
        }

        // 플레이어 동작: 인벤토리 내부 아이템 사용
        public void UseItemFromInventory(string itemId)
        {
            // 인벤토리에 아이템이 있는지 먼저 검증
            var item = Inventory.Slots.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
            {
                Console.WriteLine($"❌ {Name}의 인벤토리에 해당 아이템({itemId})이 존재하지 않습니다.");
                return;
            }

            Console.WriteLine($"\n {Name}이(가) '{item.Name}' 아이템을 사용합니다.");

            // 기획서 시스템 반영 (아이템 타입별 효과 분기 처리)
            switch (item.Type)
            {
                case "Consumable":
                    CurrentHp = MaxHp; // 체력 전회복
                    UltimateCount += item.Value; // 궁극기 사용 횟수 +1
                    Console.WriteLine($" 체력이 최대로 회복되었습니다! ({CurrentHp}/{MaxHp})");
                    Console.WriteLine($" 궁극기 사용 가능 횟수가 증가했습니다! (현재: {UltimateCount}회)");
                    break;

                case "Gacha":
                    Console.WriteLine(" 랜덤 버프 효과가 발동되었습니다. (응원의 구슬 기믹)");
                    break;

                default:
                    Console.WriteLine(" 알 수 없는 타입의 아이템 효과입니다.");
                    break;
            }

            // 사용 완료 후 인벤토리에서 차감
            Inventory.RemoveItem(itemId);
        }
    }

    // ==========================================
    // [4] 데이터 마스터 로더 (Data Loader)
    // ==========================================
    public static class ItemDataManager
    {
        public static List<Item> LoadItemDatabase(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"❌ JSON 파일을 찾을 수 없습니다: {filePath}");
                    return new List<Item>();
                }

                string jsonString = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<Item>>(jsonString) ?? new List<Item>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 데이터 로드 에러: {ex.Message}");
                return new List<Item>();
            }
        }
    }
}

