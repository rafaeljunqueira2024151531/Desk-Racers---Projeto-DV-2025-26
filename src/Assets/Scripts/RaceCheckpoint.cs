using UnityEngine;

namespace DeskRacers
{
    public class RaceCheckpoint : MonoBehaviour
    {
        public bool isFinishLine;
        public int checkpointIndex;
        public RaceManager raceManager;

        // Detecta o carro e informa o RaceManager.
        void OnTriggerEnter(Collider other)
        {
            ArcadeCarController car = other.GetComponentInParent<ArcadeCarController>();
            if (car == null || raceManager == null)
            {
                return;
            }

            if (isFinishLine)
            {
                raceManager.TryRegisterLap();
            }
            else
            {
                raceManager.RegisterCheckpoint(checkpointIndex);
            }
        }
    }
}
