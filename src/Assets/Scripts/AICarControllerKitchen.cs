using UnityEngine;

namespace DeskRacers
{
    [RequireComponent(typeof(Rigidbody))]
    public class AICarControllerKitchen : MonoBehaviour
    {
        public Transform[] waypoints;
        public float acceleration = 10f;
        public float maxSpeed = 7f;
        public float turnSpeed = 130f;
        public float waypointRadius = 0.8f;
        public float sideGrip = 10f;
        public float randomSpeedOffset = 0.8f;
        public float sharpTurnSlowAngle = 45f;
        public float stopToTurnAngle = 95f;
        public float skipWaypointIfStuckSeconds = 1.2f;

        [Header("Evitar obstaculos")]
        public LayerMask obstacleLayers = ~0;
        public float obstacleCheckDistance = 1.2f;
        public float obstacleSensorRadius = 0.12f;
        public float obstacleSideOffset = 0.22f;
        public float obstacleTurnStrength = 1.4f;
        public float obstacleSlowMultiplier = 0.45f;
        public float stuckSpeedLimit = 0.15f;
        public float stuckTimeToReverse = 0.8f;
        public float reverseSeconds = 0.65f;

        Rigidbody rb;
        int currentWaypoint;
        int completedLaps;
        float speed;
        float speedMultiplier = 1f;
        float avoidDirection;
        float stuckTimer;
        float reverseTimer;
        float angleToTarget;

        public int CurrentWaypoint => currentWaypoint;
        public int CompletedLaps => completedLaps;
        public int WaypointCount => waypoints != null ? waypoints.Length : 0;
        public float ProgressDistance => currentWaypoint * 1000f - DistanceToCurrentWaypoint();
        public float DistanceToWaypoint => DistanceToCurrentWaypoint();

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

            UpdateStuckState();
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
                GoToNextWaypoint();
                stuckTimer = 0f;
                return;
            }

            if (reverseTimer > 0f)
            {
                reverseTimer -= Time.fixedDeltaTime;
                float reverseTurn = avoidDirection == 0f ? 1f : avoidDirection;
                rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, reverseTurn * turnSpeed * Time.fixedDeltaTime, 0f));
                return;
            }

            Vector3 desiredDirection = toWaypoint.normalized;
            angleToTarget = Vector3.Angle(transform.forward, desiredDirection);
            Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }

        // Mede a distancia ate ao waypoint que a IA esta a seguir.
        float DistanceToCurrentWaypoint()
        {
            if (waypoints == null || waypoints.Length == 0 || currentWaypoint >= waypoints.Length)
            {
                return 9999f;
            }

            Vector3 toWaypoint = waypoints[currentWaypoint].position - transform.position;
            toWaypoint.y = 0f;
            return toWaypoint.magnitude;
        }

        // Aplica velocidade para a frente e corta deslize lateral.
        void ApplyMovement()
        {
            float turnSpeedMultiplier = CalculateTurnSpeedMultiplier();
            float targetSpeed = reverseTimer > 0f ? -maxSpeed * 0.45f : maxSpeed * speedMultiplier * turnSpeedMultiplier;
            speed = Mathf.MoveTowards(speed, targetSpeed, acceleration * Time.fixedDeltaTime);

            Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
            localVelocity.x = Mathf.Lerp(localVelocity.x, 0f, sideGrip * Time.fixedDeltaTime);
            localVelocity.z = speed;
            rb.linearVelocity = transform.TransformDirection(localVelocity);
        }

        // Reduz a velocidade quando o carro ainda nao esta virado para o waypoint.
        float CalculateTurnSpeedMultiplier()
        {
            if (reverseTimer > 0f)
            {
                return 1f;
            }

            if (angleToTarget >= stopToTurnAngle)
            {
                return 0f;
            }

            if (angleToTarget <= sharpTurnSlowAngle)
            {
                return 1f;
            }

            return Mathf.InverseLerp(stopToTurnAngle, sharpTurnSlowAngle, angleToTarget);
        }

        // Detecta quando a IA ficou presa e inicia uma manobra curta de marcha-atras.
        void UpdateStuckState()
        {
            bool tryingToMove = speed > maxSpeed * 0.25f;
            bool barelyMoving = rb.linearVelocity.magnitude < stuckSpeedLimit;

            if (tryingToMove && barelyMoving)
            {
                stuckTimer += Time.fixedDeltaTime;
            }
            else
            {
                stuckTimer = 0f;
            }

            if (stuckTimer >= stuckTimeToReverse)
            {
                reverseTimer = reverseSeconds;
                avoidDirection = PickTurnDirectionToWaypoint();
            }

            if (stuckTimer >= skipWaypointIfStuckSeconds)
            {
                GoToNextWaypoint();
                stuckTimer = 0f;
                reverseTimer = 0f;
                speed = 0f;
            }
        }

        // Avanca para o waypoint seguinte.
        void GoToNextWaypoint()
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Length)
            {
                currentWaypoint = 0;
                completedLaps++;
            }
        }

        // Escolhe para que lado virar para apontar ao waypoint.
        float PickTurnDirectionToWaypoint()
        {
            Transform waypoint = waypoints[currentWaypoint];
            Vector3 toWaypoint = waypoint.position - transform.position;
            toWaypoint.y = 0f;
            return Vector3.Dot(toWaypoint.normalized, transform.right) >= 0f ? 1f : -1f;
        }

        // Desenha raios de deteccao no editor para afinar a IA.
        void OnDrawGizmosSelected()
        {
            Vector3 origin = transform.position + Vector3.up * 0.08f;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(origin + transform.forward * obstacleCheckDistance, obstacleSensorRadius);
            Gizmos.DrawWireSphere(origin - transform.right * obstacleSideOffset + transform.forward * obstacleCheckDistance, obstacleSensorRadius);
            Gizmos.DrawWireSphere(origin + transform.right * obstacleSideOffset + transform.forward * obstacleCheckDistance, obstacleSensorRadius);
        }
    }
}
