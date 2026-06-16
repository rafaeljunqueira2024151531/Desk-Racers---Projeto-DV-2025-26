using UnityEngine;

namespace DeskRacers
{
    public class DestroyAfterSeconds : MonoBehaviour
    {
        public float lifeTime = 8f;

        // Destroi o objecto depois de alguns segundos.
        void Start()
        {
            Destroy(gameObject, lifeTime);
        }
    }
}
