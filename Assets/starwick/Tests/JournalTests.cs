using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class JournalTests
    {
        [UnityTest]
        public IEnumerator Journal_lists_fragments()
        {
            for (int i = 0; i < 60; i++) yield return null;

            string memory = "A test memory - a small light remembered against the dark.";
            GameState.AddFragment(memory);
            Assert.Greater(GameState.Fragments.Count, 0, "No fragment to show");

            Sw.Journal.SetOpen(true);
            Assert.IsTrue(Sw.Journal.IsOpen, "Journal did not open");
            yield return new WaitForSeconds(0.6f);

            Assert.IsTrue(Sw.Journal.ListShown, "journal list not shown while open");
            StringAssert.Contains(memory, Sw.Journal.ListContent, "journal list does not contain the fragment");

            Sw.Journal.SetOpen(false);
            Assert.IsFalse(Sw.Journal.IsOpen, "journal did not close");

            Debug.Log($"[swloop] journal listShown={Sw.Journal.ListShown} fragments={GameState.Fragments.Count}");
        }
    }
}
