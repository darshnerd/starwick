using System.Collections.Generic;
using UnityEngine;

namespace Starwick
{
    public class WickLantern : MonoBehaviour
    {
        public Color Ember = new Color(2.8f, 1.7f, 0.7f, 1f);
        public Color RingWarm = new Color(2.2f, 1.4f, 0.6f, 1f);
        public Color RingCool = new Color(1.2f, 0.9f, 2.4f, 1f);

        Transform core;
        Light flame;
        Vector3 home;
        float baseIntensity = 5f;

        readonly List<Transform> rings = new List<Transform>();
        readonly List<Vector3> spins = new List<Vector3>();

        void Awake()
        {
            home = transform.localPosition;
            var unlit = Shader.Find("Universal Render Pipeline/Unlit");

            var coreGo = MakeBall("Ember", 0.34f, Ember, unlit);
            coreGo.transform.SetParent(transform, false);
            core = coreGo.transform;

            flame = coreGo.AddComponent<Light>();
            flame.type = LightType.Point;
            flame.color = new Color(1f, 0.78f, 0.45f);
            flame.range = 9f;
            flame.intensity = baseIntensity;

            BuildRing(10, 0.62f, RingWarm, new Vector3(0f, 0f, 0f), unlit, new Vector3(0f, 55f, 0f));
            BuildRing(12, 0.78f, RingCool, new Vector3(70f, 0f, 20f), unlit, new Vector3(30f, 0f, 40f));
            BuildRing(8, 0.5f, RingWarm, new Vector3(20f, 0f, 75f), unlit, new Vector3(0f, 60f, 25f));
        }

        void BuildRing(int count, float radius, Color color, Vector3 tilt, Shader shader, Vector3 spin)
        {
            var ring = new GameObject("Ring").transform;
            ring.SetParent(transform, false);
            ring.localRotation = Quaternion.Euler(tilt);
            for (int i = 0; i < count; i++)
            {
                float a = (i / (float)count) * Mathf.PI * 2f;
                var bead = MakeBall("Bead", 0.1f, color, shader);
                bead.transform.SetParent(ring, false);
                bead.transform.localPosition = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
            }
            rings.Add(ring);
            spins.Add(spin);
        }

        GameObject MakeBall(string ballName, float scale, Color color, Shader shader)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = ballName;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            go.transform.localScale = Vector3.one * scale;
            var mat = new Material(shader);
            mat.color = color;
            mat.SetColor("_BaseColor", color);
            go.GetComponent<MeshRenderer>().material = mat;
            return go;
        }

        void Update()
        {
            float t = Time.time;
            transform.localPosition = home + new Vector3(Mathf.Sin(t * 1.1f) * 0.05f, Mathf.Sin(t * 1.6f) * 0.07f, 0f);
            transform.localRotation = Quaternion.Euler(0f, Mathf.Sin(t * 0.4f) * 6f, Mathf.Sin(t * 0.7f) * 3f);

            for (int i = 0; i < rings.Count; i++)
                rings[i].Rotate(spins[i] * Time.deltaTime, Space.Self);

            if (core != null) core.localScale = Vector3.one * (0.34f + Mathf.Sin(t * 6f) * 0.03f);
            if (flame != null) flame.intensity = baseIntensity + Mathf.Sin(t * 11f) * 0.7f + Mathf.Sin(t * 27f) * 0.3f;
        }
    }
}
