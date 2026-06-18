using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class TitleTests
    {
        [UnityTest]
        public IEnumerator Title_shows_and_dismisses()
        {
            for (int i = 0; i < 10; i++) yield return null;
            Assert.IsNotNull(Sw.Title, "no TitleUI");

            Sw.Title.Show();
            yield return null;
            Assert.IsTrue(Sw.Title.Visible, "title not visible after Show");
            Assert.IsTrue(InputService.UiBlocking, "title should block world input while shown");

            Sw.Title.Dismiss();
            yield return new WaitForSeconds(1.2f);
            Assert.IsFalse(Sw.Title.Visible, "title did not dismiss after fade");

            Debug.Log("[swloop] title show/dismiss ok");
        }
    }
}
