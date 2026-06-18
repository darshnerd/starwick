using UnityEngine;

namespace Starwick
{
    public class WickFlowMotor : MonoBehaviour
    {
        public float Distance;
        public float Speed;
        public float Height;
        public float VerticalVelocity;
        public float LaneOffset;
        public float Flow;
        public bool Grounded;

        public float BaseSpeed = 12f;
        public float MaxSpeed = 30f;
        public float Gravity = 24f;
        public float ChargeAccel = 10f;
        public float LiftPerCharge = 9f;
        public float Drag = 1.2f;

        public System.Func<float, float> GroundAt = d => 0f;
        public bool SelfDrive;

        public Vector3 Position => new Vector3(LaneOffset, Height, Distance);

        void Update()
        {
            if (!SelfDrive) return;
            float dt = Mathf.Max(0.0001f, Time.deltaTime);
            Step(FlowInput.Read(dt), dt);
        }

        public void Step(FlowInputFrame f, float dt)
        {
            float ground = GroundAt != null ? GroundAt(Distance) : 0f;

            float target = Mathf.Clamp(BaseSpeed + (f.Held ? ChargeAccel : 0f), 0f, MaxSpeed);
            Speed = Mathf.MoveTowards(Speed, target, 18f * dt);
            Distance += Speed * dt;

            LaneOffset = Mathf.Clamp(LaneOffset + f.Swipe.x * 0.01f, -3.5f, 3.5f);

            if (f.Released)
                VerticalVelocity += 4f + LiftPerCharge * Mathf.Clamp01(f.HoldTime * 2f);

            VerticalVelocity -= Gravity * dt;
            VerticalVelocity -= VerticalVelocity * Drag * dt;
            Height += VerticalVelocity * dt;

            if (Height <= ground)
            {
                Height = ground;
                if (VerticalVelocity < 0f) VerticalVelocity = 0f;
                Grounded = true;
            }
            else
            {
                Grounded = false;
            }

            float flowTarget = (!Grounded || f.Held) ? 1f : 0f;
            Flow = Mathf.MoveTowards(Flow, flowTarget, dt * 0.6f);
        }
    }
}
