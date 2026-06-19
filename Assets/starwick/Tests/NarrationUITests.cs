using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class NarrationUITests
    {
        [UnityTest]
        public IEnumerator Narration_renders_on_screen()
        {
            for (int i = 0; i < 60; i++) yield return null;

            Sw.Dialogue.Play(StoryData.FirstConstellation());
            Assert.IsTrue(Sw.Dialogue.Active, "Dialogue not active");

            yield return new WaitForSeconds(1.5f);

            Assert.IsNotNull(Sw.Narration, "no NarrationUI");
            Assert.IsTrue(Sw.Narration.BodyShown, "narration body not shown while dialogue active");
            Assert.IsNotEmpty(Sw.Narration.BodyContent, "narration body has no text");
            Assert.LessOrEqual(Sw.Narration.MaxBodyColorChannel, 1.001f,
                "narration text color is HDR (>1) - would bloom/fringe through post");

            Debug.Log($"[swloop] m4b bodyShown={Sw.Narration.BodyShown} maxColor={Sw.Narration.MaxBodyColorChannel:F2} line='{(Sw.Dialogue.Current != null ? Sw.Dialogue.Current.Text : "")}'");
        }
    }
}
