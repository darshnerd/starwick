using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starwick
{
    public class RunHUD : MonoBehaviour
    {
        public float LastSpeed;
        public float LastFlow;
        public int LastChain;
        public string LastStyle = "";
        public float LastDistance;
        public int LastRelit;
        public float LastPressure;
        public float LastProgress;
        public float LastBearing;
        public bool ResultsShown;
        public bool Faded;

        public float ThreadFill => threadFill != null ? threadFill.fillAmount : 0f;
        public float BearingX => bearingDot != null ? bearingDot.rectTransform.anchorMin.x : 0.5f;

        TMP_Text resultText;
        Image pressTint;
        Image threadTrack;
        Image threadFill;
        Image bearingDot;
        float bearingShown = 0.5f;
        float dotAlpha;

        static readonly Color ThreadWarm = new Color(1f, 0.82f, 0.5f, 0.85f);
        static readonly Color ThreadCool = new Color(0.45f, 0.5f, 0.85f, 0.85f);

        void Awake()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            pressTint = MakeImage("Pressure", null, new Color(0.01f, 0.0f, 0.03f, 0f),
                new Vector2(0.5f, 0.5f), new Vector2(1920f, 1080f));

            var dot = ProcTex.SoftDot(64);
            var dotSprite = Sprite.Create(dot, new Rect(0, 0, dot.width, dot.height), new Vector2(0.5f, 0.5f));

            threadTrack = MakeImage("ThreadTrack", null, new Color(0.3f, 0.34f, 0.5f, 0.18f),
                new Vector2(0.045f, 0.5f), new Vector2(6f, 620f));

            threadFill = MakeImage("ThreadFill", null, ThreadWarm,
                new Vector2(0.045f, 0.5f), new Vector2(6f, 620f));
            threadFill.type = Image.Type.Filled;
            threadFill.fillMethod = Image.FillMethod.Vertical;
            threadFill.fillOrigin = (int)Image.OriginVertical.Bottom;
            threadFill.fillAmount = 0f;

            bearingDot = MakeImage("Bearing", dotSprite, new Color(1f, 0.9f, 0.65f, 0f),
                new Vector2(0.5f, 0.9f), new Vector2(26f, 26f));

            resultText = MakeText("Result", Typeface.Heading, 50f,
                new Color(0.91f, 0.86f, 1f, 1f), TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(1400f, 400f));
            resultText.gameObject.SetActive(false);

            SafeArea.Apply(canvas);
        }

        public void Set(float speed, float flow, int chain, string style, float distance, int relit,
            float pressure, float progress, float bearing)
        {
            LastSpeed = speed;
            LastFlow = flow;
            LastChain = chain;
            LastStyle = style;
            LastDistance = distance;
            LastRelit = relit;
            LastPressure = pressure;
            LastProgress = Mathf.Clamp01(progress);
            LastBearing = Mathf.Clamp(bearing, -1f, 1f);

            if (threadFill != null)
            {
                threadFill.fillAmount = LastProgress;
                threadFill.color = Color.Lerp(ThreadWarm, ThreadCool, Mathf.Clamp01(pressure));
            }
            if (pressTint != null)
            {
                var c = pressTint.color;
                c.a = Mathf.Clamp01(pressure) * 0.5f;
                pressTint.color = c;
            }
            if (bearingDot != null)
            {
                float target = 0.5f + LastBearing * 0.16f;
                bearingShown = Mathf.Lerp(bearingShown, target, 0.5f);
                dotAlpha = Mathf.Lerp(dotAlpha, Mathf.Clamp01(Mathf.Abs(LastBearing) * 2.2f) * 0.7f, 0.4f);
                var rt = bearingDot.rectTransform;
                rt.anchorMin = new Vector2(bearingShown, rt.anchorMin.y);
                rt.anchorMax = new Vector2(bearingShown, rt.anchorMax.y);
                rt.anchoredPosition = Vector2.zero;
                var c = bearingDot.color;
                c.a = dotAlpha;
                bearingDot.color = c;
            }
        }

        public void ShowResults(RunResults r)
        {
            ResultsShown = true;
            if (resultText == null) return;
            resultText.gameObject.SetActive(true);
            if (r.Fragment)
                resultText.text = $"the dark drew close\n\nthe Hollow caught you\na memory remains\n+{r.Starlight} starlight";
            else
                resultText.text = $"the weave holds\n\n{r.GatesRelit} stars relit\n+{r.Starlight} starlight";
        }

        public void Fade()
        {
            Faded = true;
            if (threadTrack != null) threadTrack.gameObject.SetActive(false);
            if (threadFill != null) threadFill.gameObject.SetActive(false);
            if (bearingDot != null) bearingDot.gameObject.SetActive(false);
            if (pressTint != null) pressTint.gameObject.SetActive(false);
            if (resultText != null) resultText.gameObject.SetActive(false);
        }

        Image MakeImage(string n, Sprite sprite, Color c, Vector2 anchor, Vector2 size)
        {
            var go = new GameObject(n);
            go.transform.SetParent(transform, false);
            var img = go.AddComponent<Image>();
            if (sprite != null) img.sprite = sprite;
            img.color = c;
            img.raycastTarget = false;
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
