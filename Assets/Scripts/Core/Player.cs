using System;
using System.Collections;
using UnityEngine;

namespace TopDown
{
    /// <summary>
    /// The local player: ties movement, animation, inventory, equipment, skills, health and spells together,
    /// runs timed actions that are cancelled when the player walks away, and handles respawning.
    /// </summary>
    [RequireComponent(typeof(PlayerController), typeof(Inventory), typeof(Skills))]
    public class Player : MonoBehaviour
    {
        public static Player Instance { get; private set; }

        [SerializeField] CharacterAnimator animator;
        [Tooltip("Visual child that is lifted during jumps while the shadow stays on the ground.")]
        [SerializeField] Transform body;
        [SerializeField] SpriteRenderer shadow;
        [SerializeField] GroundItem groundItemPrefab;
        [SerializeField] AudioClip respawnClip;

        Coroutine action;
        Action onActionCancelled;

        public PlayerController Controller { get; private set; }
        public Inventory Inventory { get; private set; }
        public Equipment Equipment { get; private set; }
        public Skills Skills { get; private set; }
        public PlayerHealth Health { get; private set; }
        public PlayerAirborne Airborne { get; private set; }
        public SpellCaster Caster { get; private set; }
        public PlayerInteractor Interactor { get; private set; }
        public CharacterAnimator Animator => animator;
        public Transform Body => body;
        public SpriteRenderer Shadow => shadow;
        public bool IsBusy => action != null;

        /// <summary>Where the player wakes up after running out of hearts.</summary>
        public Vector2 Checkpoint { get; set; }

        void Awake()
        {
            Instance = this;
            Controller = GetComponent<PlayerController>();
            Inventory = GetComponent<Inventory>();
            Equipment = GetComponent<Equipment>();
            Skills = GetComponent<Skills>();
            Health = GetComponent<PlayerHealth>();
            Airborne = GetComponent<PlayerAirborne>();
            Caster = GetComponent<SpellCaster>();
            Interactor = GetComponent<PlayerInteractor>();
            Checkpoint = transform.position;
        }

        void OnEnable()
        {
            Controller.ManualMove += CancelAction;
            if (Health != null)
                Health.Died += OnDied;
        }

        void OnDisable()
        {
            Controller.ManualMove -= CancelAction;
            if (Health != null)
                Health.Died -= OnDied;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Starts a timed action, replacing any current one.</summary>
        public void StartAction(IEnumerator routine, Action onCancelled = null)
        {
            CancelAction();
            onActionCancelled = onCancelled;
            action = StartCoroutine(RunAction(routine));
        }

        public void CancelAction()
        {
            if (action == null)
                return;
            StopCoroutine(action);
            action = null;
            var cancelled = onActionCancelled;
            onActionCancelled = null;
            cancelled?.Invoke();
        }

        IEnumerator RunAction(IEnumerator routine)
        {
            yield return routine;
            action = null;
            onActionCancelled = null;
        }

        // ------------------------------------------------------------ inventory / equipment

        /// <summary>Left-click on an inventory item: wear it if possible, otherwise examine it.</summary>
        public void UseItem(int slot)
        {
            var stack = Inventory[slot];
            if (stack.IsEmpty)
                return;
            if (stack.item.IsWearable && Equipment != null)
                Equipment.Wear(slot);
            else
                ExamineItem(slot);
        }

        public void ExamineItem(int slot)
        {
            var stack = Inventory[slot];
            if (!stack.IsEmpty)
                GameLog.Add(stack.item.Examine);
        }

        public void DropItem(int slot)
        {
            var stack = Inventory[slot];
            if (stack.IsEmpty || groundItemPrefab == null || Controller.IsLocked || (Airborne != null && Airborne.Airborne))
                return;
            CancelAction();
            var dropped = Instantiate(groundItemPrefab, transform.position + new Vector3(0f, -0.05f, 0f), Quaternion.identity);
            dropped.SetItem(stack.item, stack.amount);
            Inventory.RemoveAt(slot, stack.amount);
        }

        // ------------------------------------------------------------ death

        void OnDied() => StartCoroutine(DeathRoutine());

        IEnumerator DeathRoutine()
        {
            CancelAction();
            Controller.Lock();
            GameLog.Add("<#a01818>Everything goes dark...</>");
            var fader = ScreenFader.Instance;
            yield return new WaitForSeconds(0.4f);
            if (fader != null)
                yield return fader.Fade(Color.black, fader.Alpha, 1f, 0.6f);

            if (Airborne != null)
                Airborne.ResetTo(Checkpoint);
            else
                transform.position = Checkpoint;
            Health.HealFull();
            yield return new WaitForSeconds(0.4f);
            Sfx.Play(respawnClip, 0.7f);
            GameLog.Add("Aldric's magic pulls you back to the stone circle.");
            if (fader != null)
                yield return fader.Fade(Color.black, 1f, 0f, 0.6f);
            Controller.Unlock();
        }
    }
}
