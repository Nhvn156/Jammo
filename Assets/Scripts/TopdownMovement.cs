using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerTopDownController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string speedParam = "Speed"; 

    [Header("Field Bounds (Location soccer field)")]
    [SerializeField] private bool clampToBounds = true;
    [SerializeField] private Vector2 boundsCenterXZ = Vector2.zero;
    [SerializeField] private Vector2 boundsSizeXZ = new Vector2(20f, 14f);

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 moveDirection;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        HandleInput();
        HandleRotation();
        HandleGravity();
        HandleAnimation();
        ApplyMovement();

        if (clampToBounds)
            ClampToField();
    }

    private void HandleInput()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S

        Vector3 input = new Vector3(h, 0f, v);
        moveDirection = input.magnitude > 1f ? input.normalized : input;
    }

    private void HandleRotation()
    {
        if (moveDirection.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void HandleGravity()
    {
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f; 

        velocity.y += gravity * Time.deltaTime;
    }

    private void HandleAnimation()
    {
        if (animator == null) return;
        float speed01 = moveDirection.magnitude;
        animator.SetFloat(speedParam, speed01);
    }

    private void ApplyMovement()
    {
        Vector3 horizontalMove = moveDirection * moveSpeed * Time.deltaTime;
        Vector3 verticalMove = velocity * Time.deltaTime;
        controller.Move(horizontalMove + verticalMove);
    }

    private void ClampToField()
    {
        Vector3 pos = transform.position;

        float minX = boundsCenterXZ.x - boundsSizeXZ.x / 2f;
        float maxX = boundsCenterXZ.x + boundsSizeXZ.x / 2f;
        float minZ = boundsCenterXZ.y - boundsSizeXZ.y / 2f;
        float maxZ = boundsCenterXZ.y + boundsSizeXZ.y / 2f;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);

        transform.position = pos;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!clampToBounds) return;
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3(boundsCenterXZ.x, transform.position.y, boundsCenterXZ.y);
        Vector3 size = new Vector3(boundsSizeXZ.x, 0.1f, boundsSizeXZ.y);
        Gizmos.DrawWireCube(center, size);
    }
#endif
}