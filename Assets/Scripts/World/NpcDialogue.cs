using System;
using UnityEngine;

namespace TopDown
{
    [Serializable]
    public struct DialogueLine
    {
        public bool playerSpeaks;
        [TextArea(2, 4)] public string text;

        public DialogueLine(bool player, string line)
        {
            playerSpeaks = player;
            text = line;
        }
    }

    /// <summary>An NPC you can talk to: a first conversation, then a shorter reminder.</summary>
    public class NpcDialogue : Interactable
    {
        [SerializeField] Sprite portrait;
        [SerializeField] DialogueLine[] introduction = Array.Empty<DialogueLine>();
        [SerializeField] DialogueLine[] reminder = Array.Empty<DialogueLine>();

        public bool HasIntroduced { get; private set; }
        public Sprite Portrait => portrait;
        public override string Verb => "Talk-to";

        public event Action IntroductionFinished;

        public void SetConversation(Sprite npcPortrait, DialogueLine[] first, DialogueLine[] repeat)
        {
            portrait = npcPortrait;
            introduction = first;
            reminder = repeat;
        }

        /// <summary>Replaces what the NPC says on later visits (e.g. as the story progresses).</summary>
        public void SetReminder(params DialogueLine[] lines) => reminder = lines;

        public override void Interact(Player player) => StartConversation();

        public void StartConversation()
        {
            var box = GameUI.Instance != null ? GameUI.Instance.Dialogue : null;
            if (box == null || box.IsOpen)
                return;

            bool first = !HasIntroduced;
            box.Show(DisplayName, portrait, first ? introduction : reminder, () =>
            {
                if (!first)
                    return;
                HasIntroduced = true;
                IntroductionFinished?.Invoke();
            });
        }
    }
}
