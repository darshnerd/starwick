using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starwick
{
    public class ConstellariumUI : MonoBehaviour
    {
        public bool IsOpen { get; private set; }
        public int LitStars { get; private set; }

        TMP_Text indicator;
        TMP_Text title;
        TMP_Text stats;
        Image panel;
        readonly List<Image> dots = new List<Image>();
        bool wasDown;

        static readonly Color DotDim = new Color(0.3f, 0.36f, 0.6f, 0.5f);
        static readonly Color DotLit = new Color(2.2f, 2.0f, 1.4f, 1f);

        void Start()
        {
            Sw.Constellarium = this;

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Sw.Cam;
            canvas.planeDistance = 0.88f;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            gameObject.AddComponent<GraphicRaycaster>();

            var dotTex = ProcTex.SoftDot(48);
            var dotSprite = Sprite.Create(dotTex, new Rect(0, 0, dotTex.width, dotTex.height), new Vector2(0.5f, 0.5f));

            indicator = MakeText("SkyTab", Typeface.HeadingMedium, 28f, new Color(1.6f, 1.7f, 2.3f, 1f),
                TextAlignmentOptions.Left, new Vector2(0.1f, 0.93f), new Vector2(420f, 44f));
            indicator.text = "your sky";

            panel = MakeImage("Panel", null, new Color(0.01f, 0.0f, 0.04f, 0.9f),
                new Vector2(0.5f, 0.5f), new Vector2(1340f, 800f));

            title = MakeText("Title", Typeface.Heading, 54f, new Color(2.1f, 1.8f, 1.1f, 1f),
                TextAlignmentOptions.Top, new Vector2(0.5f, 0.82f), new Vector2(1200f, 90f));
            title.text = "The Constellarium";

            stats = MakeText("Stats", Typeface.Body, 30f, new Color(1.3f, 1.35f, 1.5f, 1f),
                TextAlignmentOptions.Center, new Vector2(0.5f, 0.68f), new Vector2(1100f, 160f));

            int gw = 12, gh = 6;
            float spacing = 80f;
            float ox = -(gw - 1) * spacing * 0.5f;
            float oy = -(gh - 1) * spacing * 0.5f - 60f;
            for (int y = 0; y < gh; y++)
                for (int x = 0; x < gw; x++)
                {
                    var d = MakeImage("Dot", dotSprite, DotDim, new Vector2(0.5f, 0.45f), new Vector2(36f, 36f));
                    d.rectTransform.anchoredPosition = new Vector2(ox + x * spacing, oy + y * spacing);
                    dots.Add(d);
                }

            SetOpen(false);
        }

        void Update()
        {
            bool down = InputService.PointerDown;
            if (down && !wasDown)
            {
                var p = InputService.PointerPosition;
                if (p.x < Screen.width * 0.2f && p.y > Screen.height * 0.84f) SetOpen(!IsOpen);
            }
            wasDown = down;
        }

        public void SetOpen(bool open)
        {
            IsOpen = open;
            if (panel != null) panel.enabled = open;
            if (title != null) title.enabled = open;
            if (stats != null) stats.enabled = open;
            for (int i = 0; i < dots.Count; i++) dots[i].enabled = open;
            if (open) Refresh();
        }

        void Refresh()
        {
            stats.text = SaveData.TotalStarsRelit + " stars rekindled\n"
                       + SaveData.RunsCompleted + " journeys\n"
                       + SaveData.CompanionsSeenCount() + " / " + Roster.All.Length + " companions met";
            LitStars = Mathf.Min(dots.Count, SaveData.TotalStarsRelit);
            for (int i = 0; i < dots.Count; i++)
                dots[i].color = i < LitStars ? DotLit : DotDim;
        }

        Image MakeImage(string n, Sprite sprite, Color c, Vector2 anchor, Vector2 size)
        {
            var go = new GameObject(n);
            go.transform.SetParent(transform, false);
            var img = go.AddComponent<Image>();
            if (sprite != null) img.sprite = sprite;
            img.color = c;
            var rt = img.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.sizeDelta = size;
            return img;
        }

        TMP_Text MakeText(string n, TMP_FontAsset font, float size, Color c,
            TextAlignmentOptions align, Vector2 anchor, Vector2 sz)
        {
            var go = new GameObject(n);
            go.transform.SetParent(transform, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            if (font != null) t.font = font;
            t.fontSize = size;
            t.color = c;
            t.alignment = align;
            t.text = "";
            var rt = t.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.sizeDelta = sz;
            return t;
        }
    }
}
