using UnityEngine;
using UnityEngine.UI;
using TMPro; // [추가] TextMeshPro 네임스페이스

public class UI_Currency : SubUI
{
    enum Texts
    {
        DeathValueText,
        MemoryValueText
    }

    enum Images
    {
        Death,
        Memory
    }

    private int _deathCount = 0;
    private int _memoryCount = 0;
    private bool _initAlready = false;

    public override void Init()
    {
        if (_initAlready) return;

        // 일반 Text 대신 TextMeshProUGUI를 사용 중이라면 TMP_Text로 바인딩
        Bind<TMP_Text>(typeof(Texts));
        // 만약 일반 Text 컴포넌트라면 Bind<Text>(typeof(Texts)); 사용

        Bind<Image>(typeof(Images));

        _initAlready = true;
        RefreshCurrency();
    }

    public void AddDeath(int amount)
    {
        SetDeath(_deathCount + amount);
    }

    public void SetDeath(int value)
    {
        _deathCount = Mathf.Max(0, value);
        if (_initAlready) RefreshCurrency();
    }

    public void AddMemory(int amount)
    {
        SetMemory(_memoryCount + amount);
    }

    public void SetMemory(int value)
    {
        _memoryCount = Mathf.Max(0, value);
        if (_initAlready) RefreshCurrency();
    }

    private void RefreshCurrency()
    {
        // TMP_Text로 가져오도록 수정
        TMP_Text deathText = Get<TMP_Text>((int)Texts.DeathValueText);
        if (deathText != null)
        {
            deathText.text = $"{_deathCount:N0}";
        }

        TMP_Text memoryText = Get<TMP_Text>((int)Texts.MemoryValueText);
        if (memoryText != null)
        {
            memoryText.text = $"{_memoryCount:N0}";
        }
    }
}