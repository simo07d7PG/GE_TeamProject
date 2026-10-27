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

        move = move * speed;
        move.y = verticalVelocity;

        Vector3 camPos = cameraTransform.localPosition;
        camPos.x = Mathf.Lerp(camPos.x, targetX, 5f * Time.deltaTime);
        camPos.z = Mathf.Lerp(camPos.z, targetZ, 5f * Time.deltaTime);
        cameraTransform.localPosition = camPos;

        controller.Move(move * Time.deltaTime);
    }
}
