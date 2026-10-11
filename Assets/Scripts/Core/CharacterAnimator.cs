using System;
using UnityEngine;

namespace TopDown
{
    /// <summary>
    /// Plays 4-direction idle / walk / action (e.g. chopping) sprite loops.
    /// Reads intent from a <see cref="PlayerController"/> when present, otherwise from the Rigidbody2D velocity,
    /// so it works for the player and for physics-driven NPCs.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class CharacterAnimator : MonoBehaviour
    {
        public enum Direction { Down, Left, Right, Up }

        [Serializable]
        public class DirectionFrames
        {
            public Sprite[] idle = Array.Empty<Sprite>();
            public Sprite[] walk = Array.Empty<Sprite>();
            public Sprite[] action = Array.Empty<Sprite>();
        }

        [SerializeField] Rigidbody2D body;
        [SerializeField] PlayerController controller;
        [SerializeField] DirectionFrames down = new(), left = new(), right = new(), up = new();
        [SerializeField] float idleFps = 2f;
        [Tooltip("Walk frames per second at a speed of 1 unit/s; scales with actual speed so sprinting animates faster.")]
        [SerializeField] float walkFpsPerUnitSpeed = 2.6f;
        [Tooltip("Action loop speed; 2 frames at 3.33 fps = one axe swing every 0.6 s (one OSRS tick).")]
        [SerializeField] float actionFps = 3.333f;
        [SerializeField] float airborneFps = 12f;
        [SerializeField] float movingThreshold = 0.15f;

        SpriteRenderer spriteRenderer;
        Direction facing = Direction.Down;
        bool wasMoving;
        bool playingAction;
        float frameTime;

        public Direction Facing => facing;

        /// <summary>Loops the "action" frames (e.g. chopping) for the current facing direction.</summary>
        public bool PlayingAction
        {
            get => playingAction;
            set
            {
                if (playingAction == value)
                    return;
                playingAction = value;
                frameTime = 0f;
            }
        }

        /// <summary>Plays the walk cycle quickly regardless of velocity (used while leaping).</summary>
        public bool Airborne { get; set; }

        public void SetFrames(Direction direction, Sprite[] idleFrames, Sprite[] walkFrames, Sprite[] actionFrames = null)
        {
            var set = Get(direction);
            set.idle = idleFrames;
            set.walk = walkFrames;
            set.action = actionFrames ?? Array.Empty<Sprite>();
        }

        public static Direction DirectionOf(Vector2 v) =>
            Mathf.Abs(v.x) >= Mathf.Abs(v.y)
                ? (v.x < 0f ? Direction.Left : Direction.Right)
                : (v.y < 0f ? Direction.Down : Direction.Up);

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (body == null)
                body = GetComponentInParent<Rigidbody2D>();
            if (controller == null)
                controller = GetComponentInParent<PlayerController>();
        }

        void LateUpdate()
        {
            Vector2 velocity = controller != null ? controller.MoveVelocity : body != null ? body.linearVelocity : Vector2.zero;
            float speed = velocity.magnitude;
            bool moving = speed > movingThreshold;

            if (controller != null)
                facing = DirectionOf(controller.Facing);
            else if (moving)
                facing = DirectionOf(velocity);

            if (moving != wasMoving)
            {
                frameTime = 0f; // start a walk on a contact frame instead of mid-stride
                wasMoving = moving;
            }

            var set = Get(facing);
            Sprite[] frames;
            float fps;
            if (playingAction && set.action.Length > 0)
            {
                frames = set.action;
                fps = actionFps;
            }
            else if (Airborne)
            {
                frames = set.walk;
                fps = airborneFps;
            }
            else if (moving)
            {
                frames = set.walk;
                fps = walkFpsPerUnitSpeed * speed;
            }
            else
            {
                frames = set.idle;
                fps = idleFps;
            }

            if (frames == null || frames.Length == 0)
                return;

            frameTime += Time.deltaTime * fps;
            spriteRenderer.sprite = frames[(int)frameTime % frames.Length];
        }

        DirectionFrames Get(Direction direction) => direction switch
        {
            Direction.Left => left,
            Direction.Right => right,
            Direction.Up => up,
            _ => down,
        };
    }
}
