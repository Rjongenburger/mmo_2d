using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TopDown
{
    /// <summary>
    /// Builds and owns the OSRS-style HUD: chatbox (messages + dialogue), side panel with Inventory and
    /// Skills tabs, objective tracker, XP drops, mouse-over text, interaction prompt and right-click menus.
    /// Lives on a Screen Space - Overlay canvas with a <see cref="PixelCanvasScaler"/>.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance { get; private set; }

        [SerializeField] UiSkin skin;

        CanvasGroup hud;
        PixelText hoverText;
        Player boundPlayer;

        public UiSkin Skin => skin;
        public ChatBox Chat { get; private set; }
        public DialogueBox Dialogue { get; private set; }
        public SidePanel Side { get; private set; }
        public ObjectivePanel Objective { get; private set; }
        public XpDrops XpDrops { get; private set; }
        public InteractPrompt Prompt { get; private set; }
        public ContextMenuUI Menu { get; private set; }
        public TooltipUI Tooltip { get; private set; }
        public HeartsUI Hearts { get; private set; }
        public BannerUI Banner { get; private set; }

        /// <summary>Mouse-over text set by UI elements (inventory items, skills), shown instead of world hover text.</summary>
        public string HoverOverride { get; set; }

        /// <summary>True while UI should swallow world clicks (an open menu, or one closed this frame).</summary>
        public bool BlocksWorldInput => Menu != null && Menu.BlocksWorldInput;

        void Awake()
        {
            Instance = this;
            Build();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            Unbind();
        }

        void Build()
        {
            var root = UiFactory.Stretch("HUD", transform);
            hud = root.gameObject.AddComponent<CanvasGroup>();

            Chat = root.gameObject.AddComponent<ChatBox>();
            Chat.Build(skin, root);
            Side = root.gameObject.AddComponent<SidePanel>();
            Side.Build(skin, root);
            Objective = root.gameObject.AddComponent<ObjectivePanel>();
            Objective.Build(skin, root);
            XpDrops = root.gameObject.AddComponent<XpDrops>();
            XpDrops.Build(skin, root);
            Prompt = root.gameObject.AddComponent<InteractPrompt>();
            Prompt.Build(skin, root);

            Hearts = root.gameObject.AddComponent<HeartsUI>();
            Hearts.Build(skin, root);
            var hoverRect = UiFactory.Rect("HoverText", root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(5f, -17f), new Vector2(300f, 12f));
            hoverText = UiFactory.Text(hoverRect, skin.font, "", skin.white, wrap: false);
            Dialogue = root.gameObject.AddComponent<DialogueBox>();
            Dialogue.Build(skin, root, Chat);
            Banner = root.gameObject.AddComponent<BannerUI>();
            Banner.Build(skin, root);

            // Overlays last so they draw on top.
            Menu = gameObject.AddComponent<ContextMenuUI>();
            Menu.Build(skin, transform);
            Tooltip = gameObject.AddComponent<TooltipUI>();
            Tooltip.Build(skin, transform);
        }

        public void SetHudVisible(bool visible)
        {
            hud.alpha = visible ? 1f : 0f;
            hud.blocksRaycasts = visible;
        }

        void Update()
        {
            var player = Player.Instance;
            if (player != null && player != boundPlayer)
                Bind(player);
            hoverText.Text = ComposeHoverText(player);
        }

        void Bind(Player player)
        {
            Unbind();
            boundPlayer = player;
            player.Interactor.WorldMenuRequested += OpenWorldMenu;
            player.Skills.LevelledUp += OnLevelUp;
            if (player.Caster != null)
                player.Caster.SpellLearned += OnSpellLearned;
            Side.Bind(player);
            XpDrops.Bind(player);
            Hearts.Bind(player.Health);
        }

        void Unbind()
        {
            if (boundPlayer == null)
                return;
            boundPlayer.Interactor.WorldMenuRequested -= OpenWorldMenu;
            boundPlayer.Skills.LevelledUp -= OnLevelUp;
            if (boundPlayer.Caster != null)
                boundPlayer.Caster.SpellLearned -= OnSpellLearned;
        }

        void OnLevelUp(Skill skill, int level)
        {
            Sfx.Play(skin.levelUp, 0.6f);
            Banner.Show(skin.SkillIcon(skill), "Level up!", $"{skill} is now level {level}");
        }

        void OnSpellLearned(SpellDefinition spell) => Side.Flash(SidePanel.MagicTab);

        string ComposeHoverText(Player player)
        {
            if (Menu.IsOpen || Dialogue.IsOpen)
                return "";
            if (!string.IsNullOrEmpty(HoverOverride))
                return HoverOverride;
            if (player == null || player.Controller.IsLocked || player.Interactor.PointerOverUi || Mouse.current == null)
                return "";
            var hovered = player.Interactor.Hovered;
            if (hovered == null)
                return "Walk here";
            return hovered.CanInteract ? $"{hovered.Verb} {ColoredName(hovered)}" : ColoredName(hovered);
        }

        public string ColoredName(Interactable target)
        {
            Color c = target.NameKind switch
            {
                Interactable.Kind.Npc => skin.yellow,
                Interactable.Kind.Item => skin.orange,
                _ => skin.cyan,
            };
            return $"<{skin.Hex(c)}>{target.DisplayName}</>";
        }

        public string ItemName(ItemDefinition item) => $"<{skin.Hex(skin.orange)}>{item.DisplayName}</>";

        void OpenWorldMenu(Interactable target, Vector2 screenPosition)
        {
            var player = Player.Instance;
            if (player == null)
                return;
            Vector2 walkTarget = player.Interactor.MouseWorld;
            var options = new List<ContextMenuUI.Option>();
            if (target != null)
            {
                if (target.CanInteract)
                    options.Add(new ContextMenuUI.Option($"{target.Verb} {ColoredName(target)}", () => player.Interactor.TryInteract(target)));
                options.Add(new ContextMenuUI.Option("Walk here", () => player.Controller.WalkTo(walkTarget, 0.08f)));
                options.Add(new ContextMenuUI.Option($"Examine {ColoredName(target)}", () => GameLog.Add(target.Examine)));
            }
            else
            {
                options.Add(new ContextMenuUI.Option("Walk here", () => player.Controller.WalkTo(walkTarget, 0.08f)));
            }
            options.Add(new ContextMenuUI.Option("Cancel", null));
            Menu.Open(screenPosition, options);
        }

        /// <summary>Converts a screen position to canvas units (the canvas origin is bottom-left).</summary>
        public static Vector2 ScreenToCanvas(Vector2 screen) => screen / PixelCanvasScaler.Scale;

        public void PlayClick() => Sfx.Play(skin.click, 0.5f);
    }
}
