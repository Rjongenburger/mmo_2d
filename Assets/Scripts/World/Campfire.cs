using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TopDown
{
    /// <summary>A fire lit from logs: flickers, crackles and throws embers, then burns down to ashes.</summary>
    public class Campfire : Interactable
    {
        [SerializeField] float burnSeconds = 90f;
        [SerializeField] SpriteRenderer fireRenderer;
        [SerializeField] SpriteFlipbook flames;
        [SerializeField] Sprite ashesSprite;
        [SerializeField] Light2D fireLight;
        [SerializeField] AudioSource crackle;
        [SerializeField] Sprite emberSprite;

        bool burning = true;

        public override string Verb => null;

        void Start() => StartCoroutine(Burn());

        public override void Interact(Player player) { }

        void Update()
        {
            if (burning && Random.value < Time.deltaTime * 6f)
                Fx.Spawn(emberSprite, (Vector2)transform.position + new Vector2(Random.Range(-0.15f, 0.15f), 0.35f),
                    new Vector2(Random.Range(0.2f, 0.9f), Random.Range(0.8f, 1.6f)), Random.Range(0.6f, 1.2f));
        }

        IEnumerator Burn()
        {
            yield return new WaitForSeconds(burnSeconds);
            burning = false;
            if (flames != null)
                flames.enabled = false;
            if (fireRenderer != null)
                fireRenderer.sprite = ashesSprite;
            float startIntensity = fireLight != null ? fireLight.intensity : 0f;
            for (float t = 0f; t < 2f; t += Time.deltaTime)
            {
                if (fireLight != null)
                    fireLight.intensity = Mathf.Lerp(startIntensity, 0f, t / 2f);
                if (crackle != null)
                    crackle.volume = Mathf.Lerp(crackle.volume, 0f, t / 2f);
                yield return null;
            }
            yield return new WaitForSeconds(20f);
            Destroy(gameObject);
        }
    }
}
