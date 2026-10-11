using UnityEngine;
using UnityEngine.UI;

namespace TopDown
{
    /// <summary>Helpers for building pixel-perfect UI in code (1 canvas unit = 1 art pixel).</summary>
    public static class UiFactory
    {
        public static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return rt;
        }

        public static RectTransform Stretch(string name, Transform parent, float inset = 0f)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(inset, inset);
            rt.offsetMax = new Vector2(-inset, -inset);
            return rt;
        }

        /// <param name="sliced">Keep the sprite's border fixed and stretch the middle (flat frames).</param>
        /// <param name="tiled">Keep the border fixed and repeat the middle (textured panels such as stone or parchment).</param>
        public static Image Image(RectTransform rt, Sprite sprite, bool sliced = false, bool raycast = true, bool tiled = false)
        {
            var image = rt.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = tiled ? UnityEngine.UI.Image.Type.Tiled
                : sliced ? UnityEngine.UI.Image.Type.Sliced
                : UnityEngine.UI.Image.Type.Simple;
            image.raycastTarget = raycast;
            if (sprite == null)
                image.color = Color.clear;
            return image;
        }

        public static PixelText Text(RectTransform rt, PixelFont font, string text, Color color,
            TextAnchor alignment = TextAnchor.UpperLeft, bool wrap = true)
        {
            var label = rt.gameObject.AddComponent<PixelText>();
            label.Font = font;
            label.Text = text;
            label.color = color;
            label.Alignment = alignment;
            label.Wrap = wrap;
            label.raycastTarget = false;
            return label;
        }
    }
}
