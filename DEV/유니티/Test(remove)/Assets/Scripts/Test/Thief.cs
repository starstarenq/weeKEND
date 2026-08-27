using System;

public class Thief
{
    // 속성 (Properties)
    public string Name { get; private set; }
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public int AttackPower { get; private set; }
    public float LifeStealRatio { get; private set; }

    // 체력 변경 시 UI나 컴포넌트에 알리기 위한 이벤트
    public event Action<int, int> OnHealthChanged; // (현재 체력, 최대 체력)

    // 생성자
    public Thief(string name, int maxHealth, int attackPower, float lifeStealRatio)
    {
        Name = name;
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        AttackPower = attackPower;
        LifeStealRatio = lifeStealRatio;
    }

    // 공격 로직 (인터페이스를 활용해 적의 결합도를 낮춤)
    public void Attack(IDamageable target)
    {
        if (target == null) return;

        // 1. 적에게 피해를 입힘
        target.TakeDamage(AttackPower);

        // 2. 흡혈량 계산 및 회복
        int absorbAmount = (int)Math.Round(AttackPower * LifeStealRatio);
        Heal(absorbAmount);
    }

    // 회복 로직
    public void Heal(int amount)
    {
        CurrentHealth += amount;
        if (CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }

        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    // 피격 로직
    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth < 0)
        {
            CurrentHealth = 0;
        }

        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }
}

// 도적이나 적이 상호작용하기 위한 데미지 인터페이스
public interface IDamageable
{
    void TakeDamage(int damage);
}
