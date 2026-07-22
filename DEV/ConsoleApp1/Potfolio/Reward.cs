using System;
using System.Collections.Generic;

namespace D04_자료구조_
{
    // 보상의 종류를 정의하는 열거형
    public enum RewardType
    {
        Weapon,
        Equipment,
        StatUp
    }

    public class Reward
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public RewardType Type { get; private set; }
        public string Description { get; private set; }

        // [시니어 팁] 기준이 되는 기본 수치 값을 분리하여 들고 있습니다.
        public int BaseValue { get; private set; }

        public Reward(int id, string name, RewardType type, int baseValue, string description)
        {
            Id = id;
            Name = name;
            Type = type;
            BaseValue = baseValue;
            Description = description;
        }

        // [변경] 스테이지 배율을 적용하여 보상 정보를 화면에 출력하는 메서드
        public void PrintInfo(float multiplier)
        {
            string typeName = Type switch
            {
                RewardType.Weapon => "무기",
                RewardType.Equipment => "장비",
                RewardType.StatUp => "능력치 증가",
                _ => "알 수 없음"
            };

            // 배율이 적용된 최종 수치 계산 (정수형 변환)
            int finalValue = (int)Math.Round(BaseValue * multiplier);

            // 문자열 포맷팅을 통해 {0} 위치에 배율 적용된 수치를 동적으로 치환하여 출력
            string dynamicDescription = string.Format(Description, finalValue);

            Console.WriteLine($"[{typeName}] {Name} - {dynamicDescription}");
        }
    }

    public class RewardManager
    {
        private List<Reward> rewardDatabase = new List<Reward>();
        private Random random = new Random();

        public RewardManager()
        {
            InitRewardDatabase();
        }

        private void InitRewardDatabase()
        {
            // [변경] 기존 하드코딩된 설명문 대신, 수치가 들어갈 자리에 {0} 포맷을 지정하고 기본 수치(BaseValue)를 추가했습니다.
            // 무기 5개
            rewardDatabase.Add(new Reward(1, "낡은 철검", RewardType.Weapon, 10, "공격력이 {0} 증가합니다."));
            rewardDatabase.Add(new Reward(2, "마법의 지팡이", RewardType.Weapon, 15, "주문력이 {0} 증가합니다."));
            rewardDatabase.Add(new Reward(3, "사냥꾼의 활", RewardType.Weapon, 10, "공격 속도가 {0}% 증가합니다."));
            rewardDatabase.Add(new Reward(4, "암살자의 단검", RewardType.Weapon, 5, "치명타 확률이 {0}% 증가합니다."));
            rewardDatabase.Add(new Reward(5, "성스러운 둔기", RewardType.Weapon, 20, "언데드 추가 피해가 {0} 발생합니다."));

            // 장비 5개
            rewardDatabase.Add(new Reward(6, "가죽 갑옷", RewardType.Equipment, 5, "방어력이 {0} 증가합니다."));
            rewardDatabase.Add(new Reward(7, "강철 투구", RewardType.Equipment, 50, "최대 체력이 {0} 증가합니다."));
            rewardDatabase.Add(new Reward(8, "신속의 장화", RewardType.Equipment, 15, "이동 속도가 {0}% 증가합니다."));
            rewardDatabase.Add(new Reward(9, "치유의 반지", RewardType.Equipment, 2, "초당 체력 회복량이 {0} 증가합니다."));
            rewardDatabase.Add(new Reward(10, "마나의 목걸이", RewardType.Equipment, 30, "최대 마나가 {0} 증가합니다."));

            // 능력치 증가 5개
            rewardDatabase.Add(new Reward(11, "근력 강화", RewardType.StatUp, 3, "기본 힘 능력치가 {0} 증가합니다."));
            rewardDatabase.Add(new Reward(12, "민첩성 훈련", RewardType.StatUp, 3, "기본 민첩 능력치가 {0} 증가합니다."));
            rewardDatabase.Add(new Reward(13, "지혜의 깨달음", RewardType.StatUp, 3, "기본 지능 능력치가 {0} 증가합니다."));
            rewardDatabase.Add(new Reward(14, "강인한 정신", RewardType.StatUp, 5, "모든 상태이상 저항이 {0}% 증가합니다."));
            rewardDatabase.Add(new Reward(15, "황금의 행운", RewardType.StatUp, 10, "골드 획득량이 {0}% 증가합니다."));
        }

        // [변경] 현재 진행 중인 스테이지 번호를 매개변수로 받습니다.
        public void StartRewardSelection(int currentStage)
        {
            // [핵심] 스테이지 증가에 따른 배율 계산공식 (예: 1스테이지=1.0배, 5스테이지=1.8배)
            // 기획에 맞게 공식을 언제든지 변경할 수 있습니다.
            float stageMultiplier = 1.0f + (currentStage - 1) * 0.2f;

            Console.WriteLine("========================================");
            Console.WriteLine($" [제 {currentStage} 스테이지 보상 선택] ");
            Console.WriteLine($" (현재 보상 수치 배율: x{stageMultiplier:F1})");
            Console.WriteLine("========================================\n");

            List<Reward> selectedRewards = GetRandomRewards(3);

            for (int i = 0; i < selectedRewards.Count; i++)
            {
                Console.Write($"{i + 1}번 보상 선택지 -> ");
                // [변경] 출력할 때 현재 스테이지 배율을 넘겨 동적으로 계산하게 합니다.
                selectedRewards[i].PrintInfo(stageMultiplier);
            }

            int userChoice = GetUserChoice(selectedRewards.Count);

            Reward chosenReward = selectedRewards[userChoice - 1];
            int finalValue = (int)Math.Round(chosenReward.BaseValue * stageMultiplier);
            string chosenDescription = string.Format(chosenReward.Description, finalValue);

            Console.WriteLine("\n========================================");
            Console.WriteLine($"★ 축하합니다! [{chosenReward.Name}]을(를) 획득하셨습니다.");
            Console.WriteLine($" 효과: {chosenDescription}");
            Console.WriteLine("========================================");
        }

        private List<Reward> GetRandomRewards(int count)
        {
            List<Reward> shuffleList = new List<Reward>(rewardDatabase);
            List<Reward> result = new List<Reward>();

            for (int i = 0; i < count; i++)
            {
                if (shuffleList.Count == 0) break;

                int randomIndex = random.Next(0, shuffleList.Count);
                result.Add(shuffleList[randomIndex]);
                shuffleList.RemoveAt(randomIndex);
            }

            return result;
        }

        private int GetUserChoice(int maxOptions)
        {
            int choice = 0;
            while (true)
            {
                Console.Write($"\n원하는 보상 번호를 선택하세요 (1 ~ {maxOptions}): ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out choice) && choice >= 1 && choice <= maxOptions)
                {
                    break;
                }

                Console.WriteLine("잘못된 입력입니다. 올바른 번호를 다시 입력해주세요.");
            }
            return choice;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            RewardManager rewardManager = new RewardManager();

            // [변경] 가상의 게임 루프 테스트 (1스테이지부터 3스테이지까지 연속 진행 예시)
            for (int stage = 1; stage <= 3; stage++)
            {
                rewardManager.StartRewardSelection(stage);
                Console.WriteLine("\n다음 스테이지로 이동하려면 아무 키나 누르세요...\n");
                Console.ReadKey();
                Console.Clear();
            }
        }
    }
}
