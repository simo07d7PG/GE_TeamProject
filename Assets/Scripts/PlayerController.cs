using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // 이동
    public float moveSpeed = 5f;
    public float jumpPower = 5f;
    public float gravity = -6f;

    // 화면 회전
    public float mouseSensitivity = 0.2f;
    public Transform cameraPivot;
    public Transform cameraTransform;

    // 대시
    public float dashSpeed = 18f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 1.5f;

    private float dashTimeLeft;
    private float nextDashTime;
    private Vector3 dashDirection;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;
    private bool isAiming;
    private float pitch = 20f;
    private float verticalVelocity;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && controller.isGrounded)
        {
            verticalVelocity = jumpPower;
        }
    }

    public void OnSprint(InputValue value)
    {
        isRunning = value.isPressed;
    }

    // Crouch 액션(C 키)을 대시로 사용
    public void OnCrouch(InputValue value)
    {
        if (!value.isPressed || Time.timeScale == 0f) return;
        if (Time.time < nextDashTime) return;

        Vector3 dir = transform.forward * moveInput.y + transform.right * moveInput.x;
        dashDirection = dir.sqrMagnitude > 0.01f ? dir.normalized : transform.forward;
        dashTimeLeft = dashDuration;
        nextDashTime = Time.time + dashCooldown;

        Health hp = GetComponent<Health>();
        if (hp != null) hp.GrantInvincibility(dashDuration + 0.1f);
        Fx.Burst(transform.position + Vector3.up * 0.5f, new Color(0.6f, 0.9f, 1f), 8);
        Fx.Blip(transform.position, 700f, 0.1f, 0.4f);
    }

    void OnGUI()
    {
        if (Time.timeScale == 0f) return;

        float width = 165f;
        Rect back = new Rect((Screen.width - width) * 0.5f, Screen.height - 55f, width, 33f);
        float ready = Mathf.Clamp01(1f - (nextDashTime - Time.time) / dashCooldown);

        HudStyle.Frame(back, HudStyle.BG, HudStyle.LINE);
        float pad = HudStyle.Grid;
        HudStyle.Text(new Rect(back.x + pad, back.y + 6f, 66f, 13f), "DASH", 11, HudStyle.DIM, TextAnchor.MiddleLeft, false);
        HudStyle.Text(new Rect(back.xMax - pad - 66f, back.y + 6f, 66f, 13f), "[C]", 11, HudStyle.DIM, TextAnchor.MiddleRight, false);
        HudStyle.Bar(new Rect(back.x + pad, back.y + 21f, width - pad * 2f, 6f), ready, ready >= 1f ? HudStyle.TEXT : HudStyle.DIM);
    }

    public void OnAim(InputValue value)
    {
        if (value.isPressed)
        {
            isAiming = !isAiming;
        }
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        transform.Rotate(0f, lookInput.x * mouseSensitivity, 0f);
        pitch = pitch - lookInput.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -20f, 60f);
        cameraPivot.localEulerAngles = new Vector3(pitch, 0f, 0f);

        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 move = transform.forward * moveInput.y + transform.right * moveInput.x;
        float speed = moveSpeed;

        float targetX = 1.2f;  
        float targetZ = -2.5f; 

        if (isRunning)
        {
            speed = moveSpeed * 2f;
            targetX = 1.2f;   
            targetZ = -4.0f; 
        }
        if (isAiming)
        {
            targetX = 0.6f;   
            targetZ = -1.2f; 
        }

        if (dashTimeLeft > 0f)
        {
            dashTimeLeft -= Time.deltaTime;
            move = dashDirection * dashSpeed;
        }
        else
        {
            move = move * speed;
        }
        move.y = verticalVelocity;

        Vector3 camPos = cameraTransform.localPosition;
        camPos.x = Mathf.Lerp(camPos.x, targetX, 5f * Time.deltaTime);
        camPos.z = Mathf.Lerp(camPos.z, targetZ, 5f * Time.deltaTime);
        cameraTransform.localPosition = camPos;

        controller.Move(move * Time.deltaTime);
    }
}
