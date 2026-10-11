using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TopDown
{
    /// <summary>
    /// Jumping, flying and falling. Space jumps; Space again in mid-air (while carrying the gale feather)
    /// rides the wind for a few seconds, steered with the movement keys; Space during the flight drops you.
    /// Standing or landing anywhere that isn't <see cref="WalkableGround"/> makes you fall, losing a heart
    /// and waking up at the last safe spot.
    /// </summary>
    [RequireComponent(typeof(Player))]
    public class PlayerAirborne : MonoBehaviour
    {
        [SerializeField] float jumpVelocity = 5.2f;
        [SerializeField] float gravity = 19f;
        [SerializeField] ItemDefinition flightItem;
        [SerializeField] float flightSeconds = 2.6f;
        [SerializeField] float flightHeight = 1.25f;
        [SerializeField] float flightSpeedMultiplier = 1.35f;
        [Tooltip("Gravity while gently gliding down after a flight ends.")]
        [SerializeField] float glideGravity = 6f;
        [SerializeField] int fallDamageHalves = 2;
        [SerializeField] Transform flightMeter;
        [SerializeField] Transform flightMeterFill;
        [SerializeField] AudioClip jumpClip;
        [SerializeField] AudioClip flyClip;
        [SerializeField] AudioClip landClip;
        [SerializeField] AudioClip fallClip;
        [SerializeField] Sprite[] dustFrames;

        Player player;
        Rigidbody2D body;
        Collider2D feet;
        InputAction jumpAction;
        float verticalVelocity;
        float flightLeft;
        bool gliding;
        float offGroundTime;
        float meterWidth;

        public float Height { get; private set; }
        public bool Airborne { get; private set; }
        public bool Flying { get; private set; }
        public bool Falling { get; private set; }
        public float FlightRemaining01 => Flying ? flightLeft / flightSeconds : 0f;
        public Vector2 LastSafePosition { get; private set; }

        public event Action FlightStarted;
        public event Action Landed;
        public event Action Fell;

        bool HasFlightItem => flightItem == null || player.Inventory.Contains(flightItem);
        Vector2 Feet => body.position + new Vector2(0f, 0.08f);

        void Awake()
        {
            player = GetComponent<Player>();
            body = GetComponent<Rigidbody2D>();
            feet = GetComponent<Collider2D>();
            jumpAction = InputSystem.actions.FindAction("Player/Jump", throwIfNotFound: true);
            if (flightMeterFill != null)
                meterWidth = flightMeterFill.localScale.x;
        }

        void OnEnable() => jumpAction.Enable();

        void Start()
        {
            LastSafePosition = body.position;
            SetMeterVisible(false);
        }

        /// <summary>Puts the player back on solid ground (used after respawns and teleports).</summary>
        public void ResetTo(Vector2 position)
        {
            StopAllCoroutines();
            Airborne = Flying = Falling = gliding = false;
            Height = 0f;
            body.position = position;
            transform.position = position;
            LastSafePosition = position;
            player.Controller.SpeedMultiplier = 1f;
            ApplyVisuals();
        }

        void Update()
        {
            if (Falling)
                return;

            if (!player.Controller.IsLocked && jumpAction.WasPressedThisFrame())
            {
                if (!Airborne)
                    Jump();
                else if (!Flying && !gliding && HasFlightItem)
                    StartFlight();
                else if (Flying)
                    EndFlight();
            }

            if (Airborne)
                Simulate(Time.deltaTime);
            else
                CheckGround(Time.deltaTime);

            ApplyVisuals();
        }

        public void Jump()
        {
            if (Airborne || Falling)
                return;
            player.CancelAction();
            Airborne = true;
            verticalVelocity = jumpVelocity;
            Sfx.Play(jumpClip, 0.6f, 0.06f);
        }

        public void StartFlight()
        {
            Flying = true;
            flightLeft = flightSeconds;
            verticalVelocity = 0f;
            player.Controller.SpeedMultiplier = flightSpeedMultiplier;
            player.Animator.Flying = true;
            Sfx.Play(flyClip, 0.8f);
            SetMeterVisible(true);
            FlightStarted?.Invoke();
        }

        void EndFlight()
        {
            Flying = false;
            gliding = true;
            verticalVelocity = 0f;
            player.Controller.SpeedMultiplier = 1f;
            player.Animator.Flying = false;
            SetMeterVisible(false);
        }

        void Simulate(float dt)
        {
            if (Flying)
            {
                flightLeft -= dt;
                float bob = Mathf.Sin(Time.time * 5f) * 0.06f;
                Height = Mathf.MoveTowards(Height, flightHeight + bob, 3f * dt);
                if (flightMeterFill != null) // fill sprite pivots on its left edge, so scaling shrinks it to the left
                    flightMeterFill.localScale = new Vector3(meterWidth * Mathf.Clamp01(flightLeft / flightSeconds),
                        flightMeterFill.localScale.y, 1f);
                if (flightLeft <= 0f)
                    EndFlight();
                return;
            }

            verticalVelocity -= (gliding ? glideGravity : gravity) * dt;
            Height += verticalVelocity * dt;
            if (Height <= 0f)
                Land();
        }

        void Land()
        {
            Airborne = gliding = false;
            Height = 0f;
            verticalVelocity = 0f;
            player.Animator.Airborne = false;
            if (!WalkableGround.IsGround(Feet))
            {
                StartCoroutine(FallRoutine());
                return;
            }
            Sfx.Play(landClip, 0.6f, 0.05f);
            Fx.PlayOnce(dustFrames, body.position, 12f, 1);
            Landed?.Invoke();
        }

        void CheckGround(float dt)
        {
            if (WalkableGround.IsGround(Feet))
            {
                offGroundTime = 0f;
                if (WalkableGround.IsSafe(Feet, 0.45f))
                    LastSafePosition = body.position;
                return;
            }
            // A tiny grace period so brushing the very edge doesn't drop you instantly.
            offGroundTime += dt;
            if (offGroundTime > 0.06f && !player.Controller.IsLocked)
                StartCoroutine(FallRoutine());
        }

        IEnumerator FallRoutine()
        {
            Falling = true;
            player.CancelAction();
            player.Controller.Lock();
            player.Animator.Flying = false;
            player.Animator.Airborne = true;
            feet.enabled = false;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.linearVelocity = Vector2.zero;
            if (player.Shadow != null)
                player.Shadow.enabled = false;
            Sfx.Play(fallClip, 0.8f);
            GameLog.Add("You lose your footing and tumble into the clouds!");
            Fell?.Invoke();

            var visual = player.Body;
            var sprite = visual.GetComponent<SpriteRenderer>();
            for (float t = 0f; t < 1f; t += Time.deltaTime)
            {
                float drop = t * t * 3.5f;
                visual.localPosition = new Vector3(0f, -drop, 0f);
                float s = Mathf.Lerp(1f, 0.45f, t);
                visual.localScale = new Vector3(s, s, 1f);
                visual.localRotation = Quaternion.Euler(0f, 0f, t * 300f);
                sprite.color = new Color(1f, 1f, 1f, 1f - t * t);
                yield return null;
            }

            var fader = ScreenFader.Instance;
            if (fader != null)
                yield return fader.Fade(Color.black, 0f, 1f, 0.35f);

            visual.localPosition = Vector3.zero;
            visual.localScale = Vector3.one;
            visual.localRotation = Quaternion.identity;
            sprite.color = Color.white;
            if (player.Shadow != null)
                player.Shadow.enabled = true;
            body.bodyType = RigidbodyType2D.Dynamic;
            feet.enabled = true;
            body.position = LastSafePosition;
            transform.position = LastSafePosition;
            player.Animator.Airborne = false;
            Falling = false;
            player.Controller.Unlock();
            player.Health.TakeDamage(fallDamageHalves, ignoreInvulnerability: true);
            if (player.Health.IsDead)
                yield break; // the death sequence takes over the (still black) screen

            if (fader != null)
                yield return fader.Fade(Color.black, 1f, 0f, 0.35f);
        }

        void ApplyVisuals()
        {
            // Leave scripted moments (arrival tumble, dialogue, casting) alone while grounded and locked.
            if (Falling || (!Airborne && player.Controller.IsLocked))
                return;
            player.Body.localPosition = new Vector3(0f, Height, 0f);
            player.Animator.Airborne = Airborne && !Flying;
            if (player.Shadow != null)
            {
                bool overGround = WalkableGround.IsGround(Feet);
                float s = Mathf.Lerp(1f, 0.55f, Height / 1.6f);
                player.Shadow.transform.localScale = new Vector3(0.7f * s, 0.55f * s, 1f);
                player.Shadow.enabled = overGround;
            }
        }

        void SetMeterVisible(bool visible)
        {
            if (flightMeter != null)
                flightMeter.gameObject.SetActive(visible);
        }
    }
}
