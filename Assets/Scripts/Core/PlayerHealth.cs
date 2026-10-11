using System;
using System.Collections;
using UnityEngine;

namespace TopDown
{
    /// <summary>Zelda-style hearts, tracked in half hearts, with a short blink of invulnerability after a hit.</summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Tooltip("Maximum health in half hearts (6 = three hearts).")]
        [SerializeField] int maxHalves = 6;
        [SerializeField] float invulnerableSeconds = 1.2f;
        [SerializeField] SpriteRenderer blinkRenderer;
        [SerializeField] AudioClip hurtClip;

        float invulnerableUntil;

        public int Current { get; private set; }
        public int Max => maxHalves;
        public bool IsDead => Current <= 0;
        public bool IsInvulnerable => Time.time < invulnerableUntil;

        /// <summary>(current, max) in half hearts.</summary>
        public event Action<int, int> Changed;
        public event Action Died;

        void Awake() => Current = maxHalves;

        /// <summary>Applies damage in half hearts. Returns false if it was ignored (already dead or invulnerable).</summary>
        public bool TakeDamage(int halves, bool ignoreInvulnerability = false)
        {
            if (IsDead || (!ignoreInvulnerability && IsInvulnerable))
                return false;

            Current = Mathf.Max(0, Current - halves);
            invulnerableUntil = Time.time + invulnerableSeconds;
            Sfx.Play(hurtClip, 0.7f, 0.05f);
            Changed?.Invoke(Current, maxHalves);
            StopAllCoroutines();
            StartCoroutine(Blink());
            if (IsDead)
                Died?.Invoke();
            return true;
        }

        public void HealFull()
        {
            Current = maxHalves;
            Changed?.Invoke(Current, maxHalves);
        }

        IEnumerator Blink()
        {
            if (blinkRenderer == null)
                yield break;
            while (IsInvulnerable && !IsDead)
            {
                blinkRenderer.enabled = !blinkRenderer.enabled;
                yield return new WaitForSeconds(0.08f);
            }
            blinkRenderer.enabled = true;
        }
    }
}
