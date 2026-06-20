using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starwick
{
    public class TitleUI : MonoBehaviour
    {
        public bool Visible { get; private set; }

        CanvasGroup group;
        TMP_Text hint;
        bool dismissing;
        bool wasDown;
        float t;

        void Start()
        {
            Sw.Title = this;

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            gameObject.AddComponent<GraphicRaycaster>();
            group = gameObject.AddComponent<CanvasGroup>();

            MakeImage("Veil", new Color(0f, 0f, 0.02f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(4000f, 4000f));

            var title = MakeText("Title", Typeface.Heading, 150f, FontStyles.Normal,
                new Color(0.76f, 0.72f, 1f, 1f), new Vector2(0.5f, 0.58f), new Vector2(1700f, 240f));
            title.text = "Starwick";

            var tag = MakeText("Tagline", Typeface.Body, 38f, FontStyles.Italic,
                new Color(0.80f, 0.83f, 1f, 1f), new Vector2(0.5f, 0.47f), new Vector2(1400f, 80f));
            tag.text = "relight the dying sky";

            hint = MakeText("Hint", Typeface.BodyMedium, 40f, FontStyles.Normal,
                new Color(0.82f, 0.82f, 1f, 1f), new Vector2(0.5f, 0.32f), new Vector2(1200f, 80f));
            hint.text = "tap to begin";

            SafeArea.Apply(canvas);
            if (Application.isBatchMode) Hide();
            else Show();
        }

        public void Show()
        {
            Visible = true;
            dismissing = false;
            if (group != null) group.alpha = 1f;
        }

        public void Dismiss()
        {
            if (Visible) dismissing = true;
        }

        void Hide()
        {
            Visible = false;
            dismissing = false;
            if (group != null) group.alpha = 0f;
        }

        void Update()
        {
            t += Time.deltaTime;
            if (!Visible) return;

            if (hint != null)
            {
                var c = hint.color;
                c.a = 0.45f + 0.55f * Mathf.Abs(Mathf.Sin(t * 1.6f));
                hint.color = c;
            }

            if (!dismissing)
            {
                bool down = InputService.PointerDown;
                if (down && !wasDown) dismissing = true;
                wasDown = down;
            }
            else if (group != null)
            {
                group.alpha = Mathf.MoveTowards(group.alpha, 0f, Time.deltaTime * 1.6f);
                if (group.alpha <= 0.001f) Hide();
            }
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
            Vector2 anchor, Vector2 sz)
        {
            var go = new GameObject(n);
            go.transform.SetParent(transform, false);
            var tx = go.AddComponent<TextMeshProUGUI>();
            if (font != null) tx.font = font;
            tx.fontSize = size;
            tx.fontStyle = style;
            tx.color = c;
            tx.alignment = TextAlignmentOptions.Center;
            tx.text = "";
            var rt = tx.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.sizeDelta = sz;
            return tx;
        }
    }
}
