using System.Collections.Generic;
using UnityEngine;

namespace Starwick
{
    public class Constellation : MonoBehaviour
    {
        public float TraceRadiusPixels = 70f;
        public bool FaceCamera;
        public int SiteIndex;

        public bool Complete { get; private set; }
        public int NodeCount => local.Count;
        public int TracedCount { get; private set; }

        public System.Action OnRelight;

        readonly List<Vector3> local = new List<Vector3>
        {
            new Vector3(-4f, -1.2f, 0f),
            new Vector3(-2f, 1.4f, 0f),
            new Vector3(0f, -0.4f, 0f),
            new Vector3(2f, 1.4f, 0f),
            new Vector3(4f, -1.2f, 0f),
        };

        ParticleSystem nodes;
        LineRenderer line;
        StarRelightFx fx;

        readonly Color dim = new Color(0.55f, 0.7f, 1.3f, 1f);
        readonly Color lit = new Color(2.6f, 2.1f, 1.2f, 1f);

        void Awake()
        {
            var tex = ProcTex.SoftDot(64);
            var sprite = Shader.Find("Sprites/Default");

            nodes = gameObject.AddComponent<ParticleSystem>();
            ConfigPoints(nodes, tex, sprite, 1.1f);
            EmitNodes(dim);

            line = gameObject.AddComponent<LineRenderer>();
            line.material = new Material(sprite);
            line.useWorldSpace = true;
            line.widthMultiplier = 0.05f;
            line.numCapVertices = 4;
            line.positionCount = 0;
            var lineGlow = new Color(1.5f, 1.35f, 1.05f, 1f);
            line.startColor = lineGlow;
            line.endColor = lineGlow;

            var fxGo = new GameObject("RelightFx");
            fxGo.transform.SetParent(transform, false);
            fx = fxGo.AddComponent<StarRelightFx>();
        }

        void Update()
        {
            if (FaceCamera && Sw.Cam != null)
            {
                Vector3 to = Sw.Cam.transform.position - transform.position;
                to.y = 0f;
                if (to.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.LookRotation(to.normalized, Vector3.up);
            }

            if (!Complete && !InputService.UiBlocking && Sw.Cam != null && InputService.PointerDown)
            {
                int next = TracedCount;
                if (next < local.Count)
                {
                    Vector3 sp = Sw.Cam.WorldToScreenPoint(transform.TransformPoint(local[next]));
                    if (sp.z > 0f && Vector2.Distance(InputService.PointerPosition, sp) < TraceRadiusPixels)
                        TraceNode(next);
                }
            }
            RefreshLine();
        }

        public void TraceNode(int i)
        {
            if (Complete || i != TracedCount) return;
            TracedCount++;
            if (Sw.Sfx != null) Sw.Sfx.Shimmer(TracedCount);
            if (TracedCount >= local.Count) Relight();
        }

        public void TraceAll()
        {
            while (!Complete && TracedCount < local.Count) TraceNode(TracedCount);
        }

        public void ResetForReplay()
        {
            Complete = false;
            TracedCount = 0;
            EmitNodes(dim);
            if (line != null) line.positionCount = 0;
        }

        void Relight()
        {
            Complete = true;
            EmitNodes(lit);
            GameState.StarsRelit++;
            GameState.ConstellationsComplete++;
            SaveData.RecordRelight();
            if (fx != null) fx.Play();
            if (Sw.Sfx != null) Sw.Sfx.Chime();
            OnRelight?.Invoke();
        }

        void RefreshLine()
        {
            line.positionCount = TracedCount;
            for (int i = 0; i < TracedCount; i++)
                line.SetPosition(i, transform.TransformPoint(local[i]));
        }

        void EmitNodes(Color c)
        {
            nodes.Clear();
            var ep = new ParticleSystem.EmitParams();
            ep.startColor = c;
            for (int i = 0; i < local.Count; i++)
            {
                ep.position = local[i];
                nodes.Emit(ep, 1);
            }
        }

        void ConfigPoints(ParticleSystem ps, Texture2D tex, Shader shader, float size)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 100000f;
            main.startSpeed = 0f;
            main.startSize = size;
            main.maxParticles = 64;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emission = ps.emission;
            emission.enabled = false;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(shader) { mainTexture = tex };
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }
    }
}
