using UnityEngine;

namespace DeskRacers
{
    public class RaceCheckpoint : MonoBehaviour
    {
        public bool isFinishLine;
        public int checkpointIndex;
        public RaceManager raceManager;
        public Transform respawnPoint;
        public GameObject visualObject;

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
                raceManager.TryRegisterLap(checkpointIndex, GetRespawnTransform());
            }
            else
            {
                raceManager.RegisterCheckpoint(checkpointIndex, GetRespawnTransform());
            }
        }

        // Liga ou desliga o indicador visual deste checkpoint.
        public void SetVisualActive(bool active)
        {
            if (visualObject != null)
            {
                visualObject.SetActive(active);
            }
        }

        // Devolve o ponto exacto onde o carro deve reaparecer.
        Transform GetRespawnTransform()
        {
            return respawnPoint != null ? respawnPoint : transform;
        }
    }
}
