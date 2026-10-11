using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TopDown
{
    /// <summary>OSRS XP drops: "+25" with the skill icon floating up near the top of the screen.</summary>
    public class XpDrops : MonoBehaviour
    {
        const float Lifetime = 1.8f, Rise = 26f;

        class Drop
        {
            public RectTransform rect;
            public Image icon;
            public PixelText text;
            public float age;
        }

        readonly List<Drop> drops = new();
        UiSkin skin;
        RectTransform root;
        Player player;

        public void Build(UiSkin uiSkin, Transform parent)
        {
            skin = uiSkin;
            root = UiFactory.Rect("XpDrops", parent, Vector2.one, Vector2.one, new Vector2(-208f, -44f), new Vector2(70f, 16f));
        }

        public void Bind(Player newPlayer)
        {
            if (player != null)
                player.Skills.XpGained -= Spawn;
            player = newPlayer;
            player.Skills.XpGained += Spawn;
        }

        void OnDestroy()
        {
            if (player != null)
                player.Skills.XpGained -= Spawn;
        }

        void Spawn(Skill skill, int amount)
        {
            var rect = UiFactory.Rect("Drop", root, new Vector2(1f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(70f, 16f));
            var iconRect = UiFactory.Rect("Icon", rect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(16f, 16f));
            var icon = UiFactory.Image(iconRect, skin.SkillIcon(skill), raycast: false);
            var textRect = UiFactory.Rect("Amount", rect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(37f, 0f), new Vector2(34f, 10f));
            var text = UiFactory.Text(textRect, skin.font, $"+{amount}", Color.white, TextAnchor.MiddleLeft, wrap: false);
            drops.Add(new Drop { rect = rect, icon = icon, text = text });
        }

        void Update()
        {
            for (int i = drops.Count - 1; i >= 0; i--)
            {
                var drop = drops[i];
                drop.age += Time.deltaTime;
                float p = drop.age / Lifetime;
                if (p >= 1f)
                {
                    Destroy(drop.rect.gameObject);
                    drops.RemoveAt(i);
                    continue;
                }
                drop.rect.anchoredPosition = new Vector2(0f, Mathf.Round(Rise * p));
                float alpha = 1f - Mathf.Clamp01((p - 0.7f) / 0.3f);
                drop.icon.color = new Color(1f, 1f, 1f, alpha);
                drop.text.color = new Color(1f, 1f, 1f, alpha);
            }
        }
    }
}
