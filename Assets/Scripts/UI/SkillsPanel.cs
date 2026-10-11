using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TopDown
{
    /// <summary>OSRS-style stats tab: one tile per skill showing the level; hover for XP details.</summary>
    public class SkillsPanel : MonoBehaviour
    {
        UiSkin skin;
        PixelText[] levels;
        PixelText total;
        Player player;
        int hoveredSkill = -1;

        public void Build(UiSkin uiSkin, RectTransform page)
        {
            skin = uiSkin;
            var skills = (Skill[])Enum.GetValues(typeof(Skill));
            levels = new PixelText[skills.Length];
            for (int i = 0; i < skills.Length; i++)
            {
                var skill = skills[i];
                int col = i % 2, row = i / 2;
                var tile = UiFactory.Rect($"{skill} Tile", page, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(6f + col * 60f, -7f - row * 30f), new Vector2(58f, 28f));
                UiFactory.Image(tile, skin.skillTile, sliced: true);
                var icon = UiFactory.Rect("Icon", tile, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(5f, 0f), new Vector2(16f, 16f));
                UiFactory.Image(icon, skin.SkillIcon(skill), raycast: false);
                var level = UiFactory.Rect("Level", tile, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-5f, 0f), new Vector2(32f, 10f));
                levels[i] = UiFactory.Text(level, skin.font, "1/1", skin.yellow, TextAnchor.MiddleRight, wrap: false);

                int index = i;
                var click = tile.gameObject.AddComponent<UiClick>();
                click.Entered = () => { hoveredSkill = index; GameUI.Instance.HoverOverride = $"View <{skin.Hex(skin.orange)}>{skill}</> guide"; };
                click.Exited = () => { hoveredSkill = -1; GameUI.Instance.HoverOverride = null; GameUI.Instance.Tooltip.Hide(); };
            }

            var totalRect = UiFactory.Rect("Total", page, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(8f, 10f), new Vector2(116f, 10f));
            total = UiFactory.Text(totalRect, skin.font, "Total level: 1", skin.yellow, TextAnchor.LowerLeft, wrap: false);
        }

        public void Bind(Player newPlayer)
        {
            if (player != null)
                player.Skills.XpGained -= OnXp;
            player = newPlayer;
            player.Skills.XpGained += OnXp;
            Refresh();
        }

        void OnDestroy()
        {
            if (player != null)
                player.Skills.XpGained -= OnXp;
        }

        void OnXp(Skill skill, int amount) => Refresh();

        void Refresh()
        {
            if (player == null)
                return;
            var skills = (Skill[])Enum.GetValues(typeof(Skill));
            for (int i = 0; i < skills.Length; i++)
            {
                int level = player.Skills.GetLevel(skills[i]);
                levels[i].Text = $"{level}/{level}";
            }
            total.Text = $"Total level: {player.Skills.TotalLevel}";
        }

        void Update()
        {
            if (hoveredSkill < 0 || player == null || Mouse.current == null)
                return;
            var skill = (Skill)hoveredSkill;
            int xp = player.Skills.GetXp(skill);
            int level = player.Skills.GetLevel(skill);
            string text = level >= XpTable.MaxLevel
                ? $"{skill} XP: {xp:N0}"
                : $"{skill} XP: {xp:N0}\nNext level at: {XpTable.XpForLevel(level + 1):N0}\nRemaining XP: {XpTable.XpForLevel(level + 1) - xp:N0}";
            GameUI.Instance.Tooltip.Show(text, Mouse.current.position.ReadValue());
        }
    }
}
