using UnityEngine;

namespace DeskRacers
{
    public class ToasterLauncher : MonoBehaviour
    {
        public GameObject toastPrefab;
        public Transform spawnPoint;
        public float launchForce = 7f;
        public float launchInterval = 3f;

        // Comeca o disparo repetido das torradas.
        void Start()
        {
            InvokeRepeating(nameof(LaunchToast), 1f, launchInterval);
        }

        // Instancia e dispara uma torrada pela pista.
        void LaunchToast()
        {
            if (toastPrefab == null || spawnPoint == null)
            {
                return;
            }

            GameObject toast = Instantiate(toastPrefab, spawnPoint.position, spawnPoint.rotation);
            Rigidbody body = toast.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.AddForce(spawnPoint.forward * launchForce + Vector3.up * 2f, ForceMode.VelocityChange);
            }
        }
    }
}
