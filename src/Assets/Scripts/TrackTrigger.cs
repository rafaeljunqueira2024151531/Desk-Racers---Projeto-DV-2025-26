using UnityEngine;

namespace DeskRacers
{
    public class TrackTrigger : MonoBehaviour
    {
        public enum TriggerType
        {
            Lap,
            Slippery,
            Fan
        }

        public TriggerType type;
        public Vector3 fanForce = new Vector3(14f, 0f, 0f);
        public float slipperySeconds = 2.4f;

        RaceGameManager gameManager;

        void Awake()
        {
            gameManager = FindAnyObjectByType<RaceGameManager>();
        }

        void OnTriggerEnter(Collider other)
        {
            DeskRacersCarController car = other.GetComponentInParent<DeskRacersCarController>();
            if (car == null)
            {
                return;
            }

            if (type == TriggerType.Lap && gameManager != null)
            {
                gameManager.RegisterLap();
            }
            else if (type == TriggerType.Slippery)
            {
                car.ApplySlipperyPatch(slipperySeconds);
            }
        }

        void OnTriggerStay(Collider other)
        {
            if (type != TriggerType.Fan)
            {
                return;
            }

            Rigidbody body = other.attachedRigidbody;
            if (body != null)
            {
                body.AddForce(fanForce, ForceMode.Acceleration);
            }
        }
    }
}
