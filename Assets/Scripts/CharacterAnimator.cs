using System;
using UnityEngine;

namespace TopDown
{
    /// <summary>
    /// Plays 4-direction idle/walk sprite loops based on the Rigidbody2D's velocity.
    /// Works for the player and any NPC that moves through physics.
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
        }

        [SerializeField] Rigidbody2D body;
        [SerializeField] DirectionFrames down = new(), left = new(), right = new(), up = new();
        [SerializeField] float idleFps = 2f;
        [Tooltip("Walk frames per second at a speed of 1 unit/s; scales with actual speed so sprinting animates faster.")]
        [SerializeField] float walkFpsPerUnitSpeed = 2.4f;
        [SerializeField] float movingThreshold = 0.15f;

        SpriteRenderer spriteRenderer;
        Direction facing = Direction.Down;
        bool wasMoving;
        float frameTime;

        public Direction Facing => facing;

        public void SetFrames(Direction direction, Sprite[] idleFrames, Sprite[] walkFrames)
        {
            var set = Get(direction);
            set.idle = idleFrames;
            set.walk = walkFrames;
        }

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (body == null)
                body = GetComponentInParent<Rigidbody2D>();
        }

        void LateUpdate()
        {
            Vector2 velocity = body != null ? body.linearVelocity : Vector2.zero;
            float speed = velocity.magnitude;
            bool moving = speed > movingThreshold;

            if (moving)
            {
                // Prefer the side view on diagonals: it shows the walk cycle best.
                facing = Mathf.Abs(velocity.x) >= Mathf.Abs(velocity.y)
                    ? (velocity.x < 0f ? Direction.Left : Direction.Right)
                    : (velocity.y < 0f ? Direction.Down : Direction.Up);
            }

            if (moving != wasMoving)
            {
                // Start a walk on a contact frame instead of mid-stride.
                frameTime = 0f;
                wasMoving = moving;
            }

            var set = Get(facing);
            var frames = moving ? set.walk : set.idle;
            if (frames == null || frames.Length == 0)
                return;

            frameTime += Time.deltaTime * (moving ? walkFpsPerUnitSpeed * speed : idleFps);
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
