using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TopDown
{
    /// <summary>Big centred announcement ("New spell learned!", "Level up!") that pops in, holds and fades out.</summary>
    public class BannerUI : MonoBehaviour
    {
        const float FadeIn = 0.25f, Hold = 2.4f, FadeOut = 0.45f;

        readonly Queue<(Sprite icon, string title, string subtitle)> queue = new();
        UiSkin skin;
        RectTransform root;
        CanvasGroup group;
        Image icon;
        PixelText title, subtitle;
        float time = -1f;

        public bool IsShowing => time >= 0f;

        public void Build(UiSkin uiSkin, Transform parent)
        {
            skin = uiSkin;
            root = UiFactory.Rect("Banner", parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(200f, 40f));
            group = root.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            UiFactory.Image(root, skin.dialoguePanel, sliced: true, raycast: false);
            var iconRect = UiFactory.Rect("Icon", root, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, 0f), new Vector2(24f, 24f));
            icon = UiFactory.Image(iconRect, null, raycast: false);
            var titleRect = UiFactory.Rect("Title", root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(42f, -9f), new Vector2(150f, 10f));
            title = UiFactory.Text(titleRect, skin.font, "", skin.objectiveTitle, wrap: false);
            var subRect = UiFactory.Rect("Subtitle", root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(42f, -22f), new Vector2(150f, 10f));
            subtitle = UiFactory.Text(subRect, skin.font, "", skin.white, wrap: false);
            root.gameObject.SetActive(false);
        }

        public void Show(Sprite bannerIcon, string heading, string text) => queue.Enqueue((bannerIcon, heading, text));

        void Update()
        {
            if (time < 0f)
            {
                if (queue.Count == 0)
                    return;
                var next = queue.Dequeue();
                icon.sprite = next.icon;
                icon.color = next.icon != null ? Color.white : Color.clear;
                title.Text = next.title;
                subtitle.Text = next.subtitle;
                float width = Mathf.Max(title.Measure().x, subtitle.Measure().x) + 56f;
                root.sizeDelta = new Vector2(Mathf.Round(width), 40f);
                root.gameObject.SetActive(true);
                root.SetAsLastSibling();
                time = 0f;
            }

            time += Time.deltaTime;
            float alpha = time < FadeIn ? time / FadeIn
                : time < FadeIn + Hold ? 1f
                : 1f - (time - FadeIn - Hold) / FadeOut;
            group.alpha = Mathf.Clamp01(alpha);
            float pop = time < FadeIn ? Mathf.Round((1f - time / FadeIn) * 8f) : 0f;
            root.anchoredPosition = new Vector2(0f, -40f + pop);
            if (time >= FadeIn + Hold + FadeOut)
            {
                root.gameObject.SetActive(false);
                time = -1f;
            }
        }
    }
}
