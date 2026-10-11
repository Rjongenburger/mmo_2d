using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TopDown
{
    /// <summary>
    /// Opening cutscene: the hero sleeps in their cottage, a portal tears open, wakes them, and sucks them in.
    /// Any key skips it. Loads the next scene at the end.
    /// </summary>
    public class IntroCutscene : MonoBehaviour
    {
        [SerializeField] string nextScene = "Mountains";
        [SerializeField] SpriteRenderer bed;
        [SerializeField] Sprite bedSleeping;
        [SerializeField] Sprite bedStartled;
        [SerializeField] Sprite bedEmpty;
        [SerializeField] SpriteRenderer flyingHero;
        [SerializeField] SpriteRenderer exclaim;
        [SerializeField] Portal portal;
        [SerializeField] Transform[] rattlers;
        [SerializeField] Transform cameraTransform;
        [SerializeField] Sprite zzzSprite;
        [SerializeField] Sprite sparkSprite;
        [SerializeField] AudioSource windAudio;
        [SerializeField] RectTransform letterboxTop;
        [SerializeField] RectTransform letterboxBottom;
        [SerializeField] PixelText caption;
        [SerializeField] PixelText skipHint;

        bool skipping;
        float rattle;
        float shake;
        Vector3 cameraRest;
        Vector3[] rattlerRest;

        public bool Finished { get; private set; }

        void Start()
        {
            cameraRest = cameraTransform.position;
            rattlerRest = new Vector3[rattlers.Length];
            for (int i = 0; i < rattlers.Length; i++)
                rattlerRest[i] = rattlers[i].localPosition;
            bed.sprite = bedSleeping;
            flyingHero.enabled = false;
            exclaim.enabled = false;
            portal.SetOpen(false);
            caption.Text = "";
            StartCoroutine(Play());
        }

        void Update()
        {
            if (!skipping && Time.timeSinceLevelLoad > 1f && SkipPressed())
            {
                // Stop the timeline (and any nested animations it is waiting on) before fading out.
                skipping = true;
                StopAllCoroutines();
                rattle = shake = 0f;
                StartCoroutine(Skip());
            }

            // Rattling furniture and camera shake while the portal tears open.
            for (int i = 0; i < rattlers.Length; i++)
            {
                Vector2 jitter = rattle > 0f ? Random.insideUnitCircle * (rattle / 32f) : Vector2.zero;
                rattlers[i].localPosition = rattlerRest[i] + (Vector3)RoundToPixel(jitter);
            }
            Vector2 shakeOffset = shake > 0f ? Random.insideUnitCircle * (shake / 32f) : Vector2.zero;
            cameraTransform.position = cameraRest + (Vector3)RoundToPixel(shakeOffset);
        }

        static Vector2 RoundToPixel(Vector2 v) => new(Mathf.Round(v.x * 32f) / 32f, Mathf.Round(v.y * 32f) / 32f);

        static bool SkipPressed() =>
            (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        IEnumerator Play()
        {
            var fader = ScreenFader.Instance;
            fader.Set(Color.black, 1f);
            yield return Caption("Somewhere, in a quiet village, on an ordinary night...", 2.6f);
            yield return fader.Fade(Color.black, 1f, 0f, 1.6f);

            // Sleeping peacefully.
            for (float t = 0f; t < 3.2f; t += Time.deltaTime)
            {
                EmitZzz();
                yield return null;
            }

            // Something stirs: a hum, rattling, a spark of light.
            Fx.Burst(sparkSprite, portal.transform.position, 10, Vector2.zero, 1.5f, 0.8f, order: 260);
            for (float t = 0f; t < 1.2f; t += Time.deltaTime)
            {
                rattle = t / 1.2f * 1.5f;
                yield return null;
            }

            // The portal tears open.
            shake = 1.5f;
            yield return portal.Open(1.1f);
            if (windAudio != null)
                windAudio.Play();

            // The hero wakes with a start.
            bed.sprite = bedStartled;
            exclaim.enabled = true;
            for (float t = 0f; t < 1.3f; t += Time.deltaTime)
            {
                SuckParticles(4f);
                yield return null;
            }
            exclaim.enabled = false;

            // ...and is pulled in.
            bed.sprite = bedEmpty;
            flyingHero.enabled = true;
            Vector3 start = flyingHero.transform.position;
            Vector3 target = portal.transform.position;
            rattle = 2.5f;
            shake = 2.5f;
            for (float t = 0f; t < 1.4f; t += Time.deltaTime)
            {
                float p = t / 1.4f;
                float eased = p * p;
                Vector3 pos = Vector3.Lerp(start, target, eased) + new Vector3(0f, Mathf.Sin(p * Mathf.PI) * 0.6f, 0f);
                flyingHero.transform.position = pos;
                flyingHero.transform.localRotation = Quaternion.Euler(0f, 0f, 540f * eased);
                float s = Mathf.Lerp(1f, 0.15f, eased);
                flyingHero.transform.localScale = new Vector3(s, s, 1f);
                SuckParticles(8f);
                if (windAudio != null)
                    windAudio.volume = Mathf.Lerp(0.2f, 0.9f, p);
                yield return null;
            }
            flyingHero.enabled = false;

            // Blinding flash.
            portal.LightBoost = 6f;
            yield return fader.Fade(Color.white, 0f, 1f, 0.45f);
            yield return new WaitForSeconds(0.3f);
            Finish();
        }

        IEnumerator Caption(string text, float hold)
        {
            caption.Text = text;
            caption.VisibleCharacters = 0;
            for (int i = 0; i <= text.Length; i++)
            {
                caption.VisibleCharacters = i;
                yield return new WaitForSeconds(0.03f);
            }
            yield return new WaitForSeconds(hold);
            caption.Text = "";
        }

        void EmitZzz()
        {
            if (Random.value > Time.deltaTime * 1.1f)
                return;
            Vector2 head = (Vector2)bed.transform.position + new Vector2(0.15f, 1.75f);
            Fx.Spawn(zzzSprite, head, new Vector2(Random.Range(0.15f, 0.35f), 0.45f), 1.8f, order: 250);
        }

        void SuckParticles(float rate)
        {
            if (Random.value > Time.deltaTime * rate * 6f)
                return;
            Vector2 from = (Vector2)portal.transform.position + Random.insideUnitCircle.normalized * Random.Range(2.5f, 5f);
            var p = Fx.Spawn(sparkSprite, from, Vector2.zero, 2.5f, order: 255);
            p.HomeTowards(portal.transform, 14f);
        }

        IEnumerator Skip()
        {
            yield return ScreenFader.Instance.Fade(Color.white, 0f, 1f, 0.35f);
            Finish();
        }

        void Finish()
        {
            if (Finished)
                return;
            Finished = true;
            SceneManager.LoadScene(nextScene);
        }
    }
}
