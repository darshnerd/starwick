using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class RunWeaveTests
    {
        [UnityTest]
        public IEnumerator Track_is_a_bounded_translucent_weave()
        {
            for (int i = 0; i < 10; i++) yield return null;

            var go = new GameObject("WeaveRun");
            var dir = go.AddComponent<RunDirector>();
            dir.Begin(7793);

            int guard = 0;
            while (dir.Running && dir.Motor.Distance < 250f && guard++ < 3000)
                dir.Step(new FlowInputFrame(), 0.05f);

            Assert.GreaterOrEqual(dir.Track.TrackRenderQueue, 3000, "track is not a transparent weave");
            int maxChunks = dir.Track.ChunksAhead + dir.Track.ChunksBehind + 2;
            Assert.LessOrEqual(dir.Track.ActiveChunks, maxChunks, "track chunks not bounded (overdraw risk)");
            Assert.LessOrEqual(dir.Track.SparkBudget, 64, "edge spark budget not bounded");
            Assert.Greater(dir.Track.LastChunkVertexCount, 100, "track mesh degenerate");

            Debug.Log($"[swloop] weave queue={dir.Track.TrackRenderQueue} chunks={dir.Track.ActiveChunks} sparkBudget={dir.Track.SparkBudget} verts={dir.Track.LastChunkVertexCount}");

            Object.Destroy(go);
            yield return null;
        }
    }
}
