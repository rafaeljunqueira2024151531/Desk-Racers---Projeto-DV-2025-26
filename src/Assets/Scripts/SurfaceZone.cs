using UnityEngine;

namespace DeskRacers
{
    public class SurfaceZone : MonoBehaviour
    {
        public float gripMultiplier = 0.35f;

        // Aplica o multiplicador de aderencia enquanto o carro esta na zona.
        void OnTriggerStay(Collider other)
        {
            ArcadeCarController car = other.GetComponentInParent<ArcadeCarController>();
            if (car != null)
            {
                car.SetGripMultiplier(gripMultiplier);
            }
        }

        // Repõe a aderencia quando o carro sai da zona.
        void OnTriggerExit(Collider other)
        {
            ArcadeCarController car = other.GetComponentInParent<ArcadeCarController>();
            if (car != null)
            {
                car.SetGripMultiplier(1f);
            }
        }
    }
}
