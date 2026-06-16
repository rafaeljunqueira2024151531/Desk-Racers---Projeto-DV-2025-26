using UnityEngine;
using UnityEngine.InputSystem;

namespace DeskRacers
{
    [RequireComponent(typeof(Rigidbody))]
    public class DeskRacersCarController : MonoBehaviour
    {
        [Header("Movement")]
        public float acceleration = 28f;
        public float reverseAcceleration = 14f;
        public float maxSpeed = 20f;
        public float turnStrength = 110f;
        public float grip = 7f;
        public float driftGrip = 2.6f;
        public float jumpForce = 7.5f;

        [Header("Turbo")]
        public float turboForce = 18f;
        public float turboDuration = 0.8f;
        public float driftChargeRate = 0.42f;
        public float maxDriftCharge = 1.8f;

        [Header("State")]
        public bool ghostMode;
        public bool unlimitedTurbo;

        Rigidbody rb;
        Collider[] carColliders;
        float steerInput;
        float throttleInput;
        float turboTimer;
        float driftCharge;
        bool drifting;
        bool grounded;

        public float SpeedKmh => rb.linearVelocity.magnitude * 10.8f;
        public string PowerUpName { get; private set; } = "Mola";
        public int Coins { get; private set; }

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            carColliders = GetComponentsInChildren<Collider>();
            rb.mass = 0.55f;
            rb.linearDamping = 0.25f;
            rb.angularDamping = 1.8f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        void Update()
        {
            ReadInput();
            HandleCheats();

            if (WasPressed(Key.LeftShift))
            {
                UsePowerUp();
            }
        }

        void FixedUpdate()
        {
            grounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.85f);

            Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
            float speed01 = Mathf.Clamp01(rb.linearVelocity.magnitude / maxSpeed);
            float currentGrip = drifting ? driftGrip : grip;
            localVelocity.x = Mathf.Lerp(localVelocity.x, 0f, currentGrip * Time.fixedDeltaTime);
            rb.linearVelocity = transform.TransformDirection(localVelocity);

            if (grounded)
            {
                float accel = throttleInput >= 0f ? acceleration : reverseAcceleration;
                if (rb.linearVelocity.magnitude < maxSpeed || throttleInput < 0f)
                {
                    rb.AddForce(transform.forward * throttleInput * accel, ForceMode.Acceleration);
                }

                float steerPower = Mathf.Lerp(1f, 0.45f, speed01);
                rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, steerInput * turnStrength * steerPower * Time.fixedDeltaTime, 0f));
            }

            if (turboTimer > 0f)
            {
                turboTimer -= Time.fixedDeltaTime;
                rb.AddForce(transform.forward * turboForce, ForceMode.Acceleration);
            }
        }

        void ReadInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                throttleInput = 0f;
                steerInput = 0f;
                drifting = false;
                return;
            }

            throttleInput = 0f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                throttleInput += 1f;
            }

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                throttleInput -= 1f;
            }

            steerInput = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                steerInput -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                steerInput += 1f;
            }

            drifting = keyboard.spaceKey.isPressed && Mathf.Abs(steerInput) > 0.1f && rb.linearVelocity.magnitude > 4f;
            if (drifting)
            {
                driftCharge = Mathf.Min(maxDriftCharge, driftCharge + driftChargeRate * Time.deltaTime);
            }

            if (keyboard.spaceKey.wasReleasedThisFrame && driftCharge > 0.35f)
            {
                turboTimer = Mathf.Max(turboTimer, Mathf.Lerp(0.25f, turboDuration, driftCharge / maxDriftCharge));
                driftCharge = 0f;
            }
        }

        void HandleCheats()
        {
            if (WasPressed(Key.F1))
            {
                rb.linearVelocity = transform.forward * maxSpeed;
            }

            if (WasPressed(Key.F3))
            {
                ghostMode = !ghostMode;
                foreach (Collider carCollider in carColliders)
                {
                    carCollider.excludeLayers = ghostMode ? LayerMask.GetMask("Default") : 0;
                }
            }

            if (WasPressed(Key.F4))
            {
                unlimitedTurbo = !unlimitedTurbo;
            }
        }

        public void UsePowerUp()
        {
            if (PowerUpName == "Mola")
            {
                rb.AddForce(Vector3.up * jumpForce + transform.forward * 3f, ForceMode.VelocityChange);
            }
            else
            {
                turboTimer = Mathf.Max(turboTimer, turboDuration);
            }

            if (!unlimitedTurbo)
            {
                PowerUpName = "Turbo";
            }
        }

        public void CollectCoin()
        {
            Coins++;
        }

        public void CollectPowerUp(string powerUpName)
        {
            PowerUpName = powerUpName;
        }

        public void ApplySlipperyPatch(float seconds)
        {
            CancelInvoke(nameof(RestoreGrip));
            grip = 2.4f;
            driftGrip = 1.2f;
            Invoke(nameof(RestoreGrip), seconds);
        }

        void RestoreGrip()
        {
            grip = 7f;
            driftGrip = 2.6f;
        }

        public void LoadState(Vector3 position, Quaternion rotation, int coins)
        {
            transform.SetPositionAndRotation(position, rotation);
            Coins = coins;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        static bool WasPressed(Key key)
        {
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard[key].wasPressedThisFrame;
        }
    }
}
