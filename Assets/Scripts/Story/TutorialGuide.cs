using System.Collections;
using UnityEngine;

namespace TopDown
{
    /// <summary>
    /// Drives the opening on the mountain: talk to the mage → take the gale feather → leap across →
    /// take the axe → chop the tree → light a fire. Updates the objective panel and the hint arrow.
    /// </summary>
    public class TutorialGuide : MonoBehaviour
    {
        public enum Step { Arrive, TalkToMage, TakeFeather, LeapAcross, TakeAxe, ChopTree, LightFire, Done }

        [SerializeField] NpcDialogue mage;
        [SerializeField] SpriteFlipbook mageAnimation;
        [SerializeField] Sprite[] mageIdleFrames;
        [SerializeField] Sprite[] mageCastFrames;
        [SerializeField] GroundItem feather;
        [SerializeField] LeapPoint leapOut;
        [SerializeField] LeapPoint leapBack;
        [SerializeField] GroundItem axe;
        [SerializeField] ChoppableTree tree;
        [SerializeField] HintArrow arrow;
        [SerializeField] Sprite[] sparkleFrames;
        [SerializeField] AudioClip conjureClip;

        public Step Current { get; private set; } = Step.Arrive;

        void OnEnable()
        {
            GroundItem.PickedUp += OnPickedUp;
            LeapPoint.AnyLeap += OnLeap;
            if (mage != null)
                mage.IntroductionFinished += OnMageFinished;
            if (tree != null)
                tree.LogsChopped += OnLogsChopped;
        }

        void OnDisable()
        {
            GroundItem.PickedUp -= OnPickedUp;
            LeapPoint.AnyLeap -= OnLeap;
            if (mage != null)
                mage.IntroductionFinished -= OnMageFinished;
            if (tree != null)
                tree.LogsChopped -= OnLogsChopped;
            if (Player.Instance != null)
                Player.Instance.FireLit -= OnFireLit;
        }

        void Start()
        {
            if (feather != null)
                feather.gameObject.SetActive(false);
            if (Player.Instance != null)
                Player.Instance.FireLit += OnFireLit;
        }

        public void Begin() => SetStep(Step.TalkToMage);

        void OnMageFinished() => StartCoroutine(ConjureFeather());

        IEnumerator ConjureFeather()
        {
            if (mageAnimation != null && mageCastFrames.Length > 0)
                mageAnimation.SetFrames(mageCastFrames, 4f);
            yield return new WaitForSeconds(0.5f);
            Sfx.Play(conjureClip, 0.5f);
            if (feather != null)
            {
                Fx.PlayOnce(sparkleFrames, (Vector2)feather.transform.position + new Vector2(0f, 0.3f), 10f);
                Fx.Burst(sparkleFrames.Length > 0 ? sparkleFrames[1] : null, feather.transform.position, 8, new Vector2(0f, 0.6f), 1.2f, 0.7f);
                feather.gameObject.SetActive(true);
            }
            GameLog.Add("Aldric conjures a shimmering feather out of the wind.");
            mage.SetReminder(
                new DialogueLine(false, "Take the gale feather, then stand on the rune slab at the eastern edge."),
                new DialogueLine(false, "The wind will do the rest. Trust it."));
            SetStep(Step.TakeFeather);
            yield return new WaitForSeconds(1f);
            if (mageAnimation != null && mageIdleFrames.Length > 0)
                mageAnimation.SetFrames(mageIdleFrames, 5f);
        }

        void OnPickedUp(GroundItem item)
        {
            if (item == feather && Current <= Step.TakeFeather)
                SetStep(Step.LeapAcross);
            else if (item == axe && Current <= Step.TakeAxe)
                SetStep(Step.ChopTree);
        }

        void OnLeap(LeapPoint from, LeapPoint to)
        {
            if (from == leapOut && Current == Step.LeapAcross)
                SetStep(axe != null ? Step.TakeAxe : Step.ChopTree);
        }

        void OnLogsChopped(Player player)
        {
            if (Current < Step.LightFire)
                SetStep(Step.LightFire);
        }

        void OnFireLit(Campfire fire)
        {
            if (Current == Step.Done)
                return;
            SetStep(Step.Done);
            StartCoroutine(Epilogue());
        }

        IEnumerator Epilogue()
        {
            yield return new WaitForSeconds(2.5f);
            GameLog.Add("<#7a1a1a>Aldric's voice drifts across on the wind:</>");
            GameLog.Add("<#7a1a1a>\"Well done. Rest by the fire, hero... at dawn, we descend.\"</>");
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
                case Step.LeapAcross:
                    SetObjective("Cross the gap", "Stand on the glowing rune slab at the eastern edge and leap to the next peak.");
                    Point(leapOut);
                    break;
                case Step.TakeAxe:
                    SetObjective("An old axe", "Take the <#ff9040>Bronze axe</> from the chopping block.");
                    Point(axe);
                    break;
                case Step.ChopTree:
                    SetObjective("Woodcutting", "Chop down the pine <#00ffff>Tree</>.");
                    Point(tree);
                    break;
                case Step.LightFire:
                    SetObjective("Keep warm", "Night is falling. Click the <#ff9040>Logs</> in your inventory to light a fire.");
                    Point(null);
                    break;
                case Step.Done:
                    SetObjective("Tutorial complete", "You survived your first night on the peaks. To be continued...");
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
