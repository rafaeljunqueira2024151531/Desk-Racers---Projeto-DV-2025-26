using UnityEngine;

namespace DeskRacers
{
    [RequireComponent(typeof(Rigidbody))]
    public class AICarController : MonoBehaviour
    {
        public Transform[] waypoints;
        public float acceleration = 10f;
        public float maxSpeed = 7f;
        public float turnSpeed = 130f;
        public float waypointRadius = 0.8f;
        public float sideGrip = 10f;
        public float randomSpeedOffset = 0.8f;

        Rigidbody rb;
        int currentWaypoint;
        float speed;
        float speedMultiplier = 1f;

        // Prepara o Rigidbody e cria uma pequena variacao entre oponentes.
        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            speedMultiplier = Random.Range(1f - randomSpeedOffset * 0.1f, 1f + randomSpeedOffset * 0.1f);
        }

        // Actualiza a conducao automatica em tempo fixo.
        void FixedUpdate()
        {
            if (waypoints == null || waypoints.Length == 0)
            {
                return;
            }

            DriveToWaypoint();
            ApplyMovement();
        }

        // Calcula direccao e roda o carro para o waypoint actual.
        void DriveToWaypoint()
        {
            Transform waypoint = waypoints[currentWaypoint];
            Vector3 toWaypoint = waypoint.position - transform.position;
            toWaypoint.y = 0f;

            if (toWaypoint.magnitude <= waypointRadius)
            {
                currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(toWaypoint.normalized, Vector3.up);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }

        // Aplica velocidade para a frente e corta deslize lateral.
        void ApplyMovement()
        {
            speed = Mathf.MoveTowards(speed, maxSpeed * speedMultiplier, acceleration * Time.fixedDeltaTime);

            Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
            localVelocity.x = Mathf.Lerp(localVelocity.x, 0f, sideGrip * Time.fixedDeltaTime);
            localVelocity.z = speed;
            rb.linearVelocity = transform.TransformDirection(localVelocity);
        }
    }
}
