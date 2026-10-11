using System;
using System.Collections;
using UnityEngine;

namespace TopDown
{
    /// <summary>A tree that can be chopped for logs and Woodcutting XP; falls over, leaves a stump and regrows.</summary>
    public class ChoppableTree : Interactable
    {
        [SerializeField] ItemDefinition logs;
        [SerializeField] ItemDefinition requiredTool;
        [SerializeField] int xpPerLog = 25;
        [SerializeField] SpriteRenderer treeRenderer;
        [SerializeField] SpriteRenderer stumpRenderer;
        [Tooltip("One swing per OSRS game tick.")]
        [SerializeField] float swingInterval = 0.6f;
        [SerializeField, Range(0f, 1f)] float successChance = 0.35f;
        [Tooltip("Never take more swings than this for a log (keeps the tutorial snappy).")]
        [SerializeField] int guaranteedAfterSwings = 4;
        [SerializeField] float regrowSeconds = 20f;
        [SerializeField] AudioClip chopClip;
        [SerializeField] AudioClip fallClip;
        [SerializeField] Sprite chipSprite;

        bool felled;
        Vector3 treeRestPosition;

        public bool IsFelled => felled;
        public override string Verb => "Chop down";
        public override bool CanInteract => base.CanInteract && !felled;

        public event Action<Player> LogsChopped;

        void Awake()
        {
            if (treeRenderer != null)
                treeRestPosition = treeRenderer.transform.localPosition;
            if (stumpRenderer != null)
                stumpRenderer.enabled = false;
        }

        public override void Interact(Player player)
        {
            if (felled || player.IsBusy)
                return;
            if (requiredTool != null && !player.Inventory.Contains(requiredTool))
            {
                GameLog.Add("You need an axe to chop down this tree.");
                return;
            }
            if (!logs.Stackable && player.Inventory.FreeSlots == 0)
            {
                GameLog.Add("Your inventory is too full to hold any more logs.");
                return;
            }
            player.StartAction(Chop(player), () => player.Animator.PlayingAction = false);
        }

        IEnumerator Chop(Player player)
        {
            GameLog.Add("You swing your axe at the tree.");
            player.Controller.FaceTowards(transform.position);
            player.Animator.PlayingAction = true;
            int swings = 0;

            while (!felled)
            {
                // The action loop shows the wind-up frame first; the axe lands half way through the tick.
                yield return new WaitForSeconds(swingInterval * 0.5f);
                Sfx.Play(chopClip, 0.8f, 0.08f);
                StartCoroutine(Shake());
                Vector2 hitPoint = (Vector2)transform.position + new Vector2(0f, 0.35f);
                Fx.Burst(chipSprite, hitPoint, 4, new Vector2(0f, 1.6f), 1.4f, 0.5f, gravity: 6f);
                swings++;
                yield return new WaitForSeconds(swingInterval * 0.5f);

                if (swings < guaranteedAfterSwings && UnityEngine.Random.value > successChance)
                    continue;

                if (!player.Inventory.Add(logs))
                {
                    GameLog.Add("Your inventory is too full to hold any more logs.");
                    break;
                }
                GameLog.Add("You get some logs.");
                player.Skills.AddXp(Skill.Woodcutting, xpPerLog);
                LogsChopped?.Invoke(player);
                yield return Fall();
            }

            player.Animator.PlayingAction = false;
        }

        IEnumerator Shake()
        {
            if (treeRenderer == null)
                yield break;
            var t = treeRenderer.transform;
            for (int i = 0; i < 4; i++)
            {
                t.localPosition = treeRestPosition + new Vector3((i % 2 == 0 ? 1f : -1f) / 32f, 0f, 0f);
                yield return new WaitForSeconds(0.03f);
            }
            t.localPosition = treeRestPosition;
        }

        IEnumerator Fall()
        {
            felled = true;
            Sfx.Play(fallClip, 0.9f);
            if (stumpRenderer != null)
                stumpRenderer.enabled = true;

            if (treeRenderer != null)
            {
                var t = treeRenderer.transform;
                // Topple to the east (downwind) around the trunk base, then fade.
                for (float time = 0f; time < 0.8f; time += Time.deltaTime)
                {
                    float p = time / 0.8f;
                    t.localRotation = Quaternion.Euler(0f, 0f, -85f * p * p);
                    yield return null;
                }
                for (float time = 0f; time < 0.35f; time += Time.deltaTime)
                {
                    treeRenderer.color = new Color(1f, 1f, 1f, 1f - time / 0.35f);
                    yield return null;
                }
                treeRenderer.enabled = false;
                t.localRotation = Quaternion.identity;
                treeRenderer.color = Color.white;
            }

            StartCoroutine(Regrow());
        }

        IEnumerator Regrow()
        {
            yield return new WaitForSeconds(regrowSeconds);
            if (treeRenderer != null)
            {
                treeRenderer.enabled = true;
                var t = treeRenderer.transform;
                for (float time = 0f; time < 1f; time += Time.deltaTime)
                {
                    float s = Mathf.SmoothStep(0.3f, 1f, time);
                    t.localScale = new Vector3(s, s, 1f);
                    treeRenderer.color = new Color(1f, 1f, 1f, Mathf.Clamp01(time * 2f));
                    yield return null;
                }
                t.localScale = Vector3.one;
                treeRenderer.color = Color.white;
            }
            if (stumpRenderer != null)
                stumpRenderer.enabled = false;
            felled = false;
        }
    }
}
