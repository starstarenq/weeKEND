using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_TraitBook : PopUI
{
    [Header("Trait Items (SubUI)")]
    [SerializeField] private UI_TraitItem[] traitSubUIItems;

    [Header("Right Detail View (TMP)")]
    [SerializeField] private TextMeshProUGUI detailNameText;
    [SerializeField] private Image detailIconImage;
    [SerializeField] private TextMeshProUGUI detailDescriptionText;

    [Header("Trait Icons")]
    [SerializeField] private Sprite[] traitIcons;

    private bool _initAlready = false;
    private int _selectedIndex = 0;

    private readonly string[,] _traitData = new string[,]
    {
        { "분노", "2개 : 공격력 +10<br>4개 : 체력 +30<br>5개 : 장비스킬 해제, 체력 +70, 공격력 +50" },
        { "탐욕", "2개 : 추가 재화 획득량 +10%<br>4개 : 추가 재화 획득량 +30%<br>5개 : 장비스킬 해제, 추가 재화 획득량 +100%" },
        { "슬픔", "2개 : 방어력 +10<br>4개 : 마나 +50<br>5개 : 장비스킬 해제, 최대체력의 30% 쉴드 추가" },
        { "사랑", "2개 : 적들의 공격력 -15<br>4개 : 적들의 방어력 -30<br>5개 : 장비스킬 해제, 체력 비례 추가 데미지" },
        { "우정", "2개 : 기억의 휴식처 아이템 할인 10%<br>4개 : 할인 50%<br>5개 : 방문시 매번 한개 공짜" }
    };

    public override void Init()
    {
        if (_initAlready) return;
        _initAlready = true;
    }

    public override void ShowPopup()
    {
        Init();
        base.ShowPopup();

        // 1. 도감 창이 열릴 때 게임 시간 정지 (캐릭터/적 이동 멈춤)
        Time.timeScale = 0f;

        _selectedIndex = 0;
        RefreshTraitBook();
        UpdateSelectionHighlight();
    }

    public override void ClosePopup()
    {
        base.ClosePopup();

        // 2. 도감 창이 닫힐 때 게임 시간 재개
        Time.timeScale = 1f;
    }

    private void OnDisable()
    {
        // 예외 상황(강제 비활성화 등) 발생 시에도 시간이 멈춰있지 않도록 원상복구
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // Time.timeScale이 0이어도 Update() 함수와 Input 입력은 정상 작동합니다.
        HandleInput();
    }

    private void HandleInput()
    {
        int itemCount = Mathf.Min(traitSubUIItems != null ? traitSubUIItems.Length : 0, _traitData.GetLength(0));
        if (itemCount == 0) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _selectedIndex--;
            if (_selectedIndex < 0) _selectedIndex = itemCount - 1;
            UpdateSelectionHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _selectedIndex++;
            if (_selectedIndex >= itemCount) _selectedIndex = 0;
            UpdateSelectionHighlight();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ConfirmSelection();
        }
    }

    private void UpdateSelectionHighlight()
    {
        if (traitSubUIItems == null) return;
        int loopCount = Mathf.Min(traitSubUIItems.Length, _traitData.GetLength(0));

        for (int i = 0; i < loopCount; i++)
        {
            if (traitSubUIItems[i] != null)
            {
                traitSubUIItems[i].SetSelected(i == _selectedIndex);
            }
        }
    }

    private void ConfirmSelection()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _traitData.GetLength(0)) return;

        string name = _traitData[_selectedIndex, 0];
        string desc = _traitData[_selectedIndex, 1];

        if (detailNameText != null) detailNameText.text = name;
        if (detailDescriptionText != null) detailDescriptionText.text = desc;

        if (detailIconImage != null)
        {
            Sprite icon = null;
            if (traitSubUIItems != null && _selectedIndex < traitSubUIItems.Length && traitSubUIItems[_selectedIndex] != null && traitSubUIItems[_selectedIndex].TraitIcon != null)
            {
                icon = traitSubUIItems[_selectedIndex].TraitIcon;
            }
            else if (traitIcons != null && _selectedIndex < traitIcons.Length)
            {
                icon = traitIcons[_selectedIndex];
            }

            if (icon != null)
            {
                detailIconImage.sprite = icon;
                detailIconImage.gameObject.SetActive(true);
            }
        }
    }

    private void RefreshTraitBook()
    {
        if (traitSubUIItems == null || traitSubUIItems.Length == 0) return;

        int loopCount = Mathf.Min(traitSubUIItems.Length, _traitData.GetLength(0));

        for (int i = 0; i < loopCount; i++)
        {
            if (traitSubUIItems[i] != null)
            {
                traitSubUIItems[i].Init();

                string name = _traitData[i, 0];
                string desc = _traitData[i, 1];
                Sprite icon = (traitIcons != null && i < traitIcons.Length) ? traitIcons[i] : null;

                traitSubUIItems[i].SetTraitInfo(name, desc, icon);
            }
        }
    }
}