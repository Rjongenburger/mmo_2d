using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TopDown
{
    /// <summary>The 28-slot inventory grid: left-click uses an item, right-click opens its options.</summary>
    public class InventoryPanel : MonoBehaviour
    {
        const int Columns = 4, Rows = 7, SlotWidth = 30, SlotHeight = 28;

        UiSkin skin;
        Image[] icons;
        Image[] highlights;
        PixelText[] amounts;
        Player player;
        int hoveredSlot = -1;

        public void Build(UiSkin uiSkin, RectTransform page)
        {
            skin = uiSkin;
            icons = new Image[Inventory.Capacity];
            highlights = new Image[Inventory.Capacity];
            amounts = new PixelText[Inventory.Capacity];
            for (int i = 0; i < Inventory.Capacity; i++)
            {
                int slot = i;
                int col = i % Columns, row = i / Columns;
                var rt = UiFactory.Rect($"Slot {i}", page, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(6f + col * SlotWidth, -7f - row * SlotHeight), new Vector2(SlotWidth, SlotHeight));
                var hit = UiFactory.Image(rt, null);   // invisible raycast target for the whole slot
                hit.color = Color.clear;

                var hl = UiFactory.Rect("Highlight", rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(28f, 26f));
                highlights[i] = UiFactory.Image(hl, skin.slotHover, sliced: true, raycast: false);
                highlights[i].enabled = false;

                var icon = UiFactory.Rect("Icon", rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(24f, 24f));
                icons[i] = UiFactory.Image(icon, null, raycast: false);
                icons[i].color = Color.white;
                icons[i].enabled = false;

                var amount = UiFactory.Rect("Amount", rt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(1f, 0f), new Vector2(28f, 10f));
                amounts[i] = UiFactory.Text(amount, skin.font, "", skin.yellow, wrap: false);

                var click = rt.gameObject.AddComponent<UiClick>();
                click.Clicked = e => OnSlotClicked(slot, e);
                click.Entered = () => SetHovered(slot);
                click.Exited = () => SetHovered(-1);
            }
        }

        public void Bind(Player newPlayer)
        {
            if (player != null)
                player.Inventory.Changed -= Refresh;
            player = newPlayer;
            player.Inventory.Changed += Refresh;
            Refresh();
        }

        void OnDestroy()
        {
            if (player != null)
                player.Inventory.Changed -= Refresh;
        }

        void Refresh()
        {
            if (player == null)
                return;
            for (int i = 0; i < Inventory.Capacity; i++)
            {
                var slot = player.Inventory[i];
                icons[i].enabled = !slot.IsEmpty;
                icons[i].sprite = slot.IsEmpty ? null : slot.item.Icon;
                amounts[i].Text = !slot.IsEmpty && slot.item.Stackable && slot.amount > 1 ? slot.amount.ToString() : "";
            }
            SetHovered(hoveredSlot);
        }

        void SetHovered(int slot)
        {
            if (hoveredSlot >= 0)
                highlights[hoveredSlot].enabled = false;
            hoveredSlot = slot;
            var ui = GameUI.Instance;
            if (slot < 0 || player == null || player.Inventory[slot].IsEmpty)
            {
                if (ui != null)
                    ui.HoverOverride = null;
                return;
            }
            highlights[slot].enabled = true;
            var item = player.Inventory[slot].item;
            ui.HoverOverride = $"{item.PrimaryVerb ?? "Examine"} {ui.ItemName(item)}";
        }

        void OnSlotClicked(int slot, PointerEventData e)
        {
            if (player == null || player.Inventory[slot].IsEmpty)
                return;
            var item = player.Inventory[slot].item;
            var ui = GameUI.Instance;

            if (e.button == PointerEventData.InputButton.Left)
            {
                player.UseItem(slot);
                return;
            }
            if (e.button != PointerEventData.InputButton.Right)
                return;

            var options = new List<ContextMenuUI.Option>();
            if (item.PrimaryVerb != null)
                options.Add(new ContextMenuUI.Option($"{item.PrimaryVerb} {ui.ItemName(item)}", () => player.UseItem(slot)));
            options.Add(new ContextMenuUI.Option($"Drop {ui.ItemName(item)}", () => player.DropItem(slot)));
            options.Add(new ContextMenuUI.Option($"Examine {ui.ItemName(item)}", () => player.ExamineItem(slot)));
            options.Add(new ContextMenuUI.Option("Cancel", null));
            ui.Menu.Open(e.position, options);
        }
    }
}
