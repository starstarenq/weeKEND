using UnityEngine;

public class MachineController : MonoBehaviour
{
    // 인스펙터에서 초기값을 지정하거나 실시간 상태를 볼 수 있습니다.
    [Header("기계 설정")]
    public string defaultName = "발전기";
    public int startEnergy = 100;

    // 일반 C# 클래스 객체를 저장할 변수
    [SerializeField]
    private Machine myMachine;

    void Start()
    {
        // 1. 일반 C# 클래스 객체 생성 및 저장
        myMachine = new Machine(defaultName, startEnergy);

        // 2. 클래스의 함수 호출
        myMachine.StartMachine();
    }

    void Update()
    {
        // 예시: 스페이스바를 누를 때마다 에너지를 10씩 소비
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (myMachine != null && myMachine.isActive)
            {
                myMachine.ConsumeEnergy(10);

                // 현재 기계의 남은 에너지 확인
                Debug.Log($"{myMachine.machineName} 남은 에너지: {myMachine.energy}");
            }
        }
    }
}
