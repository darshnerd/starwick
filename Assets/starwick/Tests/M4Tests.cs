using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class M4Tests
    {
        [UnityTest]
        public IEnumerator Dialogue_plays_and_records_fragment()
        {
            for (int i = 0; i < 30; i++) yield return null;

            var seq = StoryData.FirstConstellation();
            Assert.Greater(seq.Count, 0, "Story sequence is empty");

            int fragmentsBefore = GameState.Fragments.Count;

            Sw.Dialogue.Play(seq);
            Assert.IsTrue(Sw.Dialogue.Active, "Dialogue did not become active on Play");
            Assert.IsNotNull(Sw.Dialogue.Current, "No current line after Play");

            int guard = 0;
            while (Sw.Dialogue.Active && guard++ < 50)
            {
                Sw.Dialogue.Advance();
                yield return null;
            }

            Assert.IsFalse(Sw.Dialogue.Active, "Dialogue did not end after advancing");
            Assert.Greater(GameState.Fragments.Count, fragmentsBefore, "No memory fragment recorded during the beat");

            Debug.Log($"[swloop] m4 lines={seq.Count} fragments={GameState.Fragments.Count} first='{(GameState.Fragments.Count > 0 ? GameState.Fragments[0] : "")}'");
        }
    }
}
