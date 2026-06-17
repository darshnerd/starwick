using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class M7bTests
    {
        [UnityTest]
        public IEnumerator Companion_leads_to_objective()
        {
            for (int i = 0; i < 30; i++) yield return null;
            Assert.IsTrue(Sw.Booted, "not booted");
            Assert.IsNotNull(Sw.Companion, "no companion");
            Assert.IsNotNull(Sw.Decor, "no realm decor");

            Sw.Decor.ResetSites();
            float y = Sw.Cam.transform.position.y;
            Sw.Cam.transform.position = new Vector3(0f, y, 0f);
            yield return new WaitForSeconds(2.5f);

            Vector3 camPos = Sw.Cam.transform.position;
            Vector3 toObj = Sw.Decor.ObjectiveCenter - camPos;
            toObj.y = 0f;
            toObj.Normalize();

            Vector3 toVesp = Sw.Companion.transform.position - camPos;
            toVesp.y = 0f;
            float dist = toVesp.magnitude;
            toVesp.Normalize();

            float align = Vector3.Dot(toObj, toVesp);
            Debug.Log($"[swloop] m7b align={align:F2} vespDist={dist:F2}");

            Assert.Greater(align, 0.4f, "Vesp is not leading toward the objective");
            Assert.Greater(dist, 3f, "Vesp collapsed onto the player");
        }
    }
}
