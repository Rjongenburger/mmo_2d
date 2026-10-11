using UnityEngine;

namespace TopDown
{
    /// <summary>Small yellow OSRS-style tooltip box that follows the cursor.</summary>
    public class TooltipUI : MonoBehaviour
    {
        RectTransform root;
        PixelText text;

        public void Build(UiSkin skin, Transform parent)
        {
            root = UiFactory.Rect("Tooltip", parent, Vector2.zero, new Vector2(0f, 1f), Vector2.zero, new Vector2(100f, 20f));
            UiFactory.Image(root, skin.tooltipPanel, sliced: true, raycast: false);
            var textRect = UiFactory.Rect("Text", root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(3f, -3f), new Vector2(200f, 40f));
            text = UiFactory.Text(textRect, skin.font, "", Color.black, wrap: false);
            text.Shadow = false;
            root.gameObject.SetActive(false);
        }

        public void Show(string content, Vector2 screenPosition)
        {
            text.Text = content;
            Vector2Int size = text.Measure();
            root.sizeDelta = new Vector2(size.x + 6, size.y + 6);
            var canvas = (RectTransform)root.parent;
            Vector2 pos = GameUI.ScreenToCanvas(screenPosition) + new Vector2(10f, -6f);
            pos.x = Mathf.Min(Mathf.Round(pos.x), canvas.rect.width - root.sizeDelta.x);
            pos.y = Mathf.Max(Mathf.Round(pos.y), root.sizeDelta.y);
            root.anchoredPosition = pos;
            root.gameObject.SetActive(true);
            root.SetAsLastSibling();
        }

        public void Hide() => root.gameObject.SetActive(false);
    }
}
