using UnityEngine;

namespace Starwick
{
    public class CameraRig : MonoBehaviour
    {
        public float DriftDegPerSec = 1.5f;

        void Update()
        {
            transform.Rotate(Vector3.up, DriftDegPerSec * Time.deltaTime, Space.World);
        }
    }
}
