using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("The speed at which the player moves.")]
    public float moveSpeed = 5.0f;
    public float jumpForce = 8.0f;
    public float gravity = -9.81f;

    [Tooltip("Grace period for jumping when not directly grounded.")]
    public float coyoteTime = 0.3f;

    [Tooltip("Value to set Y velocity to when grounded; provides smoother motion down slopes.")]
    public float groundYVelocity = -4.5f;

    [Header("Look Settings")]
    [Tooltip("The speed/sensitivity of mouse input for rotation.")]
    public float lookSensitivity = 20.0f;


    public Transform playerCamera;

    private Rigidbody rb;
    private Vector3 movementInput;
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool isGrounded;

    private float timeSinceGrounded;

    private float xRotation = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<CharacterController>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody component not found on this GameObject. Please add one!");
            enabled = false;
        }

        if (playerCamera == null)
        {
            // Tries to find the first child component that is a Camera
            playerCamera = GetComponentInChildren<Camera>()?.transform;
            if (playerCamera == null)
            {
                Debug.LogError("Player Camera Transform is not assigned. Drag your main camera object here!");
            }
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {

        if (playerCamera != null)
        {
            float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            transform.Rotate(Vector3.up * mouseX);
        }

        isGrounded = controller.isGrounded;

        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = groundYVelocity;
        }
        if (isGrounded) {
            timeSinceGrounded = 0f;
        } else {
            timeSinceGrounded += Time.deltaTime;
        }
        playerVelocity.y += gravity * Time.deltaTime;

        // Movement input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // TODO should make fit to slope better if possible, a bit jumpy
        Vector3 moveDirection = transform.right * horizontalInput + transform.forward * verticalInput;
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);


        // Jumping
        if (Input.GetButtonDown("Jump") && (isGrounded || timeSinceGrounded < coyoteTime))
        {
            playerVelocity.y = jumpForce;
            timeSinceGrounded = coyoteTime;
        }

        controller.Move(playerVelocity * Time.deltaTime);
    }


    void FixedUpdate()
    {
        if (rb != null)
        {
            Vector3 desiredVelocity = movementInput * moveSpeed;

            rb.velocity = new Vector3(desiredVelocity.x, rb.velocity.y, desiredVelocity.z);
        }
    }
}