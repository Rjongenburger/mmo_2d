using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TopDown
{
    /// <summary>A swirling magic portal that can open, pulse and collapse.</summary>
    public class Portal : MonoBehaviour
    {
        [SerializeField] Transform visual;
        [SerializeField] Light2D portalLight;
        [SerializeField] float lightIntensity = 2.2f;
        [SerializeField] Sprite sparkSprite;
        [SerializeField] AudioClip openClip;

        float openness;
        float flash;

        public bool IsOpen => openness > 0.01f;
        public float LightBoost { get; set; }

        void Awake() => Apply();

        public void SetOpen(bool open)
        {
            openness = open ? 1f : 0f;
            Apply();
        }

        public IEnumerator Open(float duration)
        {
            Sfx.Play(openClip, 0.9f);
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float p = t / duration;
                openness = 1f + Mathf.Sin(p * Mathf.PI) * 0.15f * p - (1f - p) * (1f - p); // ease out with a little overshoot
                yield return null;
            }
            openness = 1f;
        }

        public IEnumerator Close(float duration)
        {
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                openness = 1f - Mathf.SmoothStep(0f, 1f, t / duration);
                yield return null;
            }
            openness = 0f;
            flash = 1f;
            Fx.Burst(sparkSprite, transform.position, 14, Vector2.zero, 2.2f, 0.6f);
        }

        void Update()
        {
            if (IsOpen && Random.value < Time.deltaTime * 10f)
            {
                Vector2 offset = Random.insideUnitCircle.normalized * 0.95f;
                Fx.Spawn(sparkSprite, (Vector2)transform.position + offset, -offset * 1.2f, 0.7f, order: 260);
            }
            flash = Mathf.MoveTowards(flash, 0f, Time.deltaTime * 2.5f);
            Apply();
        }

        void Apply()
        {
            float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.03f;
            float scale = Mathf.Max(0f, openness) * pulse;
            if (visual != null)
            {
                visual.localScale = new Vector3(scale, scale, 1f);
                visual.gameObject.SetActive(openness > 0.001f);
            }
            if (portalLight != null)
                portalLight.intensity = lightIntensity * Mathf.Clamp01(openness) + flash * 4f + LightBoost;
        }
    }
}
