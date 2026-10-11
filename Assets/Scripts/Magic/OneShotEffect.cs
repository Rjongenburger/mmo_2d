using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TopDown
{
    /// <summary>Plays a flipbook once with an optional fading light flash, then removes itself (explosions).</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class OneShotEffect : MonoBehaviour
    {
        [SerializeField] Sprite[] frames;
        [SerializeField] float fps = 16f;
        [SerializeField] Light2D flash;
        [SerializeField] Sprite sparkSprite;
        [SerializeField] int sparks = 8;

        SpriteRenderer sr;
        float time;
        float flashIntensity;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (flash != null)
                flashIntensity = flash.intensity;
        }

        void Start() => Fx.Burst(sparkSprite, transform.position, sparks, Vector2.up * 0.5f, 2.2f, 0.45f, gravity: 3f);

        void Update()
        {
            time += Time.deltaTime;
            int frame = (int)(time * fps);
            if (frame >= frames.Length)
            {
                Destroy(gameObject);
                return;
            }
            sr.sprite = frames[frame];
            if (flash != null)
                flash.intensity = flashIntensity * (1f - time * fps / frames.Length);
        }
    }
}
