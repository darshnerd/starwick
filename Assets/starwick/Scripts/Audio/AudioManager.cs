using UnityEngine;

namespace Starwick
{
    public class AudioManager : MonoBehaviour
    {
        public float DroneVol { get; private set; }
        public float PadVol { get; private set; }
        public float MotifVol { get; private set; }
        public float TensionVol { get; private set; }
        public float Energy { get; private set; }

        AudioSource pad;
        AudioSource tension;
        float relightPulse;

        void Start()
        {
            Sw.Audio = this;

            pad = gameObject.AddComponent<AudioSource>();
            pad.clip = ProcAudio.Pad();
            pad.loop = true;
            pad.spatialBlend = 0f;
            pad.volume = 0f;
            pad.Play();

            tension = gameObject.AddComponent<AudioSource>();
            tension.clip = ProcAudio.Tension();
            tension.loop = true;
            tension.spatialBlend = 0f;
            tension.volume = 0f;
            tension.Play();

            if (Sw.Constellation != null) Sw.Constellation.OnRelight += OnRelight;
        }

        void OnRelight()
        {
            relightPulse = 1f;
        }

        void Update()
        {
            float dt = Mathf.Max(0.0001f, Time.deltaTime);

            float prox = Proximity();
            bool tracing = Sw.Constellation != null && !Sw.Constellation.Complete && Sw.Constellation.TracedCount > 0;
            bool choosing = Sw.Narration != null && Sw.Narration.ChoiceActive;

            float droneT = 0.4f;
            float padT = Mathf.Lerp(0.06f, 0.5f, prox) + relightPulse * 0.3f;
            float motifT = (tracing ? 0.5f : 0f) + prox * 0.25f + relightPulse * 0.5f;
            float tensionT = choosing ? 0.5f : 0f;

            relightPulse = Mathf.MoveTowards(relightPulse, 0f, dt * 0.4f);

            DroneVol = Mathf.MoveTowards(DroneVol, droneT, dt * 0.6f);
            PadVol = Mathf.MoveTowards(PadVol, padT, dt * 0.8f);
            MotifVol = Mathf.MoveTowards(MotifVol, motifT, dt * 1.2f);
            TensionVol = Mathf.MoveTowards(TensionVol, tensionT, dt * 1.5f);

            if (Sw.Ambient != null) Sw.Ambient.volume = DroneVol;
            if (Sw.Motif != null) Sw.Motif.volume = MotifVol;
            if (pad != null) pad.volume = PadVol;
            if (tension != null) tension.volume = TensionVol;

            Energy = Mathf.Clamp01(DroneVol * 0.3f + PadVol * 0.5f + MotifVol * 0.7f + TensionVol * 0.4f + relightPulse * 0.8f);
        }

        float Proximity()
        {
            if (Sw.Cam == null) return 0f;

            Vector3 target;
            if (Sw.Decor != null) target = Sw.Decor.ObjectiveCenter;
            else if (Sw.Realm != null && Sw.Realm.Sites != null && Sw.Realm.Sites.Length > 0) target = Sw.Realm.Sites[0];
            else return 0f;

            var p = Sw.Cam.transform.position;
            float d = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(target.x, target.z));
            return Mathf.Clamp01(1f - d / 30f);
        }
    }
}
