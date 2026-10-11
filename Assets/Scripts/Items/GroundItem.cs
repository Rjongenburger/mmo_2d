using System;
using UnityEngine;

namespace TopDown
{
    /// <summary>An item lying in the world that can be taken into the inventory.</summary>
    public class GroundItem : Interactable
    {
        [SerializeField] ItemDefinition item;
        [SerializeField] int amount = 1;
        [SerializeField] SpriteRenderer visual;
        [Tooltip("Use the item's inventory icon as the world sprite (off for bespoke visuals like an axe stuck in a block).")]
        [SerializeField] bool useItemIcon = true;
        [SerializeField] bool bob = true;
        [SerializeField] AudioClip pickupClip;

        float bobPhase;

        /// <summary>Raised after any ground item is taken.</summary>
        public static event Action<GroundItem> PickedUp;

        public ItemDefinition Item => item;
        public override string DisplayName => item != null ? item.DisplayName : base.DisplayName;
        public override string Verb => "Take";
        public override string Examine => item != null ? item.Examine : base.Examine;

        public void SetItem(ItemDefinition newItem, int newAmount = 1)
        {
            item = newItem;
            amount = newAmount;
            if (useItemIcon && visual != null && item != null)
                visual.sprite = item.Icon;
        }

        void Start()
        {
            bobPhase = UnityEngine.Random.value * 6f;
            if (useItemIcon && visual != null && item != null)
                visual.sprite = item.Icon;
        }

        void Update()
        {
            if (bob && visual != null)
                visual.transform.localPosition = new Vector3(0f, 0.06f + Mathf.Sin(Time.time * 3f + bobPhase) * 0.04f, 0f);
        }

        public override void Interact(Player player)
        {
            if (item == null)
                return;
            if (!player.Inventory.Add(item, amount))
            {
                GameLog.Add("You don't have enough inventory space.");
                return;
            }
            Sfx.Play(pickupClip, 0.7f);
            PickedUp?.Invoke(this);
            Destroy(gameObject);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => PickedUp = null;
    }
}
