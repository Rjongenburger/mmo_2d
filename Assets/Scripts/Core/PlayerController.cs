using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TopDown
{
    /// <summary>
    /// 8-way top-down movement driven by the project-wide "Player/Move" and "Player/Sprint" actions
    /// (Assets/Settings/InputSystem_Actions.inputactions): WASD / arrow keys / left stick, Shift to sprint.
    /// Also supports OSRS-style "walk here" targets, being locked during cutscenes/dialogue,
    /// and an external push (wind).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 3.6f;
        [SerializeField] float sprintMultiplier = 1.6f;
        [Tooltip("How quickly velocity reaches the target speed (units/s²). Higher = snappier.")]
        [SerializeField] float acceleration = 60f;

        Rigidbody2D body;
        InputAction moveAction;
        InputAction sprintAction;
        Vector2 moveInput;
        int lockCount;

        Vector2? walkTarget;
        float walkStopDistance;
        Action walkArrived;
        float walkBestDistance;
        float walkStuckTime;

        /// <summary>Last non-zero movement direction (or the direction last faced via <see cref="FaceTowards"/>).</summary>
        public Vector2 Facing { get; private set; } = Vector2.down;

        /// <summary>Velocity the player is trying to move at, excluding external pushes like wind.</summary>
        public Vector2 MoveVelocity { get; private set; }

        /// <summary>Extra velocity applied while the player is free to move (e.g. wind gusts).</summary>
        public Vector2 ExternalVelocity { get; set; }

        public bool IsLocked => lockCount > 0;
        public bool IsWalkingToTarget => walkTarget.HasValue;

        /// <summary>Raised every frame the player steers with keys/stick; used to cancel skilling actions.</summary>
        public event Action ManualMove;

        public void Lock()
        {
            lockCount++;
            CancelWalk();
        }

        public void Unlock() => lockCount = Mathf.Max(0, lockCount - 1);

        public void WalkTo(Vector2 point, float stopDistance, Action arrived = null)
        {
            if (IsLocked)
                return;
            walkTarget = point;
            walkStopDistance = Mathf.Max(0.05f, stopDistance);
            walkArrived = arrived;
            walkBestDistance = float.MaxValue;
            walkStuckTime = 0f;
        }

        public void CancelWalk()
        {
            walkTarget = null;
            walkArrived = null;
        }

        public void FaceTowards(Vector2 worldPoint)
        {
            Vector2 delta = worldPoint - (Vector2)transform.position;
            if (delta.sqrMagnitude > 0.0001f)
                Facing = delta.normalized;
        }

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;

            moveAction = InputSystem.actions.FindAction("Player/Move", throwIfNotFound: true);
            sprintAction = InputSystem.actions.FindAction("Player/Sprint");
        }

        void OnEnable()
        {
            moveAction.Enable();
            sprintAction?.Enable();
        }

        void Update()
        {
            Vector2 input = IsLocked ? Vector2.zero : Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);

            if (input.sqrMagnitude > 0.01f)
            {
                CancelWalk();
                moveInput = input;
                ManualMove?.Invoke();
            }
            else if (walkTarget is Vector2 target && !IsLocked)
            {
                moveInput = SteerTowards(target);
            }
            else
            {
                moveInput = Vector2.zero;
            }

            if (moveInput.sqrMagnitude > 0.01f)
                Facing = moveInput.normalized;
        }

        Vector2 SteerTowards(Vector2 target)
        {
            Vector2 toTarget = target - body.position;
            float distance = toTarget.magnitude;
            if (distance <= walkStopDistance)
            {
                var arrived = walkArrived;
                CancelWalk();
                arrived?.Invoke();
                return Vector2.zero;
            }

            // Give up if something blocks the straight path for a while.
            if (distance < walkBestDistance - 0.02f)
            {
                walkBestDistance = distance;
                walkStuckTime = 0f;
            }
            else if ((walkStuckTime += Time.deltaTime) > 0.6f)
            {
                CancelWalk();
                return Vector2.zero;
            }

            return toTarget / distance;
        }

        void FixedUpdate()
        {
            if (body.bodyType != RigidbodyType2D.Dynamic)
            {
                MoveVelocity = Vector2.zero;
                return;
            }

            bool sprinting = sprintAction != null && sprintAction.IsPressed();
            MoveVelocity = moveInput * (moveSpeed * (sprinting ? sprintMultiplier : 1f));
            // External pushes (wind) only act while walking, so a standing player braces instead of sliding.
            bool walking = moveInput.sqrMagnitude > 0.01f;
            Vector2 target = MoveVelocity + (walking && !IsLocked ? ExternalVelocity : Vector2.zero);
            body.linearVelocity = Vector2.MoveTowards(body.linearVelocity, target, acceleration * Time.fixedDeltaTime);
        }
    }
}
