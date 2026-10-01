using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SimpleController_UsingPlayerInput : MonoBehaviour
{
    // 플레이어 입력 코드는 입력을 눌렀습니다! 권한을 주겠다
    // 방법 : Action 이벤트 기반 키워드 사용하기
    // AltFire 다른 클래스에서 실행하겠다
    public event Action OnAltFireInputTriggered;


    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Combat")]
    public GameObject projectile;
    public float projectileSpeed = 20f;

    [Header("Dash Settings")]
    [Tooltip("대시 순간 속도 (추천: 15~20)")]
    public float dashPower = 15f;
    [Tooltip("대시가 지속되는 시간 (초)")]
    public float dashDuration = 0.2f;
    [Tooltip("대시 재사용 대기시간 (쿨타임)")]
    public float dashCooldown = 1f;

    private Rigidbody m_Rb;
    private Vector2 m_MoveInput;
    private Camera m_MainCamera;

    // 상태 제어 변수
    private bool m_IsDashing;
    private bool m_CanDash = true;

    private void Awake()
    {
        m_Rb = GetComponent<Rigidbody>();
        m_MainCamera = Camera.main;
        m_Rb.freezeRotation = true;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        m_MoveInput = context.ReadValue<Vector2>();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        // 대시 중에는 기본 공격 불가
        if (m_IsDashing) return;

        if (context.started)
        {
            Fire();
        }
    }

    // [마우스 우클릭] 정면 방향 순간 대시 기능
    public void OnAltFire(InputAction.CallbackContext context)
    {
        OnAltFireInputTriggered?.Invoke();

        // 버튼을 누른 순간(Started)이고, 대시 가능 상태이며, 현재 대시 중이 아닐 때만 실행
        if (context.started && m_CanDash && !m_IsDashing)
        {
           
            //StartCoroutine(DashRoutine());
        }
    }

    private void Update()
    {
        // 대시 중에는 마우스 방향으로 강제 회전하는 것을 막고 싶다면 정지 가능 (현재는 허용)
        RotateTowardsMouse();
    }

    private void FixedUpdate()
    {
        // 대시 중일 때는 FixedUpdate의 기본 이동 로직이 리지드바디 속도를 덮어쓰지 못하게 차단
        if (m_IsDashing) return;

        m_Rb.linearVelocity = m_MoveInput * moveSpeed;
    }

    private void RotateTowardsMouse()
    {
        if (m_MainCamera == null) return;

        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = m_MainCamera.ScreenToWorldPoint(mouseScreenPos);

        Vector2 lookDirection = (mouseWorldPos - transform.position).normalized;
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // 순간 대시를 부드럽게 처리하는 코루틴
    private IEnumerator DashRoutine()
    {
        m_CanDash = false;
        m_IsDashing = true;

        // 대시 시작 시 기존의 이동 속도를 깨끗하게 초기화합니다.
        m_Rb.linearVelocity = Vector2.zero;

        // 현재 캐릭터의 정면(바라보는 방향) 구하기
        // 2D 회전(Z축) 기준으로는 transform.right가 캐릭터의 정면 뱡향이 됩니다.
        Vector2 dashDirection = transform.right;

        // 순간적으로 강한 힘을 가해 앞으로 날아가게 합니다.
        m_Rb.AddForce(dashDirection * dashPower, ForceMode.Impulse);

        // 정해진 대시 시간만큼 대기 (미끄러지며 날아가는 시간)
        yield return new WaitForSeconds(dashDuration);

        // 대시 종료 후 속도를 급제동하여 멈춰 세웁니다.
        m_Rb.linearVelocity = Vector2.zero;
        m_IsDashing = false;

        // 쿨타임만큼 기다린 후 다시 대시를 사용할 수 있게 합니다.
        yield return new WaitForSeconds(dashCooldown);
        m_CanDash = true;
    }

    private void Fire()
    {
        if (projectile == null) return;

        GameObject newProjectile = Instantiate(projectile, transform.position + transform.right * 0.6f, transform.rotation);

        Rigidbody2D projRb = newProjectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.AddForce(transform.right * projectileSpeed, ForceMode2D.Impulse);
        }
    }
}
