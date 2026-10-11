using UnityEngine;

namespace TopDown
{
    /// <summary>Tiny sprite-based particle effects (wood chips, sparkles, dust puffs) without particle-system setup.</summary>
    public static class Fx
    {
        public const int DefaultOrder = 300;

        /// <summary>Spawns <paramref name="count"/> sprites flying out from <paramref name="position"/>.</summary>
        public static void Burst(Sprite sprite, Vector2 position, int count, Vector2 baseVelocity, float spread,
            float lifetime, float gravity = 0f, int order = DefaultOrder)
        {
            if (sprite == null)
                return;
            for (int i = 0; i < count; i++)
            {
                Vector2 velocity = baseVelocity + Random.insideUnitCircle * spread;
                Spawn(sprite, position, velocity, lifetime * Random.Range(0.7f, 1.2f), gravity, order);
            }
        }

        public static FxParticle Spawn(Sprite sprite, Vector2 position, Vector2 velocity, float lifetime,
            float gravity = 0f, int order = DefaultOrder)
        {
            var go = new GameObject("Fx");
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            var particle = go.AddComponent<FxParticle>();
            particle.Init(velocity, lifetime, gravity);
            return particle;
        }

        /// <summary>Plays a flipbook once at a position, then removes it.</summary>
        public static void PlayOnce(Sprite[] frames, Vector2 position, float fps, int order = DefaultOrder)
        {
            if (frames == null || frames.Length == 0)
                return;
            var go = new GameObject("Fx Anim");
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = order;
            go.AddComponent<FxAnimation>().Init(frames, fps);
        }
    }

    public class FxParticle : MonoBehaviour
    {
        Vector2 velocity;
        float lifetime, age, gravity;
        SpriteRenderer sr;
        Transform homingTarget;
        float homingStrength;

        public void Init(Vector2 startVelocity, float life, float gravityAcceleration)
        {
            velocity = startVelocity;
            lifetime = life;
            gravity = gravityAcceleration;
            sr = GetComponent<SpriteRenderer>();
        }

        /// <summary>Steers the particle towards a target (portal suction).</summary>
        public void HomeTowards(Transform target, float strength)
        {
            homingTarget = target;
            homingStrength = strength;
        }

        void Update()
        {
            age += Time.deltaTime;
            if (age >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            if (homingTarget != null)
            {
                Vector2 to = (Vector2)homingTarget.position - (Vector2)transform.position;
                velocity += to.normalized * (homingStrength * Time.deltaTime);
                if (to.sqrMagnitude < 0.04f)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            velocity.y -= gravity * Time.deltaTime;
            transform.position += (Vector3)(velocity * Time.deltaTime);
            Color c = sr.color;
            c.a = 1f - Mathf.Clamp01((age / lifetime - 0.6f) / 0.4f);
            sr.color = c;
        }
    }

    public class FxAnimation : MonoBehaviour
    {
        Sprite[] frames;
        float fps, time;
        SpriteRenderer sr;

        public void Init(Sprite[] animationFrames, float framesPerSecond)
        {
            frames = animationFrames;
            fps = framesPerSecond;
            sr = GetComponent<SpriteRenderer>();
            sr.sprite = frames[0];
        }

        void Update()
        {
            time += Time.deltaTime * fps;
            int frame = (int)time;
            if (frame >= frames.Length)
            {
                Destroy(gameObject);
                return;
            }
            sr.sprite = frames[frame];
        }
    }
}
