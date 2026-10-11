using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace TopDown
{
    /// <summary>
    /// Finds what the player can act on: the nearest interactable in reach (E key / gamepad) and whatever
    /// is under the mouse (left-click = default action or "Walk here", right-click = options menu).
    /// </summary>
    [RequireComponent(typeof(Player))]
    public class PlayerInteractor : MonoBehaviour
    {
        Player player;
        Camera cam;
        InputAction interactAction;

        public Interactable Focused { get; private set; }
        public Interactable Hovered { get; private set; }
        public Vector2 MouseWorld { get; private set; }
        public bool PointerOverUi { get; private set; }
        /// <summary>True when the mouse is over the game world (not UI) and the player is free to act.</summary>
        public bool CanUsePointer => !PointerOverUi && !player.Controller.IsLocked && Mouse.current != null &&
                                     (GameUI.Instance == null || !GameUI.Instance.BlocksWorldInput);

        /// <summary>Raised on right-click in the world: hovered interactable (may be null) and screen position.</summary>
        public event Action<Interactable, Vector2> WorldMenuRequested;

        void Awake()
        {
            player = GetComponent<Player>();
            // The default "Interact" action uses a Hold interaction; WasPressedThisFrame ignores it so a tap works.
            interactAction = InputSystem.actions.FindAction("Player/Interact", throwIfNotFound: true);
        }

        void OnEnable() => interactAction.Enable();

        void Update()
        {
            if (cam == null)
                cam = Camera.main;

            Focused = player.Controller.IsLocked || player.IsBusy ? null : FindFocus();
            UpdatePointer();

            if (player.Controller.IsLocked)
                return;

            if (interactAction.WasPressedThisFrame() && Focused != null)
                Interact(Focused);

            if (!CanUsePointer)
                return;
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (Hovered != null && Hovered.CanInteract)
                    TryInteract(Hovered);
                else
                {
                    player.CancelAction();
                    player.Controller.WalkTo(MouseWorld, 0.08f);
                }
            }
            else if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                WorldMenuRequested?.Invoke(Hovered, Mouse.current.position.ReadValue());
            }
        }

        void UpdatePointer()
        {
            Hovered = null;
            PointerOverUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (cam == null || Mouse.current == null)
                return;

            Vector3 screen = Mouse.current.position.ReadValue();
            MouseWorld = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z));
            if (PointerOverUi)
                return;

            // Prefer the object drawn in front (lowest y), like sprite sorting.
            float bestY = float.MaxValue;
            foreach (var candidate in Interactable.All)
            {
                if (candidate.ContainsPoint(MouseWorld) && candidate.transform.position.y < bestY)
                {
                    bestY = candidate.transform.position.y;
                    Hovered = candidate;
                }
            }
        }

        Interactable FindFocus()
        {
            Interactable best = null;
            float bestScore = float.MaxValue;
            Vector2 feet = transform.position;
            Vector2 facing = player.Controller.Facing;
            foreach (var candidate in Interactable.All)
            {
                if (!candidate.CanInteract)
                    continue;
                Vector2 to = candidate.InteractPoint - feet;
                float distance = to.magnitude;
                if (distance > candidate.Range)
                    continue;
                float score = distance - (distance > 0.01f ? Vector2.Dot(facing, to / distance) * 0.35f : 0.35f);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }
            return best;
        }

        public bool InRange(Interactable target) =>
            Vector2.Distance(transform.position, target.InteractPoint) <= target.Range;

        /// <summary>Acts on the target now if in reach, otherwise walks over first (OSRS click behaviour).</summary>
        public void TryInteract(Interactable target)
        {
            if (target == null || !target.CanInteract || player.Controller.IsLocked)
                return;
            if (InRange(target))
            {
                Interact(target);
                return;
            }
            player.CancelAction();
            player.Controller.WalkTo(target.InteractPoint, target.Range * 0.8f, () =>
            {
                if (target != null && target.CanInteract && InRange(target))
                    Interact(target);
            });
        }

        void Interact(Interactable target)
        {
            player.Controller.FaceTowards(target.InteractPoint);
            target.Interact(player);
        }
    }
}
