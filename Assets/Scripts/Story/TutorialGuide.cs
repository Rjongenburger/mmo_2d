using System.Collections;
using UnityEngine;

namespace TopDown
{
    /// <summary>
    /// Drives the opening on the mountain: talk to the mage → learn Fire Bolt and take the gale feather →
    /// fly to the next peak → defeat the wind crows. Updates the objective panel and the hint arrow.
    /// </summary>
    public class TutorialGuide : MonoBehaviour
    {
        public enum Step { Arrive, TalkToMage, TakeFeather, FlyAcross, DefeatCrows, Done }

        [SerializeField] NpcDialogue mage;
        [SerializeField] SpriteFlipbook mageAnimation;
        [SerializeField] Sprite[] mageIdleFrames;
        [SerializeField] Sprite[] mageCastFrames;
        [SerializeField] SpellDefinition firstSpell;
        [SerializeField] GroundItem feather;
        [SerializeField] Transform secondPeak;
        [SerializeField] float secondPeakMinX = 16.5f;
        [SerializeField] int crowsToDefeat = 4;
        [SerializeField] HintArrow arrow;
        [SerializeField] Sprite[] sparkleFrames;
        [SerializeField] AudioClip conjureClip;
        [SerializeField] AudioClip spellLearnedClip;

        int crowsDefeated;

        public Step Current { get; private set; } = Step.Arrive;
        public int CrowsDefeated => crowsDefeated;

        void OnEnable()
        {
            GroundItem.PickedUp += OnPickedUp;
            Enemy.Killed += OnEnemyKilled;
            if (mage != null)
                mage.IntroductionFinished += OnMageFinished;
        }

        void OnDisable()
        {
            GroundItem.PickedUp -= OnPickedUp;
            Enemy.Killed -= OnEnemyKilled;
            if (mage != null)
                mage.IntroductionFinished -= OnMageFinished;
        }

        void Start()
        {
            if (feather != null)
                feather.gameObject.SetActive(false);
        }

        public void Begin() => SetStep(Step.TalkToMage);

        void Update()
        {
            // Landing anywhere on the second peak completes the crossing.
            var player = Player.Instance;
            if (Current == Step.FlyAcross && player != null && player.transform.position.x > secondPeakMinX &&
                !player.Airborne.Airborne && !player.Airborne.Falling)
                SetStep(Step.DefeatCrows);
        }

        void OnMageFinished() => StartCoroutine(GiveGifts());

        IEnumerator GiveGifts()
        {
            if (mageAnimation != null && mageCastFrames.Length > 0)
                mageAnimation.SetFrames(mageCastFrames, 4f);
            yield return new WaitForSeconds(0.4f);

            // The first spell.
            var player = Player.Instance;
            player.Caster.Learn(firstSpell);
            Sfx.Play(spellLearnedClip, 0.7f);
            Fx.PlayOnce(sparkleFrames, (Vector2)player.transform.position + new Vector2(0f, 1.2f), 10f);
            if (GameUI.Instance != null)
                GameUI.Instance.Banner.Show(firstSpell.Icon, "New spell learned!", firstSpell.DisplayName);
            GameLog.Add($"You learned <#ff8a2a>{firstSpell.DisplayName}</>! Click a target or press F to cast it.");
            yield return new WaitForSeconds(1.2f);

            // The gale feather.
            Sfx.Play(conjureClip, 0.45f);
            if (feather != null)
            {
                Fx.PlayOnce(sparkleFrames, (Vector2)feather.transform.position + new Vector2(0f, 0.3f), 10f);
                feather.gameObject.SetActive(true);
            }
            GameLog.Add("Aldric conjures a shimmering feather out of the wind.");
            mage.SetReminder(
                new DialogueLine(false, "Take the gale feather and run to the edge. Jump with <#ffe08a>Space</>, then press <#ffe08a>Space</> again while in the air."),
                new DialogueLine(false, "The wind will carry you. Steer it like you'd steer your own feet."));
            SetStep(Step.TakeFeather);

            yield return new WaitForSeconds(0.8f);
            if (mageAnimation != null && mageIdleFrames.Length > 0)
                mageAnimation.SetFrames(mageIdleFrames, 5f);
        }

        void OnPickedUp(GroundItem item)
        {
            if (item == feather && Current <= Step.TakeFeather)
                SetStep(Step.FlyAcross);
        }

        void OnEnemyKilled(Enemy enemy)
        {
            if (Current != Step.DefeatCrows && Current != Step.FlyAcross)
                return;
            crowsDefeated++;
            if (crowsDefeated >= crowsToDefeat)
            {
                SetStep(Step.Done);
                StartCoroutine(Epilogue());
            }
            else
                SetStep(Step.DefeatCrows);
        }

        IEnumerator Epilogue()
        {
            yield return new WaitForSeconds(2f);
            GameLog.Add("<#7a1a1a>Aldric's voice drifts across on the wind:</>");
            GameLog.Add("<#7a1a1a>\"Well cast! You are a quick study, hero... at dawn, we descend.\"</>");
        }

        void SetStep(Step step)
        {
            Current = step;
            switch (step)
            {
                case Step.TalkToMage:
                    SetObjective("Talk to the mage", "Speak with <#ffff00>Aldric</>.");
                    Point(mage);
                    break;
                case Step.TakeFeather:
                    SetObjective("The gale feather", "Take the <#ff9040>Gale feather</> Aldric conjured.");
                    Point(feather);
                    break;
                case Step.FlyAcross:
                    SetObjective("Ride the wind", "Run to the eastern edge. Press <#ffe08a>Space</> to jump, then <#ffe08a>Space</> again in mid-air to fly to the next peak.");
                    Point(secondPeak);
                    break;
                case Step.DefeatCrows:
                    SetObjective("Wind crows", $"Defeat the crows with <#ff8a2a>Fire Bolt</>: click a crow or press <#ffe08a>F</>. ({crowsDefeated}/{crowsToDefeat})");
                    Point(null);
                    break;
                case Step.Done:
                    SetObjective("Tutorial complete", "The peak is quiet again. You've mastered your first spell. To be continued...");
                    Point(null);
                    break;
            }
        }

        void SetObjective(string title, string text)
        {
            if (GameUI.Instance != null)
                GameUI.Instance.Objective.Set(title, text);
        }

        void Point(Component target)
        {
            if (arrow != null)
                arrow.PointAt(target);
        }
    }
}
