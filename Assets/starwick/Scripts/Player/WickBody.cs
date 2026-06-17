using System.Collections.Generic;
using UnityEngine;

namespace Starwick
{
    public class WickBody : MonoBehaviour
    {
        public Color EmberColor = new Color(2.9f, 1.8f, 0.8f, 1f);
        public Color RingWarm = new Color(2.4f, 1.6f, 0.7f, 1f);
        public Color RingCool = new Color(1.0f, 0.8f, 2.4f, 1f);

        Transform body;
        Transform ember;
        Light flame;
        readonly List<Transform> rings = new List<Transform>();
        readonly List<Vector3> ringSpins = new List<Vector3>();

        Spring hover;
        Spring3 lean;
        Spring pulse;

        Vector3 lastCamPos;
        float relightT = -2f;
        float vis = 1f;

        void Awake()
        {
            var unlit = Shader.Find("Universal Render Pipeline/Unlit");

            body = new GameObject("Body").transform;
            body.SetParent(transform, false);

            var emberGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            emberGo.name = "Ember";
            var ec = emberGo.GetComponent<Collider>();
            if (ec != null) Destroy(ec);
            emberGo.transform.SetParent(body, false);
            emberGo.transform.localScale = Vector3.one * 0.34f;
            var em = new Material(unlit);
            em.color = EmberColor;
            em.SetColor("_BaseColor", EmberColor);
            emberGo.GetComponent<MeshRenderer>().material = em;
            ember = emberGo.transform;

            flame = emberGo.AddComponent<Light>();
            flame.type = LightType.Point;
            flame.color = new Color(1f, 0.78f, 0.45f);
            flame.range = 9f;
            flame.intensity = 5f;

            AddRing(ProcMesh.Torus(0.6f, 0.035f, 48, 8), unlit, RingWarm, new Vector3(0f, 0f, 0f), new Vector3(0f, 50f, 0f));
            AddRing(ProcMesh.Torus(0.78f, 0.03f, 48, 8), unlit, RingCool, new Vector3(64f, 0f, 28f), new Vector3(30f, 0f, 18f));
            AddRing(ProcMesh.Torus(0.5f, 0.03f, 48, 8), unlit, RingWarm, new Vector3(20f, 0f, 75f), new Vector3(0f, 40f, 24f));

            lastCamPos = transform.position;
        }

        void Start()
        {
            if (Sw.Constellation != null)
                Sw.Constellation.OnRelight += RelightPose;
        }

        public void RelightPose()
        {
            relightT = Time.time;
        }

        void AddRing(Mesh mesh, Shader shader, Color color, Vector3 tilt, Vector3 spin)
        {
            var go = new GameObject("Ring");
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            var mat = new Material(shader);
            mat.color = color;
            mat.SetColor("_BaseColor", color);
            mr.material = mat;
            go.transform.SetParent(body, false);
            go.transform.localRotation = Quaternion.Euler(tilt);
            rings.Add(go.transform);
            ringSpins.Add(spin);
        }

        void Update()
        {
            float t = Time.time;
            float dt = Mathf.Max(0.0001f, Time.deltaTime);

            hover.Step(Mathf.Sin(t * 1.3f) * 0.07f, 90f, 14f, dt);

            Vector3 camPos = transform.parent != null ? transform.parent.position : transform.position;
            Vector3 vel = (camPos - lastCamPos) / dt;
            lastCamPos = camPos;
            Vector3 localVel = Sw.Cam != null ? Sw.Cam.transform.InverseTransformDirection(vel) : vel;
            Vector3 leanTarget = Vector3.ClampMagnitude(new Vector3(-localVel.x, 0f, -localVel.z) * 0.04f, 0.5f);
            lean.Step(leanTarget, 60f, 12f, dt);

            float visTarget = InputService.UiBlocking ? 0f : 1f;
            vis = Mathf.MoveTowards(vis, visTarget, dt * 4f);

            body.localPosition = new Vector3(lean.value.x, hover.value + lean.value.y, lean.value.z);
            body.localRotation = Quaternion.Euler(lean.value.z * 40f, 0f, -lean.value.x * 40f);
            body.localScale = Vector3.one * vis;

            float flare = 0f;
            if (relightT > -1f)
            {
                float e = (t - relightT) / 1.2f;
                if (e >= 1f) relightT = -2f;
                else flare = Mathf.Sin(Ease.OutCubic(Mathf.Clamp01(e)) * Mathf.PI);
            }

            pulse.Step(1f + Mathf.Sin(t * 5f) * 0.04f + flare * 0.6f, 120f, 16f, dt);
            if (ember != null) ember.localScale = Vector3.one * 0.34f * pulse.value;
            if (flame != null) flame.intensity = (5f + Mathf.Sin(t * 11f) * 0.6f + flare * 6f) * vis;

            for (int i = 0; i < rings.Count; i++)
            {
                float s = 1f + flare * 0.4f;
                rings[i].Rotate(ringSpins[i] * dt, Space.Self);
                rings[i].localScale = Vector3.one * s;
            }
        }
    }
}
