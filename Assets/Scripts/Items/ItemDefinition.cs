using UnityEngine;

namespace TopDown
{
    /// <summary>Where a wearable item goes, laid out like the OSRS worn-equipment screen.</summary>
    public enum EquipSlot { None, Head, Cape, Neck, Ammo, Weapon, Body, Shield, Legs, Hands, Feet, Ring }

    [CreateAssetMenu(menuName = "TopDown/Item", fileName = "Item")]
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField] string displayName = "Item";
        [SerializeField] Sprite icon;
        [SerializeField] bool stackable;
        [SerializeField, TextArea] string examine = "";
        [Tooltip("Slot this item is worn in; None for items you can only carry.")]
        [SerializeField] EquipSlot equipSlot = EquipSlot.None;

        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public bool Stackable => stackable;
        public string Examine => examine;
        public EquipSlot EquipSlot => equipSlot;
        public bool IsWearable => equipSlot != EquipSlot.None;

        /// <summary>Verb for the item's default (left-click) action, or null if it has none.</summary>
        public string PrimaryVerb => equipSlot switch
        {
            EquipSlot.None => null,
            EquipSlot.Weapon or EquipSlot.Shield => "Wield",
            _ => "Wear",
        };

        public void Setup(string name, Sprite itemIcon, bool isStackable, string examineText, EquipSlot slot = EquipSlot.None)
        {
            displayName = name;
            icon = itemIcon;
            stackable = isStackable;
            examine = examineText;
            equipSlot = slot;
        }
    }
}
