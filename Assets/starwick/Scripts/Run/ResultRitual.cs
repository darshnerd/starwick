using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starwick
{
    public class ResultRitual : MonoBehaviour
    {
        public enum Stage { Hidden, Postcard, Seed, Starlight, Hearth }

        public const int RitualLayer = 29;

        public Stage State { get; private set; } = Stage.Hidden;
        public string SeedCode { get; private set; } = "";
        public HearthView Hearth { get; private set; }
        public Camera RitualCam { get; private set; }
        public Vector3 HearthCenter { get; private set; }
        public bool Skippable => active && State != Stage.Hearth;

        RunResults results;
        RawImage postcardImage;
        TMP_Text seedText;
        TMP_Text titleText;
        TMP_Text continueText;
        float t;
        bool active;
        bool wasDown;

        public void Begin(RunResults r, Texture2D postcard, int companionIndex, int seed)
        {
            results = r;
            SeedCode = MemorySeed.Encode(companionIndex, seed);
            BuildOverlay(postcard);
            State = Stage.Postcard;
            t = 0f;
            active = true;
        }

        void BuildOverlay(Texture2D postcard)
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var pcGo = new GameObject("Postcard");
            pcGo.transform.SetParent(transform, false);
            postcardImage = pcGo.AddComponent<RawImage>();
            postcardImage.texture = postcard;
            var prt = postcardImage.rectTransform;
            prt.anchorMin = new Vector2(0.5f, 0.56f);
            prt.anchorMax = new Vector2(0.5f, 0.56f);
            prt.sizeDelta = new Vector2(960f, 540f);

            titleText = MakeText("RitualTitle", Typeface.Heading, 48f, new Color(0.92f, 0.88f, 1f, 1f),
                new Vector2(0.5f, 0.86f), new Vector2(1400f, 80f));
            titleText.text = "the weave holds";

            seedText = MakeText("SeedText", Typeface.HeadingMedium, 34f, new Color(0.75f, 0.82f, 1f, 1f),
                new Vector2(0.5f, 0.2f), new Vector2(1200f, 60f));
            seedText.text = "";

            continueText = MakeText("Continue", Typeface.HeadingMedium, 24f, new Color(0.6f, 0.62f, 0.8f, 0.7f),
                new Vector2(0.5f, 0.08f), new Vector2(900f, 44f));
            continueText.text = "tap to continue";

            SafeArea.Apply(canvas);
        }

        TMP_Text MakeText(string n, TMP_FontAsset font, float size, Color c, Vector2 anchor, Vector2 sz)
        {
            var go = new GameObject(n);
            go.transform.SetParent(transform, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            if (font != null) t.font = font;
            t.fontSize = size;
            t.color = c;
            t.alignment = TextAlignmentOptions.Center;
            t.text = "";
            var rt = t.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.sizeDelta = sz;
            return t;
        }

        public void Skip()
        {
            if (Skippable) t += 5f;
        }

        public void Tick(float dt)
        {
            if (!active) return;
            t += dt;

            if (State == Stage.Postcard && t > 1.2f)
            {
                State = Stage.Seed;
                t = 0f;
                if (seedText != null) seedText.text = "memory seed   " + SeedCode;
            }
            else if (State == Stage.Seed && t > 1.0f)
            {
                State = Stage.Starlight;
                t = 0f;
                StartStarlight();
            }
            else if (State == Stage.Starlight && t > 1.5f)
            {
                State = Stage.Hearth;
                t = 0f;
                if (titleText != null) titleText.text = "starlight returns to the hearth";
                if (continueText != null) continueText.text = "";
            }
        }

        void StartStarlight()
        {
            var basePos = new Vector3(0f, -40f, 0f);
            var go = new GameObject("RitualHearth");
            go.transform.SetParent(transform.parent, false);
            go.transform.position = basePos;
            Hearth = go.AddComponent<HearthView>();
            Hearth.Build();
            Hearth.React(results);
            SetLayer(go.transform, RitualLayer);

            HearthCenter = basePos + new Vector3(0f, 1.5f, 0f);
            var camGo = new GameObject("RitualCam");
            camGo.transform.SetParent(transform, false);
            RitualCam = camGo.AddComponent<Camera>();
            RitualCam.clearFlags = CameraClearFlags.SolidColor;
            RitualCam.backgroundColor = new Color(0.02f, 0.02f, 0.05f);
            RitualCam.cullingMask = 1 << RitualLayer;
            RitualCam.farClipPlane = 200f;
            RitualCam.fieldOfView = 48f;
            RitualCam.depth = 10f;
            RitualCam.transform.position = basePos + new Vector3(0f, 5.5f, -16f);
            RitualCam.transform.LookAt(HearthCenter);
        }

        static void SetLayer(Transform tr, int layer)
        {
            tr.gameObject.layer = layer;
            for (int i = 0; i < tr.childCount; i++) SetLayer(tr.GetChild(i), layer);
        }

        void Update()
        {
            bool down = InputService.PointerDown;
            if (down && !wasDown) Skip();
            wasDown = down;
            Tick(Time.deltaTime);
        }
    }
}
