using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starwick
{
    public class NarrationUI : MonoBehaviour
    {
        TMP_Text body;
        TMP_Text speaker;
        Image backdrop;
        string full = "";
        float revealChars;
        bool wasDown;

        Image choiceBackdrop;
        Image choiceBoxA;
        Image choiceBoxB;
        TMP_Text choicePrompt;
        TMP_Text choiceA;
        TMP_Text choiceB;
        bool choiceActive;
        bool singleButton;
        float choiceReveal;
        System.Action<int> onChoice;

        public bool ChoiceActive => choiceActive;
        public float ChoiceReveal => choiceReveal;
        public bool BodyShown => body != null && body.enabled;
        public string BodyContent => body != null ? body.text : "";
        public float MaxBodyColorChannel => body != null ? Mathf.Max(body.color.r, body.color.g, body.color.b) : 0f;

        void Start()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            gameObject.AddComponent<GraphicRaycaster>();

            backdrop = MakeImage("Backdrop", new Color(0f, 0f, 0.02f, 0.88f),
                new Vector2(0.5f, 0.17f), new Vector2(1500f, 260f));

            speaker = MakeText("Speaker", Typeface.HeadingMedium, 32f, FontStyles.Normal,
                new Color(0.77f, 0.68f, 1f, 1f), TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.27f), new Vector2(1400f, 50f));

            body = MakeText("Body", Typeface.Body, 38f, FontStyles.Normal,
                new Color(0.93f, 0.93f, 1f, 1f), TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.155f), new Vector2(1400f, 190f));

            choiceBackdrop = MakeImage("ChoiceBackdrop", new Color(0f, 0f, 0.02f, 0.92f),
                new Vector2(0.5f, 0.49f), new Vector2(1480f, 440f));

            choicePrompt = MakeText("ChoicePrompt", Typeface.Heading, 44f, FontStyles.Italic,
                new Color(0.77f, 0.70f, 1f, 1f), TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.63f), new Vector2(1320f, 150f));

            choiceBoxA = MakeImage("ChoiceBoxA", new Color(0.12f, 0.14f, 0.3f, 0.88f),
                new Vector2(0.3f, 0.41f), new Vector2(540f, 120f));
            choiceA = MakeText("ChoiceA", Typeface.BodyMedium, 36f, FontStyles.Normal,
                Color.white, TextAlignmentOptions.Center,
                new Vector2(0.3f, 0.41f), new Vector2(500f, 110f));

            choiceBoxB = MakeImage("ChoiceBoxB", new Color(0.12f, 0.14f, 0.3f, 0.88f),
                new Vector2(0.7f, 0.41f), new Vector2(540f, 120f));
            choiceB = MakeText("ChoiceB", Typeface.BodyMedium, 36f, FontStyles.Normal,
                Color.white, TextAlignmentOptions.Center,
                new Vector2(0.7f, 0.41f), new Vector2(500f, 110f));

            SafeArea.Apply(canvas);
            SetVisible(false);
            SetChoiceVisible(false);

            if (Sw.Dialogue != null)
            {
                Sw.Dialogue.OnLine += ShowLine;
                Sw.Dialogue.OnEnd += HideAll;
            }
        }

        void ShowLine(DialogueLine line)
        {
            SetVisible(true);
            speaker.text = line != null ? line.Speaker : "";
            full = line != null ? line.Text : "";
            body.text = full;
            revealChars = 0f;
            body.maxVisibleCharacters = 0;
        }

        void HideAll()
        {
            SetVisible(false);
        }

        public void ShowChoices(string prompt, string a, string b, System.Action<int> cb)
        {
            LayoutButtons(false);
            choicePrompt.text = prompt;
            choiceA.text = a;
            choiceB.text = b;
            onChoice = cb;
            choiceActive = true;
            choiceReveal = 0f;
            SetChoiceVisible(true);
            ApplyChoiceScale();
        }

        public void ShowPrompt(string prompt, string label, System.Action cb)
        {
            LayoutButtons(true);
            choicePrompt.text = prompt;
            choiceA.text = label;
            onChoice = i => { if (i == 1) cb?.Invoke(); };
            choiceActive = true;
            choiceReveal = 0f;
            SetChoiceVisible(true);
            ApplyChoiceScale();
        }

        public void HideChoices()
        {
            choiceActive = false;
            onChoice = null;
            SetChoiceVisible(false);
        }

        void Update()
        {
            if (body != null && body.enabled && revealChars < full.Length)
            {
                revealChars += Time.deltaTime * 45f;
                body.maxVisibleCharacters = Mathf.Clamp((int)revealChars, 0, full.Length);
            }

            if (choiceActive)
            {
                choiceReveal = Mathf.MoveTowards(choiceReveal, 1f, Time.deltaTime * 4f);
                ApplyChoiceScale();
            }

            bool down = InputService.PointerDown;
            if (down && !wasDown)
            {
                if (Sw.Dialogue != null && Sw.Dialogue.Active)
                {
                    if (revealChars < full.Length)
                    {
                        revealChars = full.Length;
                        body.maxVisibleCharacters = full.Length;
                    }
                    else
                    {
                        Sw.Dialogue.Advance();
                    }
                }
                else if (choiceActive)
                {
                    Vector2 p = InputService.PointerPosition;
                    int side = 0;
                    if (choiceBoxA != null && choiceBoxA.enabled &&
                        RectTransformUtility.RectangleContainsScreenPoint(choiceBoxA.rectTransform, p, null))
                        side = 1;
                    else if (choiceBoxB != null && choiceBoxB.enabled &&
                        RectTransformUtility.RectangleContainsScreenPoint(choiceBoxB.rectTransform, p, null))
                        side = 2;

                    if (side != 0)
                    {
                        var cb = onChoice;
                        if (Sw.Sfx != null) Sw.Sfx.Confirm();
                        HideChoices();
                        cb?.Invoke(side);
                    }
                }
            }
            wasDown = down;
        }

        void ApplyChoiceScale()
        {
            float s = Mathf.Lerp(0.82f, 1f, Ease.OutBack(choiceReveal));
            var sc = Vector3.one * s;
            if (choicePrompt != null) choicePrompt.rectTransform.localScale = sc;
            if (choiceBoxA != null) choiceBoxA.rectTransform.localScale = sc;
            if (choiceA != null) choiceA.rectTransform.localScale = sc;
            if (choiceBoxB != null) choiceBoxB.rectTransform.localScale = sc;
            if (choiceB != null) choiceB.rectTransform.localScale = sc;
        }

        void LayoutButtons(bool single)
        {
            singleButton = single;
            float ax = single ? 0.5f : 0.3f;
            SetAnchorX(choiceBoxA.rectTransform, ax);
            SetAnchorX(choiceA.rectTransform, ax);
        }

        void SetAnchorX(RectTransform rt, float x)
        {
            rt.anchorMin = new Vector2(x, rt.anchorMin.y);
            rt.anchorMax = new Vector2(x, rt.anchorMax.y);
            rt.anchoredPosition = Vector2.zero;
        }

        void SetVisible(bool v)
        {
            if (backdrop != null) backdrop.enabled = v;
            if (speaker != null) speaker.enabled = v;
            if (body != null) body.enabled = v;
        }

        void SetChoiceVisible(bool v)
        {
            bool two = v && !singleButton;
            if (choiceBackdrop != null) choiceBackdrop.enabled = v;
            if (choicePrompt != null) choicePrompt.enabled = v;
            if (choiceBoxA != null) choiceBoxA.enabled = v;
            if (choiceA != null) choiceA.enabled = v;
            if (choiceBoxB != null) choiceBoxB.enabled = two;
            if (choiceB != null) choiceB.enabled = two;
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
