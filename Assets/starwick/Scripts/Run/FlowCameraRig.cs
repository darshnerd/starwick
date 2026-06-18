using UnityEngine;

namespace Starwick
{
    public class FlowCameraRig : MonoBehaviour
    {
        public WickFlowMotor Target;
        public float BaseFov = 60f;

        public Transform StableAim { get; private set; }
        public Transform VisualCam { get; private set; }
        public Camera Cam { get; private set; }

        float shake;

        public Vector3 StableForward => StableAim != null ? StableAim.forward : transform.forward;
        public Quaternion StableRotation => StableAim != null ? StableAim.rotation : transform.rotation;

        void Awake()
        {
            StableAim = new GameObject("StableAim").transform;
            StableAim.SetParent(transform, false);

            var vc = new GameObject("VisualCamera");
            vc.transform.SetParent(StableAim, false);
            VisualCam = vc.transform;
            Cam = vc.AddComponent<Camera>();
            Cam.clearFlags = CameraClearFlags.SolidColor;
            Cam.backgroundColor = new Color(0.015f, 0.02f, 0.05f);
            Cam.fieldOfView = BaseFov;
            Cam.farClipPlane = 2000f;
        }

        public void Punch()
        {
            shake = 1f;
        }

        void LateUpdate()
        {
            if (Target != null) Follow(Target, Mathf.Max(0.0001f, Time.deltaTime));
        }

        public void Follow(WickFlowMotor t, float dt)
        {
            Vector3 motorPos = t.Position;

            Vector3 desired = motorPos + new Vector3(0f, 3.2f, -8.5f);
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-6f * dt));

            Vector3 lookAt = motorPos + new Vector3(0f, 1f, 7f);
            Vector3 dir = lookAt - StableAim.position;
            if (dir.sqrMagnitude > 0.0001f)
                StableAim.rotation = Quaternion.Slerp(StableAim.rotation,
                    Quaternion.LookRotation(dir.normalized, Vector3.up), 1f - Mathf.Exp(-8f * dt));

            float fov = BaseFov + t.Speed * 0.4f + (!t.Grounded ? 6f : 0f);
            Cam.fieldOfView = Mathf.Lerp(Cam.fieldOfView, fov, dt * 3f);

            float k = shake * shake * 2.5f;
            VisualCam.localPosition = Vector3.zero;
            VisualCam.localRotation = Quaternion.Euler(
                Mathf.Sin(Time.time * 41f) * k,
                Mathf.Cos(Time.time * 37f) * k,
                Mathf.Sin(Time.time * 47f) * k * 0.6f);
            shake = Mathf.MoveTowards(shake, 0f, dt * 2.2f);
        }
    }
}
