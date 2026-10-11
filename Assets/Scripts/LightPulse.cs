using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TopDown
{
    /// <summary>Gently pulses and/or flickers a 2D light around its starting intensity.</summary>
    [RequireComponent(typeof(Light2D))]
    public class LightPulse : MonoBehaviour
    {
        [SerializeField] float pulseSpeed = 1.2f;
        [Tooltip("Fraction of the base intensity the sine pulse swings by.")]
        [Range(0f, 1f)] [SerializeField] float pulseAmount = 0.2f;
        [Tooltip("Fraction of the base intensity added as fast random flicker.")]
        [Range(0f, 1f)] [SerializeField] float flickerAmount;

        Light2D light2D;
        float baseIntensity;
        float seed;

        public void Configure(float speed, float pulse, float flicker)
        {
            pulseSpeed = speed;
            pulseAmount = pulse;
            flickerAmount = flicker;
        }

        void Awake()
        {
            light2D = GetComponent<Light2D>();
            baseIntensity = light2D.intensity;
            seed = Random.value * 100f;
        }

        void Update()
        {
            float t = Time.time;
            float pulse = Mathf.Sin((t + seed) * pulseSpeed * Mathf.PI * 2f) * pulseAmount;
            float flicker = (Mathf.PerlinNoise(seed, t * 12f) - 0.5f) * 2f * flickerAmount;
            light2D.intensity = baseIntensity * (1f + pulse + flicker);
        }
    }
}
