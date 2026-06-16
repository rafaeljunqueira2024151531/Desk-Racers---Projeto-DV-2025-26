using UnityEngine;

namespace DeskRacers
{
    public class DeskRacersPickup : MonoBehaviour
    {
        public enum PickupType
        {
            Coin,
            Spring,
            Turbo
        }

        public PickupType type;
        public float respawnSeconds = 8f;

        Collider pickupCollider;
        Renderer pickupRenderer;

        void Awake()
        {
            pickupCollider = GetComponent<Collider>();
            pickupRenderer = GetComponent<Renderer>();
        }

        void Update()
        {
            transform.Rotate(0f, 120f * Time.deltaTime, 0f, Space.World);
        }

        void OnTriggerEnter(Collider other)
        {
            DeskRacersCarController car = other.GetComponentInParent<DeskRacersCarController>();
            if (car == null)
            {
                return;
            }

            if (type == PickupType.Coin)
            {
                car.CollectCoin();
            }
            else
            {
                car.CollectPowerUp(type == PickupType.Spring ? "Mola" : "Turbo");
            }

            SetVisible(false);
            Invoke(nameof(Respawn), respawnSeconds);
        }

        void Respawn()
        {
            SetVisible(true);
        }

        void SetVisible(bool visible)
        {
            if (pickupCollider != null)
            {
                pickupCollider.enabled = visible;
            }

            if (pickupRenderer != null)
            {
                pickupRenderer.enabled = visible;
            }
        }
    }
}
