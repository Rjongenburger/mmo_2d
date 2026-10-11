using System;
using UnityEngine;

namespace TopDown
{
    /// <summary>A spell bolt that homes in on its target (or flies straight), bursting on impact.</summary>
    public class Projectile : MonoBehaviour
    {
        [SerializeField] float speed = 9f;
        [SerializeField] float turnRate = 14f;
        [SerializeField] float maxLifetime = 1.6f;
        [SerializeField] Transform visual;
        [SerializeField] Sprite trailSprite;
        [SerializeField] GameObject impactPrefab;
        [SerializeField] AudioClip impactClip;

        Enemy target;
        Vector2 direction;
        int damage;
        Action<Enemy> onHit;
        float age;
        bool done;

        public void Launch(Enemy homingTarget, Vector2 initialDirection, int hitDamage, Action<Enemy> hitCallback)
        {
            target = homingTarget;
            direction = initialDirection.sqrMagnitude > 0.0001f ? initialDirection.normalized : Vector2.right;
            damage = hitDamage;
            onHit = hitCallback;
            Orient();
        }

        void Update()
        {
            if (done)
                return;
            float dt = Time.deltaTime;
            age += dt;

            if (target != null && !target.IsDead)
            {
                Vector2 desired = (target.AimPoint - (Vector2)transform.position).normalized;
                direction = Vector2.Lerp(direction, desired, Mathf.Clamp01(turnRate * dt)).normalized;
            }
            transform.position += (Vector3)(direction * (speed * dt));
            Orient();

            if (UnityEngine.Random.value < dt * 40f)
                Fx.Spawn(trailSprite, (Vector2)transform.position - direction * 0.2f + UnityEngine.Random.insideUnitCircle * 0.06f,
                    -direction * 0.6f + Vector2.up * 0.4f, 0.35f, order: 255);

            var hit = target != null && !target.IsDead && Vector2.Distance(transform.position, target.AimPoint) < 0.3f
                ? target
                : Enemy.HitTest(transform.position, 0.3f);
            if (hit != null)
            {
                hit.TakeHit(damage, direction);
                onHit?.Invoke(hit);
                Burst();
            }
            else if (age > maxLifetime)
            {
                Burst();
            }
        }

        void Orient()
        {
            if (visual != null)
                visual.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }

        void Burst()
        {
            done = true;
            if (impactPrefab != null)
                Instantiate(impactPrefab, transform.position, Quaternion.identity);
            Sfx.Play(impactClip, 0.7f, 0.08f);
            Destroy(gameObject);
        }
    }
}
