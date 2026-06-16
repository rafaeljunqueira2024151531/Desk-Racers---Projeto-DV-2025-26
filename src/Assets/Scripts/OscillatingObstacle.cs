using UnityEngine;

namespace DeskRacers
{
    public class OscillatingObstacle : MonoBehaviour
    {
        public Vector3 localMove = new Vector3(0f, 0f, 4f);
        public float speed = 1.2f;

        Vector3 startPosition;

        void Start()
        {
            startPosition = transform.localPosition;
        }

        void Update()
        {
            transform.localPosition = startPosition + localMove * Mathf.Sin(Time.time * speed);
        }
    }
}
