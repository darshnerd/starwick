using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Starwick
{
    public class PostFx : MonoBehaviour
    {
        Bloom bloom;
        float baseBloom = 1.9f;

        void Awake()
        {
            var volume = gameObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volume.profile = profile;

            bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(baseBloom);
            bloom.threshold.Override(0.65f);
            bloom.scatter.Override(0.75f);

            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.34f);
            vignette.smoothness.Override(0.62f);

            var color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(0.15f);
            color.saturation.Override(10f);
            color.contrast.Override(8f);

            if (Sw.Cam != null)
            {
                var data = Sw.Cam.GetUniversalAdditionalCameraData();
                data.renderPostProcessing = true;
            }
        }

        void Update()
        {
            if (bloom != null)
                bloom.intensity.value = baseBloom + 0.35f * Mathf.Sin(Time.time * 0.35f);
        }
            
        }
}
