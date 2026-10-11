using System;
using UnityEngine;

namespace TopDown
{
    /// <summary>OSRS-style 28-slot inventory.</summary>
    public class Inventory : MonoBehaviour
    {
        public const int Capacity = 28;

        [Serializable]
        public struct Slot
        {
            public ItemDefinition item;
            public int amount;

            public bool IsEmpty => item == null || amount <= 0;
        }

        [SerializeField] Slot[] slots = new Slot[Capacity];

        public event Action Changed;

        public Slot this[int index] => slots[index];

        public int FreeSlots
        {
            get
            {
                int free = 0;
                foreach (var slot in slots)
                    if (slot.IsEmpty)
                        free++;
                return free;
            }
        }

        public bool Contains(ItemDefinition item) => IndexOf(item) >= 0;

        public int IndexOf(ItemDefinition item)
        {
            if (item == null)
                return -1;
            for (int i = 0; i < slots.Length; i++)
                if (slots[i].item == item && slots[i].amount > 0)
                    return i;
            return -1;
        }

        /// <summary>Adds items; returns false (and changes nothing) if there is no room.</summary>
        public bool Add(ItemDefinition item, int amount = 1)
        {
            if (item == null || amount <= 0)
                return false;

            if (item.Stackable)
            {
                int index = IndexOf(item);
                if (index < 0)
                    index = FirstEmpty();
                if (index < 0)
                    return false;
                slots[index].item = item;
                slots[index].amount += amount;
            }
            else
            {
                if (FreeSlots < amount)
                    return false;
                for (int n = 0; n < amount; n++)
                    slots[FirstEmpty()] = new Slot { item = item, amount = 1 };
            }

            Changed?.Invoke();
            return true;
        }

        public void RemoveAt(int index, int amount = 1)
        {
            if (index < 0 || index >= slots.Length || slots[index].IsEmpty)
                return;
            slots[index].amount -= amount;
            if (slots[index].amount <= 0)
                slots[index] = default;
            Changed?.Invoke();
        }

        public void Swap(int a, int b)
        {
            if (a == b)
                return;
            (slots[a], slots[b]) = (slots[b], slots[a]);
            Changed?.Invoke();
        }

        int FirstEmpty()
        {
            for (int i = 0; i < slots.Length; i++)
                if (slots[i].IsEmpty)
                    return i;
            return -1;
        }

        void Awake()
        {
            if (slots == null || slots.Length != Capacity)
                Array.Resize(ref slots, Capacity);
        }
    }
}
