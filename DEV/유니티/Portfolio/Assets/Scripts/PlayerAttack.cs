using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    void Update()
    {
        // 1. 공격 (마우스 좌클릭 - 0)
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("공격 키(마우스 좌클릭)가 입력되었습니다.");
        }

        // 2. 보조 스킬 (마우스 우클릭 - 1)
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("보조 스킬 키(마우스 우클릭)가 입력되었습니다.");
        }

        // 3. 회피 (Left Shift)
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Debug.Log("회피 키(Shift)가 입력되었습니다.");
        }

        // 4. 궁극기 (Q)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("궁극기 키(Q)가 입력되었습니다.");
        }

        // 5. 장비 스킬 (숫자키 1 ~ 5)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("장비 스킬 1번 키가 입력되었습니다.");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("장비 스킬 2번 키가 입력되었습니다.");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("장비 스킬 3번 키가 입력되었습니다.");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("장비 스킬 4번 키가 입력되었습니다.");
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log("장비 스킬 5번 키가 입력되었습니다.");
        }
    }
}
