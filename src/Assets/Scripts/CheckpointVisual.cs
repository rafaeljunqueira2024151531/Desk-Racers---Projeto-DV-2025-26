using UnityEngine;

namespace DeskRacers
{
    public class CheckpointVisual : MonoBehaviour
    {
        public float rotateSpeed;
        public float pulseAmount;
        public float pulseSpeed = 3f;

        Vector3 startScale;

        // Guarda a escala inicial do indicador.
        void Awake()
        {
            startScale = transform.localScale;
        }

        // Faz o checkpoint rodar e pulsar para ser facil de ver.
        void Update()
        {
            transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = startScale * pulse;
        }
    }
}
