using UnityEngine;
using UnityEngine.InputSystem;

namespace DeskRacers
{
    [RequireComponent(typeof(Rigidbody))]
    public class ArcadeCarController : MonoBehaviour
    {
        [Header("Movimento")]
        public float acceleration = 30f;
        public float reverseAcceleration = 14f;
        public float maxSpeed = 18f;
        public float turnSpeed = 115f;
        public float normalGrip = 7f;
        public float driftGrip = 2.4f;
        public float brakeDrag = 2.5f;
        public float maxReverseSpeed = 3f;
        public float coastDeceleration = 1.2f;
        public float directionChangeMultiplier = 2.5f;
        public float launchSpeed = 1.2f;

        [Header("Drift e turbo")]
        public float driftChargeSpeed = 0.7f;
        public float maxDriftCharge = 1.5f;
        public float turboForce = 22f;
        public float turboTime = 0.75f;

        [Header("Power-ups")]
        public float jumpForce = 7.5f;
        public PowerUpType currentPowerUp = PowerUpType.Spring;

        [Header("Cheats")]
        public bool ghostMode;
        public bool unlimitedTurbo;

        Rigidbody rb;
        Collider[] colliders;
        float throttle;
        float steering;
        float turboTimer;
        float driftCharge;
        float currentSpeed;
        bool driftHeld;
        bool inputLocked;
        float gripMultiplier = 1f;

        public int Coins { get; private set; }
        public float SpeedMiniUnits => rb.linearVelocity.magnitude * 10f;

        // Prepara referencias ao Rigidbody e aos colliders do carro.
        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            colliders = GetComponentsInChildren<Collider>();
            rb.mass = 0.65f;
            rb.linearDamping = 0.05f;
            rb.angularDamping = 1.6f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        // Le input de teclado/comando e trata cheats de teste.
        void Update()
        {
            if (inputLocked)
            {
                throttle = 0f;
                steering = 0f;
                return;
            }

            ReadKeyboardInput();
            ReadGamepadInput();
            HandleCheats();
            HandlePowerUpInput();
        }

        // Aplica fisica arcade ao carro em tempo fixo.
        void FixedUpdate()
        {
            ApplyAcceleration();
            ApplySteering();
            ApplyTurbo();
        }

        // Le WASD, setas, espaco e shift.
        void ReadKeyboardInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            throttle = 0f;
            steering = 0f;

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                throttle += 1f;
            }

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                throttle -= 1f;
            }

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                steering -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                steering += 1f;
            }

            bool wasDrifting = driftHeld;
            driftHeld = keyboard.spaceKey.isPressed;

            if (wasDrifting && keyboard.spaceKey.wasReleasedThisFrame)
            {
                ReleaseDriftTurbo();
            }
        }

        // Le comando estilo PlayStation atraves do Input System.
        void ReadGamepadInput()
        {
            Gamepad pad = Gamepad.current;
            if (pad == null)
            {
                return;
            }

            Vector2 leftStick = pad.leftStick.ReadValue();
            steering = Mathf.Abs(leftStick.x) > Mathf.Abs(steering) ? leftStick.x : steering;

            float rightTrigger = pad.rightTrigger.ReadValue();
            float leftTrigger = pad.leftTrigger.ReadValue();
            if (rightTrigger > 0.05f || leftTrigger > 0.05f)
            {
                throttle = rightTrigger - leftTrigger;
            }

            bool wasDrifting = driftHeld;
            driftHeld = driftHeld || pad.buttonEast.isPressed;
            if (wasDrifting && pad.buttonEast.wasReleasedThisFrame)
            {
                ReleaseDriftTurbo();
            }
        }

        // Mantem velocidade lateral controlada para simular aderencia arcade.
        void ApplySideGrip()
        {
            Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
            float targetGrip = driftHeld ? driftGrip : normalGrip;
            localVelocity.x = Mathf.Lerp(localVelocity.x, 0f, targetGrip * gripMultiplier * Time.fixedDeltaTime);
            rb.linearVelocity = transform.TransformDirection(localVelocity);

            if (driftHeld && Mathf.Abs(steering) > 0.2f && rb.linearVelocity.magnitude > 4f)
            {
                driftCharge = Mathf.Min(maxDriftCharge, driftCharge + driftChargeSpeed * Time.fixedDeltaTime);
            }
        }

        // Controla a velocidade do carro directamente para garantir embalo e marcha atras.
        void ApplyAcceleration()
        {
            Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
            float targetSpeed = 0f;
            float speedChange = coastDeceleration;

            if (throttle > 0.05f)
            {
                if (currentSpeed < -0.05f)
                {
                    targetSpeed = 0f;
                    speedChange = acceleration * directionChangeMultiplier;
                }
                else
                {
                    targetSpeed = maxSpeed;
                    speedChange = acceleration;

                    if (currentSpeed < launchSpeed)
                    {
                        currentSpeed = launchSpeed;
                    }
                }
            }
            else if (throttle < -0.05f)
            {
                if (currentSpeed > 0.05f)
                {
                    targetSpeed = 0f;
                    speedChange = reverseAcceleration * directionChangeMultiplier;
                }
                else
                {
                    targetSpeed = -maxReverseSpeed;
                    speedChange = reverseAcceleration;

                    if (currentSpeed > -launchSpeed)
                    {
                        currentSpeed = -Mathf.Min(launchSpeed, maxReverseSpeed);
                    }
                }
            }

            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedChange * Time.fixedDeltaTime);

            float targetGrip = driftHeld ? driftGrip : normalGrip;
            if (Mathf.Abs(steering) < 0.05f)
            {
                localVelocity.x = 0f;
                rb.angularVelocity = new Vector3(0f, Mathf.Lerp(rb.angularVelocity.y, 0f, 12f * Time.fixedDeltaTime), 0f);
            }
            else
            {
                localVelocity.x = Mathf.Lerp(localVelocity.x, 0f, targetGrip * gripMultiplier * Time.fixedDeltaTime);
            }

            localVelocity.z = currentSpeed;
            rb.linearVelocity = transform.TransformDirection(localVelocity);
            rb.linearDamping = 0.05f;
        }

        // Roda o carro de forma arcade, mais estavel a alta velocidade.
        void ApplySteering()
        {
            if (Mathf.Abs(currentSpeed) < 0.05f)
            {
                return;
            }

            float speedFactor = Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxSpeed);
            float direction = currentSpeed >= 0f ? 1f : -1f;
            float turn = steering * turnSpeed * direction * Mathf.Lerp(1f, 0.55f, speedFactor) * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));
        }

        // Aplica turbo enquanto o temporizador estiver activo.
        void ApplyTurbo()
        {
            if (turboTimer <= 0f)
            {
                return;
            }

            turboTimer -= Time.fixedDeltaTime;
            currentSpeed = Mathf.Min(maxSpeed + turboForce, currentSpeed + turboForce * Time.fixedDeltaTime);
        }

        // Converte carga de drift num pequeno turbo.
        void ReleaseDriftTurbo()
        {
            if (driftCharge > 0.25f)
            {
                turboTimer = Mathf.Lerp(0.2f, turboTime, driftCharge / maxDriftCharge);
            }

            driftCharge = 0f;
        }

        // Usa o power-up actual quando o jogador carrega no botao.
        void HandlePowerUpInput()
        {
            Keyboard keyboard = Keyboard.current;
            Gamepad pad = Gamepad.current;
            bool keyboardPressed = keyboard != null && keyboard.leftShiftKey.wasPressedThisFrame;
            bool padPressed = pad != null && pad.buttonSouth.wasPressedThisFrame;

            if (keyboardPressed || padPressed)
            {
                UsePowerUp();
            }
        }

        // Activa cheats rapidos para demonstracao em aula.
        void HandleCheats()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.f1Key.wasPressedThisFrame)
            {
                currentSpeed = maxSpeed;
            }

            if (keyboard.f3Key.wasPressedThisFrame)
            {
                SetGhostMode(!ghostMode);
            }

            if (keyboard.f4Key.wasPressedThisFrame)
            {
                unlimitedTurbo = !unlimitedTurbo;
            }
        }

        // Executa o efeito do power-up guardado.
        public void UsePowerUp()
        {
            if (currentPowerUp == PowerUpType.Spring)
            {
                rb.AddForce(Vector3.up * jumpForce + transform.forward * 3f, ForceMode.VelocityChange);
            }
            else if (currentPowerUp == PowerUpType.Turbo)
            {
                turboTimer = turboTime;
            }
            else if (currentPowerUp == PowerUpType.Oil)
            {
                turboTimer = 0.25f;
            }

            if (!unlimitedTurbo)
            {
                currentPowerUp = PowerUpType.None;
            }
        }

        // Guarda um novo power-up para uso posterior.
        public void SetPowerUp(PowerUpType powerUp)
        {
            currentPowerUp = powerUp;
        }

        // Soma moedas apanhadas na pista.
        public void AddCoins(int amount)
        {
            Coins += amount;
        }

        // Altera temporariamente a aderencia do carro.
        public void SetGripMultiplier(float multiplier)
        {
            gripMultiplier = multiplier;
        }

        // Bloqueia ou desbloqueia input quando ha menus abertos.
        public void SetInputLocked(bool locked)
        {
            inputLocked = locked;
        }

        // Liga ou desliga colisoes do carro para teste rapido da pista.
        public void SetGhostMode(bool enabled)
        {
            ghostMode = enabled;
            foreach (Collider carCollider in colliders)
            {
                carCollider.isTrigger = enabled;
            }
        }

        // Coloca o carro numa posicao guardada e limpa velocidades.
        public void TeleportTo(Vector3 position, Quaternion rotation, int savedCoins)
        {
            transform.SetPositionAndRotation(position, rotation);
            Coins = savedCoins;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            currentSpeed = 0f;
        }
    }

    public enum PowerUpType
    {
        None,
        Spring,
        Turbo,
        Oil
    }
}
