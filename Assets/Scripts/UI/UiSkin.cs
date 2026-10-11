using UnityEngine;

namespace TopDown
{
    /// <summary>Sprites, font and colours for the OSRS-style HUD.</summary>
    [CreateAssetMenu(menuName = "TopDown/UI Skin", fileName = "UiSkin")]
    public class UiSkin : ScriptableObject
    {
        public PixelFont font;
        public Sprite stonePanel;
        public Sprite parchmentPanel;
        public Sprite menuPanel;
        public Sprite menuHeader;
        public Sprite tooltipPanel;
        public Sprite darkPanel;
        public Sprite slotHover;
        public Sprite tabNormal;
        public Sprite tabActive;
        public Sprite iconInventory;
        public Sprite iconSkills;
        public Sprite keycap;
        public Sprite portraitFrame;
        public Sprite skillTile;
        public Sprite playerPortrait;
        public Sprite[] skillIcons;
        public AudioClip click;

        [Header("Dialogue")]
        public Sprite dialoguePanel;
        public Sprite namePlate;
        public Sprite portraitRing;
        public Sprite portraitBackdrop;
        public Sprite continueArrow;
        public AudioClip dialogueBlip;
        public Color dialogueText = new(0.957f, 0.937f, 0.902f);
        public Color playerPlateColor = new(0.17f, 0.62f, 0.58f);
        public float playerVoicePitch = 1.25f;

        [Header("Tabs, hearts and banners")]
        public Sprite iconEquipment;
        public Sprite iconSpellbook;
        public Sprite equipSlot;
        [Tooltip("Faded slot outlines indexed by EquipSlot (index 0 / None unused).")]
        public Sprite[] slotSilhouettes;
        public Sprite spellFrame;
        public Sprite spellFrameActive;
        public Sprite heartFull;
        public Sprite heartHalf;
        public Sprite heartEmpty;
        public AudioClip levelUp;

        [Header("OSRS text colours")]
        public Color white = Color.white;
        public Color yellow = new(1f, 1f, 0f);
        public Color orange = new(1f, 0.565f, 0.251f);
        public Color cyan = new(0f, 1f, 1f);
        public Color objectiveTitle = new(1f, 0.596f, 0.122f);
        public Color chatText = new(0f, 0f, 0f);
        public Color npcName = new(0.5f, 0f, 0f);
        public Color continueText = new(0f, 0f, 1f);
        public Color menuHeaderText = Color.black;

        public Sprite SlotSilhouette(EquipSlot slot) =>
            slotSilhouettes != null && (int)slot < slotSilhouettes.Length ? slotSilhouettes[(int)slot] : null;

        public Sprite SkillIcon(Skill skill) =>
            skillIcons != null && (int)skill < skillIcons.Length ? skillIcons[(int)skill] : null;

        public string Hex(Color c) => "#" + ColorUtility.ToHtmlStringRGB(c);
    }
}
