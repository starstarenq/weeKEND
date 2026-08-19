using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_EnemyHPBar : SubUI
{
    enum Texts
    {
        EnemyNameText
    }

    enum Images
    {
        HPBarFill,
        HPDamageFill // 피격 잔상 효과용 (선택)
    }

    [Header("타이머 설정")]
    [SerializeField] private float hideDelay = 4.0f; // 공격을 멈춘 후 UI가 사라질 시간
    private float lastHitTime;
    
    private EnemyHP currentTargetEnemy;

    public override void Init()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (currentTargetEnemy == null || currentTargetEnemy.IsDead)
        {
            gameObject.SetActive(false);
            return;
        }

        // 일정 시간 동안 추가 피격이 없으면 체력바 비활성화
        if (Time.time - lastHitTime > hideDelay)
        {
            gameObject.SetActive(false);
            return;
        }

        // 체력바 비율 갱신
        float ratio = currentTargetEnemy.MaxHP > 0 ? currentTargetEnemy.CurrentHP / currentTargetEnemy.MaxHP : 0f;
        Image fillImg = Get<Image>((int)Images.HPBarFill);
        if (fillImg != null)
        {
            fillImg.fillAmount = ratio;
        }
    }

    /// <summary>
    /// 플레이어가 적을 공격했을 때 호출하여 체력바를 갱신하고 노출함
    /// </summary>
    public void TargetEnemy(EnemyHP enemy)
    {
        if (enemy == null || enemy.IsDead) return;

        currentTargetEnemy = enemy;
        lastHitTime = Time.time;

        TextMeshProUGUI nameText = Get<TextMeshProUGUI>((int)Texts.EnemyNameText);
        if (nameText != null)
        {
            nameText.text = enemy.EnemyName;
        }

        gameObject.SetActive(true);
    }
}