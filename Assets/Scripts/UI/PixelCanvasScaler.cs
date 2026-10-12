using UnityEngine;
using UnityEngine.UI;

namespace TopDown
{
    /// <summary>
    /// Scales the canvas so the reference resolution fits the screen. The UI art is smoothed rather than pixel art,
    /// so fractional scales are fine and the UI keeps the same proportions at every resolution.
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class PixelCanvasScaler : MonoBehaviour
    {
        [SerializeField] Vector2Int referenceResolution = new(640, 360);

        CanvasScaler scaler;

        public static float Scale { get; private set; } = 1f;

        void Awake()
        {
            scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.referencePixelsPerUnit = 32;
            Apply();
        }

        void Update() => Apply();

        void Apply()
        {
            float scale = Mathf.Max(1f, Mathf.Min(
                (float)Screen.width / referenceResolution.x, (float)Screen.height / referenceResolution.y));
            Scale = scale;
            if (!Mathf.Approximately(scaler.scaleFactor, scale))
                scaler.scaleFactor = scale;
        }
    }
}
