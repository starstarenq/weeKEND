// 의사 코드 (Pseudo code)

// 주제: 조건문을 써서 특정 조건일 때만 실행하는 기능 만들자.

// 플레이어의 상태는 **이다.

// 플레이어가 가질 수 있는 상태는 일반 상태, 무적 상태, 게임 오버 상태 .....

// 상태에 따라서 어떤 행동을 할 것인지 각각 의사코드를 작성하시오.

// if(일반 상태).. 어떤 행동을 하라.

// else if(무적 상태).. 데미지 받으면 안된다.

// else if(게임 오버 상태).. 

// else ... 잘못된 상태.

// 플레이어의 상태는 일반 상태이다.

// 플레이어가 가질 수 있는 상태는 일반 상태, 피로 상태, 게임 오버 상태가 있다.

// if(일반 상태) 체력이 전체 체력의 25%이상 일때 항시 유지되는 일반적인 상태, 아무런 제약 없는 평범한 상태이다.

// else if(피로 상태) 체력이 전체 체력의 25%미만 일때 피로 상태가 된다. 피로 상태에서는 이동속도 감소, 방어력 감소, 공격력 감소가 생긴다.

// else if(게임 오버 상태) 체력이 0이 되었을 때 게임 오버 상태가 된다. 게임 오버 상태에서는 플레이어가 더 이상 행동할 수 없고, 게임을 재시작해야 한다.

// else 잘못된 상태이다.

using System;

namespace PlayerStatusSystem
{ 
    // 1. 플레이어의 상태 종류를 정의하는 열거형
    public enum PlayerState
    {
        Normal,     // 일반 상태
        Fatigue,    // 피로 상태
        GameOver,   // 게임 오버 상태
        Invalid     // 잘못된 상태
    }

    // 2. 플레이어의 로직을 담당하는 클래스
    public class Player
    {
        public float MaxHp { get; private set; } = 100f;
        public float CurrentHp { get; private set; }
        public PlayerState CurrentState { get; private set; }

        // 상태에 따른 디버프 배율
        public float MoveSpeedMultiplier { get; private set; } = 1.0f;
        public float DefenseMultiplier { get; private set; } = 1.0f;
        public float AttackPowerMultiplier { get; private set; } = 1.0f;

        public Player()
        {
            CurrentHp = MaxHp;
            CurrentState = PlayerState.Normal;
        }

        // 체력을 변경하는 메서드
        public void UpdateHp(float newHp)
        {
            CurrentHp = Math.Clamp(newHp, 0f, MaxHp);
            UpdateState();
        }

        // 기획 조건을 검사하여 상태를 업데이트하는 메서드
        private void UpdateState()
        {
            float hpPercentage = (CurrentHp / MaxHp) * 100f;

            // 1. 게임 오버 상태 (체력 0)
            if (CurrentHp <= 0)
            {
                CurrentState = PlayerState.GameOver;
                ApplyStateEffects(0f, 0f, 0f);
                Console.WriteLine($"[상태 변경] 게임 오버! (현재 체력: {CurrentHp})");
            }
            // 2. 일반 상태 (체력 25% 이상)
            else if (hpPercentage >= 25f)
            {
                CurrentState = PlayerState.Normal;
                ApplyStateEffects(1.0f, 1.0f, 1.0f);
                Console.WriteLine($"[상태 변경] 일반 상태 유지 (현재 체력: {CurrentHp} / {hpPercentage}%)");
            }
            // 3. 피로 상태 (체력 25% 미만)
            else if (hpPercentage < 25f && CurrentHp > 0)
            {
                CurrentState = PlayerState.Fatigue;
                ApplyStateEffects(0.5f, 0.7f, 0.8f); // 이동속도 50%, 방어력 70%, 공격력 80%로 감소
                Console.WriteLine($"[상태 변경] 피로 상태 발생! 능력치 감소 (현재 체력: {CurrentHp} / {hpPercentage}%)");
            }
            // 4. 잘못된 상태
            else
            {
                CurrentState = PlayerState.Invalid;
                Console.WriteLine("[오류] 잘못된 상태입니다.");
            }
        }

        private void ApplyStateEffects(float speed, float def, float atk)
        {
            MoveSpeedMultiplier = speed;
            DefenseMultiplier = def;
            AttackPowerMultiplier = atk;
        }

        public bool CanAction()
        {
            return CurrentState != PlayerState.GameOver;
        }
    }

    // 3. 프로그램이 시작되는 실행 클래스
    class Program
    {
        // 프로그램의 진입점 (Main 함수)
        static void Main(string[] args)
        {
            Console.WriteLine("=== 플레이어 상태 시스템 시뮬레이션 시작 ===\n");

            // 플레이어 생성 (초기 상태: 일반 상태, 체력 100)
            Player player = new Player();
            Console.WriteLine($"초기 상태: {player.CurrentState} (이동속도 배율: {player.MoveSpeedMultiplier})");
            Console.WriteLine("------------------------------------------------");

            // 시나리오 1: 데미지를 입어 체력이 50이 됨 (50% -> 일반 상태 유지)
            Console.WriteLine("시나리오 1: 플레이어가 몬스터에게 공격받아 체력이 50이 되었습니다.");
            player.UpdateHp(50);
            Console.WriteLine($"-> 현재 플레이어 상태: {player.CurrentState}");
            Console.WriteLine($"-> 능력치 변동 - 공격력 배율: {player.AttackPowerMultiplier}\n");

            // 시나리오 2: 추가 데미지를 입어 체력이 15가 됨 (15% -> 피로 상태 전환)
            Console.WriteLine("시나리오 2: 추가 타격을 입어 체력이 15로 떨어졌습니다.");
            player.UpdateHp(15);
            Console.WriteLine($"-> 현재 플레이어 상태: {player.CurrentState}");
            Console.WriteLine($"-> 능력치 변동 - 이동속도 배율: {player.MoveSpeedMultiplier} (디버프 적용됨)\n");

            // 시나리오 3: 치명타를 맞아 체력이 0이 됨 (0% -> 게임 오버 상태 전환)
            Console.WriteLine("시나리오 3: 강력한 일격을 맞아 체력이 0이 되었습니다.");
            player.UpdateHp(0);
            Console.WriteLine($"-> 현재 플레이어 상태: {player.CurrentState}");

            // 시나리오 4: 게임 오버 후 행동 가능 여부 확인
            Console.WriteLine("\n시나리오 4: 플레이어가 행동을 시도합니다.");
            if (player.CanAction())
            {
                Console.WriteLine("-> 플레이어가 행동을 취합니다.");
            }
            else
            {
                Console.WriteLine("-> [행동 불가] 게임 오버 상태이므로 더 이상 행동할 수 없습니다. 재시작이 필요합니다.");
            }

            Console.WriteLine("\n=== 시뮬레이션 종료 ===");
        }
    }
}

