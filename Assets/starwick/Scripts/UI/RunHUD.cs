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

        TMP_Text distText;
        TMP_Text resultText;
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

            distText = MakeText("Distance", Typeface.HeadingMedium, 26f,
                new Color(0.62f, 0.6f, 0.82f, 0.7f), TextAlignmentOptions.Top,
                new Vector2(0.5f, 0.96f), new Vector2(900f, 44f));

            resultText = MakeText("Result", Typeface.Heading, 50f,
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

            if (distText != null) distText.text = $"{Mathf.RoundToInt(distance)}";
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
            if (r.Fragment)
                resultText.text = $"the dark drew close\n\nthe Hollow caught you\na memory remains\n+{r.Starlight} starlight";
            else
                resultText.text = $"the weave holds\n\n{r.GatesRelit} stars relit\n+{r.Starlight} starlight";
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
