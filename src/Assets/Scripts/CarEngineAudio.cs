using UnityEngine;

namespace DeskRacers
{
    [RequireComponent(typeof(AudioSource))]
    public class CarEngineAudio : MonoBehaviour
    {
        public ArcadeCarController car;
        public Rigidbody carRigidbody;

        [Header("Clips")]
        public AudioClip idleClip;
        public AudioClip lowClip;
        public AudioClip mediumClip;
        public AudioClip highClip;
        public AudioClip startupClip;

        [Header("Som")]
        public float minPitch = 0.8f;
        public float maxPitch = 1.55f;
        public float minVolume = 0.18f;
        public float maxVolume = 0.65f;

        AudioSource engineSource;
        AudioSource startupSource;
        AudioClip currentLoopClip;
        bool muted;
        bool paused;

        // Prepara as fontes de audio do motor e do arranque.
        void Awake()
        {
            engineSource = GetComponent<AudioSource>();
            engineSource.loop = true;
            engineSource.playOnAwake = false;
            engineSource.spatialBlend = 0.65f;

            startupSource = gameObject.AddComponent<AudioSource>();
            startupSource.loop = false;
            startupSource.playOnAwake = false;
            startupSource.spatialBlend = 0.65f;
        }

        // Toca o som de arranque e inicia o loop do motor.
        void Start()
        {
            muted = false;
            paused = false;

            if (carRigidbody == null)
            {
                carRigidbody = GetComponent<Rigidbody>();
            }

            if (startupClip != null)
            {
                startupSource.clip = startupClip;
                startupSource.Play();
            }

            SetLoopClip(idleClip);
        }

        // Actualiza clip, pitch e volume de acordo com a velocidade.
        void Update()
        {
            if (muted || paused || carRigidbody == null)
            {
                return;
            }

            float speed01 = Mathf.Clamp01(carRigidbody.linearVelocity.magnitude / 8f);
            AudioClip wantedClip = PickClip(speed01);
            SetLoopClip(wantedClip);

            engineSource.pitch = Mathf.Lerp(minPitch, maxPitch, speed01);
            engineSource.volume = Mathf.Lerp(minVolume, maxVolume, speed01);
        }

        // Desliga todos os sons do motor.
        public void StopEngine()
        {
            muted = true;
            if (engineSource != null)
            {
                engineSource.Stop();
            }

            if (startupSource != null)
            {
                startupSource.Stop();
            }
        }

        // Volta a permitir sons do motor.
        public void ResumeEngine()
        {
            muted = false;
            paused = false;
            currentLoopClip = null;
            SetLoopClip(idleClip);
        }

        // Pausa temporariamente o som do motor.
        public void PauseEngine()
        {
            paused = true;
            if (engineSource != null)
            {
                engineSource.Pause();
            }

            if (startupSource != null)
            {
                startupSource.Pause();
            }
        }

        // Retoma o som do motor apos uma pausa.
        public void UnpauseEngine()
        {
            if (muted)
            {
                return;
            }

            paused = false;
            if (engineSource != null)
            {
                engineSource.UnPause();
                if (!engineSource.isPlaying && engineSource.clip != null)
                {
                    engineSource.Play();
                }
            }

            if (startupSource != null)
            {
                startupSource.UnPause();
            }
        }

        // Escolhe o clip de motor mais adequado ao nivel de velocidade.
        AudioClip PickClip(float speed01)
        {
            if (speed01 < 0.12f && idleClip != null)
            {
                return idleClip;
            }

            if (speed01 < 0.42f && lowClip != null)
            {
                return lowClip;
            }

            if (speed01 < 0.76f && mediumClip != null)
            {
                return mediumClip;
            }

            return highClip != null ? highClip : mediumClip;
        }

        // Troca o loop actual sem reiniciar se o clip for o mesmo.
        void SetLoopClip(AudioClip clip)
        {
            if (clip == null || currentLoopClip == clip)
            {
                return;
            }

            currentLoopClip = clip;
            engineSource.clip = clip;
            engineSource.Play();
        }
    }
}
