using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Starwick
{
    public class PostFx : MonoBehaviour
    {
        Bloom bloom;
        ColorAdjustments color;
        float baseBloom = 1.55f;
        float warm = 0.45f;
        float warmTarget = 0.45f;

        public float Warm => warm;

        void Awake()
        {
            var volume = gameObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volume.profile = profile;

            bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(baseBloom);
            bloom.threshold.Override(0.85f);
            bloom.scatter.Override(0.75f);

            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.34f);
            vignette.smoothness.Override(0.62f);

            color = profile.Add<ColorAdjustments>(true);
            color.saturation.Override(10f);
            color.contrast.Override(8f);
            ApplyMood();

            if (Sw.Cam != null)
            {
                var data = Sw.Cam.GetUniversalAdditionalCameraData();
                data.renderPostProcessing = true;
            }
        }

        public void SetMoodWarm(float t)
        {
            warmTarget = Mathf.Clamp01(t);
        }

        void Update()
        {
            if (bloom != null)
                bloom.intensity.value = baseBloom + 0.35f * Mathf.Sin(Time.time * 0.35f);

            warm = Mathf.MoveTowards(warm, warmTarget, Time.deltaTime * 0.5f);
            ApplyMood();
        }

        void ApplyMood()
        {
            if (color == null) return;
            var cool = new Color(0.68f, 0.85f, 1.25f);
            var gold = new Color(1.3f, 1.05f, 0.68f);
            color.colorFilter.Override(Color.Lerp(cool, gold, warm));
            color.postExposure.Override(Mathf.Lerp(-0.05f, 0.5f, warm));
        }
    }
}
