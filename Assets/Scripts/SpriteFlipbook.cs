using UnityEngine;

namespace TopDown
{
    /// <summary>Loops a set of sprites (blinking server LEDs, bubbling tanks, ...).</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFlipbook : MonoBehaviour
    {
        [SerializeField] Sprite[] frames;
        [SerializeField] float fps = 4f;
        [Tooltip("Start each instance at a random point so identical props don't animate in lockstep.")]
        [SerializeField] bool randomStart = true;

        SpriteRenderer spriteRenderer;
        float time;

        public void SetFrames(Sprite[] newFrames, float framesPerSecond)
        {
            frames = newFrames;
            fps = framesPerSecond;
        }

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (randomStart)
                time = Random.value * 10f;
        }

        void Update()
        {
            if (frames == null || frames.Length == 0)
                return;

            time += Time.deltaTime * fps;
            spriteRenderer.sprite = frames[(int)time % frames.Length];
        }
    }
}
