using System;
using System.Collections.Generic;
using UnityEngine;

namespace TopDown
{
    /// <summary>Bitmap font: an atlas texture plus one pixel rect per character (all glyphs share one height).</summary>
    [CreateAssetMenu(menuName = "TopDown/Pixel Font", fileName = "PixelFont")]
    public class PixelFont : ScriptableObject
    {
        [SerializeField] Texture2D atlas;
        [SerializeField] int glyphHeight = 9;
        [SerializeField] int lineHeight = 11;
        [SerializeField] int spaceAdvance = 3;
        [SerializeField] int letterSpacing = 1;
        [Tooltip("Atlas pixels per font pixel (the art generator writes a smoothed atlas at 4x).")]
        [SerializeField, Min(1)] int atlasScale = 1;
        [SerializeField] string characters = "";
        [Tooltip("Glyph rects in atlas pixels, origin bottom-left, matching 'characters' index for index.")]
        [SerializeField] RectInt[] rects = Array.Empty<RectInt>();

        Dictionary<char, RectInt> lookup;

        public Texture2D Atlas => atlas;
        public int GlyphHeight => glyphHeight;
        public int LineHeight => lineHeight;
        public int LetterSpacing => letterSpacing;
        public int AtlasScale => atlasScale;

        public void Setup(Texture2D texture, int height, int line, int space, int spacing, string chars, RectInt[] glyphRects,
            int scale = 1)
        {
            atlas = texture;
            glyphHeight = height;
            lineHeight = line;
            spaceAdvance = space;
            letterSpacing = spacing;
            atlasScale = Mathf.Max(1, scale);
            characters = chars;
            rects = glyphRects;
            lookup = null;
        }

        public bool TryGetGlyph(char c, out RectInt rect)
        {
            if (lookup == null)
            {
                lookup = new Dictionary<char, RectInt>(characters.Length);
                for (int i = 0; i < characters.Length && i < rects.Length; i++)
                    lookup[characters[i]] = rects[i];
            }
            if (lookup.TryGetValue(c, out rect))
                return true;
            return lookup.TryGetValue('?', out rect);
        }

        /// <summary>Horizontal advance for a character, including letter spacing.</summary>
        public int Advance(char c)
        {
            if (c == ' ')
                return spaceAdvance + letterSpacing;
            return TryGetGlyph(c, out var r) ? r.width + letterSpacing : 0;
        }

        void OnEnable() => lookup = null;
        void OnValidate() => lookup = null;
    }
}
