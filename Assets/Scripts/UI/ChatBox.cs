using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TopDown
{
    /// <summary>Parchment chatbox in the bottom-left showing the latest game messages.</summary>
    public class ChatBox : MonoBehaviour
    {
        public const int Width = 380, Height = 92;
        const int MaxMessages = 6;

        readonly List<string> history = new();
        PixelText messages;

        public RectTransform Panel { get; private set; }

        public void Build(UiSkin skin, Transform parent)
        {
            Panel = UiFactory.Rect("Chatbox", parent, Vector2.zero, Vector2.zero, new Vector2(4f, 4f), new Vector2(Width, Height));
            UiFactory.Image(Panel, skin.parchmentPanel, tiled: true);
            var inner = UiFactory.Stretch("Messages", Panel, 8f);
            inner.gameObject.AddComponent<RectMask2D>();
            messages = UiFactory.Text(inner, skin.font, "", skin.chatText, TextAnchor.LowerLeft);
            messages.Shadow = false;
            Add("Welcome to the peaks.");
        }

        void OnEnable() => GameLog.MessageAdded += Add;
        void OnDisable() => GameLog.MessageAdded -= Add;

        public void Add(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;
            history.Add(message);
            if (history.Count > MaxMessages)
                history.RemoveAt(0);
            if (messages != null)
                messages.Text = string.Join("\n", history);
        }

        public void ShowMessages(bool show) => messages.enabled = show;
    }
}
