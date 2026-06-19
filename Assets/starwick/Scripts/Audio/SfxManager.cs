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
        AudioClip detune;
        AudioClip[] notes;
        AudioClip[] chords;

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
            detune = ProcSfx.Detune();

            notes = new AudioClip[8];
            for (int i = 0; i < notes.Length; i++) notes[i] = ProcSfx.Note(i);
            chords = new AudioClip[4];
            for (int i = 0; i < chords.Length; i++) chords[i] = ProcSfx.Chord(i);
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

        public void Note(int degree)
        {
            if (notes == null) { PlayCount++; return; }
            int d = Mathf.Clamp(degree, 0, notes.Length - 1);
            Play(notes[d], 1f);
            Haptics.Light();
        }

        public void Chord(int tier)
        {
            if (chords == null) { PlayCount++; return; }
            int t = Mathf.Clamp(tier, 0, chords.Length - 1);
            Play(chords[t], 1f);
            Haptics.Medium();
        }

        public void Detune()
        {
            Play(detune, 1f);
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
