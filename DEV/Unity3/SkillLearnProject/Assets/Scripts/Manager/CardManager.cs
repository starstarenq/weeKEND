using System.Collections.Generic;
using UnityEngine;

public class CardManager : Singleton<CardManager>
{
    Dictionary<string, CardData> cardDataOrigin = new();
    public CardData[] AllCardDatas;

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    void Init()
    {
        // ScriptableObject 데이터를 모두 가져와서 cardDataOrigin 그 데이터를 사용합니다.

        foreach(var data in AllCardDatas)
        {
            cardDataOrigin.Add(data.CardTitle, data);
        }

        // JSON로 만들어진 TextAsset을 읽어서 데이터를 생성합니다.
    }

    // C# Class 참조 자동 생성되는 문제를 해결한다.
    // cardDataOrigin 직접 수정하면 안됩니다. 싱글톤 / GetId 접근해서 쓰고 / ReadOnly

    public CardData GetCardData(string cardName)
    {
        return cardDataOrigin[cardName].Clone();
    }
}
