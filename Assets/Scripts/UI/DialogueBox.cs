using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TopDown
{
    /// <summary>
    /// Story dialogue: a night-blue panel that slides up from the bottom with a framed portrait, a coloured
    /// name plate per speaker, typewriter text with a little "voice" blip, and a bouncing arrow when a line
    /// is complete. Space / E / Enter / click advances. Locks the player while open.
    /// </summary>
    public class DialogueBox : MonoBehaviour
    {
        const float Width = 400f, Height = 76f, CharactersPerSecond = 48f, Margin = 8f;

        UiSkin skin;
        ChatBox chat;
        RectTransform root;
        CanvasGroup group;
        RectTransform plate;
        Image plateImage;
        PixelText plateText;
        RectTransform portraitRoot;
        Image portrait;
        RectTransform bodyRect;
        PixelText body;
        RectTransform arrow;
        AudioSource voice;

        string npcName;
        Sprite npcPortrait;
        Color npcPlateColor;
        float npcPitch;
        DialogueLine[] lines;
        int index;
        float revealed;
        int blipped;
        int openedFrame;
        float appear;
        float arrowX = -12f;
        Action finished;

        public bool IsOpen { get; private set; }
        public int LineIndex => index;

        public void Build(UiSkin uiSkin, Transform parent, ChatBox chatBox)
        {
            skin = uiSkin;
            chat = chatBox;
            root = UiFactory.Rect("Dialogue", parent, Vector2.zero, Vector2.zero, new Vector2(48f, Margin), new Vector2(Width, Height));
            group = root.gameObject.AddComponent<CanvasGroup>();
            UiFactory.Image(root, skin.dialoguePanel, sliced: true);

            portraitRoot = UiFactory.Rect("Portrait", root, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, 0f), new Vector2(56f, 56f));
            UiFactory.Image(UiFactory.Stretch("Backdrop", portraitRoot, 3f), skin.portraitBackdrop, raycast: false);
            var face = UiFactory.Rect("Face", portraitRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(48f, 48f));
            portrait = UiFactory.Image(face, null, raycast: false);
            UiFactory.Image(UiFactory.Stretch("Ring", portraitRoot), skin.portraitRing, sliced: true, raycast: false);

            plate = UiFactory.Rect("NamePlate", root, new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(72f, -1f), new Vector2(60f, 14f));
            plateImage = UiFactory.Image(plate, skin.namePlate, sliced: true, raycast: false);
            var plateLabel = UiFactory.Rect("Name", plate, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(200f, 10f));
            plateText = UiFactory.Text(plateLabel, skin.font, "", Color.white, TextAnchor.MiddleCenter, wrap: false);
            plateText.ShadowColor = new Color(0.04f, 0.05f, 0.09f, 0.85f);

            bodyRect = UiFactory.Rect("Text", root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(76f, -15f), new Vector2(Width - 90f, 46f));
            body = UiFactory.Text(bodyRect, skin.font, "", skin.dialogueText);
            body.ShadowColor = new Color(0.03f, 0.04f, 0.08f, 1f);

            arrow = UiFactory.Rect("Continue", root, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-12f, 8f), new Vector2(9f, 6f));
            UiFactory.Image(arrow, skin.continueArrow, raycast: false);

            voice = root.gameObject.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.spatialBlend = 0f;
            root.gameObject.SetActive(false);
        }

        public void Show(string speaker, Sprite speakerPortrait, DialogueLine[] conversation, Action onFinished,
            Color plateColor, float voicePitch)
        {
            if (conversation == null || conversation.Length == 0)
            {
                onFinished?.Invoke();
                return;
            }
            npcName = speaker;
            npcPortrait = speakerPortrait;
            npcPlateColor = plateColor;
            npcPitch = voicePitch;
            lines = conversation;
            finished = onFinished;
            index = 0;
            IsOpen = true;
            appear = 0f;
            openedFrame = Time.frameCount;
            root.gameObject.SetActive(true);
            root.SetAsLastSibling();
            chat.SetVisible(false);
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

            plateText.Text = player ? "You" : npcName;
            plateImage.color = player ? skin.playerPlateColor : npcPlateColor;
            float plateWidth = plateText.Measure().x + 14f;
            plate.sizeDelta = new Vector2(plateWidth, 14f);

            portrait.sprite = player ? skin.playerPortrait : npcPortrait;
            portrait.color = portrait.sprite != null ? Color.white : Color.clear;

            // NPCs speak from the left, the hero from the right.
            Vector2 side = new(player ? 1f : 0f, 0.5f);
            portraitRoot.anchorMin = portraitRoot.anchorMax = portraitRoot.pivot = side;
            portraitRoot.anchoredPosition = new Vector2(player ? -10f : 10f, 0f);
            plate.anchorMin = plate.anchorMax = new Vector2(player ? 1f : 0f, 1f);
            plate.pivot = new Vector2(player ? 1f : 0f, 0.5f);
            plate.anchoredPosition = new Vector2(player ? -72f : 72f, -1f);
            bodyRect.anchoredPosition = new Vector2(player ? 14f : 76f, -15f);
            arrowX = player ? -76f : -12f; // keep the continue arrow clear of the portrait

            body.Text = line.text;
            body.VisibleCharacters = 0;
            revealed = 0f;
            blipped = 0;
        }

        void Update()
        {
            if (!IsOpen)
                return;

            // Slide up and fade in.
            appear = Mathf.MoveTowards(appear, 1f, Time.deltaTime * 6f);
            float eased = 1f - (1f - appear) * (1f - appear);
            group.alpha = eased;
            float canvasWidth = ((RectTransform)root.parent).rect.width;
            float x = Mathf.Max(Margin, (canvasWidth - SidePanel.Width - 12f - Width) * 0.5f);
            root.anchoredPosition = new Vector2(Mathf.Round(x), Mathf.Round(Margin - 10f * (1f - eased)));

            // Typewriter with a voice blip every couple of letters.
            int total = body.CharacterCount;
            revealed = Mathf.Min(total, revealed + Time.deltaTime * CharactersPerSecond);
            int shown = (int)revealed;
            body.VisibleCharacters = shown;
            if (shown >= blipped + 2)
            {
                blipped = shown;
                voice.pitch = (lines[index].playerSpeaks ? skin.playerVoicePitch : npcPitch) * UnityEngine.Random.Range(0.94f, 1.06f);
                voice.PlayOneShot(skin.dialogueBlip, 0.4f);
            }

            bool complete = shown >= total;
            arrow.gameObject.SetActive(complete);
            if (complete)
                arrow.anchoredPosition = new Vector2(arrowX, 8f + Mathf.Round(Mathf.Abs(Mathf.Sin(Time.time * 5f)) * 2f));

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

        /// <summary>Advances to the next line. Public for tests and UI buttons.</summary>
        public void Next()
        {
            if (!IsOpen)
                return;
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
            chat.SetVisible(true);
            if (Player.Instance != null)
                Player.Instance.Controller.Unlock();
            var callback = finished;
            finished = null;
            callback?.Invoke();
        }
    }
}
