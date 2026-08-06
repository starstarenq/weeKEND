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
        { "분노", "2개 : 공격력 +10/n 4개 : 체력 +30/n" },
        { "탐욕", "모든 재화 획득 확률이 상승합니다." },
        { "슬픔", "방어력과 MP최대치가 증가합니다." },
        { "사랑", "적들의 공격력과 방어력이 하락합니다." },
        { "우정", "기억의 휴식처에서의 모든 아이템 가격이 할인됩니다." }
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