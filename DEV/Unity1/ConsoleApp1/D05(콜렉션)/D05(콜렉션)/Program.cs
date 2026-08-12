using D05_콜렉션_;
using System;

class Program
{
    static void Main(string[] args)
    {
        // 1. 읽어올 XML 파일 경로 지정 (파일명이 monsters.xml인 경우)
        // 실행 파일(.exe)과 같은 폴더에 xml 파일이 있어야 합니다.
        string filePath = "monsters.xml";

        Console.WriteLine("========================================");
        Console.WriteLine(" [회고인생] 몬스터 XML 데이터 로드 시작 ");
        Console.WriteLine("========================================");

        try
        {
            // 2. XML 로더를 통해 데이터 파싱 및 가져오기
            MonsterDataCollection container = MonsterDataLoader.LoadMonsterData(filePath);

            if (container != null && container.Monsters.Count > 0)
            {
                Console.WriteLine($"성공: 총 {container.Monsters.Count}마리의 몬스터 데이터를 불러왔습니다.\n");

                // 3. 로드된 데이터 반복문으로 화면에 출력하기
                foreach (MonsterData monster in container.Monsters)
                {
                    Console.WriteLine($"----------------------------------------");
                    Console.WriteLine($"[{monster.Id}] {monster.Name} ({monster.Type})");
                    Console.WriteLine($"등장 챕터: {monster.Chapter}");
                    Console.WriteLine($"설명: {monster.Description}");
                    Console.WriteLine($"능력치 - HP: {monster.Stats.Hp} | ATK: {monster.Stats.Atk} | DEF: {monster.Stats.Def} | SPEED: {monster.Stats.Speed}");
                    Console.WriteLine($"처치 보상 - EXP: {monster.Rewards.Exp} | 데스 크리스탈: {monster.Rewards.DeathCrystal}");
                }
                Console.WriteLine($"----------------------------------------");
            }
            else
            {
                Console.WriteLine("오류: 불러온 몬스터 데이터가 비어있거나 올바르지 않습니다.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"프로그램 실행 중 치명적 오류 발생: {ex.Message}");
        }

        // 콘솔 창이 바로 닫히지 않도록 대기
        Console.WriteLine("\n아무 키나 누르면 종료됩니다...");
        Console.ReadKey();
    }
}
