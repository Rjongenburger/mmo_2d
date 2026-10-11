using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TopDown
{
    /// <summary>Row of hearts in the top-left corner that shakes when you get hurt.</summary>
    public class HeartsUI : MonoBehaviour
    {
        readonly List<Image> hearts = new();
        UiSkin skin;
        RectTransform root;
        PlayerHealth health;
        int lastValue = -1;
        float shake;

        public void Build(UiSkin uiSkin, Transform parent)
        {
            skin = uiSkin;
            root = UiFactory.Rect("Hearts", parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(5f, -4f), new Vector2(60f, 9f));
        }

        public void Bind(PlayerHealth playerHealth)
        {
            if (health != null)
                health.Changed -= Refresh;
            health = playerHealth;
            if (health == null)
                return;
            health.Changed += Refresh;
            Refresh(health.Current, health.Max);
        }

        void OnDestroy()
        {
            if (health != null)
                health.Changed -= Refresh;
        }

        void Refresh(int current, int max)
        {
            int count = (max + 1) / 2;
            while (hearts.Count < count)
            {
                var rt = UiFactory.Rect($"Heart {hearts.Count}", root, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(hearts.Count * 10f, 0f), new Vector2(9f, 9f));
                hearts.Add(UiFactory.Image(rt, skin.heartFull, raycast: false));
            }
            for (int i = 0; i < hearts.Count; i++)
            {
                int halves = Mathf.Clamp(current - i * 2, 0, 2);
                hearts[i].sprite = halves == 2 ? skin.heartFull : halves == 1 ? skin.heartHalf : skin.heartEmpty;
                hearts[i].gameObject.SetActive(i < count);
            }
            if (lastValue >= 0 && current < lastValue)
                shake = 0.35f;
            lastValue = current;
        }

        void Update()
        {
            if (shake <= 0f)
                return;
            shake -= Time.deltaTime;
            float offset = shake > 0f ? Mathf.Round(Mathf.Sin(Time.time * 60f) * 2f) : 0f;
            root.anchoredPosition = new Vector2(5f + offset, -4f);
        }
    }
}
