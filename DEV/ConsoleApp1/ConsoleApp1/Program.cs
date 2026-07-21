using System;

class Program
{
    static void Main(string[] args)
    {
        // === 1. 초기 변수 선언 (클래스 미사용) ===
        int playerHp = 1250;
        int maxHp = 1250;
        float emotionGauge = 50.0f; // 0.0 ~ 100.0 (불행 ~ 행복)
        int ultimateUseCount = 3;

        int deathCrystal = 0;
        int esperanzaCount = 1; // 테스트용으로 처음에 1개 지급

        int currentChapter = 1; // 1: 유년기, 2: 청소년기, 3: 성인
        int currentStage = 1;

        bool isRunning = true;

        Console.WriteLine("=========================================");
        Console.WriteLine("          ★ 실시간 RPG [회고인생] ★        ");
        Console.WriteLine("=========================================");

        // === 2. 메인 게임 루프 ===
        while (isRunning)
        {
            // 현재 상태 출력
            string chapterName = currentChapter == 1 ? "유년기" : currentChapter == 2 ? "청소년기" : "성인";
            Console.WriteLine($"\n[현재 위치] {chapterName} 던전 - 스테이지 {currentStage}");
            Console.WriteLine($"[플레이어 HP] {playerHp} / {maxHp}");
            Console.WriteLine($"[감정 게이지] {emotionGauge:F1}% (낮을수록 난이도 상승)");
            Console.WriteLine($"[남은 궁극기] {ultimateUseCount}회 | [데스 크리스탈] {deathCrystal}개");
            Console.WriteLine($"[소비 아이템] 에스페란사 {esperanzaCount}개");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("1. 기억 스테이지 진행 (전투)");
            Console.WriteLine("2. 아이템 사용 (에스페란사)");
            Console.WriteLine("3. 게임 종료");
            Console.Write("원하는 행동의 번호를 입력하세요: ");

            string input = Console.ReadLine();
            int menuChoice;

            // int.TryParse를 이용한 메뉴 선택 예외 처리
            if (!int.TryParse(input, out menuChoice))
            {
                Console.WriteLine("[오류] 숫자만 입력할 수 있습니다. 다시 시도하세요.");
                continue;
            }

            // === 3. 메뉴별 기능 처리 ===
            if (menuChoice == 1)
            {
                Console.WriteLine($"\n▶ {currentStage} 스테이지 기억 속으로 진입합니다...");

                // 감정 게이지가 낮을수록 몬스터가 강해져 HP가 더 많이 깎임
                int damage = emotionGauge < 40.0f ? 400 : 200;
                playerHp -= damage;

                // 스테이지 클리어 보상
                deathCrystal += 150;
                Console.WriteLine($"[전투 결과] 몬스터에게 {damage}의 피해를 입었습니다.");
                Console.WriteLine($"[보상 획득] 데스 크리스탈 +150개");

                // 스테이지 클리어 후 감정 변화 입력 유도 (float.TryParse 사용)
                Console.Write("\n이 기억은 어땠나요? 발생한 감정 수치를 입력하세요 (-50.0 ~ 50.0): ");
                string emotionInput = Console.ReadLine();
                float emotionChange;

                if (float.TryParse(emotionInput, out emotionChange))
                {
                    emotionGauge += emotionChange;
                    // 감정 게이지 최소 0, 최대 100 제한
                    if (emotionGauge < 0.0f) emotionGauge = 0.0f;
                    if (emotionGauge > 100.0f) emotionGauge = 100.0f;
                    Console.WriteLine($"[감정 변화] 감정 게이지가 {emotionChange:F1}%만큼 변동되었습니다.");
                }
                else
                {
                    Console.WriteLine("[알림] 올바른 수치(숫자)가 입력되지 않아 감정 변화가 없습니다.");
                }

                // HP 고갈 시 게임 오버 체크
                if (playerHp <= 0)
                {
                    Console.WriteLine("\n[GAME OVER] 주인공이 정신적 충격을 이기지 못하고 쓰러졌습니다.");
                    isRunning = false;
                    break;
                }

                // 스테이지 진행도 상승
                currentStage++;
                if (currentStage > 5) // 5스테이지 클리어 시 다음 챕터로
                {
                    Console.WriteLine($"\n🎉 축하합니다! {chapterName} 챕터의 최종 보스를 클리어했습니다!");
                    currentChapter++;
                    currentStage = 1;
                    ultimateUseCount = 3; // 챕터 이동 시 궁극기 초기화

                    if (currentChapter > 3)
                    {
                        Console.WriteLine("\n[HAPPY ENDING] 모든 기억을 돌아보고 사후세계의 법원으로 향합니다.");
                        isRunning = false;
                    }
                }
            }
            else if (menuChoice == 2)
            {
                // 에스페란사 아이템 사용 (기획서 규칙: 체력 전회복 + 궁극기 사용 횟수 +1)
                if (esperanzaCount > 0)
                {
                    playerHp = maxHp;
                    ultimateUseCount += 1;
                    esperanzaCount--;
                    Console.WriteLine("\n✨ [아이템 사용] '에스페란사'를 사용하여 HP를 모두 회복하고 궁극기 횟수가 1 증가했습니다!");
                }
                else
                {
                    Console.WriteLine("\n[알림] 소지하고 있는 에스페란사 아이템이 없습니다.");
                }
            }
            else if (menuChoice == 3)
            {
                Console.WriteLine("\n게임서가 종료됩니다. 다음에 다시 회고해 주세요.");
                isRunning = false;
            }
            else
            {
                Console.WriteLine("[알림] 1번부터 3번 사이의 메뉴를 선택해 주세요.");
            }
        }
    }
}
