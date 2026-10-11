using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TopDown
{
    /// <summary>Decoration that lights up while the player carries a given item (wind runes at the cliff edges).</summary>
    public class GlowWhenHolding : MonoBehaviour
    {
        [SerializeField] ItemDefinition item;
        [SerializeField] SpriteRenderer target;
        [SerializeField] Sprite dimSprite;
        [SerializeField] Sprite glowSprite;
        [SerializeField] Light2D glowLight;

        void Update()
        {
            bool lit = Player.Instance != null && Player.Instance.Inventory.Contains(item);
            if (target != null)
                target.sprite = lit ? glowSprite : dimSprite;
            if (glowLight != null)
                glowLight.enabled = lit;
        }
    }
}
