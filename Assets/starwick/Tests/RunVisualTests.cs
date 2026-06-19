using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class RunVisualTests
    {
        [UnityTest]
        public IEnumerator Run_has_glyph_gates_track_seams_and_sky()
        {
            for (int i = 0; i < 10; i++) yield return null;

            var go = new GameObject("RunDirVisual");
            var dir = go.AddComponent<RunDirector>();
            dir.Begin(7793);

            yield return null;
            yield return null;

            Assert.Greater(dir.GlyphLineCount, 0, "no constellation glyph threads were built for gates");

            Assert.IsNotNull(dir.Track, "no track generator");
            Assert.Greater(dir.Track.LastChunkVertexCount, 100,
                "track chunk is still a flat 2-vert ribbon - art-directed mesh missing");

            var sky = dir.GetComponentInChildren<FlowSky>();
            Assert.IsNotNull(sky, "no FlowSky backdrop on the run camera");
            Assert.Greater(sky.StarCount, 0, "FlowSky has no stars");

            Debug.Log($"[swloop] runviz glyphLines={dir.GlyphLineCount} chunkVerts={dir.Track.LastChunkVertexCount} skyStars={sky.StarCount}");

            Object.Destroy(go);
            yield return null;
        }
    }
}
