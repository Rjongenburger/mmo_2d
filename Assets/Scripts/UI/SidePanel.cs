using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TopDown
{
    /// <summary>
    /// Bottom-right stone panel with OSRS-style tabs (Inventory, Skills). Clicking the active tab collapses
    /// the panel, like OSRS resizable mode.
    /// </summary>
    public class SidePanel : MonoBehaviour
    {
        public const int Width = 132, BodyHeight = 210, TabHeight = 26;

        UiSkin skin;
        RectTransform body;
        Image[] tabs;
        RectTransform[] pages;
        int active;
        bool collapsed;

        public InventoryPanel Inventory { get; private set; }
        public SkillsPanel Skills { get; private set; }
        public int ActiveTab => collapsed ? -1 : active;

        public void Build(UiSkin uiSkin, Transform parent)
        {
            skin = uiSkin;
            var container = UiFactory.Rect("SidePanel", parent, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-4f, 4f),
                new Vector2(Width, BodyHeight + TabHeight));
            body = UiFactory.Rect("Body", container, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(Width, BodyHeight));
            UiFactory.Image(body, skin.stonePanel, tiled: true);

            Sprite[] icons = { skin.iconInventory, skin.iconSkills };
            string[] names = { "Inventory", "Skills" };
            tabs = new Image[icons.Length];
            pages = new RectTransform[icons.Length];
            for (int i = 0; i < icons.Length; i++)
            {
                int tabIndex = i;
                var tab = UiFactory.Rect($"Tab {names[i]}", container, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(4f + i * 32f, 0f), new Vector2(30f, TabHeight));
                tabs[i] = UiFactory.Image(tab, skin.tabNormal, sliced: true);
                var icon = UiFactory.Rect("Icon", tab, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(16f, 16f));
                UiFactory.Image(icon, icons[i], raycast: false);
                var click = tab.gameObject.AddComponent<UiClick>();
                click.Clicked = e => { if (e.button == PointerEventData.InputButton.Left) Select(tabIndex); };
                click.Entered = () => GameUI.Instance.HoverOverride = names[tabIndex];
                click.Exited = () => GameUI.Instance.HoverOverride = null;

                pages[i] = UiFactory.Stretch($"{names[i]} Page", body);
            }

            Inventory = gameObject.AddComponent<InventoryPanel>();
            Inventory.Build(skin, pages[0]);
            Skills = gameObject.AddComponent<SkillsPanel>();
            Skills.Build(skin, pages[1]);
            Refresh();
        }

        public void Bind(Player player)
        {
            Inventory.Bind(player);
            Skills.Bind(player);
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
            GameUI.Instance.PlayClick();
            Refresh();
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
