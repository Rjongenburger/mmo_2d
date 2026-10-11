using System.Collections.Generic;
using UnityEngine;

namespace TopDown
{
    /// <summary>
    /// Anything the player can act on with E / a mouse click, OSRS style: "Chop down Tree", "Talk-to Aldric".
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
        public enum Kind { Object, Npc, Item }

        static readonly List<Interactable> all = new();
        public static IReadOnlyList<Interactable> All => all;

        [SerializeField] string displayName = "Object";
        [SerializeField] string verb = "Use";
        [SerializeField] Kind kind = Kind.Object;
        [SerializeField, TextArea] string examine = "";
        [Tooltip("How close (in units, from the player's feet) the player must be to interact.")]
        [SerializeField] float range = 1.1f;
        [SerializeField] Vector2 interactOffset;
        [Tooltip("Renderer whose bounds act as the mouse-over area.")]
        [SerializeField] SpriteRenderer clickArea;

        public virtual string DisplayName => displayName;
        /// <summary>Default action; null/empty means the object can only be examined.</summary>
        public virtual string Verb => verb;
        public Kind NameKind => kind;
        public virtual string Examine => examine;
        public float Range => range;
        public Vector2 InteractPoint => (Vector2)transform.position + interactOffset;
        public SpriteRenderer ClickArea => clickArea;
        public virtual bool CanInteract => isActiveAndEnabled && !string.IsNullOrEmpty(Verb);

        public void Setup(string name, string defaultVerb, Kind nameKind, string examineText, float interactRange,
            SpriteRenderer area, Vector2 offset = default)
        {
            displayName = name;
            verb = defaultVerb;
            kind = nameKind;
            examine = examineText;
            range = interactRange;
            clickArea = area;
            interactOffset = offset;
        }

        public virtual bool ContainsPoint(Vector2 worldPoint)
        {
            if (clickArea == null || !clickArea.enabled || !clickArea.gameObject.activeInHierarchy)
                return false;
            Bounds b = clickArea.bounds;
            return worldPoint.x >= b.min.x && worldPoint.x <= b.max.x && worldPoint.y >= b.min.y && worldPoint.y <= b.max.y;
        }

        public abstract void Interact(Player player);

        protected virtual void OnEnable() => all.Add(this);
        protected virtual void OnDisable() => all.Remove(this);

        // Enter Play Mode Options skip domain reloads, so static state must be reset explicitly.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => all.Clear();
    }
}
