using System.Collections;
using UnityEngine;

public class CoroutineLab : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 코루틴을 시작합니다.
        StartCoroutine(MyCoroutine());
    }

    // 요구사항을 처리하는 코루틴 메서드입니다.
    IEnumerator MyCoroutine()
    {
        // 0.1초 기다렸다가 디버그로 1 출력하세요.
        yield return new WaitForSeconds(0.1f);
        Debug.Log(1);

        // 1초 기다렸다가 디버그로 발사 출력하세요.
        yield return new WaitForSeconds(1f);
        Debug.Log("발사");

        // 2초 뒤에 디버그에 완료를 출력하세요.
        yield return new WaitForSeconds(2f);
        Debug.Log("완료");
    }
}