using System;
using UnityEngine;

namespace TopDown
{
    /// <summary>What the player is wearing, one item per <see cref="EquipSlot"/>.</summary>
    [RequireComponent(typeof(Inventory))]
    public class Equipment : MonoBehaviour
    {
        static readonly int SlotCount = Enum.GetValues(typeof(EquipSlot)).Length;

        [Tooltip("Indexed by EquipSlot; entries set here are worn from the start.")]
        [SerializeField] ItemDefinition[] worn = new ItemDefinition[SlotCount];

        Inventory inventory;

        public event Action Changed;

        public ItemDefinition this[EquipSlot slot] => worn[(int)slot];

        void Awake()
        {
            inventory = GetComponent<Inventory>();
            if (worn == null || worn.Length != SlotCount)
                Array.Resize(ref worn, SlotCount);
        }

        /// <summary>Wears the item in an inventory slot, swapping out anything already in that equipment slot.</summary>
        public bool Wear(int inventorySlot)
        {
            var stack = inventory[inventorySlot];
            if (stack.IsEmpty || !stack.item.IsWearable)
                return false;

            var item = stack.item;
            int slot = (int)item.EquipSlot;
            var previous = worn[slot];
            inventory.RemoveAt(inventorySlot);
            worn[slot] = item;
            if (previous != null)
                inventory.Add(previous); // the slot we just emptied guarantees room
            Changed?.Invoke();
            return true;
        }

        /// <summary>Moves a worn item back into the inventory.</summary>
        public bool Remove(EquipSlot slot)
        {
            var item = worn[(int)slot];
            if (item == null)
                return false;
            if (!inventory.Add(item))
            {
                GameLog.Add("You don't have enough inventory space to remove that.");
                return false;
            }
            worn[(int)slot] = null;
            Changed?.Invoke();
            return true;
        }
    }
}
