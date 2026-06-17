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
        float speed;
        float speedMultiplier = 1f;
        float targetSpeedMultiplier = 1f;
        float avoidDirection;
        float stuckTimer;
        float reverseTimer;

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
                currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
                return;
            }

            if (reverseTimer > 0f)
            {
                reverseTimer -= Time.fixedDeltaTime;
                float reverseTurn = avoidDirection == 0f ? 1f : avoidDirection;
                rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, reverseTurn * turnSpeed * Time.fixedDeltaTime, 0f));
                return;
            }

            Vector3 desiredDirection = AvoidObstacles(toWaypoint.normalized);
            Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }

        // Ajusta a direccao quando existe obstaculo a frente do carro.
        Vector3 AvoidObstacles(Vector3 desiredDirection)
        {
            Vector3 origin = transform.position + Vector3.up * 0.05f;
            Vector3 leftOrigin = origin - transform.right * obstacleSideOffset;
            Vector3 rightOrigin = origin + transform.right * obstacleSideOffset;

            bool centerBlocked = Physics.SphereCast(origin, obstacleSensorRadius, transform.forward, out _, obstacleCheckDistance, obstacleLayers, QueryTriggerInteraction.Ignore);
            bool leftBlocked = Physics.SphereCast(leftOrigin, obstacleSensorRadius, transform.forward, out _, obstacleCheckDistance, obstacleLayers, QueryTriggerInteraction.Ignore);
            bool rightBlocked = Physics.SphereCast(rightOrigin, obstacleSensorRadius, transform.forward, out _, obstacleCheckDistance, obstacleLayers, QueryTriggerInteraction.Ignore);

            targetSpeedMultiplier = centerBlocked || leftBlocked || rightBlocked ? obstacleSlowMultiplier : 1f;

            if (!centerBlocked && !leftBlocked && !rightBlocked)
            {
                avoidDirection = 0f;
                return desiredDirection;
            }

            if (leftBlocked && !rightBlocked)
            {
                avoidDirection = 1f;
            }
            else if (rightBlocked && !leftBlocked)
            {
                avoidDirection = -1f;
            }
            else
            {
                avoidDirection = Vector3.Dot(desiredDirection, transform.right) >= 0f ? 1f : -1f;
            }

            Vector3 avoidance = (desiredDirection + transform.right * avoidDirection * obstacleTurnStrength).normalized;
            return avoidance;
        }

        // Aplica velocidade para a frente e corta deslize lateral.
        void ApplyMovement()
        {
            float targetSpeed = reverseTimer > 0f ? -maxSpeed * 0.35f : maxSpeed * speedMultiplier * targetSpeedMultiplier;
            speed = Mathf.MoveTowards(speed, targetSpeed, acceleration * Time.fixedDeltaTime);

            Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
            localVelocity.x = Mathf.Lerp(localVelocity.x, 0f, sideGrip * Time.fixedDeltaTime);
            localVelocity.z = speed;
            rb.linearVelocity = transform.TransformDirection(localVelocity);
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
                stuckTimer = 0f;
                avoidDirection = avoidDirection == 0f ? 1f : avoidDirection;
            }
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
