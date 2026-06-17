using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class FP2Tests
    {
        [UnityTest]
        public IEnumerator Choice_ui_pops_in()
        {
            for (int i = 0; i < 30; i++) yield return null;
            Assert.IsTrue(Sw.Booted, "not booted");
            Assert.IsNotNull(Sw.Narration, "no NarrationUI");

            if (Sw.Narration.ChoiceActive) Sw.Narration.HideChoices();
            yield return null;

            Sw.Narration.ShowChoices("Offer?", "Let it rest", "Send it onward", i => { });
            yield return null;
            float r0 = Sw.Narration.ChoiceReveal;

            yield return new WaitForSeconds(0.5f);
            float r1 = Sw.Narration.ChoiceReveal;

            Sw.Narration.HideChoices();

            Assert.Less(r0, 0.4f, "choices did not start small (no pop-in)");
            Assert.Greater(r1, 0.9f, "choices did not finish revealing");

            Debug.Log($"[swloop] fp2 reveal0={r0:F2} reveal1={r1:F2}");
        }
    }
}
