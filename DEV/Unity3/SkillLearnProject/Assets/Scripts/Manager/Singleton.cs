using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // 전역 접근을 가능하게 만들기 위한 정적(static) 변수
    public static T Instance { get; private set; }

    private void Awake()
    {
        // 유일하게 사용할 수 있게 만드는 예외 처리 (중복 생성 방지)
        if (Instance == null)
        {
            Instance = this as T;

            // 씬이 바뀌어도 GameManager 오브젝트가 파괴되지 않도록 설정 (선택 사항)
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 이미 다른 GameManager가 존재한다면 새로 생성된 것을 파괴
            Destroy(gameObject);
        }
    }
}
