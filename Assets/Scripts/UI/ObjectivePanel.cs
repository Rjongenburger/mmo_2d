using UnityEngine;

namespace TopDown
{
    /// <summary>Quest-helper style objective box in the top-right that flashes when it changes.</summary>
    public class ObjectivePanel : MonoBehaviour
    {
        const float Width = 196f, Padding = 6f;

        UiSkin skin;
        RectTransform panel;
        PixelText title, body;
        float flash;

        public string Title => title.Text;
        public string Body => body.Text;

        public void Build(UiSkin uiSkin, Transform parent)
        {
            skin = uiSkin;
            panel = UiFactory.Rect("Objective", parent, Vector2.one, Vector2.one, new Vector2(-4f, -4f), new Vector2(Width, 40f));
            UiFactory.Image(panel, skin.darkPanel, sliced: true, raycast: false);
            var titleRect = UiFactory.Rect("Title", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(Padding, -Padding), new Vector2(Width - 2 * Padding, 10f));
            title = UiFactory.Text(titleRect, skin.font, "", skin.objectiveTitle, wrap: false);
            var bodyRect = UiFactory.Rect("Body", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(Padding, -Padding - 14f), new Vector2(Width - 2 * Padding, 30f));
            body = UiFactory.Text(bodyRect, skin.font, "", skin.white);
            panel.gameObject.SetActive(false);
        }

        public void Set(string heading, string description)
        {
            panel.gameObject.SetActive(true);
            title.Text = heading;
            body.Text = description;
            int bodyHeight = body.Measure(Width - 2 * Padding).y;
            body.rectTransform.sizeDelta = new Vector2(Width - 2 * Padding, bodyHeight);
            panel.sizeDelta = new Vector2(Width, Padding + 14f + bodyHeight + Padding);
            flash = 1f;
        }

        void Update()
        {
            if (flash <= 0f)
                return;
            flash = Mathf.Max(0f, flash - Time.deltaTime * 1.2f);
            title.color = Color.Lerp(skin.objectiveTitle, Color.white, Mathf.PingPong(flash * 4f, 1f));
        }
    }
}
