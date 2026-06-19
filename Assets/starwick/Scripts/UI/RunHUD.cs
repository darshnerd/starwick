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
        public bool ResultsShown;

        TMP_Text comboText;
        TMP_Text styleText;
        TMP_Text distText;
        TMP_Text resultText;
        Image flowFill;
        Image pressTint;

        void Awake()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            pressTint = MakeImage("Pressure", new Color(0.01f, 0.0f, 0.03f, 0f),
                new Vector2(0.5f, 0.5f), new Vector2(1920f, 1080f));

            distText = MakeText("Distance", Typeface.HeadingMedium, 34f,
                new Color(0.76f, 0.71f, 1f, 1f), TextAlignmentOptions.TopLeft,
                new Vector2(0.5f, 0.95f), new Vector2(1700f, 60f));

            MakeImage("FlowBg", new Color(0.1f, 0.12f, 0.2f, 0.7f),
                new Vector2(0.5f, 0.07f), new Vector2(620f, 26f));
            flowFill = MakeImage("FlowFill", new Color(0.7f, 0.55f, 1f, 1f),
                new Vector2(0.5f, 0.07f), new Vector2(620f, 26f));
            flowFill.type = Image.Type.Filled;
            flowFill.fillMethod = Image.FillMethod.Horizontal;
            flowFill.fillAmount = 0f;

            comboText = MakeText("Combo", Typeface.Heading, 64f,
                new Color(1f, 0.86f, 0.55f, 1f), TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.14f), new Vector2(800f, 80f));

            styleText = MakeText("Style", Typeface.BodyMedium, 36f,
                new Color(0.82f, 0.73f, 1f, 1f), TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.1f), new Vector2(800f, 50f));

            resultText = MakeText("Result", Typeface.Heading, 52f,
                new Color(0.91f, 0.86f, 1f, 1f), TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(1400f, 400f));
            resultText.gameObject.SetActive(false);
        }

        public void Set(float speed, float flow, int chain, string style, float distance, int relit, float pressure)
        {
            LastSpeed = speed;
            LastFlow = flow;
            LastChain = chain;
            LastStyle = style;
            LastDistance = distance;
            LastRelit = relit;
            LastPressure = pressure;

            if (flowFill != null) flowFill.fillAmount = Mathf.Clamp01(flow);
            if (comboText != null) comboText.text = chain > 1 ? $"x{chain}" : "";
            if (styleText != null) styleText.text = style;
            if (distText != null) distText.text = $"{Mathf.RoundToInt(distance)} m   ·   {relit} relit";
            if (pressTint != null)
            {
                var c = pressTint.color;
                c.a = Mathf.Clamp01(pressure) * 0.5f;
                pressTint.color = c;
            }
        }

        public void ShowResults(RunResults r)
        {
            ResultsShown = true;
            if (resultText == null) return;
            resultText.gameObject.SetActive(true);
            resultText.text = $"the weave holds\n\n{r.GatesRelit} stars relit\nbest weave  x{r.BestChain}\n+{r.Starlight} starlight";
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
