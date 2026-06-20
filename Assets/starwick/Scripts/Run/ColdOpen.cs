using UnityEngine;

namespace Starwick
{
    public class ColdOpen : MonoBehaviour
    {
        public enum Stage { Idle, Lit, Pulling, Opened }

        public Stage State { get; private set; } = Stage.Idle;
        public float ThreadLength { get; private set; }
        public bool AutoLaunch = true;
        public float OpenThreshold = 0.6f;
        public System.Action OnOpen;

        Transform hearth;
        Transform hollow;
        Transform wick;
        Material wickMat;
        Light wickLight;
        LineRenderer thread;
        Vector3 hollowStart;
        Vector3 hollowRest;
        float descent;
        bool wasDown;
        Vector2 lastPointer;
        float travelGlow;
        float passiveGlow;

        static readonly Color WickDim = new Color(0.25f, 0.2f, 0.12f, 1f);
        static readonly Color WickLit = new Color(2.9f, 2.1f, 1.2f, 1f);

        void Awake()
        {
            var unlit = Shader.Find("Universal Render Pipeline/Unlit");

            hearth = MakeOrb("ColdHearth", new Color(0.12f, 0.13f, 0.22f, 1f), unlit, 2.2f);
            hearth.position = new Vector3(0f, 0f, 14f);

            wick = MakeOrb("ColdWick", WickDim, unlit, 0.6f);
            wick.position = new Vector3(0f, 1.2f, 6f);
            wickMat = wick.GetComponent<MeshRenderer>().material;

            var lgo = new GameObject("ColdWickLight");
            lgo.transform.SetParent(wick, false);
            wickLight = lgo.AddComponent<Light>();
            wickLight.type = LightType.Point;
            wickLight.color = new Color(1f, 0.8f, 0.5f);
            wickLight.range = 12f;
            wickLight.intensity = 0f;

            hollow = MakeOrb("ColdHollow", Roster.Current.Glow, unlit, 0.5f);
            hollowStart = new Vector3(2.4f, 6f, 7.5f);
            hollowRest = new Vector3(2.2f, 1.8f, 7f);
            hollow.position = hollowStart;

            var tgo = new GameObject("ColdThread");
            tgo.transform.SetParent(transform, false);
            thread = tgo.AddComponent<LineRenderer>();
            thread.material = new Material(Shader.Find("Sprites/Default"));
            thread.useWorldSpace = true;
            thread.widthMultiplier = 0.12f;
            thread.numCapVertices = 3;
            thread.positionCount = 2;
            var glow = new Color(1.5f, 1.35f, 1.05f, 1f);
            thread.startColor = glow;
            thread.endColor = glow;
            thread.enabled = false;
        }

        Transform MakeOrb(string n, Color c, Shader unlit, float scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = n;
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            go.transform.SetParent(transform, false);
            go.transform.localScale = Vector3.one * scale;
            var m = new Material(unlit);
            m.color = c;
            m.SetColor("_BaseColor", c);
            go.GetComponent<MeshRenderer>().material = m;
            return go.transform;
        }

        void Update()
        {
            float dt = Mathf.Max(0.0001f, Time.deltaTime);
            descent = Mathf.MoveTowards(descent, 1f, dt * 0.6f);
            if (hollow != null) hollow.position = Vector3.Lerp(hollowStart, hollowRest, Ease.OutBack(descent));

            if (State == Stage.Opened) return;

            bool down = InputService.PointerDown;

            if (down && State == Stage.Idle)
            {
                Light();
                lastPointer = InputService.PointerPosition;
            }

            if (down && (State == Stage.Lit || State == Stage.Pulling))
            {
                Vector2 p = InputService.PointerPosition;
                float travel = (p - lastPointer).magnitude;
                lastPointer = p;
                travelGlow = Mathf.Clamp01(travelGlow + travel * 0.004f);
                passiveGlow = Mathf.Min(0.45f, passiveGlow + dt * 0.12f);
                ThreadLength = Mathf.Clamp01(travelGlow + passiveGlow);
                if (ThreadLength > 0.05f) State = Stage.Pulling;
                RefreshThread();
            }

            if (!down && wasDown && State == Stage.Pulling)
            {
                if (ThreadLength >= OpenThreshold) Open();
                else { State = Stage.Lit; ThreadLength = 0f; travelGlow = 0f; passiveGlow = 0f; RefreshThread(); }
            }

            wasDown = down;
        }

        void Light()
        {
            State = Stage.Lit;
            if (wickMat != null) { wickMat.color = WickLit; wickMat.SetColor("_BaseColor", WickLit); }
            if (wickLight != null) wickLight.intensity = 4.5f;
            if (thread != null) thread.enabled = true;
            if (Sw.Sfx != null) Sw.Sfx.Shimmer(0);
        }

        void RefreshThread()
        {
            if (thread == null || wick == null) return;
            Vector3 from = wick.position;
            Vector3 to = Vector3.Lerp(from, hearth != null ? hearth.position : from + Vector3.forward * 8f, ThreadLength);
            thread.SetPosition(0, from);
            thread.SetPosition(1, to);
        }

        void Open()
        {
            State = Stage.Opened;
            ThreadLength = 1f;
            RefreshThread();
            if (Sw.Sfx != null) Sw.Sfx.Chime();
            OnOpen?.Invoke();
            if (AutoLaunch && !Application.isBatchMode) Launch();
        }

        public static int SeedForRun(int runs)
        {
            return Roster.Current.Seed + runs * 17 + 1;
        }

        void Launch()
        {
            int runs = PlayerProfileStore.Current != null ? PlayerProfileStore.Current.TotalRuns : 0;
            int seed = SeedForRun(runs);
            if (Sw.RunSession != null) Sw.RunSession.Enter(seed);
            gameObject.SetActive(false);
        }
    }
}
