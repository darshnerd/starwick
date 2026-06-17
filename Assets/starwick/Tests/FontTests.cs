using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class FontTests
    {
        [UnityTest]
        public IEnumerator Custom_typefaces_load()
        {
            for (int i = 0; i < 10; i++) yield return null;

            string heading = Typeface.HeadingFamily;
            string body = Typeface.BodyFamily;
            Debug.Log($"[swloop] fonts custom={Typeface.UsingCustomFonts} heading='{heading}' body='{body}'");

            Assert.IsTrue(Typeface.UsingCustomFonts, "fell back to default TMP font");
            StringAssert.Contains("Grotesk", heading);
            StringAssert.Contains("Sora", body);
        }
    }
}
