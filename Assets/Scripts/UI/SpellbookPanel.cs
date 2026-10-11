using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TopDown
{
    /// <summary>Magic tab: every spell in the spellbook; learned ones can be picked as the active spell.</summary>
    public class SpellbookPanel : MonoBehaviour
    {
        const int Columns = 4;
        const float Cell = 30f;

        readonly List<(SpellDefinition spell, Image frame, Image icon)> cells = new();
        UiSkin skin;
        RectTransform grid;
        PixelText activeLabel;
        Player player;
        SpellDefinition hovered;

        public void Build(UiSkin uiSkin, RectTransform page)
        {
            skin = uiSkin;
            var title = UiFactory.Rect("Title", page, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -7f), new Vector2(116f, 10f));
            UiFactory.Text(title, skin.font, "Spellbook", skin.objectiveTitle, TextAnchor.UpperCenter, wrap: false);
            grid = UiFactory.Rect("Grid", page, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -22f), new Vector2(Columns * Cell, 120f));
            var active = UiFactory.Rect("Active", page, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(6f, 8f), new Vector2(120f, 32f));
            activeLabel = UiFactory.Text(active, skin.font, "", skin.white, TextAnchor.LowerCenter);
        }

        public void Bind(Player newPlayer)
        {
            Unbind();
            player = newPlayer;
            if (player.Caster == null)
                return;
            player.Caster.SpellLearned += OnSpellsChanged;
            player.Caster.ActiveChanged += Refresh;
            BuildCells();
            Refresh();
        }

        void OnDestroy() => Unbind();

        void Unbind()
        {
            if (player == null || player.Caster == null)
                return;
            player.Caster.SpellLearned -= OnSpellsChanged;
            player.Caster.ActiveChanged -= Refresh;
        }

        void OnSpellsChanged(SpellDefinition spell) => Refresh();

        void BuildCells()
        {
            foreach (var cell in cells)
                Destroy(cell.frame.gameObject);
            cells.Clear();
            var spells = player.Caster.Spellbook;
            for (int i = 0; i < spells.Count; i++)
            {
                var spell = spells[i];
                var rt = UiFactory.Rect(spell.DisplayName, grid, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(2f + i % Columns * Cell, -(i / Columns) * Cell), new Vector2(26f, 26f));
                var frame = UiFactory.Image(rt, skin.spellFrame);
                var iconRect = UiFactory.Rect("Icon", rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(24f, 24f));
                var icon = UiFactory.Image(iconRect, spell.LockedIcon, raycast: false);
                icon.color = Color.white;
                cells.Add((spell, frame, icon));

                var click = rt.gameObject.AddComponent<UiClick>();
                click.Clicked = e => Choose(spell);
                click.Entered = () =>
                {
                    hovered = spell;
                    GameUI.Instance.HoverOverride = player.Caster.Knows(spell) ? $"Select <{skin.Hex(skin.orange)}>{spell.DisplayName}</>" : spell.DisplayName;
                };
                click.Exited = () =>
                {
                    hovered = null;
                    GameUI.Instance.HoverOverride = null;
                    GameUI.Instance.Tooltip.Hide();
                };
            }
        }

        void Choose(SpellDefinition spell)
        {
            if (!player.Caster.Knows(spell))
            {
                GameLog.Add($"You haven't learned {spell.DisplayName} yet. (Magic level {spell.LevelRequired})");
                return;
            }
            GameUI.Instance.PlayClick();
            player.Caster.SetActive(spell);
        }

        void Refresh()
        {
            if (player == null || player.Caster == null)
                return;
            foreach (var (spell, frame, icon) in cells)
            {
                bool known = player.Caster.Knows(spell);
                icon.sprite = known ? spell.Icon : spell.LockedIcon;
                frame.sprite = spell == player.Caster.Active ? skin.spellFrameActive : skin.spellFrame;
            }
            var active = player.Caster.Active;
            activeLabel.Text = active != null
                ? $"Active: <{skin.Hex(skin.orange)}>{active.DisplayName}</>\nClick a foe or press <{skin.Hex(skin.yellow)}>F</>"
                : "No spells learned yet";
        }

        void Update()
        {
            if (hovered == null || player == null || Mouse.current == null)
                return;
            bool known = player.Caster.Knows(hovered);
            string text = known
                ? $"{hovered.DisplayName} (level {hovered.LevelRequired})\n{hovered.Description}\nDamage: {hovered.Damage}"
                : $"{hovered.DisplayName}\nUnlocks at Magic level {hovered.LevelRequired}.";
            GameUI.Instance.Tooltip.Show(text, Mouse.current.position.ReadValue());
        }
    }
}
