using UnityEngine;

namespace Starwick
{
    public class CameraRig : MonoBehaviour
    {
        public float MoveSpeed = 6f;
        public float LookSpeed = 0.12f;
        public float EyeHeight = 1.7f;

        float yaw;
        float pitch;
        float shake;
        Spring groundY;

        public float Shake => shake;

        void Start()
        {
            var e = transform.eulerAngles;
            yaw = e.y;
            pitch = e.x;
            float gy = GroundY(transform.position);
            groundY.value = gy;
            groundY.velocity = 0f;
            transform.position = new Vector3(transform.position.x, gy, transform.position.z);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

            if (Sw.Constellation != null) Sw.Constellation.OnRelight += Punch;
        }

        public void Punch()
        {
            shake = 1f;
        }

        void Update()
        {
            float dt = Mathf.Max(0.0001f, Time.deltaTime);

            var look = InputService.LookDelta;
            if (look.sqrMagnitude > 0.0001f)
            {
                yaw += look.x * LookSpeed;
                pitch = Mathf.Clamp(pitch - look.y * LookSpeed, -70f, 70f);
            }

            if (!InputService.UiBlocking)
            {
                var move = InputService.MoveAxis;
                if (move.sqrMagnitude > 0.0001f)
                {
                    var flat = Quaternion.Euler(0f, yaw, 0f);
                    var dir = flat * new Vector3(move.x, 0f, move.y);
                    if (dir.sqrMagnitude > 1f) dir.Normalize();
                    var p = transform.position + dir * MoveSpeed * dt;
                    transform.position = new Vector3(p.x, transform.position.y, p.z);
                }
            }

            groundY.Step(GroundY(transform.position), 120f, 18f, dt);
            transform.position = new Vector3(transform.position.x, groundY.value, transform.position.z);

            float k = shake * shake * 2.6f;
            float ox = Mathf.Sin(Time.time * 41f) * k;
            float oy = Mathf.Cos(Time.time * 37f) * k;
            float oz = Mathf.Sin(Time.time * 47f) * k * 0.6f;
            transform.rotation = Quaternion.Euler(pitch + ox, yaw + oy, oz);
            shake = Mathf.MoveTowards(shake, 0f, dt * 2.2f);
        }

        float GroundY(Vector3 p)
        {
            float h = Sw.Realm != null ? Sw.Realm.Height(p.x, p.z) : 0f;
            return h + EyeHeight;
        }
    }
}
