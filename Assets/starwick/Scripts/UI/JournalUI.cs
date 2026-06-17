using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starwick
{
    public class JournalUI : MonoBehaviour
    {
        public bool IsOpen { get; private set; }

        TMP_Text indicator;
        TMP_Text title;
        TMP_Text list;
        Image panel;
        bool wasDown;
        int shownCount = -1;

        void Start()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Sw.Cam;
            canvas.planeDistance = 0.9f;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            gameObject.AddComponent<GraphicRaycaster>();

            var font = TMP_Settings.defaultFontAsset;

            indicator = MakeText("Indicator", font, 30f, FontStyles.Normal,
                new Color(1.9f, 1.6f, 2.3f, 1f), TextAlignmentOptions.Right,
                new Vector2(0.9f, 0.93f), new Vector2(420f, 44f));

            panel = MakeImage("Panel", new Color(0.01f, 0.0f, 0.04f, 0.88f),
                new Vector2(0.5f, 0.5f), new Vector2(1320f, 780f));

            title = MakeText("Title", font, 54f, FontStyles.Italic,
                new Color(2.1f, 1.8f, 1.1f, 1f), TextAlignmentOptions.Top,
                new Vector2(0.5f, 0.8f), new Vector2(1200f, 90f));
            title.text = "Memories";

            list = MakeText("List", font, 32f, FontStyles.Normal,
                Color.white, TextAlignmentOptions.TopLeft,
                new Vector2(0.5f, 0.44f), new Vector2(1180f, 560f));

            SetOpen(false);
        }

        void Update()
        {
            int n = GameState.Fragments.Count;
            if (n != shownCount)
            {
                shownCount = n;
                if (indicator != null) indicator.text = "✦ " + n;
                if (IsOpen) Refresh();
            }

            bool down = InputService.PointerDown;
            if (down && !wasDown)
            {
                var p = InputService.PointerPosition;
                if (p.x > Screen.width * 0.80f && p.y > Screen.height * 0.84f)
                    SetOpen(!IsOpen);
            }
            wasDown = down;
        }

        public void SetOpen(bool open)
        {
            IsOpen = open;
            if (panel != null) panel.enabled = open;
            if (title != null) title.enabled = open;
            if (list != null) list.enabled = open;
            if (open) Refresh();
        }

        void Refresh()
        {
            var sb = new StringBuilder();
            if (GameState.Fragments.Count == 0)
                sb.Append("<i>No memories recovered yet.</i>");
            else
                for (int i = 0; i < GameState.Fragments.Count; i++)
                    sb.Append("✦  ").Append(GameState.Fragments[i]).Append("\n\n");
            if (list != null) list.text = sb.ToString();
        }

        Image MakeImage(string n, Color c, Vector2 anchor, Vector2 size)
        {
            var go = new GameObject(n);
            go.transform.SetParent(transform, false);
            var img = go.AddComponent<Image>();
            img.color = c;
            var rt = img.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.sizeDelta = size;
            return img;
        }

        TMP_Text MakeText(string n, TMP_FontAsset font, float size, FontStyles style, Color c,
            TextAlignmentOptions align, Vector2 anchor, Vector2 sz)
        {
            var go = new GameObject(n);
            go.transform.SetParent(transform, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            if (font != null) t.font = font;
            t.fontSize = size;
            t.fontStyle = style;
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
