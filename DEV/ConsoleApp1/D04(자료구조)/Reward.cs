using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D04_자료구조_
{
        // 보상의 종류를 정의하는 열거형
        public enum RewardType
        {
            Weapon,
            Equipment,
            StatUp
        }

        //  
        public class Reward
        {
            public int Id { get; private set; }
            public string Name { get; private set; }
            public RewardType Type { get; private set; }
            public string Description { get; private set; }

            public Reward(int id, string name, RewardType type, string description)
            {
                Id = id;
                Name = name;
                Type = type;
                Description = description;
            }

            // 보상 정보를 화면에 출력하는 메서수
            public void PrintInfo()
            {
                string typeName = Type switch
                {
                    RewardType.Weapon => "무기",
                    RewardType.Equipment => "장비",
                    RewardType.StatUp => "능력치 증가",
                    _ => "알 수 없음"
                };

                Console.WriteLine($"[{typeName}] {Name} - {Description}");
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

        // 총 15개의 초기 보상 데이터 구성 (무기 5, 장비 5, 능력치 5)
        private void InitRewardDatabase()
        {
            // 무기 5개
            rewardDatabase.Add(new Reward(1, "낡은 철검", RewardType.Weapon, "공격력이 10 증가합니다."));
            rewardDatabase.Add(new Reward(2, "마법의 지팡이", RewardType.Weapon, "주문력이 15 증가합니다."));
            rewardDatabase.Add(new Reward(3, "사냥꾼의 활", RewardType.Weapon, "공격 속도가 10% 증가합니다."));
            rewardDatabase.Add(new Reward(4, "암살자의 단검", RewardType.Weapon, "치명타 확률이 5% 증가합니다."));
            rewardDatabase.Add(new Reward(5, "성스러운 둔기", RewardType.Weapon, "언데드 추가 피해가 발생합니다."));

            // 장비 5개
            rewardDatabase.Add(new Reward(6, "가죽 갑옷", RewardType.Equipment, "방어력이 5 증가합니다."));
            rewardDatabase.Add(new Reward(7, "강철 투구", RewardType.Equipment, "최대 체력이 50 증가합니다."));
            rewardDatabase.Add(new Reward(8, "신속의 장화", RewardType.Equipment, "이동 속도가 15% 증가합니다."));
            rewardDatabase.Add(new Reward(9, "치유의 반지", RewardType.Equipment, "초당 체력 회복량이 2 증가합니다."));
            rewardDatabase.Add(new Reward(10, "마나의 목걸이", RewardType.Equipment, "최대 마나가 30 증가합니다."));

            // 능력치 증가 5개
            rewardDatabase.Add(new Reward(11, "근력 강화", RewardType.StatUp, "기본 힘 능력치가 3 증가합니다."));
            rewardDatabase.Add(new Reward(12, "민첩성 훈련", RewardType.StatUp, "기본 민첩 능력치가 3 증가합니다."));
            rewardDatabase.Add(new Reward(13, "지혜의 깨달음", RewardType.StatUp, "기본 지능 능력치가 3 증가합니다."));
            rewardDatabase.Add(new Reward(14, "강인한 정신", RewardType.StatUp, "모든 상태이상 저항이 5% 증가합니다."));
            rewardDatabase.Add(new Reward(15, "황금의 행운", RewardType.StatUp, "골드 획득량이 10% 증가합니다."));
        }

        // 보상 선택 프로세스를 실행하는 메인 메서드
        public void StartRewardSelection()
        {
            Console.WriteLine("========================================");
            Console.WriteLine(" 보상 선택 스테이지에 진입했습니다! ");
            Console.WriteLine("========================================\n");

            // 3. 중복 없이 랜덤으로 3개 뽑기
            List<Reward> selectedRewards = GetRandomRewards(3);

            // 4. 화면에 한 개씩 데이터 정보 출력
            for (int i = 0; i < selectedRewards.Count; i++)
            {
                Console.Write($"{i + 1}번 보상 선택지 -> ");
                selectedRewards[i].PrintInfo();
            }

            // 5. 유저 선택 입력 처리
            int userChoice = GetUserChoice(selectedRewards.Count);

            // 최종 선택 결과 출력
            Reward chosenReward = selectedRewards[userChoice - 1];
            Console.WriteLine("\n========================================");
            Console.WriteLine($"★ 축하합니다! [{chosenReward.Name}]을(를) 획득하셨습니다.");
            Console.WriteLine("========================================");
        }

        // Fisher-Yates Shuffle 알고리즘의 변형을 사용해 중복 없이 랜덤 추출
        private List<Reward> GetRandomRewards(int count)
        {
            // 원본 데이터 보존을 위해 리스트 복사
            List<Reward> shuffleList = new List<Reward>(rewardDatabase);
            List<Reward> result = new List<Reward>();

            for (int i = 0; i < count; i++)
            {
                if (shuffleList.Count == 0) break;

                int randomIndex = random.Next(0, shuffleList.Count);
                result.Add(shuffleList[randomIndex]);
                shuffleList.RemoveAt(randomIndex); // 뽑힌 아이템은 리스트에서 제거하여 중복 방지
            }

            return result;
        }

        // 유저가 올바른 번호를 입력할 때까지 반복해서 입력을 받는 메서드
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
            rewardManager.StartRewardSelection();
        }
    }
}

