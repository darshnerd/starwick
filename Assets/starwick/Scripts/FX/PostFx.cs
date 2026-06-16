using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Starwick
{
    public class PostFx : MonoBehaviour
    {
        void Awake()
        {
            var volume = gameObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volume.profile = profile;

            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(1.4f);
            bloom.threshold.Override(0.7f);
            bloom.scatter.Override(0.7f);

            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.32f);
            vignette.smoothness.Override(0.6f);

            var color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(0.15f);
            color.saturation.Override(8f);
            color.contrast.Override(6f);

            if (Sw.Cam != null)
            {
                var data = Sw.Cam.GetUniversalAdditionalCameraData();
                data.renderPostProcessing = true;
            }
        }
    }
}
