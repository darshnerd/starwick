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
        System.Action<int> onChoice;

        public bool ChoiceActive => choiceActive;

        void Start()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Sw.Cam;
            canvas.planeDistance = 1f;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            gameObject.AddComponent<GraphicRaycaster>();

            var font = TMP_Settings.defaultFontAsset;

            backdrop = MakeImage("Backdrop", new Color(0f, 0f, 0.02f, 0.55f),
                new Vector2(0.5f, 0.17f), new Vector2(1500f, 260f));

            speaker = MakeText("Speaker", font, 34f, FontStyles.Italic,
                new Color(1.7f, 1.5f, 2.2f, 1f), TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.27f), new Vector2(1400f, 50f));

            body = MakeText("Body", font, 40f, FontStyles.Normal,
                Color.white, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.155f), new Vector2(1400f, 190f));

            choiceBackdrop = MakeImage("ChoiceBackdrop", new Color(0f, 0f, 0.02f, 0.62f),
                new Vector2(0.5f, 0.49f), new Vector2(1480f, 440f));

            choicePrompt = MakeText("ChoicePrompt", font, 44f, FontStyles.Italic,
                new Color(1.7f, 1.55f, 2.2f, 1f), TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.63f), new Vector2(1320f, 150f));

            choiceBoxA = MakeImage("ChoiceBoxA", new Color(0.12f, 0.14f, 0.3f, 0.88f),
                new Vector2(0.3f, 0.41f), new Vector2(540f, 120f));
            choiceA = MakeText("ChoiceA", font, 36f, FontStyles.Normal,
                Color.white, TextAlignmentOptions.Center,
                new Vector2(0.3f, 0.41f), new Vector2(500f, 110f));

            choiceBoxB = MakeImage("ChoiceBoxB", new Color(0.12f, 0.14f, 0.3f, 0.88f),
                new Vector2(0.7f, 0.41f), new Vector2(540f, 120f));
            choiceB = MakeText("ChoiceB", font, 36f, FontStyles.Normal,
                Color.white, TextAlignmentOptions.Center,
                new Vector2(0.7f, 0.41f), new Vector2(500f, 110f));

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
            choicePrompt.text = prompt;
            choiceA.text = a;
            choiceB.text = b;
            onChoice = cb;
            choiceActive = true;
            SetChoiceVisible(true);
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
                    var cb = onChoice;
                    int side = InputService.PointerPosition.x < Screen.width * 0.5f ? 1 : 2;
                    HideChoices();
                    cb?.Invoke(side);
                }
            }
            wasDown = down;
        }

        void SetVisible(bool v)
        {
            if (backdrop != null) backdrop.enabled = v;
            if (speaker != null) speaker.enabled = v;
            if (body != null) body.enabled = v;
        }

        void SetChoiceVisible(bool v)
        {
            if (choiceBackdrop != null) choiceBackdrop.enabled = v;
            if (choiceBoxA != null) choiceBoxA.enabled = v;
            if (choiceBoxB != null) choiceBoxB.enabled = v;
            if (choicePrompt != null) choicePrompt.enabled = v;
            if (choiceA != null) choiceA.enabled = v;
            if (choiceB != null) choiceB.enabled = v;
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
