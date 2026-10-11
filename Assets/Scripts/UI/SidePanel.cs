using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TopDown
{
    /// <summary>
    /// Bottom-right stone panel with tabs: Inventory, Worn Equipment, Skills and Magic. Clicking the active
    /// tab collapses the panel, like OSRS resizable mode. A tab can flash to draw attention to something new.
    /// </summary>
    public class SidePanel : MonoBehaviour
    {
        public const int Width = 132, BodyHeight = 210, TabHeight = 26;
        public const int InventoryTab = 0, EquipmentTab = 1, SkillsTab = 2, MagicTab = 3;

        UiSkin skin;
        RectTransform body;
        Image[] tabs;
        RectTransform[] pages;
        int active;
        bool collapsed;
        int flashingTab = -1;
        float flashUntil;

        public InventoryPanel Inventory { get; private set; }
        public EquipmentPanel Equipment { get; private set; }
        public SkillsPanel Skills { get; private set; }
        public SpellbookPanel Spellbook { get; private set; }
        public int ActiveTab => collapsed ? -1 : active;

        public void Build(UiSkin uiSkin, Transform parent)
        {
            skin = uiSkin;
            var container = UiFactory.Rect("SidePanel", parent, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-4f, 4f),
                new Vector2(Width, BodyHeight + TabHeight));
            body = UiFactory.Rect("Body", container, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(Width, BodyHeight));
            UiFactory.Image(body, skin.stonePanel, tiled: true);

            Sprite[] icons = { skin.iconInventory, skin.iconEquipment, skin.iconSkills, skin.iconSpellbook };
            string[] names = { "Inventory", "Worn Equipment", "Skills", "Magic" };
            tabs = new Image[icons.Length];
            pages = new RectTransform[icons.Length];
            for (int i = 0; i < icons.Length; i++)
            {
                int tabIndex = i;
                var tab = UiFactory.Rect($"Tab {names[i]}", container, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(2f + i * 32f, 0f), new Vector2(30f, TabHeight));
                tabs[i] = UiFactory.Image(tab, skin.tabNormal, sliced: true);
                var icon = UiFactory.Rect("Icon", tab, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(16f, 16f));
                UiFactory.Image(icon, icons[i], raycast: false);
                var click = tab.gameObject.AddComponent<UiClick>();
                click.Clicked = e => { if (e.button == PointerEventData.InputButton.Left) Select(tabIndex); };
                click.Entered = () => GameUI.Instance.HoverOverride = names[tabIndex];
                click.Exited = () => GameUI.Instance.HoverOverride = null;

                pages[i] = UiFactory.Stretch($"{names[i]} Page", body);
            }

            Inventory = gameObject.AddComponent<InventoryPanel>();
            Inventory.Build(skin, pages[InventoryTab]);
            Equipment = gameObject.AddComponent<EquipmentPanel>();
            Equipment.Build(skin, pages[EquipmentTab]);
            Skills = gameObject.AddComponent<SkillsPanel>();
            Skills.Build(skin, pages[SkillsTab]);
            Spellbook = gameObject.AddComponent<SpellbookPanel>();
            Spellbook.Build(skin, pages[MagicTab]);
            Refresh();
        }

        public void Bind(Player player)
        {
            Inventory.Bind(player);
            Equipment.Bind(player);
            Skills.Bind(player);
            Spellbook.Bind(player);
        }

        public void Select(int tab)
        {
            if (tab == active && !collapsed)
                collapsed = true;
            else
            {
                active = tab;
                collapsed = false;
            }
            if (tab == flashingTab)
                flashingTab = -1;
            GameUI.Instance.PlayClick();
            Refresh();
        }

        /// <summary>Makes a tab blink for a few seconds (e.g. after learning a spell).</summary>
        public void Flash(int tab, float seconds = 6f)
        {
            flashingTab = tab;
            flashUntil = Time.time + seconds;
        }

        void Update()
        {
            if (flashingTab < 0)
                return;
            if (Time.time > flashUntil)
            {
                flashingTab = -1;
                Refresh();
                return;
            }
            bool on = Mathf.Repeat(Time.time * 3f, 1f) < 0.5f;
            tabs[flashingTab].sprite = on || (flashingTab == active && !collapsed) ? skin.tabActive : skin.tabNormal;
        }

        void Refresh()
        {
            body.gameObject.SetActive(!collapsed);
            for (int i = 0; i < tabs.Length; i++)
            {
                tabs[i].sprite = i == active && !collapsed ? skin.tabActive : skin.tabNormal;
                pages[i].gameObject.SetActive(i == active);
            }
        }
    }
}
