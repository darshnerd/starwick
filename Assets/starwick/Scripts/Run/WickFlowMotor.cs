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

        public float AirTime;
        public bool Diving;
        public bool LastLandingPerfect;
        public bool LastLandingBad;
        public float LastLandingImpact;

        public float BaseSpeed = 12f;
        public float MinSpeed = 5f;
        public float MaxSpeed = 30f;
        public float Gravity = 24f;
        public float ChargeAccel = 10f;
        public float LiftPerCharge = 9f;
        public float Drag = 1.2f;
        public float SlopeAccel = 16f;
        public float DiveAccel = 30f;

        public System.Func<float, float> GroundAt = d => 0f;
        public System.Func<float, float, float, Vector3> WorldAt;
        public bool SelfDrive;

        public Vector3 Position => WorldAt != null
            ? WorldAt(Distance, LaneOffset, Height)
            : new Vector3(LaneOffset, Height, Distance);

        void Update()
        {
            if (!SelfDrive) return;
            float dt = Mathf.Max(0.0001f, Time.deltaTime);
            Step(FlowInput.Read(dt), dt);
        }

        public void Step(FlowInputFrame f, float dt)
        {
            float ground = GroundAt != null ? GroundAt(Distance) : 0f;
            float ahead = GroundAt != null ? GroundAt(Distance + 2f) : 0f;
            float slope = (ahead - ground) * 0.5f;
            float slopeBonus = -slope * SlopeAccel;

            float target = Mathf.Clamp(BaseSpeed + (f.Held ? ChargeAccel : 0f) + slopeBonus, MinSpeed, MaxSpeed);
            Speed = Mathf.MoveTowards(Speed, target, 18f * dt);
            Distance += Speed * dt;

            LaneOffset = Mathf.Clamp(LaneOffset + f.Swipe.x * 0.01f, -3.5f, 3.5f);

            Diving = !Grounded && f.Held;
            if (Diving) VerticalVelocity -= DiveAccel * dt;

            if (f.Released)
                VerticalVelocity += 4f + LiftPerCharge * Mathf.Clamp01(f.HoldTime * 2f);

            VerticalVelocity -= Gravity * dt;
            VerticalVelocity -= VerticalVelocity * Drag * dt;
            Height += VerticalVelocity * dt;

            bool wasGrounded = Grounded;
            LastLandingPerfect = false;
            LastLandingBad = false;

            if (Height <= ground)
            {
                float impact = -VerticalVelocity;
                Height = ground;

                if (!wasGrounded)
                {
                    LastLandingImpact = impact;
                    if (AirTime > 0.3f && impact < 12f && !Diving)
                    {
                        LastLandingPerfect = true;
                        Speed = Mathf.Min(MaxSpeed, Speed + 3f + Mathf.Min(AirTime, 2f) * 3f);
                    }
                    else if (impact > 20f || Diving)
                    {
                        LastLandingBad = true;
                    }
                }

                if (VerticalVelocity < 0f) VerticalVelocity = 0f;
                Grounded = true;
                AirTime = 0f;
            }
            else
            {
                Grounded = false;
                AirTime += dt;
            }

            float flowTarget = (!Grounded || f.Held) ? 1f : 0f;
            Flow = Mathf.MoveTowards(Flow, flowTarget, dt * 0.6f);
        }
    }
}
