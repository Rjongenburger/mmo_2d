using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TopDown
{
    /// <summary>
    /// The player's spells. Click an enemy (or press F to target the hovered / nearest one) to cast the
    /// active spell; casting is a short wind-up and release that throws a homing projectile.
    /// </summary>
    [RequireComponent(typeof(Player))]
    public class SpellCaster : MonoBehaviour
    {
        [Tooltip("Every spell in display order (learned or not).")]
        [SerializeField] SpellDefinition[] spellbook = Array.Empty<SpellDefinition>();
        [SerializeField] List<SpellDefinition> known = new();
        [SerializeField] SpellDefinition active;
        [SerializeField] float range = 6.5f;
        [SerializeField] float windUp = 0.22f;
        [SerializeField] float recover = 0.2f;

        Player player;
        float nextCastTime;

        public IReadOnlyList<SpellDefinition> Spellbook => spellbook;
        public SpellDefinition Active => active;
        public float Range => range;
        public bool IsCasting { get; private set; }

        public event Action<SpellDefinition> SpellLearned;
        public event Action ActiveChanged;
        public event Action<Enemy, SpellDefinition> HitEnemy;

        void Awake() => player = GetComponent<Player>();

        public bool Knows(SpellDefinition spell) => spell != null && known.Contains(spell);

        public void Learn(SpellDefinition spell)
        {
            if (spell == null || known.Contains(spell))
                return;
            known.Add(spell);
            SpellLearned?.Invoke(spell);
            if (active == null)
                SetActive(spell);
        }

        public void SetActive(SpellDefinition spell)
        {
            if (!Knows(spell) || !spell.Castable)
                return;
            active = spell;
            ActiveChanged?.Invoke();
        }

        void Update()
        {
            if (player.Controller.IsLocked || IsCasting)
                return;
            var keyboard = Keyboard.current;
            bool pressed = (keyboard != null && keyboard.fKey.wasPressedThisFrame) ||
                           (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame);
            if (pressed)
                CastQuick();
        }

        /// <summary>F key: the enemy under the mouse, else the nearest one in range, else straight ahead.</summary>
        public void CastQuick()
        {
            var target = player.Interactor.Hovered as Enemy;
            if (target == null || target.IsDead)
                target = Enemy.Nearest(transform.position, range);
            if (target != null)
                CastAt(target);
            else
                CastInDirection(player.Controller.Facing);
        }

        public void CastAt(Enemy target)
        {
            if (!CanCast() || target == null || target.IsDead)
                return;
            if (Vector2.Distance(transform.position, target.transform.position) > range)
            {
                player.Controller.WalkTo(target.transform.position, range * 0.85f, () => CastAt(target));
                return;
            }
            StartCoroutine(Cast(target, Vector2.zero));
        }

        public void CastInDirection(Vector2 direction)
        {
            if (CanCast())
                StartCoroutine(Cast(null, direction.sqrMagnitude > 0.01f ? direction.normalized : Vector2.down));
        }

        bool CanCast()
        {
            if (active == null)
            {
                GameLog.Add("You don't know any spells yet.");
                return false;
            }
            if (IsCasting || Time.time < nextCastTime || player.Controller.IsLocked)
                return false;
            if (player.Airborne != null && (player.Airborne.Airborne || player.Airborne.Falling))
                return false;
            return true;
        }

        IEnumerator Cast(Enemy target, Vector2 direction)
        {
            IsCasting = true;
            var spell = active;
            nextCastTime = Time.time + spell.Cooldown;
            player.CancelAction();
            Vector2 aim = target != null ? target.AimPoint : (Vector2)transform.position + direction;
            player.Controller.FaceTowards(aim);
            player.Controller.Lock();
            player.Animator.PlayingAction = true;

            yield return new WaitForSeconds(windUp);

            Vector2 facing = player.Controller.Facing;
            Vector2 hand = (Vector2)transform.position + new Vector2(0f, 0.6f) + facing * 0.45f;
            if (target != null && !target.IsDead)
                direction = (target.AimPoint - hand).normalized;
            var bolt = Instantiate(spell.Projectile, hand, Quaternion.identity);
            bolt.Launch(target, direction, spell.Damage, enemy =>
            {
                player.Skills.AddXp(Skill.Magic, spell.Experience);
                HitEnemy?.Invoke(enemy, spell);
            });
            Sfx.Play(spell.CastClip, 0.8f, 0.06f);

            yield return new WaitForSeconds(recover);
            player.Animator.PlayingAction = false;
            player.Controller.Unlock();
            IsCasting = false;
        }

        void OnDisable()
        {
            if (!IsCasting)
                return;
            IsCasting = false;
            player.Animator.PlayingAction = false;
            player.Controller.Unlock();
        }
    }
}
