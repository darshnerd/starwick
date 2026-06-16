using UnityEngine;

namespace Starwick
{
    public class CameraRig : MonoBehaviour
    {
        public float MoveSpeed = 9f;
        public float LookSpeed = 0.12f;
        public float IdleDriftDegPerSec = 1.0f;

        float yaw;
        float pitch;

        void Start()
        {
            var e = transform.eulerAngles;
            yaw = e.y;
            pitch = e.x;
        }

        void Update()
        {
            bool active = false;

            var look = InputService.LookDelta;
            if (look.sqrMagnitude > 0.0001f)
            {
                yaw += look.x * LookSpeed;
                pitch = Mathf.Clamp(pitch - look.y * LookSpeed, -80f, 80f);
                active = true;
            }

            var move = InputService.MoveAxis;
            if (move.sqrMagnitude > 0.0001f)
            {
                var dir = transform.right * move.x + transform.forward * move.y;
                transform.position += dir.normalized * MoveSpeed * Time.deltaTime;
                active = true;
            }

            if (!active)
                yaw += IdleDriftDegPerSec * Time.deltaTime;

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }
}
