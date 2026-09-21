using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using UnityEditor.Search;

public class Card : MonoBehaviour
{
    [SerializeField] CardData cardData;
    [SerializeField] TextMeshProUGUI CardName;
    //card
    //  costarea
    //    costtext(TextMeshProUGUI)
    [SerializeField] TextMeshProUGUI CostText;
    [SerializeField] Image CardImage;
    [SerializeField] TextMeshProUGUI CardDescription;

    private void Start()
    {
       Init();
        // Initialize the card
    }
    public void Init()
    {
   CostText.text = cardData.CostText.ToString();
    CardName.text = cardData.CardName;
        CardImage.sprite = cardData.CardImage;
        //CardDescription.text = cardData.CardDescription;
    }
    string CombineUsingFormat(CardDescription cardDescription)
    {
        return cardDescription.Description.Replace("%d", cardDescription.value.ToString());
    }
}
