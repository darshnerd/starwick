using System.Collections.Generic;
using UnityEngine;

namespace Starwick
{
    public class RealmSite : MonoBehaviour
    {
        public int Index { get; private set; }
        public bool Lit { get; private set; }
        public bool IsNext { get; private set; }
        public float Warmth { get; private set; }
        public Vector3 Center { get; private set; }

        readonly List<Material> crystals = new List<Material>();
        Transform beacon;
        Material beaconMat;
        Light beaconLight;
        float warmthTarget;
        float beaconTarget = 0.22f;
        float beaconValue;

        static readonly Color Cool = new Color(0.45f, 0.6f, 1.5f, 1f);
        static readonly Color Warm = new Color(2.4f, 1.7f, 0.8f, 1f);
        static readonly Color BeaconHue = new Color(0.7f, 0.9f, 1.6f, 1f);

        public void Build(Vector3 center, int index)
        {
            Center = center;
            Index = index;
            var unlit = Shader.Find("Universal Render Pipeline/Unlit");

            int count = 7;
            for (int k = 0; k < count; k++)
            {
                float ang = (k / (float)count) * Mathf.PI * 2f + index * 1.3f;
                float rr = 1.6f + Mathf.Abs(Mathf.Sin(ang * 1.7f + index)) * 2.6f;
                float cx = center.x + Mathf.Cos(ang) * rr;
                float cz = center.z + Mathf.Sin(ang) * rr;
                float h = Sw.Realm != null ? Sw.Realm.Height(cx, cz) : center.y;

                float tall = 1.8f + Mathf.Abs(Mathf.Sin(ang * 2.3f + index * 0.7f)) * 3.4f;
                float wide = 0.32f + (tall * 0.07f);
                float lean = 6f + Mathf.Abs(Mathf.Cos(ang * 3.1f)) * 12f;

                var c = new GameObject("Crystal");
                c.transform.SetParent(transform, false);
                c.transform.position = new Vector3(cx, h + tall * 0.6f, cz);
                c.transform.localScale = new Vector3(wide, tall, wide * 0.82f);
                c.transform.rotation = Quaternion.Euler(Mathf.Cos(ang) * lean, ang * Mathf.Rad2Deg, Mathf.Sin(ang) * lean);
                c.AddComponent<MeshFilter>().sharedMesh = ProcMesh.Octahedron(1f);
                var mr = c.AddComponent<MeshRenderer>();
                var m = new Material(unlit);
                m.color = Cool;
                m.SetColor("_BaseColor", Cool);
                mr.material = m;
                crystals.Add(m);
            }

            var beaconGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beaconGo.name = "Beacon";
            var bcol = beaconGo.GetComponent<Collider>();
            if (bcol != null) Destroy(bcol);
            beaconGo.transform.SetParent(transform, false);
            beaconGo.transform.position = new Vector3(center.x, center.y + 7f, center.z);
            beaconGo.transform.localScale = new Vector3(0.16f, 7f, 0.16f);
            beaconMat = new Material(unlit);
            beaconMat.color = BeaconHue;
            beaconMat.SetColor("_BaseColor", BeaconHue);
            beaconGo.GetComponent<MeshRenderer>().material = beaconMat;
            beacon = beaconGo.transform;

            var lgo = new GameObject("BeaconLight");
            lgo.transform.SetParent(transform, false);
            lgo.transform.position = new Vector3(center.x, center.y + 3f, center.z);
            beaconLight = lgo.AddComponent<Light>();
            beaconLight.type = LightType.Point;
            beaconLight.color = new Color(0.7f, 0.85f, 1.5f);
            beaconLight.range = 16f;
            beaconLight.intensity = 0f;
        }

        public void MarkNext()
        {
            IsNext = true;
            beaconTarget = 1f;
        }

        public void MarkDormant()
        {
            IsNext = false;
            beaconTarget = 0.22f;
        }

        public void Light()
        {
            Lit = true;
            IsNext = false;
            warmthTarget = 1f;
            beaconTarget = 0f;
        }

        public void ResetState()
        {
            Lit = false;
            IsNext = false;
            Warmth = 0f;
            warmthTarget = 0f;
            beaconValue = 0f;
            beaconTarget = 0.22f;
        }

        void Update()
        {
            float dt = Mathf.Max(0.0001f, Time.deltaTime);

            Warmth = Mathf.MoveTowards(Warmth, warmthTarget, dt * 0.9f);
            var cc = Color.Lerp(Cool, Warm, Warmth);
            for (int i = 0; i < crystals.Count; i++)
            {
                crystals[i].color = cc;
                crystals[i].SetColor("_BaseColor", cc);
            }

            beaconValue = Mathf.MoveTowards(beaconValue, beaconTarget, dt * 1.4f);
            float pulse = beaconValue * (0.75f + 0.25f * Mathf.Sin(Time.time * 3f));
            if (beacon != null) beacon.gameObject.SetActive(beaconValue > 0.02f);
            if (beaconMat != null)
            {
                var bc = BeaconHue * (0.4f + pulse * 1.6f);
                bc.a = 1f;
                beaconMat.color = bc;
                beaconMat.SetColor("_BaseColor", bc);
            }
            if (beaconLight != null) beaconLight.intensity = pulse * 7f;
        }
    }
}
