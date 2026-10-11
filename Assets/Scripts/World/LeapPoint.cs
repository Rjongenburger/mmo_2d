using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TopDown
{
    /// <summary>
    /// A rune slab at a cliff edge. With the required item (the gale feather) the wind carries the
    /// player across the gap to the paired leap point.
    /// </summary>
    public class LeapPoint : Interactable
    {
        [SerializeField] ItemDefinition requiredItem;
        [SerializeField] LeapPoint destination;
        [SerializeField] float duration = 1.3f;
        [SerializeField] float arcHeight = 1.6f;
        [SerializeField] SpriteRenderer slab;
        [SerializeField] Sprite dimSprite;
        [SerializeField] Sprite glowSprite;
        [SerializeField] Light2D glowLight;
        [SerializeField] AudioClip leapClip;
        [SerializeField] AudioClip landClip;
        [SerializeField] Sprite[] dustFrames;

        public static event Action<LeapPoint, LeapPoint> AnyLeap;

        public LeapPoint Destination => destination;
        public override string Verb => "Leap-across";

        public void Link(LeapPoint other) => destination = other;

        bool PlayerHasItem => Player.Instance != null && (requiredItem == null || Player.Instance.Inventory.Contains(requiredItem));

        void Update()
        {
            bool ready = PlayerHasItem;
            if (slab != null)
                slab.sprite = ready ? glowSprite : dimSprite;
            if (glowLight != null)
                glowLight.enabled = ready;
        }

        public override void Interact(Player player)
        {
            if (destination == null || player.IsBusy)
                return;
            if (!PlayerHasItem)
            {
                GameLog.Add("The gap is far too wide to jump.");
                return;
            }
            player.StartAction(Leap(player));
        }

        IEnumerator Leap(Player player)
        {
            var controller = player.Controller;
            var rb = player.GetComponent<Rigidbody2D>();
            var col = player.GetComponent<Collider2D>();
            Vector2 start = rb.position;
            Vector2 end = destination.InteractPoint;

            controller.Lock();
            controller.FaceTowards(end);
            col.enabled = false;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            player.Animator.Airborne = true;
            GameLog.Add("The wind lifts you across the gap!");
            Sfx.Play(leapClip, 0.9f);

            for (float time = 0f; time < duration; time += Time.deltaTime)
            {
                float p = time / duration;
                rb.position = Vector2.Lerp(start, end, Mathf.SmoothStep(0f, 1f, p));
                player.Body.localPosition = new Vector3(0f, arcHeight * 4f * p * (1f - p), 0f);
                if (player.Shadow != null)
                    player.Shadow.color = new Color(1f, 1f, 1f, 1f - Mathf.Sin(p * Mathf.PI));
                yield return null;
            }

            rb.position = end;
            player.Body.localPosition = Vector3.zero;
            if (player.Shadow != null)
                player.Shadow.color = Color.white;
            rb.bodyType = RigidbodyType2D.Dynamic;
            col.enabled = true;
            player.Animator.Airborne = false;
            controller.Unlock();
            Sfx.Play(landClip, 0.8f);
            Fx.PlayOnce(dustFrames, end, 12f, 1);
            AnyLeap?.Invoke(this, destination);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => AnyLeap = null;
    }
}
