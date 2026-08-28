using UnityEngine;

// 직업군 분류
public enum JobCategory
{
    Warrior,    // 전사
    Mage,       // 마법사
    Archer,     // 궁수
    Rogue,      // 도적
    PirateSTR,  // 해적 (힘)
    PirateDEX,  // 해적 (민첩)
    Xenon,      // 제논 (하이브리드)
    DemonAvenger // 데몬어벤져 (HP 특화)
}

[CreateAssetMenu(fileName = "NewJobStats", menuName = "MapleStory/Job Base Stats")]
public class JobStatsData : ScriptableObject
{
    [Header("직업 정보")]
    public string jobName;           // 직업명 (예: 히어로, 비숍 등)
    public JobCategory category;     // 직업군

    [Header("30레벨 기본 스탯 (장비 미착용)")]
    public int level = 30;
    public int maxHP;
    public int maxMP;

    [Space(10)]
    public int str;
    public int dex;
    public int intel; // int는 C# 키워드이므로 intel 사용
    public int luk;

    [Header("주스탯 / 부스탯 분류")]
    public string mainStat;
    public string subStat;
}