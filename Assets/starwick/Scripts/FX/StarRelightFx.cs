using System.Collections;
using UnityEngine;

namespace Starwick
{
    public class StarRelightFx : MonoBehaviour
    {
        static readonly Color Warm = new Color(2.9f, 2.2f, 1.3f, 1f);

        Transform core;
        Transform shock;
        ParticleSystem motes;
        Material shockMat;
        float baseFov = 60f;
        bool companionReacted;
        bool motesFired;

        void Awake()
        {
            var unlit = Shader.Find("Universal Render Pipeline/Unlit");
            var sprite = Shader.Find("Sprites/Default");

            var coreGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var c = coreGo.GetComponent<Collider>();
            if (c != null) Destroy(c);
            coreGo.transform.SetParent(transform, false);
            var cm = new Material(unlit);
            cm.color = Warm;
            cm.SetColor("_BaseColor", Warm);
            coreGo.GetComponent<MeshRenderer>().material = cm;
            core = coreGo.transform;
            core.localScale = Vector3.zero;

            var shockGo = new GameObject("Shock");
            shockGo.AddComponent<MeshFilter>().sharedMesh = ProcMesh.Torus(1f, 0.05f, 64, 8);
            shockMat = new Material(unlit);
            shockMat.color = Warm;
            shockMat.SetColor("_BaseColor", Warm);
            shockGo.AddComponent<MeshRenderer>().material = shockMat;
            shock = shockGo.transform;
            shock.SetParent(transform, false);
            shock.localScale = Vector3.zero;

            var moteGo = new GameObject("Motes");
            moteGo.transform.SetParent(transform, false);
            motes = moteGo.AddComponent<ParticleSystem>();
            motes.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = motes.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 0.7f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.4f);
            main.startColor = Warm;
            main.maxParticles = 80;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = motes.emission;
            emission.enabled = false;
            var pr = motes.GetComponent<ParticleSystemRenderer>();
            var pm = new Material(sprite);
            pm.mainTexture = ProcTex.SoftDot(64);
            pr.material = pm;
            pr.renderMode = ParticleSystemRenderMode.Billboard;
        }

        public void Play()
        {
            StopAllCoroutines();
            StartCoroutine(Sequence());
        }

        IEnumerator Sequence()
        {
            companionReacted = false;
            motesFired = false;
            if (Sw.Cam != null)
            {
                baseFov = Sw.Cam.fieldOfView;
                shock.rotation = Quaternion.LookRotation((Sw.Cam.transform.position - transform.position).normalized);
            }

            const float dur = 2.2f;
            float e = 0f;
            while (e < dur)
            {
                e += Time.deltaTime;

                if (e < 0.3f)
                    core.localScale = Vector3.one * Mathf.Lerp(0.5f, 0.12f, e / 0.3f);

                if (!motesFired && e >= 0.3f)
                {
                    motesFired = true;
                    FireMotes();
                }

                if (e >= 0.9f && e < 1.25f)
                    core.localScale = Vector3.one * Mathf.Lerp(0.12f, 1.5f, Ease.OutBack((e - 0.9f) / 0.35f));
                else if (e >= 1.25f)
                    core.localScale = Vector3.one * Mathf.Lerp(1.5f, 0.9f, Mathf.Clamp01((e - 1.25f) / 0.95f));

                if (e >= 1.2f)
                {
                    float k = Mathf.Clamp01((e - 1.2f) / 0.8f);
                    shock.localScale = Vector3.one * Mathf.Lerp(0.2f, 7f, Ease.OutCubic(k));
                    var col = Warm;
                    col.a = 1f - k;
                    shockMat.color = col;
                    shockMat.SetColor("_BaseColor", col);
                }

                if (!companionReacted && e >= 1.2f)
                {
                    companionReacted = true;
                    if (Sw.Companion != null) Sw.Companion.React();
                }

                if (Sw.Cam != null && e >= 1.2f)
                    Sw.Cam.fieldOfView = baseFov - Mathf.Sin(Mathf.Clamp01((e - 1.2f) / 1.0f) * Mathf.PI) * 5f;

                yield return null;
            }

            core.localScale = Vector3.zero;
            shock.localScale = Vector3.zero;
            if (Sw.Cam != null) Sw.Cam.fieldOfView = baseFov;
        }

        void FireMotes()
        {
            const int n = 36;
            for (int i = 0; i < n; i++)
            {
                float a = (i / (float)n) * Mathf.PI * 2f;
                Vector3 dir = new Vector3(Mathf.Cos(a), Mathf.Sin(a) * 0.6f, Mathf.Sin(a));
                Vector3 tangential = Vector3.Cross(dir, Vector3.forward) * 2.5f;
                var ep = new ParticleSystem.EmitParams();
                ep.position = transform.position + dir * 5f;
                ep.velocity = (-dir * 7f) + tangential;
                motes.Emit(ep, 1);
            }
        }
    }
}
