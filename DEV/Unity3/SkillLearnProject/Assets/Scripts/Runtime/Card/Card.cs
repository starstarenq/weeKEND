using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] CardData cardData;

    // Card
    //  CostArea
    //      CostText(TextMeshProUGUI)

    [SerializeField] TextMeshProUGUI CostText;
    [SerializeField] TextMeshProUGUI CardTitle;
    [SerializeField] Image CardImage;
    [SerializeField] TextMeshProUGUI CardDescription;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        // UI 컴포넌트에 데이터를 집어 넣는 작업
        CostText.text = cardData.CostText.ToString();
        CardTitle.text = cardData.CardTitle;
        CardImage.sprite = cardData.CardImage;
        CardDescription.text = FormatAttackMessage(cardData.CardDescription);

        // string타입에 규칙을 설명 %d 텍스트안에 주입할 타입이 정수이다.
        // string과 int 데이터가 있을 때 이를 조합하는 방법을 함수로 만들어줘
    }

    string FormatAttackMessage(CardDescription cardDescription)
    {
        // %d 텍스트를 찾아 정수형 value를 문자열로 변환(ToString)하여 교체합니다.
        return cardDescription.Description.Replace("%d", cardDescription.value.ToString());
    }
}
