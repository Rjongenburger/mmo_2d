using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TopDown
{
    /// <summary>Worn-equipment tab: a paper-doll grid of slots. Click a worn item to take it off.</summary>
    public class EquipmentPanel : MonoBehaviour
    {
        const float Cell = 36f, SlotSize = 28f;

        static readonly (EquipSlot slot, int column, int row)[] Layout =
        {
            (EquipSlot.Head, 1, 0),
            (EquipSlot.Cape, 0, 1), (EquipSlot.Neck, 1, 1), (EquipSlot.Ammo, 2, 1),
            (EquipSlot.Weapon, 0, 2), (EquipSlot.Body, 1, 2), (EquipSlot.Shield, 2, 2),
            (EquipSlot.Legs, 1, 3),
            (EquipSlot.Hands, 0, 4), (EquipSlot.Feet, 1, 4), (EquipSlot.Ring, 2, 4),
        };

        readonly Dictionary<EquipSlot, (Image icon, Image ghost)> slots = new();
        Player player;

        public void Build(UiSkin skin, RectTransform page)
        {
            foreach (var (slot, column, row) in Layout)
            {
                var equipSlot = slot;
                var rt = UiFactory.Rect($"{slot} Slot", page, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(16f + column * Cell, -8f - row * Cell), new Vector2(SlotSize, SlotSize));
                UiFactory.Image(rt, skin.equipSlot, sliced: true);
                var ghostRect = UiFactory.Rect("Outline", rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(24f, 24f));
                var ghost = UiFactory.Image(ghostRect, skin.SlotSilhouette(slot), raycast: false);
                var iconRect = UiFactory.Rect("Icon", rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(24f, 24f));
                var icon = UiFactory.Image(iconRect, null, raycast: false);
                icon.color = Color.white;
                icon.enabled = false;
                slots[slot] = (icon, ghost);

                var click = rt.gameObject.AddComponent<UiClick>();
                click.Clicked = e => OnClicked(equipSlot, e);
                click.Entered = () => GameUI.Instance.HoverOverride = HoverText(equipSlot);
                click.Exited = () => GameUI.Instance.HoverOverride = null;
            }

            var title = UiFactory.Rect("Title", page, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(8f, 9f), new Vector2(116f, 10f));
            UiFactory.Text(title, skin.font, "Worn equipment", skin.yellow, TextAnchor.LowerCenter, wrap: false);
        }

        public void Bind(Player newPlayer)
        {
            if (player != null && player.Equipment != null)
                player.Equipment.Changed -= Refresh;
            player = newPlayer;
            if (player.Equipment != null)
                player.Equipment.Changed += Refresh;
            Refresh();
        }

        void OnDestroy()
        {
            if (player != null && player.Equipment != null)
                player.Equipment.Changed -= Refresh;
        }

        void Refresh()
        {
            if (player == null || player.Equipment == null)
                return;
            foreach (var (slot, (icon, ghost)) in slots)
            {
                var item = player.Equipment[slot];
                icon.enabled = item != null;
                icon.sprite = item != null ? item.Icon : null;
                ghost.enabled = item == null;
            }
        }

        string HoverText(EquipSlot slot)
        {
            var item = player != null && player.Equipment != null ? player.Equipment[slot] : null;
            return item != null ? $"Remove {GameUI.Instance.ItemName(item)}" : $"{slot} slot";
        }

        void OnClicked(EquipSlot slot, PointerEventData e)
        {
            var item = player != null && player.Equipment != null ? player.Equipment[slot] : null;
            if (item == null)
                return;
            var ui = GameUI.Instance;
            if (e.button == PointerEventData.InputButton.Left)
            {
                player.Equipment.Remove(slot);
                ui.HoverOverride = HoverText(slot);
                return;
            }
            if (e.button != PointerEventData.InputButton.Right)
                return;
            ui.Menu.Open(e.position, new List<ContextMenuUI.Option>
            {
                new($"Remove {ui.ItemName(item)}", () => player.Equipment.Remove(slot)),
                new($"Examine {ui.ItemName(item)}", () => GameLog.Add(item.Examine)),
                new("Cancel", null),
            });
        }
    }
}
