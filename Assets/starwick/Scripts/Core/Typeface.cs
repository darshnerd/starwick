using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Starwick
{
    public static class Typeface
    {
        static TMP_FontAsset heading;
        static TMP_FontAsset headingMedium;
        static TMP_FontAsset body;
        static TMP_FontAsset bodyMedium;
        static TMP_FontAsset fallback;
        static bool built;

        public static TMP_FontAsset Heading => Ensure() ? heading : TMP_Settings.defaultFontAsset;
        public static TMP_FontAsset HeadingMedium => Ensure() ? headingMedium : TMP_Settings.defaultFontAsset;
        public static TMP_FontAsset Body => Ensure() ? body : TMP_Settings.defaultFontAsset;
        public static TMP_FontAsset BodyMedium => Ensure() ? bodyMedium : TMP_Settings.defaultFontAsset;

        public static bool UsingCustomFonts => Ensure() && heading != null && heading != TMP_Settings.defaultFontAsset;
        public static string HeadingFamily => Ensure() && heading != null ? heading.faceInfo.familyName : "";
        public static string BodyFamily => Ensure() && body != null ? body.faceInfo.familyName : "";
        public static string HeadingAssetName => Ensure() && heading != null ? heading.name : "";

        static bool Ensure()
        {
            if (built) return heading != null;
            built = true;

            fallback = Build("Fonts/NotoSans-Regular");
            heading = Build("Fonts/SpaceGrotesk-SemiBold");
            headingMedium = Build("Fonts/SpaceGrotesk-Medium");
            body = Build("Fonts/Sora-Regular");
            bodyMedium = Build("Fonts/Sora-Medium");

            if (fallback != null)
                foreach (var f in new[] { heading, headingMedium, body, bodyMedium })
                {
                    if (f == null) continue;
                    if (f.fallbackFontAssetTable == null)
                        f.fallbackFontAssetTable = new List<TMP_FontAsset>();
                    f.fallbackFontAssetTable.Add(fallback);
                }

            return heading != null;
        }

        static TMP_FontAsset Build(string resourcePath)
        {
            var font = Resources.Load<Font>(resourcePath);
            if (font == null) return null;
            var asset = TMP_FontAsset.CreateFontAsset(font, 120, 12, GlyphRenderMode.SDFAA, 2048, 2048);
            if (asset != null) asset.name = font.name;
            return asset;
        }
    }
}
