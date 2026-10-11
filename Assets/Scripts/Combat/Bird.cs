using System.Collections;
using UnityEngine;

namespace TopDown
{
    /// <summary>
    /// A wind crow: circles its perch, swoops down to peck a nearby player now and then, and tumbles out
    /// of the sky when hit hard enough. Respawns after a while. Deliberately easy.
    /// </summary>
    public class Bird : Enemy
    {
        enum State { Circling, Swooping, Retreating, Dead }

        [SerializeField] Transform body;
        [SerializeField] SpriteRenderer sprite;
        [SerializeField] SpriteRenderer shadow;
        [SerializeField] Sprite[] flapFrames;
        [SerializeField] Sprite diveFrame;
        [SerializeField] float flyHeight = 1.4f;
        [SerializeField] float circleRadius = 1.4f;
        [SerializeField] float circleSpeed = 1.1f;
        [SerializeField] float aggroRange = 4.5f;
        [SerializeField] float swoopSpeed = 3.6f;
        [SerializeField] float swoopCooldown = 3.5f;
        [SerializeField] int damageHalves = 1;
        [SerializeField] float respawnSeconds = 25f;
        [SerializeField] Sprite featherSprite;
        [SerializeField] AudioClip cawClip;
        [SerializeField] AudioClip deathClip;

        State state = State.Circling;
        Vector2 home;
        Vector2 position;
        float height;
        float angle;
        float direction = 1f;
        float stateTime;
        float cooldown;
        float flap;
        float hurtFlash;

        public override Vector2 AimPoint => body != null ? (Vector2)body.position : (Vector2)transform.position;
        public bool IsSwooping => state == State.Swooping;

        protected override void Awake()
        {
            base.Awake();
            home = transform.position;
            position = home;
            height = flyHeight;
            angle = Random.value * Mathf.PI * 2f;
            direction = Random.value < 0.5f ? -1f : 1f;
            cooldown = Random.Range(1f, swoopCooldown);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            stateTime += dt;
            cooldown -= dt;
            var player = Player.Instance;

            switch (state)
            {
                case State.Circling:
                    angle += circleSpeed * direction * dt;
                    Vector2 orbit = home + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * 0.6f) * circleRadius;
                    position = Vector2.MoveTowards(position, orbit, 3f * dt);
                    height = Mathf.MoveTowards(height, flyHeight + Mathf.Sin(Time.time * 2f + angle) * 0.15f, 1.5f * dt);
                    if (cooldown <= 0f && CanTarget(player) && Vector2.Distance(player.transform.position, home) < aggroRange)
                        Enter(State.Swooping);
                    break;

                case State.Swooping:
                    if (!CanTarget(player))
                    {
                        Enter(State.Retreating);
                        break;
                    }
                    Vector2 targetPos = (Vector2)player.transform.position + new Vector2(0f, 0.15f);
                    position = Vector2.MoveTowards(position, targetPos, swoopSpeed * dt);
                    height = Mathf.MoveTowards(height, 0.55f, 2.2f * dt);
                    if (Vector2.Distance(position, targetPos) < 0.4f && height < 0.8f)
                    {
                        player.Health.TakeDamage(damageHalves);
                        Enter(State.Retreating);
                    }
                    else if (stateTime > 2.6f)
                        Enter(State.Retreating);
                    break;

                case State.Retreating:
                    position = Vector2.MoveTowards(position, home, 2.4f * dt);
                    height = Mathf.MoveTowards(height, flyHeight, 1.5f * dt);
                    if (stateTime > 1.4f)
                        Enter(State.Circling);
                    break;

                case State.Dead:
                    return;
            }

            Animate(dt);
        }

        bool CanTarget(Player player) =>
            player != null && !player.Controller.IsLocked && player.Health != null && !player.Health.IsDead &&
            (player.Airborne == null || (!player.Airborne.Airborne && !player.Airborne.Falling));

        void Enter(State next)
        {
            if (next == State.Swooping)
                Sfx.Play(cawClip, 0.55f, 0.12f);
            if (next == State.Retreating)
                cooldown = swoopCooldown * Random.Range(0.8f, 1.3f);
            state = next;
            stateTime = 0f;
        }

        void Animate(float dt)
        {
            Vector2 previous = transform.position;
            transform.position = position;
            body.localPosition = new Vector3(0f, height, 0f);
            float dx = position.x - previous.x;
            if (Mathf.Abs(dx) > 0.001f)
                sprite.flipX = dx < 0f;

            if (state == State.Swooping && diveFrame != null)
                sprite.sprite = diveFrame;
            else
            {
                flap += dt * (state == State.Retreating ? 14f : 9f);
                sprite.sprite = flapFrames[(int)flap % flapFrames.Length];
            }

            hurtFlash = Mathf.MoveTowards(hurtFlash, 0f, dt * 6f);
            sprite.color = Color.Lerp(Color.white, new Color(1f, 0.35f, 0.3f), hurtFlash);
            if (shadow != null)
            {
                float s = Mathf.Lerp(0.55f, 0.3f, height / 1.6f);
                shadow.transform.localScale = new Vector3(s, s * 0.6f, 1f);
            }
        }

        protected override void OnHurt(Vector2 direction)
        {
            hurtFlash = 1f;
            position += direction * 0.35f;
            Fx.Burst(featherSprite, AimPoint, 3, Vector2.zero, 1.4f, 0.6f, gravity: 2f);
            Enter(State.Retreating);
        }

        protected override void OnDeath(Vector2 direction)
        {
            state = State.Dead;
            Sfx.Play(deathClip, 0.7f, 0.08f);
            Fx.Burst(featherSprite, AimPoint, 10, Vector2.up * 0.4f, 2.2f, 0.9f, gravity: 2.5f);
            StartCoroutine(Fall(direction));
        }

        IEnumerator Fall(Vector2 direction)
        {
            float spin = direction.x >= 0f ? -1f : 1f;
            for (float t = 0f; t < 0.9f; t += Time.deltaTime)
            {
                height = Mathf.Max(0f, height - (2.5f + t * 6f) * Time.deltaTime);
                position += direction * (0.8f * Time.deltaTime);
                transform.position = position;
                body.localPosition = new Vector3(0f, height, 0f);
                body.localRotation = Quaternion.Euler(0f, 0f, spin * 540f * t);
                sprite.color = new Color(1f, 1f, 1f, 1f - t / 0.9f);
                yield return null;
            }
            sprite.enabled = false;
            if (shadow != null)
                shadow.enabled = false;
            body.localRotation = Quaternion.identity;

            yield return new WaitForSeconds(respawnSeconds);

            position = home + Random.insideUnitCircle * circleRadius;
            height = flyHeight + 1.5f;
            sprite.enabled = true;
            if (shadow != null)
                shadow.enabled = true;
            Revive();
            for (float t = 0f; t < 0.8f; t += Time.deltaTime)
            {
                sprite.color = new Color(1f, 1f, 1f, t / 0.8f);
                yield return null;
            }
            sprite.color = Color.white;
            Enter(State.Retreating);
        }
    }
}
