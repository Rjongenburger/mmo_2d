using System.Collections;
using UnityEngine;

namespace TopDown
{
    /// <summary>
    /// Opening of the mountain scene: fade in from the portal flash, the hero tumbles out of the portal,
    /// the portal collapses, and the mage starts talking.
    /// </summary>
    public class MountainArrival : MonoBehaviour
    {
        [SerializeField] Portal portal;
        [SerializeField] Transform landingPoint;
        [SerializeField] NpcDialogue mage;
        [SerializeField] TutorialGuide guide;
        [SerializeField] Sprite[] dustFrames;
        [SerializeField] AudioClip landClip;

        /// <summary>Skips the timed sequence (used by automated tests).</summary>
        public static bool SkipForTests { get; set; }

        public bool Finished { get; private set; }

        IEnumerator Start()
        {
            var player = Player.Instance;
            var ui = GameUI.Instance;

            player.Checkpoint = landingPoint.position;
            if (SkipForTests)
            {
                player.Airborne.ResetTo(landingPoint.position);
                portal.SetOpen(false);
                if (ui != null) ui.SetHudVisible(true);
                guide.Begin();
                mage.StartConversation();
                Finished = true;
                yield break;
            }

            player.Controller.Lock();
            if (ui != null) ui.SetHudVisible(false);
            portal.SetOpen(true);
            player.Body.gameObject.SetActive(false);
            player.Shadow.enabled = false;
            Vector2 portalCenter = portal.transform.position;
            player.Airborne.ResetTo(landingPoint.position);

            if (ScreenFader.Instance != null)
            {
                ScreenFader.Instance.Set(Color.white, 1f);
                yield return ScreenFader.Instance.Fade(Color.white, 1f, 0f, 1.4f);
            }
            yield return new WaitForSeconds(0.5f);

            // Tumble out of the portal onto the rock.
            player.Body.gameObject.SetActive(true);
            player.Controller.FaceTowards((Vector2)landingPoint.position + Vector2.down);
            Vector2 landing = landingPoint.position;
            for (float t = 0f; t < 0.55f; t += Time.deltaTime)
            {
                float p = t / 0.55f;
                Vector2 pos = Vector2.Lerp(portalCenter, landing, p);
                player.Body.position = new Vector3(pos.x, pos.y + Mathf.Sin(p * Mathf.PI) * 0.6f, 0f);
                player.Body.localRotation = Quaternion.Euler(0f, 0f, -360f * p);
                yield return null;
            }
            player.Body.localPosition = Vector3.zero;
            player.Body.localRotation = Quaternion.identity;
            player.Shadow.enabled = true;
            Sfx.Play(landClip, 0.9f);
            Fx.PlayOnce(dustFrames, landing, 12f, 1);
            GameLog.Add("You land hard on cold rock. The wind howls around you.");

            yield return new WaitForSeconds(0.8f);
            player.Controller.FaceTowards(portalCenter);
            yield return new WaitForSeconds(0.5f);
            yield return portal.Close(1.1f);
            yield return new WaitForSeconds(0.7f);

            player.Controller.FaceTowards(mage.transform.position);
            if (ui != null) ui.SetHudVisible(true);
            player.Controller.Unlock();
            guide.Begin();
            mage.StartConversation();
            Finished = true;
        }
    }
}
