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
        public float Pop => pop.value;

        readonly List<Material> crystals = new List<Material>();
        readonly List<Transform> crystalT = new List<Transform>();
        readonly List<Vector3> crystalBase = new List<Vector3>();
        ParticleSystem beaconPs;
        Light beaconLight;
        float warmthTarget;
        float beaconTarget = 0.22f;
        float beaconValue;
        Spring pop;

        static readonly Color Cool = new Color(0.45f, 0.6f, 1.5f, 1f);
        static readonly Color Warm = new Color(2.4f, 1.7f, 0.8f, 1f);

        public void Build(Vector3 center, int index)
        {
            Center = center;
            Index = index;
            pop.value = 1f;
            var lit = Shader.Find("Universal Render Pipeline/Lit");

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
                var baseScale = new Vector3(wide, tall, wide * 0.82f);
                c.transform.localScale = baseScale;
                c.transform.rotation = Quaternion.Euler(Mathf.Cos(ang) * lean, ang * Mathf.Rad2Deg, Mathf.Sin(ang) * lean);
                c.AddComponent<MeshFilter>().sharedMesh = ProcMesh.Octahedron(1f);
                var mr = c.AddComponent<MeshRenderer>();
                var m = new Material(lit);
                m.SetColor("_BaseColor", Cool * 0.45f);
                m.SetFloat("_Smoothness", 0.74f);
                m.SetFloat("_Metallic", 0.12f);
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", Cool * 0.85f);
                mr.material = m;
                crystals.Add(m);
                crystalT.Add(c.transform);
                crystalBase.Add(baseScale);
            }

            var glowGo = new GameObject("Beacon");
            glowGo.transform.SetParent(transform, false);
            glowGo.transform.position = new Vector3(center.x, center.y + 0.4f, center.z);
            glowGo.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            beaconPs = glowGo.AddComponent<ParticleSystem>();
            beaconPs.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = beaconPs.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = 2.3f;
            main.startSpeed = 3.4f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
            main.startColor = new Color(0.7f, 0.9f, 1.9f, 0.65f);
            main.maxParticles = 80;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = beaconPs.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            var shape = beaconPs.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 4f;
            shape.radius = 0.3f;
            var col = beaconPs.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.25f), new GradientAlphaKey(0f, 1f) });
            col.color = new ParticleSystem.MinMaxGradient(grad);
            var pr = beaconPs.GetComponent<ParticleSystemRenderer>();
            var pm = new Material(Shader.Find("Sprites/Default"));
            pm.mainTexture = ProcTex.SoftDot(64);
            pr.material = pm;
            pr.renderMode = ParticleSystemRenderMode.Billboard;
            beaconPs.Play();

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
            pop.value = 1.32f;
            pop.velocity = 0f;
        }

        public void ResetState()
        {
            Lit = false;
            IsNext = false;
            Warmth = 0f;
            warmthTarget = 0f;
            beaconValue = 0f;
            beaconTarget = 0.22f;
            pop.value = 1f;
            pop.velocity = 0f;
        }

        void Update()
        {
            float dt = Mathf.Max(0.0001f, Time.deltaTime);

            Warmth = Mathf.MoveTowards(Warmth, warmthTarget, dt * 0.9f);
            var cc = Color.Lerp(Cool, Warm, Warmth);
            for (int i = 0; i < crystals.Count; i++)
            {
                crystals[i].SetColor("_BaseColor", cc * 0.45f);
                crystals[i].SetColor("_EmissionColor", cc * 0.9f);
            }

            pop.Step(1f, 110f, 11f, dt);
            for (int i = 0; i < crystalT.Count; i++)
                if (crystalT[i] != null) crystalT[i].localScale = crystalBase[i] * pop.value;

            beaconValue = Mathf.MoveTowards(beaconValue, beaconTarget, dt * 1.4f);
            float pulse = beaconValue * (0.75f + 0.25f * Mathf.Sin(Time.time * 3f));
            if (beaconPs != null)
            {
                var em = beaconPs.emission;
                em.rateOverTime = beaconValue * 22f;
            }
            if (beaconLight != null) beaconLight.intensity = pulse * 7f;
        }
    }
}
