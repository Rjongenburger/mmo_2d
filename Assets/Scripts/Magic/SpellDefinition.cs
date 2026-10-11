using UnityEngine;

namespace TopDown
{
    [CreateAssetMenu(menuName = "TopDown/Spell", fileName = "Spell")]
    public class SpellDefinition : ScriptableObject
    {
        [SerializeField] string displayName = "Spell";
        [SerializeField, TextArea] string description = "";
        [SerializeField] Sprite icon;
        [SerializeField] Sprite lockedIcon;
        [SerializeField] int levelRequired = 1;
        [SerializeField] int damage = 2;
        [Tooltip("Magic experience for each hit.")]
        [SerializeField] int experience = 25;
        [SerializeField] float cooldown = 0.8f;
        [SerializeField] Projectile projectile;
        [SerializeField] AudioClip castClip;

        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public Sprite LockedIcon => lockedIcon != null ? lockedIcon : icon;
        public int LevelRequired => levelRequired;
        public int Damage => damage;
        public int Experience => experience;
        public float Cooldown => cooldown;
        public Projectile Projectile => projectile;
        public AudioClip CastClip => castClip;
        /// <summary>Spells without a projectile are teasers that can't be cast yet.</summary>
        public bool Castable => projectile != null;

        public void Setup(string name, string text, Sprite spellIcon, Sprite locked, int level, int hitDamage, int xp,
            float castCooldown, Projectile bolt, AudioClip clip)
        {
            displayName = name;
            description = text;
            icon = spellIcon;
            lockedIcon = locked;
            levelRequired = level;
            damage = hitDamage;
            experience = xp;
            cooldown = castCooldown;
            projectile = bolt;
            castClip = clip;
        }
    }
}
