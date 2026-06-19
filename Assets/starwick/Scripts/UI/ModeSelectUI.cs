using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Starwick
{
    public class ModeSelectUI : MonoBehaviour
    {
        Image wanderBox;
        Image flowBox;
        CanvasGroup group;
        RunLauncher launcher;
        bool dismissed;

        void Start()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            group = gameObject.AddComponent<CanvasGroup>();

            MakeImage("Veil", new Color(0f, 0f, 0.02f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(1920f, 1080f));

            MakeText("Prompt", Typeface.Heading, 48f, new Color(1.8f, 1.7f, 2.3f, 1f),
                new Vector2(0.5f, 0.66f), new Vector2(1400f, 120f), "how will you tend the sky?");

            wanderBox = MakeImage("WanderBox", new Color(0.12f, 0.14f, 0.3f, 0.9f),
                new Vector2(0.32f, 0.45f), new Vector2(540f, 150f));
            MakeText("WanderLabel", Typeface.BodyMedium, 40f, Color.white,
                new Vector2(0.32f, 0.45f), new Vector2(500f, 120f), "wander");

            flowBox = MakeImage("FlowBox", new Color(0.24f, 0.16f, 0.3f, 0.9f),
                new Vector2(0.68f, 0.45f), new Vector2(540f, 150f));
            MakeText("FlowLabel", Typeface.BodyMedium, 40f, Color.white,
                new Vector2(0.68f, 0.45f), new Vector2(500f, 120f), "flow run");

            SetVisible(false);
        }

        void Update()
        {
            if (dismissed) return;

            bool titleUp = Sw.Title != null && Sw.Title.Visible;
            SetVisible(!titleUp);
            if (titleUp) return;

            var p = Pointer.current;
            if (p == null || !p.press.wasPressedThisFrame) return;
            Vector2 pos = p.position.ReadValue();

            if (RectTransformUtility.RectangleContainsScreenPoint(flowBox.rectTransform, pos, null))
            {
                StartFlow();
            }
            else if (RectTransformUtility.RectangleContainsScreenPoint(wanderBox.rectTransform, pos, null))
            {
                dismissed = true;
                SetVisible(false);
            }
        }

        void StartFlow()
        {
            dismissed = true;
            SetVisible(false);
            var go = new GameObject("RunLauncher");
            go.transform.SetParent(transform.parent, false);
            launcher = go.AddComponent<RunLauncher>();
            launcher.EnterRun(Roster.Current.Seed);
        }

        void SetVisible(bool v)
        {
            if (group == null) return;
            group.alpha = v ? 1f : 0f;
            group.blocksRaycasts = v;
            group.interactable = v;
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

        TMP_Text MakeText(string n, TMP_FontAsset font, float size, Color c,
            Vector2 anchor, Vector2 sz, string text)
        {
            var go = new GameObject(n);
            go.transform.SetParent(transform, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            if (font != null) t.font = font;
            t.fontSize = size;
            t.color = c;
            t.alignment = TextAlignmentOptions.Center;
            t.text = text;
            var rt = t.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.sizeDelta = sz;
            return t;
        }
    }
}
