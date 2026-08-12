using System;
using System.Collections.Generic;
using System.IO;
using GameSystem;

namespace GameSystemTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string jsonPath = "Item.json";

            // 가짜 JSON 데이터 생성 (실행 환경에서 파일이 없을 때를 대비한 안전장치)
            if (!File.Exists(jsonPath))
            {
                string dummyJson = @"[
                    {""Id"":""ITEM_ESPERANZA"",""Name"":""에스페란사"",""Type"":""Consumable"",""Description"":""체력을 모두 회복하고 궁극기 횟수를 1 증가시킵니다."",""Value"":1},
                    {""Id"":""ITEM_CHEERING_BALL"",""Name"":""응원의 구슬"",""Type"":""Gacha"",""Description"":""사용 시 랜덤한 버프 스탯을 획득합니다."",""Value"":0}
                ]";
                File.WriteAllText(jsonPath, dummyJson);
            }

            Console.WriteLine("==================================================");
            Console.WriteLine("▶ 1. 시스템 초기화: Item.json 마스터 데이터 로드");
            Console.WriteLine("==================================================");
            List<Item> itemDatabase = ItemDataManager.LoadItemDatabase(jsonPath);

            // 데이터 로드 확인
            foreach (var item in itemDatabase)
            {
                Console.WriteLine($"로딩된 아이템: {item.Id} - {item.Name}");
            }

            Console.WriteLine("\n==================================================");
            Console.WriteLine("▶ 2. 플레이어 및 인벤토리 인스턴스 생성");
            Console.WriteLine("==================================================");
            Player player = new Player("주인공(유저)");
            Console.WriteLine($"플레이어 생성: {player.Name} (초기 HP: {player.CurrentHp}/{player.MaxHp}, 궁극기 횟수: {player.UltimateCount})");

            Console.WriteLine("\n==================================================");
            Console.WriteLine("▶ 3. 아이템 획득 및 인벤토리 삽입 테스트");
            Console.WriteLine("==================================================");
            // 데이터베이스에서 아이템 파싱 후 플레이어 인벤토리에 파밍 시뮬레이션
            Item esperanza = itemDatabase.Find(i => i.Id == "ITEM_ESPERANZA");
            Item cheeringBall = itemDatabase.Find(i => i.Id == "ITEM_CHEERING_BALL");

            player.PickupItem(esperanza);
            player.PickupItem(cheeringBall);

            Console.WriteLine("\n==================================================");
            Console.WriteLine("▶ 4. 플레이어 인벤토리 호출 및 오픈 기능 테스트");
            Console.WriteLine("==================================================");
            player.OpenInventory();

            Console.WriteLine("==================================================");
            Console.WriteLine("▶ 5. 인벤토리 내 아이템 사용 및 실시간 효과 반영 검증");
            Console.WriteLine("==================================================");
            // 에스페란사 아이템 사용 (체력 전회복 + 궁극기 카운트 업 검증)
            player.UseItemFromInventory("ITEM_ESPERANZA");

            Console.WriteLine("\n==================================================");
            Console.WriteLine("▶ 6. 아이템 사용 후 차감 및 최종 상태 확인");
            Console.WriteLine("==================================================");
            player.OpenInventory();

            Console.WriteLine("테스트 프로세스 종료.");
        }
    }
}
