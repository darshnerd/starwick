using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class RunTrackTests
    {
        [UnityTest]
        public IEnumerator Track_generates_chunks_and_motor_follows()
        {
            Assert.AreEqual(RunTrack.HeightFor(7, 50f), RunTrack.HeightFor(7, 50f), 0.0001f, "track height not deterministic");
            float sa = 0f, sb = 0f;
            for (int k = 0; k < 8; k++) { sa += RunTrack.HeightFor(7, k * 13f); sb += RunTrack.HeightFor(99, k * 13f); }
            Assert.AreNotEqual(sa, sb, "different seeds produced identical track");

            var go = new GameObject("Gen");
            var gen = go.AddComponent<RunTrackGenerator>();
            yield return null;

            int bound = gen.ChunksAhead + gen.ChunksBehind + 2;
            Assert.Greater(gen.ActiveChunks, 0, "no chunks built at start");
            Assert.LessOrEqual(gen.ActiveChunks, bound, "too many chunks at start");

            gen.Advance(600f);
            Assert.LessOrEqual(gen.ActiveChunks, bound, "chunk pool unbounded after advancing");

            var mGo = new GameObject("FlowMotor");
            var m = mGo.AddComponent<WickFlowMotor>();
            m.GroundAt = d => gen.Track.Height(d);
            m.Height = gen.Track.Height(0f);
            for (int i = 0; i < 60; i++) m.Step(new FlowInputFrame { Held = true }, 0.05f);

            float terr = gen.Track.Height(m.Distance);
            Assert.Less(Mathf.Abs(m.Height - terr), 3f, "motor did not ride the track surface");

            Object.Destroy(go);
            Object.Destroy(mGo);
            yield return null;
            Debug.Log($"[swloop] f3 chunks={gen.ActiveChunks} dist={m.Distance:F1} dH={Mathf.Abs(m.Height - terr):F2}");
        }
    }
}
