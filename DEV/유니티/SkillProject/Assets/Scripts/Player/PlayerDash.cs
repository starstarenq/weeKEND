using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 25f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 2f;

    private Rigidbody rb;
    private PlayerMovement playerMovement;
    private bool isDashing = false;
    private bool canDash = true;

    [HideInInspector]
    public bool isInvincible = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (isDashing) return;

        bool dashPressed = Input.GetKeyDown(KeyCode.LeftShift);
        if (Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            dashPressed = true;
        }

        if (dashPressed && canDash)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    private IEnumerator DashCoroutine()
    {
        canDash = false;
        isDashing = true;
        isInvincible = true;

        // [핵심] 대시 동안 PlayerMovement가 속도를 덮어쓰지 못하도록 일시 중단
        if (playerMovement != null) playerMovement.enabled = false;

        Vector3 dashDir = transform.forward;
        rb.linearVelocity = new Vector3(dashDir.x * dashSpeed, rb.linearVelocity.y, dashDir.z * dashSpeed);

        yield return new WaitForSeconds(dashDuration);

        // 대시 종료 후 PlayerMovement 다시 활성화
        if (playerMovement != null) playerMovement.enabled = true;

        isDashing = false;
        isInvincible = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}