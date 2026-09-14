using System.Collections;
using UnityEngine;

public class CoroutineLab : MonoBehaviour
{
    void Start()
    {
        // StartCoroutine을 사용해 아래에서 만든 코루틴 함수를 실행합니다.
        StartCoroutine(ExecuteSequence());
    }

    /// <summary>
    /// 지정된 시간 간격으로 디버그 로그를 출력하는 코루틴 함수
    /// </summary>
    private IEnumerator ExecuteSequence()
    {
        // 1. 0.1초 기다렸다 디버그로 1 출력
        yield return new WaitForSeconds(0.1f);
        Debug.Log("1");

        // 2. 1초 기다렸다 디버그로 발사 출력
        yield return new WaitForSeconds(1.0f);
        Debug.Log("발사");

        // 3. 2초 뒤에 디버그에 완료 출력
        yield return new WaitForSeconds(2.0f);
        Debug.Log("완료");
    }
}
