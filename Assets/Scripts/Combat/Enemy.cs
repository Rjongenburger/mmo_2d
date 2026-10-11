using System;
using System.Collections.Generic;
using UnityEngine;

namespace TopDown
{
    /// <summary>Something the player can fight. Clicking it ("Attack") casts the active spell at it.</summary>
    public abstract class Enemy : Interactable
    {
        static readonly List<Enemy> alive = new();

        [SerializeField] int maxHitpoints = 2;

        protected int hitpoints;

        public static IReadOnlyList<Enemy> Alive => alive;
        public static event Action<Enemy> Killed;

        public bool IsDead => hitpoints <= 0;
        public override string Verb => "Attack";
        public override bool CanInteract => base.CanInteract && !IsDead;
        /// <summary>Where spells should aim (e.g. a flying bird's body, not its shadow).</summary>
        public virtual Vector2 AimPoint => transform.position;

        protected virtual void Awake() => hitpoints = maxHitpoints;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsDead)
                alive.Add(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            alive.Remove(this);
        }

        public override void Interact(Player player)
        {
            if (player.Caster != null)
                player.Caster.CastAt(this);
        }

        public void TakeHit(int damage, Vector2 direction)
        {
            if (IsDead)
                return;
            hitpoints -= damage;
            if (hitpoints > 0)
            {
                OnHurt(direction);
                return;
            }
            alive.Remove(this);
            OnDeath(direction);
            Killed?.Invoke(this);
        }

        protected void Revive()
        {
            hitpoints = maxHitpoints;
            if (!alive.Contains(this))
                alive.Add(this);
        }

        protected abstract void OnHurt(Vector2 direction);
        protected abstract void OnDeath(Vector2 direction);

        public static Enemy Nearest(Vector2 from, float maxDistance)
        {
            Enemy best = null;
            float bestDistance = maxDistance;
            foreach (var enemy in alive)
            {
                float d = Vector2.Distance(from, enemy.transform.position);
                if (d <= bestDistance)
                {
                    bestDistance = d;
                    best = enemy;
                }
            }
            return best;
        }

        /// <summary>First living enemy whose aim point is within <paramref name="radius"/> of a point.</summary>
        public static Enemy HitTest(Vector2 point, float radius)
        {
            foreach (var enemy in alive)
                if (Vector2.Distance(point, enemy.AimPoint) <= radius)
                    return enemy;
            return null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            alive.Clear();
            Killed = null;
        }
    }
}
