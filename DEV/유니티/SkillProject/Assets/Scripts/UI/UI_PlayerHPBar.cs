using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_PlayerHPBar : SubUI
{
    enum Sliders
    {
        PlayerHPSlider
    }

    enum Texts
    {
        PlayerHPText
    }

    enum Images
    {
        PlayerAvatarImage
    }

    [Header("UI 컴포넌트 직접할당 (권장)")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Image playerAvatarImage;

    [Header("게이지 줄어드는 속도")]
    [SerializeField] private float fillSpeed = 5.0f;

    private float targetRatio = 1.0f;
    private float displayHp = 100f;
    private float targetMaxHp = 100f;

    // 💡 [해결] 누락되었던 초기화 체크 변수 추가
    private bool _initAlready = false;

    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        if (_initAlready) return;

        Bind<Slider>(typeof(Sliders));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));

        if (hpSlider == null) hpSlider = Get<Slider>((int)Sliders.PlayerHPSlider);
        if (hpText == null) hpText = Get<TextMeshProUGUI>((int)Texts.PlayerHPText);
        if (playerAvatarImage == null) playerAvatarImage = Get<Image>((int)Images.PlayerAvatarImage);

        if (hpSlider == null) hpSlider = GetComponentInChildren<Slider>(true);
        if (hpText == null) hpText = GetComponentInChildren<TextMeshProUGUI>(true);
        if (playerAvatarImage == null) playerAvatarImage = GetComponentInChildren<Image>(true);

        if (hpSlider != null)
        {
            hpSlider.minValue = 0f;
            hpSlider.maxValue = 1f;
        }

        _initAlready = true;
    }

    private void Update()
    {
        if (hpSlider == null) return;

        hpSlider.value = Mathf.Lerp(hpSlider.value, targetRatio, Time.deltaTime * fillSpeed);
        displayHp = Mathf.Lerp(displayHp, targetRatio * targetMaxHp, Time.deltaTime * fillSpeed);

        if (hpText != null)
        {
            hpText.text = $"{Mathf.CeilToInt(displayHp)} / {Mathf.CeilToInt(targetMaxHp)}";
        }
    }

    public void UpdateHP(float currentHp, float maxHp)
    {
        Init();

        targetMaxHp = maxHp;
        float clampedHp = Mathf.Clamp(currentHp, 0f, maxHp);
        targetRatio = maxHp > 0 ? clampedHp / maxHp : 0f;
    }

    public void SetPlayerAvatar(Sprite avatarSprite)
    {
        Init();

        if (playerAvatarImage != null && avatarSprite != null)
        {
            playerAvatarImage.sprite = avatarSprite;
            playerAvatarImage.gameObject.SetActive(true);
        }
    }
}