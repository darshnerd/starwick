using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Starwick
{
    public class PostFx : MonoBehaviour
    {
        Bloom bloom;
        ColorAdjustments color;
        DepthOfField dof;
        float baseBloom = 1.85f;
        float warm = 0.45f;
        float warmTarget = 0.45f;
        float focus;

        public float Warm => warm;

        void Awake()
        {
            var volume = gameObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volume.profile = profile;

            var tonemap = profile.Add<Tonemapping>(true);
            tonemap.mode.Override(TonemappingMode.Neutral);

            bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(baseBloom);
            bloom.threshold.Override(0.7f);
            bloom.scatter.Override(0.82f);
            bloom.tint.Override(new Color(1f, 0.96f, 0.92f));

            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.26f);
            vignette.smoothness.Override(0.6f);

            var grain = profile.Add<FilmGrain>(true);
            grain.type.Override(FilmGrainLookup.Thin1);
            grain.intensity.Override(0.18f);
            grain.response.Override(0.7f);

            var ca = profile.Add<ChromaticAberration>(true);
            ca.intensity.Override(0.08f);

            dof = profile.Add<DepthOfField>(true);
            dof.mode.Override(DepthOfFieldMode.Gaussian);
            dof.gaussianStart.Override(400f);
            dof.gaussianEnd.Override(500f);
            dof.gaussianMaxRadius.Override(1.0f);

            color = profile.Add<ColorAdjustments>(true);
            color.saturation.Override(20f);
            color.contrast.Override(4f);
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
            float dt = Time.deltaTime;

            if (bloom != null)
                bloom.intensity.value = baseBloom + 0.35f * Mathf.Sin(Time.time * 0.35f);

            warm = Mathf.MoveTowards(warm, warmTarget, dt * 0.5f);
            ApplyMood();

            bool focusOn = (Sw.Dialogue != null && Sw.Dialogue.Active) ||
                           (Sw.Narration != null && Sw.Narration.ChoiceActive);
            float focusTarget = focusOn ? 1f : 0f;
            focus = Mathf.MoveTowards(focus, focusTarget, dt * 2.5f);
            if (dof != null)
            {
                dof.gaussianStart.value = Mathf.Lerp(400f, 7f, focus);
                dof.gaussianEnd.value = Mathf.Lerp(500f, 30f, focus);
            }
        }

        void ApplyMood()
        {
            if (color == null) return;
            var cool = new Color(0.68f, 0.85f, 1.25f);
            var gold = new Color(1.3f, 1.05f, 0.68f);
            color.colorFilter.Override(Color.Lerp(cool, gold, warm));
            color.postExposure.Override(Mathf.Lerp(0.2f, 0.7f, warm));
        }
    }
}
