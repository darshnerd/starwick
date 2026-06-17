using UnityEngine;

namespace Starwick
{
    public class SfxManager : MonoBehaviour
    {
        public int PlayCount { get; private set; }

        AudioSource src;
        AudioClip tick;
        AudioClip shimmer;
        AudioClip chime;
        AudioClip confirm;

        void Start()
        {
            Sw.Sfx = this;

            src = gameObject.AddComponent<AudioSource>();
            src.spatialBlend = 0f;
            src.playOnAwake = false;
            src.volume = 0.7f;

            tick = ProcSfx.Tick();
            shimmer = ProcSfx.Shimmer();
            chime = ProcSfx.Chime();
            confirm = ProcSfx.Confirm();
        }

        public void Tick()
        {
            Play(tick, 1f);
            Haptics.Light();
        }

        public void Shimmer(int step)
        {
            Play(shimmer, 1f + step * 0.06f);
            Haptics.Light();
        }

        public void Chime()
        {
            Play(chime, 1f);
            Haptics.Heavy();
        }

        public void Confirm()
        {
            Play(confirm, 1f);
            Haptics.Medium();
        }

        void Play(AudioClip c, float pitch)
        {
            PlayCount++;
            if (src == null || c == null) return;
            src.pitch = pitch;
            src.PlayOneShot(c);
        }
    }
}
