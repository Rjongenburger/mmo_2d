using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TopDown
{
    /// <summary>OSRS right-click "Choose Option" menu.</summary>
    public class ContextMenuUI : MonoBehaviour
    {
        public readonly struct Option
        {
            public readonly string Label;
            public readonly Action Action;

            public Option(string label, Action action)
            {
                Label = label;
                Action = action;
            }
        }

        const int RowHeight = 13, HeaderHeight = 14, Padding = 3;

        readonly List<Option> options = new();
        readonly List<(RectTransform rect, PixelText text, Image highlight)> rows = new();
        UiSkin skin;
        RectTransform root;
        PixelText header;
        int closedFrame = -1;

        public bool IsOpen { get; private set; }
        public bool BlocksWorldInput => IsOpen || closedFrame == Time.frameCount;
        public IReadOnlyList<Option> Options => options;

        public void Build(UiSkin uiSkin, Transform parent)
        {
            skin = uiSkin;
            root = UiFactory.Rect("ContextMenu", parent, Vector2.zero, new Vector2(0f, 1f), Vector2.zero, new Vector2(100f, 40f));
            UiFactory.Image(root, skin.menuPanel, sliced: true);
            var headerRect = UiFactory.Rect("Header", root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(1f, -1f), new Vector2(98f, HeaderHeight - 1));
            UiFactory.Image(headerRect, skin.menuHeader, sliced: true, raycast: false);
            var headerText = UiFactory.Rect("Text", headerRect, Vector2.zero, Vector2.zero, new Vector2(Padding, 2f), new Vector2(96f, 10f));
            header = UiFactory.Text(headerText, skin.font, "Choose Option", skin.menuHeaderText, TextAnchor.LowerLeft, wrap: false);
            header.Shadow = false;
            root.gameObject.SetActive(false);
        }

        public void Open(Vector2 screenPosition, IEnumerable<Option> newOptions)
        {
            options.Clear();
            options.AddRange(newOptions);
            while (rows.Count < options.Count)
            {
                var rowRect = UiFactory.Rect($"Row {rows.Count}", root, new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(100f, RowHeight));
                var highlight = UiFactory.Image(rowRect, null, raycast: false);
                var textRect = UiFactory.Rect("Text", rowRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(Padding, 0f), new Vector2(200f, 10f));
                var text = UiFactory.Text(textRect, skin.font, "", skin.white, TextAnchor.MiddleLeft, wrap: false);
                rows.Add((rowRect, text, highlight));
            }

            int width = Mathf.Max(header.Measure().x, 0);
            for (int i = 0; i < rows.Count; i++)
            {
                bool used = i < options.Count;
                rows[i].rect.gameObject.SetActive(used);
                if (!used)
                    continue;
                rows[i].text.Text = options[i].Label;
                width = Mathf.Max(width, rows[i].text.Measure().x);
            }
            width += Padding * 2 + 2;
            int height = HeaderHeight + options.Count * RowHeight + 2;
            root.sizeDelta = new Vector2(width, height);
            ((RectTransform)root.Find("Header")).sizeDelta = new Vector2(width - 2, HeaderHeight - 1);
            for (int i = 0; i < options.Count; i++)
            {
                rows[i].rect.sizeDelta = new Vector2(width - 2, RowHeight);
                rows[i].rect.anchoredPosition = new Vector2(1f, -HeaderHeight - i * RowHeight);
            }

            // Centre horizontally on the cursor like OSRS, clamped to the canvas.
            var canvasRect = (RectTransform)root.parent;
            Vector2 pos = GameUI.ScreenToCanvas(screenPosition);
            pos.x = Mathf.Clamp(Mathf.Round(pos.x - width * 0.5f), 0f, canvasRect.rect.width - width);
            pos.y = Mathf.Clamp(Mathf.Round(pos.y + 2f), height, canvasRect.rect.height);
            root.anchoredPosition = pos;
            root.gameObject.SetActive(true);
            root.SetAsLastSibling();
            IsOpen = true;
        }

        public void Close()
        {
            if (!IsOpen)
                return;
            IsOpen = false;
            closedFrame = Time.frameCount;
            root.gameObject.SetActive(false);
        }

        /// <summary>Runs option <paramref name="index"/> and closes the menu (also used by tests).</summary>
        public void Choose(int index)
        {
            if (index < 0 || index >= options.Count)
                return;
            var action = options[index].Action;
            Close();
            GameUI.Instance.PlayClick();
            action?.Invoke();
        }

        void Update()
        {
            if (!IsOpen || Mouse.current == null)
                return;

            Vector2 mouse = GameUI.ScreenToCanvas(Mouse.current.position.ReadValue());
            Vector2 origin = root.anchoredPosition;
            Vector2 size = root.sizeDelta;
            bool inside = mouse.x >= origin.x && mouse.x <= origin.x + size.x && mouse.y <= origin.y && mouse.y >= origin.y - size.y;
            int hovered = -1;
            if (inside)
            {
                int row = Mathf.FloorToInt((origin.y - HeaderHeight - mouse.y) / RowHeight);
                if (row >= 0 && row < options.Count)
                    hovered = row;
            }
            for (int i = 0; i < options.Count; i++)
                rows[i].highlight.color = i == hovered ? new Color(1f, 1f, 1f, 0.12f) : Color.clear;

            // OSRS closes the menu once the cursor wanders away from it.
            bool farAway = mouse.x < origin.x - 12f || mouse.x > origin.x + size.x + 12f || mouse.y > origin.y + 12f || mouse.y < origin.y - size.y - 12f;
            if (farAway || (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame))
            {
                Close();
                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (hovered >= 0)
                    Choose(hovered);
                else
                    Close();
            }
        }
    }
}
