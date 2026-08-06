using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 네임스페이스 추가

public class UI_TraitItem : SubUI
{
    [Header("UI Component References (TMP)")]
    [SerializeField] private TextMeshProUGUI traitNameText; // 인스펙터에서 TraitNameText 연결

    [Header("Selection Highlight")]
    [SerializeField] private Graphic highlightTarget; // 포커스 시 색상이 변경될 대상 (미지정 시 traitNameText 변경)
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private GameObject highlightObject; // 별도 강조용 테두리 오브젝트 (선택)

    [Header("Item Data")]
    [SerializeField] private Sprite traitIcon;

    private bool _initAlready = false;

    public Sprite TraitIcon => traitIcon;

    public override void Init()
    {
        if (_initAlready) return;

        // 인스펙터에 수동 연결이 안 되어있을 경우 자식에서 TMP 컴포넌트 자동 탐색
        if (traitNameText == null)
        {
            traitNameText = GetComponentInChildren<TextMeshProUGUI>();
        }

        _initAlready = true;
    }

    /// <summary>
    /// 특성 데이터 적용 및 TMP 텍스트 갱신
    /// </summary>
    public void SetTraitInfo(string traitName, string traitDesc, Sprite icon = null)
    {
        Init();

        if (traitNameText != null)
        {
            traitNameText.text = traitName;
        }

        if (icon != null)
        {
            traitIcon = icon;

            // Button 자식의 Image에 스프라이트 적용
            Image btnImage = GetComponentInChildren<Button>()?.GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.sprite = icon;
            }
        }
    }

    /// <summary>
    /// 방향키 포커스 이동 시 선택 상태 강조
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        Init();

        if (highlightObject != null)
        {
            highlightObject.SetActive(isSelected);
        }

        Graphic target = highlightTarget != null ? highlightTarget : traitNameText;
        if (target != null)
        {
            target.color = isSelected ? selectedColor : normalColor;
        }
    }
}