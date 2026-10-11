using UnityEngine;
using UnityEngine.UI;

namespace TopDown
{
    /// <summary>Scales the canvas by the largest whole number that fits the reference resolution, keeping pixels square.</summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class PixelCanvasScaler : MonoBehaviour
    {
        [SerializeField] Vector2Int referenceResolution = new(640, 360);

        CanvasScaler scaler;

        public static int Scale { get; private set; } = 1;

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
            int scale = Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(
                (float)Screen.width / referenceResolution.x, (float)Screen.height / referenceResolution.y)));
            Scale = scale;
            if (!Mathf.Approximately(scaler.scaleFactor, scale))
                scaler.scaleFactor = scale;
        }
    }
}
