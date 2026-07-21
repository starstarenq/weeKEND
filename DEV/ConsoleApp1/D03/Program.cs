using System;

// 1. 플레이어 인스턴스 생성 (최대 체력 100, 이동 속도 5.0)
Player player = new Player(100f, 5.0f);

// 2. 이동 및 공격 테스트
player.Move(1.0f, 0.0f);
player.Attack(); // 1타
player.Attack(); // 2타

// 3. 피격 시뮬레이션 (일반 몬스터에게 일반 공격 피격)
// 기본 데미지 10 * 일반 몬스터(1.0) * 일반 공격(1.0) = 10 데미지
player.TakeDamage(10f, MonsterType.Normal, AttackType.Normal);

// 4. 피격 시뮬레이션 (최종 보스에게 스킬 공격 피격)
// 기본 데미지 20 * 최종 보스(2.5) * 스킬 공격(1.5) = 75 데미지
player.TakeDamage(20f, MonsterType.FinalBoss, AttackType.Skill);

// 5. 에스페란사 획득하여 체력 완전 회복
player.ObtainEsperanza();


// =================================================================
// 정의된 플레이어 클래스 및 열거형
// =================================================================

public enum MonsterType { Normal, Medium, MidBoss, FinalBoss }
public enum AttackType { Normal, Skill }

public class Player
{

    // --- 변수 (Fields & Properties) ---
    public string Name { get; private set; }
    public float Health { get; private set; }
    public float MaxHealth { get; private set; }
    public bool GameOver { get; private set; }

    public (float x, float y) Direction { get; set; }
    public float MoveSpeed { get; set; }

    private int currentCombo;
    private const int MaxCombo = 3;
    private DateTime lastAttackTime;
    private const double ComboWindow = 1.5;

    // 생성자 (초기화용)
    public Player(float maxHealth, float moveSpeed)
    {
        MaxHealth = maxHealth;
        Health = maxHealth;
        MoveSpeed = moveSpeed;
        GameOver = false;
    }

    // --- 메서드 (Methods) ---
    public void Move(float x, float y)
    {
        if (GameOver) return;
        Direction = (x, y);
        Console.WriteLine($"[이동] 방향: ({x}, {y}), 속도: {MoveSpeed}");
    }

    public void Attack()
    {
        if (GameOver) return;

        if ((DateTime.Now - lastAttackTime).TotalSeconds > ComboWindow)
        {
            currentCombo = 0;
        }

        currentCombo++;
        lastAttackTime = DateTime.Now;

        Console.WriteLine($"[공격] 콤보 시전! ({currentCombo}/{MaxCombo})");

        if (currentCombo >= MaxCombo)
        {
            currentCombo = 0;
        }
    }

    public void TakeDamage(float baseDamage, MonsterType monsterType, AttackType attackType)
    {
        if (GameOver) return;

        float monsterMultiplier = monsterType switch
        {
            MonsterType.Normal => 1.0f,
            MonsterType.Medium => 1.3f,
            MonsterType.MidBoss => 1.7f,
            MonsterType.FinalBoss => 2.5f,
            _ => 1.0f
        };

        float attackMultiplier = attackType switch
        {
            AttackType.Normal => 1.0f,
            AttackType.Skill => 1.5f,
            _ => 1.0f
        };

        float finalDamage = baseDamage * monsterMultiplier * attackMultiplier;
        Health -= finalDamage;

        Console.WriteLine($"[피격] {monsterType}의 {attackType} 시전! 데미지 {finalDamage} 적용 (남은 체력: {Health}/{MaxHealth})");

        if (Health <= 0)
        {
            Health = 0;
            TriggerGameOver();
        }
    }

    public void ObtainEsperanza()
    {
        if (GameOver) return;
        Health = MaxHealth;
        Console.WriteLine($"[아이템] 에스페란사 획득! 체력이 최대({MaxHealth})로 회복되었습니다.");
    }

    private void TriggerGameOver()
    {
        GameOver = true;
        Console.WriteLine("[게임 오버] 플레이어의 체력이 없습니다.");
    }
    public class Enemy
    {
        // 플레이어 클래스와 완전히 독립된 독립형 'Name' 프로퍼티입니다
        public string Name { get; private set; }
        public MonsterType Type { get; private set; }
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public float BaseDamage { get; private set; }
        public bool IsDead { get; private set; }

        public (float x, float y) Direction { get; set; }
        public float MoveSpeed { get; set; }

        private bool isProvoked;
        private Player targetPlayer;
        private const int AttackIntervalMs = 1500;

        // 메서드 시그니처
        public void Move(float x, float y) { }
        public void TakeDamage(float damage, Player attacker) { }
        private void StartCounterAttackLoop() { }
    }
}
