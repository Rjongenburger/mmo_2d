using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TopDown
{
    /// <summary>
    /// OSRS-style NPC dialogue inside the chatbox: speaker portrait, name, typewriter text and
    /// "Click here to continue". Space / E / Enter / click advances. Locks the player while open.
    /// </summary>
    public class DialogueBox : MonoBehaviour
    {
        const float CharactersPerSecond = 55f;

        UiSkin skin;
        ChatBox chat;
        RectTransform root;
        Image portrait;
        RectTransform portraitRect;
        PixelText nameText, bodyText, continueText;

        string npcName;
        Sprite npcPortrait;
        DialogueLine[] lines;
        int index;
        float revealed;
        int openedFrame;
        Action finished;

        public bool IsOpen { get; private set; }
        public int LineIndex => index;

        public void Build(UiSkin uiSkin, ChatBox chatBox)
        {
            skin = uiSkin;
            chat = chatBox;
            root = UiFactory.Stretch("Dialogue", chat.Panel, 7f);
            portraitRect = UiFactory.Rect("Portrait", root, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(6f, 0f), new Vector2(52f, 52f));
            UiFactory.Image(portraitRect, skin.portraitFrame, sliced: true, raycast: false);
            var face = UiFactory.Rect("Face", portraitRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(48f, 48f));
            portrait = UiFactory.Image(face, null, raycast: false);
            portrait.preserveAspect = true;
            face.SetAsFirstSibling();

            var nameRect = UiFactory.Rect("Name", root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(64f, -3f), new Vector2(Width - 128f, 10f));
            nameText = UiFactory.Text(nameRect, skin.font, "", skin.npcName, TextAnchor.UpperCenter, wrap: false);
            nameText.Shadow = false;
            var bodyRect = UiFactory.Rect("Body", root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(64f, -17f), new Vector2(Width - 128f, 40f));
            bodyText = UiFactory.Text(bodyRect, skin.font, "", skin.chatText, TextAnchor.UpperCenter);
            bodyText.Shadow = false;
            var continueRect = UiFactory.Rect("Continue", root, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(64f, 4f), new Vector2(Width - 128f, 10f));
            continueText = UiFactory.Text(continueRect, skin.font, "Click here to continue", skin.continueText, TextAnchor.LowerCenter, wrap: false);
            continueText.Shadow = false;
            root.gameObject.SetActive(false);
        }

        float Width => ChatBox.Width - 14f;

        public void Show(string speaker, Sprite speakerPortrait, DialogueLine[] conversation, Action onFinished)
        {
            if (conversation == null || conversation.Length == 0)
            {
                onFinished?.Invoke();
                return;
            }
            npcName = speaker;
            npcPortrait = speakerPortrait;
            lines = conversation;
            finished = onFinished;
            index = 0;
            IsOpen = true;
            openedFrame = Time.frameCount;
            root.gameObject.SetActive(true);
            chat.ShowMessages(false);
            if (Player.Instance != null)
            {
                Player.Instance.CancelAction();
                Player.Instance.Controller.Lock();
            }
            ShowLine();
        }

        void ShowLine()
        {
            var line = lines[index];
            bool player = line.playerSpeaks;
            nameText.Text = player ? "You" : npcName;
            portrait.sprite = player ? skin.playerPortrait : npcPortrait;
            portrait.color = portrait.sprite != null ? Color.white : Color.clear;
            portraitRect.anchorMin = portraitRect.anchorMax = portraitRect.pivot = new Vector2(player ? 1f : 0f, 0.5f);
            portraitRect.anchoredPosition = new Vector2(player ? -6f : 6f, 0f);
            bodyText.Text = line.text;
            bodyText.VisibleCharacters = 0;
            revealed = 0f;
            continueText.enabled = false;
        }

        void Update()
        {
            if (!IsOpen)
                return;

            int total = bodyText.CharacterCount;
            revealed = Mathf.Min(total, revealed + Time.deltaTime * CharactersPerSecond);
            bodyText.VisibleCharacters = (int)revealed;
            bool complete = (int)revealed >= total;
            continueText.enabled = complete;

            if (Time.frameCount <= openedFrame || !AdvancePressed())
                return;
            if (!complete)
                revealed = total;
            else
                Next();
        }

        static bool AdvancePressed()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.eKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame))
                return true;
            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
                return true;
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        }

        /// <summary>Advances to the next line (finishing the typewriter first). Public for tests and UI buttons.</summary>
        public void Next()
        {
            if (!IsOpen)
                return;
            GameUI.Instance?.PlayClick();
            index++;
            if (index >= lines.Length)
                Close();
            else
                ShowLine();
        }

        void Close()
        {
            IsOpen = false;
            root.gameObject.SetActive(false);
            chat.ShowMessages(true);
            if (Player.Instance != null)
                Player.Instance.Controller.Unlock();
            var callback = finished;
            finished = null;
            callback?.Invoke();
        }
    }
}
