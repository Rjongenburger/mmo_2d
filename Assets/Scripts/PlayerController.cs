using UnityEngine;
using UnityEngine.InputSystem;

namespace TopDown
{
    /// <summary>
    /// 8-way top-down movement driven by the project-wide "Player/Move" and "Player/Sprint" actions
    /// (Assets/Settings/InputSystem_Actions.inputactions): WASD / arrow keys / left stick, Shift to sprint.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float sprintMultiplier = 1.6f;
        [Tooltip("How quickly velocity reaches the target speed (units/s²). Higher = snappier.")]
        [SerializeField] float acceleration = 60f;

        Rigidbody2D body;
        InputAction moveAction;
        InputAction sprintAction;
        Vector2 moveInput;

        /// <summary>Last non-zero movement direction; useful for attacks, interaction and animation.</summary>
        public Vector2 Facing { get; private set; } = Vector2.down;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;

            moveAction = InputSystem.actions.FindAction("Player/Move", throwIfNotFound: true);
            sprintAction = InputSystem.actions.FindAction("Player/Sprint");
        }

        void Update()
        {
            moveInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
            if (moveInput.sqrMagnitude > 0.01f)
                Facing = moveInput.normalized;
        }

        void FixedUpdate()
        {
            bool sprinting = sprintAction != null && sprintAction.IsPressed();
            Vector2 targetVelocity = moveInput * (moveSpeed * (sprinting ? sprintMultiplier : 1f));
            body.linearVelocity = Vector2.MoveTowards(body.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
    }
}
