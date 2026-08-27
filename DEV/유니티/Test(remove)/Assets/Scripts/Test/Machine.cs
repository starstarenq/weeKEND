using UnityEngine;

// 유니티 인스펙터에 노출하기 위해 직렬화 설정을 합니다.
[System.Serializable]
public class Machine
{
    // 기계의 속성 데이터
    public string machineName;
    public int energy;
    public bool isActive;

    // 생성자
    public Machine(string name, int initialEnergy)
    {
        this.machineName = name;
        this.energy = initialEnergy;
        this.isActive = false;
    }

    // 기계의 로직 함수
    public void StartMachine()
    {
        isActive = true;
        Debug.Log($"{machineName} 가동을 시작합니다.");
    }

    public void ConsumeEnergy(int amount)
    {
        if (!isActive) return;

        energy -= amount;
        if (energy <= 0)
        {
            energy = 0;
            isActive = false;
            Debug.Log($"{machineName}의 에너지가 고갈되어 정지합니다.");
        }
    }
}
