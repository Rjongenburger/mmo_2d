using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TopDown
{
    /// <summary>Full-screen colour fade used for scene transitions and portal flashes.</summary>
    public class ScreenFader : MonoBehaviour
    {
        public static ScreenFader Instance { get; private set; }

        [SerializeField] Image image;

        void Awake()
        {
            Instance = this;
            if (image != null)
                image.raycastTarget = false;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Current opacity of the fade overlay.</summary>
        public float Alpha => image.color.a;

        public void Set(Color color, float alpha)
        {
            color.a = alpha;
            image.color = color;
        }

        public IEnumerator Fade(Color color, float from, float to, float duration)
        {
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                Set(color, Mathf.Lerp(from, to, t / duration));
                yield return null;
            }
            Set(color, to);
        }
    }
}
