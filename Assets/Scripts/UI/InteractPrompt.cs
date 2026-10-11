using UnityEngine;
using UnityEngine.UI;

namespace TopDown
{
    /// <summary>"[E] Chop down Tree" hint floating above whatever the player can interact with.</summary>
    public class InteractPrompt : MonoBehaviour
    {
        UiSkin skin;
        RectTransform root;
        RectTransform labelRect;
        PixelText label;
        Camera cam;

        public void Build(UiSkin uiSkin, Transform parent)
        {
            skin = uiSkin;
            root = UiFactory.Rect("InteractPrompt", parent, Vector2.zero, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(120f, 13f));
            var key = UiFactory.Rect("Key", root, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(11f, 12f));
            UiFactory.Image(key, skin.keycap, raycast: false);
            var letter = UiFactory.Rect("Letter", key, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(3f, -1f), new Vector2(8f, 9f));
            var e = UiFactory.Text(letter, skin.font, "E", new Color(0.17f, 0.14f, 0.2f), wrap: false);
            e.Shadow = false;
            labelRect = UiFactory.Rect("Label", root, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(200f, 10f));
            label = UiFactory.Text(labelRect, skin.font, "", skin.white, TextAnchor.MiddleLeft, wrap: false);
            root.gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            var ui = GameUI.Instance;
            var player = Player.Instance;
            var target = player != null ? player.Interactor.Focused : null;
            bool show = target != null && !ui.Dialogue.IsOpen && !ui.Menu.IsOpen;
            root.gameObject.SetActive(show);
            if (!show)
                return;
            if (cam == null)
                cam = Camera.main;

            label.Text = $"{target.Verb} {ui.ColoredName(target)}";
            int width = 14 + label.Measure().x;
            root.sizeDelta = new Vector2(width, 13f);

            float top = target.ClickArea != null ? target.ClickArea.bounds.max.y : target.transform.position.y + 1f;
            Vector3 world = new(target.transform.position.x, top + 0.1f, 0f);
            Vector2 canvasPos = GameUI.ScreenToCanvas(cam.WorldToScreenPoint(world));
            root.anchoredPosition = new Vector2(Mathf.Round(canvasPos.x), Mathf.Round(canvasPos.y));
        }
    }
}
