using System;
using System.Collections;
using UnityEngine;

namespace TopDown
{
    /// <summary>
    /// The local player: ties movement, animation, inventory and skills together and runs
    /// timed actions (chopping, lighting fires) that are cancelled when the player walks away.
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
        [SerializeField] Campfire campfirePrefab;
        [SerializeField] LayerMask fireBlockers = ~0;
        [SerializeField] AudioClip igniteClip;

        Coroutine action;
        Action onActionCancelled;

        public PlayerController Controller { get; private set; }
        public Inventory Inventory { get; private set; }
        public Skills Skills { get; private set; }
        public PlayerInteractor Interactor { get; private set; }
        public CharacterAnimator Animator => animator;
        public Transform Body => body;
        public SpriteRenderer Shadow => shadow;
        public bool IsBusy => action != null;

        public event Action<Campfire> FireLit;

        void Awake()
        {
            Instance = this;
            Controller = GetComponent<PlayerController>();
            Inventory = GetComponent<Inventory>();
            Skills = GetComponent<Skills>();
            Interactor = GetComponent<PlayerInteractor>();
        }

        void OnEnable() => Controller.ManualMove += CancelAction;
        void OnDisable() => Controller.ManualMove -= CancelAction;

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

        // ------------------------------------------------------------ inventory actions

        public void UseItem(int slot)
        {
            var stack = Inventory[slot];
            if (stack.IsEmpty)
                return;
            switch (stack.item.PrimaryAction)
            {
                case ItemAction.Light:
                    TryLightFire(slot);
                    break;
                default:
                    ExamineItem(slot);
                    break;
            }
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
            if (stack.IsEmpty || groundItemPrefab == null || Controller.IsLocked)
                return;
            CancelAction();
            var dropped = Instantiate(groundItemPrefab, transform.position + new Vector3(0f, -0.05f, 0f), Quaternion.identity);
            dropped.SetItem(stack.item, stack.amount);
            Inventory.RemoveAt(slot, stack.amount);
        }

        void TryLightFire(int slot)
        {
            if (Controller.IsLocked || campfirePrefab == null)
                return;
            if (!FindFireSpot(out Vector2 spot))
            {
                GameLog.Add("You can't light a fire here.");
                return;
            }
            StartAction(LightFire(slot, spot), () => Controller.Unlock());
        }

        IEnumerator LightFire(int slot, Vector2 spot)
        {
            var logs = Inventory[slot].item;
            GameLog.Add("You attempt to light the logs.");
            Controller.FaceTowards(spot);
            Controller.Lock();
            yield return new WaitForSeconds(1.2f);
            Controller.Unlock();

            int index = Inventory[slot].item == logs ? slot : Inventory.IndexOf(logs);
            if (index < 0)
                yield break;
            Inventory.RemoveAt(index);
            var fire = Instantiate(campfirePrefab, spot, Quaternion.identity);
            Sfx.Play(igniteClip, 0.8f);
            GameLog.Add("The fire catches and the logs begin to burn.");
            FireLit?.Invoke(fire);
        }

        bool FindFireSpot(out Vector2 spot)
        {
            Vector2 feet = transform.position;
            Vector2 facing = Controller.Facing.sqrMagnitude > 0.01f ? Controller.Facing.normalized : Vector2.down;
            Vector2[] candidates = { facing, Vector2.left, Vector2.right, Vector2.down, Vector2.up };
            foreach (var dir in candidates)
            {
                spot = feet + dir * 0.85f;
                var hit = Physics2D.OverlapCircle(spot, 0.3f, fireBlockers);
                if (hit == null || hit.isTrigger)
                    return true;
            }
            spot = default;
            return false;
        }
    }
}
