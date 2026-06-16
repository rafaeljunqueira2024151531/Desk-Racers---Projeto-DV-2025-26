using UnityEngine;

namespace DeskRacers
{
    public class FanZone : MonoBehaviour
    {
        public Vector3 worldForce = new Vector3(12f, 0f, 0f);

        // Empurra rigidbodies dentro da zona de vento.
        void OnTriggerStay(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;
            if (body != null)
            {
                body.AddForce(worldForce, ForceMode.Acceleration);
            }
        }
    }
}
