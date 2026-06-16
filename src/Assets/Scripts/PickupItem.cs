using UnityEngine;

namespace DeskRacers
{
    public class PickupItem : MonoBehaviour
    {
        public PowerUpType powerUpType = PowerUpType.Turbo;
        public int coinAmount;
        public float respawnSeconds = 8f;

        Collider itemCollider;
        Renderer[] renderers;

        // Guarda referencias para esconder e mostrar o pickup.
        void Awake()
        {
            itemCollider = GetComponent<Collider>();
            renderers = GetComponentsInChildren<Renderer>();
        }

        // Roda o pickup para ficar mais visivel.
        void Update()
        {
            transform.Rotate(0f, 120f * Time.deltaTime, 0f, Space.World);
        }

        // Entrega moeda ou power-up ao carro.
        void OnTriggerEnter(Collider other)
        {
            ArcadeCarController car = other.GetComponentInParent<ArcadeCarController>();
            if (car == null)
            {
                return;
            }

            if (coinAmount > 0)
            {
                car.AddCoins(coinAmount);
            }
            else
            {
                car.SetPowerUp(powerUpType);
            }

            SetVisible(false);
            Invoke(nameof(Respawn), respawnSeconds);
        }

        // Faz o pickup reaparecer depois do tempo definido.
        void Respawn()
        {
            SetVisible(true);
        }

        // Liga ou desliga renderer e collider do pickup.
        void SetVisible(bool visible)
        {
            if (itemCollider != null)
            {
                itemCollider.enabled = visible;
            }

            foreach (Renderer itemRenderer in renderers)
            {
                itemRenderer.enabled = visible;
            }
        }
    }
}
