using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TopDown
{
    /// <summary>
    /// Crisp bitmap-font UI text with OSRS-style drop shadow, word wrapping, a typewriter reveal and
    /// inline colour tags: "Chop down &lt;#00ffff&gt;Tree&lt;/&gt;".
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class PixelText : MaskableGraphic
    {
        [SerializeField] PixelFont font;
        [SerializeField, TextArea(2, 6)] string text = "";
        [SerializeField] TextAnchor alignment = TextAnchor.UpperLeft;
        [SerializeField] bool wrap = true;
        [SerializeField] bool shadow = true;
        [SerializeField] Color shadowColor = Color.black;
        [Tooltip("-1 shows everything; otherwise only this many characters (typewriter effect).")]
        [SerializeField] int visibleCharacters = -1;

        struct Glyph
        {
            public char character;
            public Color32 color;
        }

        struct Line
        {
            public int start, count, width;
        }

        readonly List<Glyph> glyphs = new();
        readonly List<Line> lines = new();
        bool parsedDirty = true;

        public PixelFont Font
        {
            get => font;
            set { font = value; parsedDirty = true; SetAllDirty(); }
        }

        public string Text
        {
            get => text;
            set
            {
                value ??= "";
                if (text == value)
                    return;
                text = value;
                parsedDirty = true;
                SetVerticesDirty();
            }
        }

        public int VisibleCharacters
        {
            get => visibleCharacters;
            set
            {
                if (visibleCharacters == value)
                    return;
                visibleCharacters = value;
                SetVerticesDirty();
            }
        }

        public TextAnchor Alignment
        {
            get => alignment;
            set { alignment = value; SetVerticesDirty(); }
        }

        public bool Wrap
        {
            get => wrap;
            set { wrap = value; SetVerticesDirty(); }
        }

        public bool Shadow
        {
            get => shadow;
            set { shadow = value; SetVerticesDirty(); }
        }

        public Color ShadowColor
        {
            get => shadowColor;
            set { shadowColor = value; SetVerticesDirty(); }
        }

        /// <summary>Number of printable characters (excluding colour tags).</summary>
        public int CharacterCount
        {
            get
            {
                Parse();
                return glyphs.Count;
            }
        }

        public override Texture mainTexture => font != null && font.Atlas != null ? font.Atlas : s_WhiteTexture;

        protected override void OnValidate()
        {
            parsedDirty = true;
            base.OnValidate();
        }

        /// <summary>Size the text needs when wrapped to <paramref name="maxWidth"/> (pass 0 for no wrapping).</summary>
        public Vector2Int Measure(float maxWidth = 0f)
        {
            if (font == null)
                return Vector2Int.zero;
            Parse();
            Layout(maxWidth > 0f ? maxWidth : float.MaxValue);
            int width = 0;
            foreach (var line in lines)
                width = Mathf.Max(width, line.width);
            int height = lines.Count == 0 ? 0 : lines.Count * font.LineHeight - (font.LineHeight - font.GlyphHeight);
            return new Vector2Int(width + (shadow ? 1 : 0), height + (shadow ? 1 : 0));
        }

        void Parse()
        {
            if (!parsedDirty)
                return;
            parsedDirty = false;
            glyphs.Clear();
            var stack = new Stack<Color32>();
            Color32 current = Color.white;
            bool tinted = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '<')
                {
                    int close = text.IndexOf('>', i);
                    if (close > i)
                    {
                        string tag = text.Substring(i + 1, close - i - 1);
                        if (tag == "/")
                        {
                            if (stack.Count > 0)
                                current = stack.Pop();
                            tinted = stack.Count > 0;
                            i = close;
                            continue;
                        }
                        if (tag.StartsWith("#") && ColorUtility.TryParseHtmlString(tag, out var parsed))
                        {
                            stack.Push(current);
                            current = parsed;
                            tinted = true;
                            i = close;
                            continue;
                        }
                    }
                }
                // White marks "use the graphic colour" so untagged text follows `color`.
                glyphs.Add(new Glyph { character = c, color = tinted ? current : new Color32(255, 255, 255, 0) });
            }
        }

        void Layout(float maxWidth)
        {
            lines.Clear();
            int lineStart = 0, lineWidth = 0;
            int i = 0;
            while (i < glyphs.Count)
            {
                char c = glyphs[i].character;
                if (c == '\n')
                {
                    lines.Add(new Line { start = lineStart, count = i - lineStart, width = lineWidth });
                    i++;
                    lineStart = i;
                    lineWidth = 0;
                    continue;
                }

                // Measure the next word (including a leading space).
                int wordEnd = i;
                int wordWidth = 0;
                if (c == ' ')
                {
                    wordWidth += font.Advance(' ');
                    wordEnd++;
                }
                while (wordEnd < glyphs.Count && glyphs[wordEnd].character != ' ' && glyphs[wordEnd].character != '\n')
                {
                    wordWidth += font.Advance(glyphs[wordEnd].character);
                    wordEnd++;
                }

                if (lineWidth > 0 && lineWidth + wordWidth - font.LetterSpacing > maxWidth)
                {
                    lines.Add(new Line { start = lineStart, count = i - lineStart, width = lineWidth - font.LetterSpacing });
                    if (glyphs[i].character == ' ')
                        i++;
                    lineStart = i;
                    lineWidth = 0;
                    continue;
                }

                lineWidth += wordWidth;
                i = wordEnd;
            }
            lines.Add(new Line { start = lineStart, count = glyphs.Count - lineStart, width = Mathf.Max(0, lineWidth - font.LetterSpacing) });
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (font == null || font.Atlas == null)
                return;

            Parse();
            Rect rect = rectTransform.rect;
            Layout(wrap ? rect.width : float.MaxValue);

            int totalHeight = lines.Count * font.LineHeight - (font.LineHeight - font.GlyphHeight);
            float top = alignment switch
            {
                TextAnchor.MiddleLeft or TextAnchor.MiddleCenter or TextAnchor.MiddleRight => rect.center.y + totalHeight * 0.5f,
                TextAnchor.LowerLeft or TextAnchor.LowerCenter or TextAnchor.LowerRight => rect.yMin + totalHeight,
                _ => rect.yMax,
            };
            top = Mathf.Round(top);

            Color32 baseColor = color;
            Color32 shade = shadowColor;
            shade.a = (byte)(shadowColor.a * color.a * 255f);
            int limit = visibleCharacters < 0 ? int.MaxValue : visibleCharacters;

            for (int pass = shadow ? 0 : 1; pass < 2; pass++)
            {
                int shown = 0;
                float y = top;
                foreach (var line in lines)
                {
                    float x = alignment switch
                    {
                        TextAnchor.UpperCenter or TextAnchor.MiddleCenter or TextAnchor.LowerCenter => rect.center.x - line.width * 0.5f,
                        TextAnchor.UpperRight or TextAnchor.MiddleRight or TextAnchor.LowerRight => rect.xMax - line.width,
                        _ => rect.xMin,
                    };
                    x = Mathf.Round(x);
                    for (int g = line.start; g < line.start + line.count; g++)
                    {
                        if (shown >= limit)
                            break;
                        var glyph = glyphs[g];
                        shown++;
                        if (glyph.character == ' ')
                        {
                            x += font.Advance(' ');
                            continue;
                        }
                        if (!font.TryGetGlyph(glyph.character, out var r))
                            continue;

                        Color32 tint = glyph.color.a == 0 ? baseColor : new Color32(glyph.color.r, glyph.color.g, glyph.color.b, baseColor.a);
                        if (pass == 0)
                            AddQuad(vh, x + 1, y - 1, r, shade);
                        else
                            AddQuad(vh, x, y, r, tint);
                        x += r.width + font.LetterSpacing;
                    }
                    y -= font.LineHeight;
                }
            }
        }

        void AddQuad(VertexHelper vh, float left, float top, RectInt r, Color32 c)
        {
            float tw = font.Atlas.width, th = font.Atlas.height;
            Vector2 uvMin = new(r.xMin / tw, r.yMin / th);
            Vector2 uvMax = new(r.xMax / tw, r.yMax / th);
            int index = vh.currentVertCount;
            vh.AddVert(new Vector3(left, top - r.height), c, uvMin);
            vh.AddVert(new Vector3(left, top), c, new Vector2(uvMin.x, uvMax.y));
            vh.AddVert(new Vector3(left + r.width, top), c, uvMax);
            vh.AddVert(new Vector3(left + r.width, top - r.height), c, new Vector2(uvMax.x, uvMin.y));
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index + 2, index + 3, index);
        }
    }
}
