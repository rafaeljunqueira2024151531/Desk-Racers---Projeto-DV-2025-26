using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DeskRacers
{
    public class RaceManager : MonoBehaviour
    {
        [Header("Corrida")]
        public ArcadeCarController player;
        public string trackName = "Setup Gamer";
        public int totalLaps = 3;
        public int checkpointCount = 4;
        public Transform startRespawnPoint;

        [Header("UI")]
        public Text speedText;
        public Text lapText;
        public Text positionText;
        public Text powerUpText;
        public Text messageText;
        public GameObject pausePanel;

        int lap = 1;
        int nextCheckpoint;
        float elapsedTime;
        bool paused;
        bool unlockAll;
        bool raceFinished;
        Vector3 lastCheckpointPosition;
        Quaternion lastCheckpointRotation;
        RaceCheckpoint[] checkpoints;

        string SavePath => Path.Combine(Application.persistentDataPath, "deskracers_save.json");
        string LogPath => Path.Combine(Application.persistentDataPath, "log.txt");

        // Inicializa o estado da corrida e fecha o menu de pausa.
        void Start()
        {
            Time.timeScale = 1f;
            if (player != null)
            {
                Transform respawn = startRespawnPoint != null ? startRespawnPoint : player.transform;
                lastCheckpointPosition = respawn.position;
                lastCheckpointRotation = respawn.rotation;
            }

            checkpoints = FindObjectsByType<RaceCheckpoint>(FindObjectsInactive.Include);
            RefreshCheckpointVisuals();

            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }

        // Actualiza tempo, UI, pausa e cheats globais.
        void Update()
        {
            if (!paused && !raceFinished)
            {
                elapsedTime += Time.deltaTime;
            }

            Keyboard keyboard = Keyboard.current;
            Gamepad pad = Gamepad.current;
            bool pausePressed = keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
            pausePressed = pausePressed || (pad != null && pad.startButton.wasPressedThisFrame);

            if (pausePressed)
            {
                TogglePause();
            }

            if (keyboard != null && keyboard.f2Key.wasPressedThisFrame)
            {
                unlockAll = true;
                ShowMessage("Cheat F2: pistas desbloqueadas.");
            }

            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                RespawnAtLastCheckpoint();
            }

            if (pad != null && (pad.selectButton.wasPressedThisFrame || pad.leftStickButton.wasPressedThisFrame))
            {
                RespawnAtLastCheckpoint();
            }

            RefreshHud();
        }

        // Actualiza os textos do HUD com os dados actuais.
        void RefreshHud()
        {
            if (player == null)
            {
                return;
            }

            if (speedText != null)
            {
                speedText.text = $"{player.SpeedMiniUnits:000} cm/s";
            }

            if (lapText != null)
            {
                lapText.text = $"{lap}/{totalLaps}";
            }

            if (positionText != null)
            {
                positionText.text = "1/4";
            }

            if (powerUpText != null)
            {
                powerUpText.text = player.currentPowerUp.ToString();
            }
        }

        // Abre ou fecha o menu de pausa.
        public void TogglePause()
        {
            paused = !paused;
            Time.timeScale = paused ? 0f : 1f;

            if (player != null)
            {
                player.SetInputLocked(paused);
            }

            if (pausePanel != null)
            {
                pausePanel.SetActive(paused);
            }
        }

        // Regista a passagem por um checkpoint na ordem certa.
        public void RegisterCheckpoint(int checkpointIndex, Transform checkpointTransform)
        {
            if (raceFinished)
            {
                return;
            }

            if (checkpointIndex != nextCheckpoint)
            {
                return;
            }

            SaveCheckpointRespawn(checkpointTransform);
            nextCheckpoint++;
            if (nextCheckpoint >= checkpointCount)
            {
                nextCheckpoint = 0;
            }

            RefreshCheckpointVisuals();
        }

        // Tenta contar uma volta quando o jogador passa pela meta.
        public void TryRegisterLap(int finishCheckpointIndex, Transform finishTransform)
        {
            if (raceFinished)
            {
                return;
            }

            if (nextCheckpoint != finishCheckpointIndex)
            {
                ShowMessage("Volta invalida: passa pelos checkpoints.");
                return;
            }

            SaveCheckpointRespawn(finishTransform);
            nextCheckpoint = 0;
            lap++;
            if (lap > totalLaps)
            {
                lap = totalLaps;
                FinishRace();
            }
            else
            {
                ShowMessage($"Volta {lap}/{totalLaps}");
            }

            RefreshCheckpointVisuals();
        }

        // Termina a corrida e bloqueia novos checkpoints.
        void FinishRace()
        {
            raceFinished = true;
            nextCheckpoint = -1;

            if (player != null)
            {
                player.SetInputLocked(true);
            }

            ShowMessage($"Corrida terminada em {FormatTime(elapsedTime)}");
            RefreshCheckpointVisuals();
        }

        // Volta o jogador ao ultimo checkpoint valido.
        public void RespawnAtLastCheckpoint()
        {
            if (player == null || raceFinished)
            {
                return;
            }

            player.TeleportTo(lastCheckpointPosition, lastCheckpointRotation, player.Coins);
            ShowMessage("Reposicionado no ultimo checkpoint.");
        }

        // Guarda a posicao e rotacao usadas para respawn.
        void SaveCheckpointRespawn(Transform checkpointTransform)
        {
            if (checkpointTransform == null)
            {
                return;
            }

            lastCheckpointPosition = checkpointTransform.position;
            lastCheckpointRotation = checkpointTransform.rotation;
        }

        // Grava o estado essencial da corrida em JSON.
        public void SaveGame()
        {
            if (player == null)
            {
                return;
            }

            SaveData data = new SaveData
            {
                sceneName = SceneManager.GetActiveScene().name,
                trackName = trackName,
                position = player.transform.position,
                rotation = player.transform.rotation,
                lap = lap,
                nextCheckpoint = nextCheckpoint,
                elapsedTime = elapsedTime,
                coins = player.Coins,
                unlockAll = unlockAll,
                raceFinished = raceFinished,
                lastCheckpointPosition = lastCheckpointPosition,
                lastCheckpointRotation = lastCheckpointRotation
            };

            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
            AppendLog($"Save executado na Volta {lap} aos {FormatTime(elapsedTime)}");
            ShowMessage("Jogo gravado.");
        }

        // Carrega o estado guardado se existir um save.
        public void LoadGame()
        {
            if (!File.Exists(SavePath) || player == null)
            {
                ShowMessage("Nao existe save.");
                return;
            }

            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            lap = Mathf.Clamp(data.lap, 1, totalLaps);
            nextCheckpoint = Mathf.Clamp(data.nextCheckpoint, 0, checkpointCount - 1);
            elapsedTime = Mathf.Max(0f, data.elapsedTime);
            unlockAll = data.unlockAll;
            raceFinished = data.raceFinished;
            lastCheckpointPosition = data.lastCheckpointPosition;
            lastCheckpointRotation = data.lastCheckpointRotation;
            player.TeleportTo(data.position, data.rotation, data.coins);
            player.SetInputLocked(raceFinished);
            AppendLog($"Load executado na Volta {lap} aos {FormatTime(elapsedTime)}");
            ShowMessage("Jogo carregado.");
            RefreshCheckpointVisuals();
        }

        // Mostra o proximo checkpoint e apaga os restantes indicadores.
        void RefreshCheckpointVisuals()
        {
            if (checkpoints == null)
            {
                return;
            }

            foreach (RaceCheckpoint checkpoint in checkpoints)
            {
                bool shouldShow = !raceFinished && checkpoint.checkpointIndex == nextCheckpoint;
                checkpoint.SetVisualActive(shouldShow);
            }
        }

        // Recarrega a scene actual.
        public void RestartRace()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // Volta para a scene de menu principal.
        public void BackToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        // Escreve uma linha no log anti-save scumming.
        void AppendLog(string line)
        {
            File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {line}{Environment.NewLine}");
        }

        // Mostra uma mensagem temporaria no ecra.
        void ShowMessage(string message)
        {
            if (messageText == null)
            {
                return;
            }

            messageText.text = message;
            CancelInvoke(nameof(ClearMessage));
            Invoke(nameof(ClearMessage), 3f);
        }

        // Limpa a mensagem temporaria do HUD.
        void ClearMessage()
        {
            if (messageText != null)
            {
                messageText.text = string.Empty;
            }
        }

        // Formata segundos em mm:ss.
        static string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
    }

    [Serializable]
    public class SaveData
    {
        public string sceneName;
        public string trackName;
        public Vector3 position;
        public Quaternion rotation;
        public int lap;
        public int nextCheckpoint;
        public float elapsedTime;
        public int coins;
        public bool unlockAll;
        public bool raceFinished;
        public Vector3 lastCheckpointPosition;
        public Quaternion lastCheckpointRotation;
    }
}
