using UnityEngine;

public class PlayerBaseStats : MonoBehaviour
{
    [SerializeField] private JobStatsData jobData;

    private void Start()
    {
        if (jobData != null)
        {
            PrintStats();
        }
    }

    public void PrintStats()
    {
        Debug.Log($"[{jobData.jobName} - Lv.{jobData.level}] 기본 스탯 정보");
        Debug.Log($"STR: {jobData.str} | DEX: {jobData.dex} | INT: {jobData.intel} | LUK: {jobData.luk}");
        Debug.Log($"주스탯: {jobData.mainStat} / 부스탯: {jobData.subStat}");
    }

    // 주 스탯 수치 반환
    public int GetMainStatValue()
    {
        return jobData.mainStat switch
        {
            "STR" => jobData.str,
            "DEX" => jobData.dex,
            "INT" => jobData.intel,
            "LUK" => jobData.luk,
            "HP" => jobData.maxHP,
            _ => 0
        };
    }
}