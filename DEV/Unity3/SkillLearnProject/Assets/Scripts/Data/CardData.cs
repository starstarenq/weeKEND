using UnityEngine;

[System.Serializable]
public enum CardEffect
{
    Attack,
    Defense,
    Utility
}

public struct CardDescription
{
    public string Description;
    public int value;

   
    
}

[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
public class CardData : ScriptableObject
{
    public int CostText;
    public string CardName;
    public Sprite CardImage;
    public CardDescription CardDescription;
}
