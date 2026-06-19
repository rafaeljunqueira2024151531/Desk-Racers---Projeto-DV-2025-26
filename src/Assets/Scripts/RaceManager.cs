using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
        public TMP_Text speedText;
        public TMP_Text lapText;
        public TMP_Text positionText;
        public TMP_Text powerUpText;
        public TMP_Text timerText;
        public TMP_Text messageText;
        public GameObject pausePanel;
        public GameObject optionsPanel;
        public Slider volumeSlider;
        public GameObject firstPauseButton;

        [Header("Fim da corrida")]
        public GameObject finishPanel;
        public TMP_Text finalPositionText;
        public GameObject firstFinishButton;
        public string nextTrackSceneName;

        int lap = 1;
        int nextCheckpoint;
        float elapsedTime;
        bool paused;
        bool unlockAll;
        bool raceFinished;
        Vector3 lastCheckpointPosition;
        Quaternion lastCheckpointRotation;
        RaceCheckpoint[] checkpoints;
        AICarController[] opponents;

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
            opponents = FindObjectsByType<AICarController>(FindObjectsInactive.Include);
            RefreshCheckpointVisuals();

            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }

            if (optionsPanel != null)
            {
                optionsPanel.SetActive(false);
            }

            if (volumeSlider != null)
            {
                volumeSlider.value = AudioListener.volume;
                volumeSlider.onValueChanged.AddListener(SetVolume);
            }

            if (finishPanel != null)
            {
                finishPanel.SetActive(false);
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
            bool backPressed = pausePressed || (pad != null && pad.buttonEast.wasPressedThisFrame);
            pausePressed = pausePressed || (pad != null && pad.startButton.wasPressedThisFrame);

            if (paused && backPressed)
            {
                BackFromPauseOrOptions();
                RefreshHud();
                return;
            }

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
                int racers = 1 + (opponents != null ? opponents.Length : 0);
                positionText.text = $"{CalculatePlayerPosition()}/{racers}";
            }

            if (powerUpText != null)
            {
                powerUpText.text = player.currentPowerUp.ToString();
            }

            if (timerText != null)
            {
                timerText.text = FormatTimeWithCentiseconds(elapsedTime);
            }
        }

        // Calcula a posicao do jogador comparando progresso com os oponentes.
        int CalculatePlayerPosition()
        {
            if (player == null || opponents == null)
            {
                return 1;
            }

            float playerProgress = GetPlayerProgress();
            int position = 1;

            foreach (AICarController opponent in opponents)
            {
                if (opponent != null && GetOpponentProgress(opponent) > playerProgress)
                {
                    position++;
                }
            }

            return position;
        }

        // Calcula progresso de um oponente, incluindo voltas ja completadas.
        float GetOpponentProgress(AICarController opponent)
        {
            if (opponent.WaypointCount <= 0)
            {
                return -9999f;
            }

            float normalizedCheckpoint = (opponent.CurrentWaypoint / (float)opponent.WaypointCount) * checkpointCount;
            float progress = (opponent.CompletedLaps * checkpointCount + normalizedCheckpoint) * 1000f;
            return progress - opponent.DistanceToWaypoint;
        }

        // Calcula progresso do jogador pelo proximo checkpoint e distancia ate ele.
        float GetPlayerProgress()
        {
            Transform targetCheckpoint = GetCheckpointTransform(nextCheckpoint);
            if (targetCheckpoint == null)
            {
                return lap * checkpointCount * 1000f;
            }

            Vector3 toCheckpoint = targetCheckpoint.position - player.transform.position;
            toCheckpoint.y = 0f;
            return ((lap - 1) * checkpointCount + nextCheckpoint) * 1000f - toCheckpoint.magnitude;
        }

        // Encontra o transform do checkpoint com o indice indicado.
        Transform GetCheckpointTransform(int checkpointIndex)
        {
            if (checkpoints == null)
            {
                return null;
            }

            foreach (RaceCheckpoint checkpoint in checkpoints)
            {
                if (checkpoint != null && checkpoint.checkpointIndex == checkpointIndex)
                {
                    return checkpoint.transform;
                }
            }

            return null;
        }

        // Abre ou fecha o menu de pausa.
        public void TogglePause()
        {
            if (raceFinished)
            {
                return;
            }

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

            SetRaceAudioPaused(paused);

            if (paused)
            {
                SelectUiObject(firstPauseButton);
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

            ShowFinishPanel();
            StopRaceAudio();
            RefreshCheckpointVisuals();
            Time.timeScale = 0f;
        }

        // Desliga os sons de motor quando a corrida acaba.
        void StopRaceAudio()
        {
            CarEngineAudio[] engineAudios = FindObjectsByType<CarEngineAudio>(FindObjectsInactive.Include);
            foreach (CarEngineAudio engineAudio in engineAudios)
            {
                engineAudio.StopEngine();
            }
        }

        // Pausa ou retoma os sons dos motores durante o menu de pausa.
        void SetRaceAudioPaused(bool audioPaused)
        {
            CarEngineAudio[] engineAudios = FindObjectsByType<CarEngineAudio>(FindObjectsInactive.Include);
            foreach (CarEngineAudio engineAudio in engineAudios)
            {
                if (audioPaused)
                {
                    engineAudio.PauseEngine();
                }
                else
                {
                    engineAudio.UnpauseEngine();
                }
            }
        }

        // Fecha o menu de pausa depois de save/load.
        void ClosePauseAfterSaveLoad()
        {
            if (!paused || raceFinished)
            {
                return;
            }

            paused = false;
            Time.timeScale = 1f;

            if (player != null)
            {
                player.SetInputLocked(false);
            }

            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }

            if (optionsPanel != null)
            {
                optionsPanel.SetActive(false);
            }

            SetRaceAudioPaused(false);
        }

        // Mostra a tela final com a classificacao do jogador.
        void ShowFinishPanel()
        {
            if (finishPanel != null)
            {
                finishPanel.SetActive(true);
            }

            if (finalPositionText != null)
            {
                int racers = 1 + (opponents != null ? opponents.Length : 0);
                finalPositionText.text = $"Terminaste em {CalculatePlayerPosition()}/{racers}\nTempo: {FormatTimeWithCentiseconds(elapsedTime)}";
            }

            SelectUiObject(firstFinishButton);
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

        // Abre ou fecha o painel de opcoes.
        public void ToggleOptionsPanel()
        {
            if (optionsPanel != null)
            {
                optionsPanel.SetActive(!optionsPanel.activeSelf);
            }
        }

        // Volta atras no menu de pausa ou retoma a corrida.
        public void BackFromPauseOrOptions()
        {
            if (optionsPanel != null && optionsPanel.activeSelf)
            {
                optionsPanel.SetActive(false);
                SelectUiObject(firstPauseButton);

                return;
            }

            TogglePause();
        }

        // Ajusta o volume geral do jogo.
        public void SetVolume(float value)
        {
            AudioListener.volume = value;
        }

        // Seleciona um botao da UI sem rebentar se a referencia foi apagada no editor.
        void SelectUiObject(GameObject target)
        {
            try
            {
                if (EventSystem.current == null || target == null || !target.activeInHierarchy)
                {
                    return;
                }

                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(target);
            }
            catch (MissingReferenceException)
            {
                // Acontece quando um botao foi apagado/recriado mas ainda ficou ligado no Inspector.
            }
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
            ClosePauseAfterSaveLoad();
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
            Time.timeScale = raceFinished ? 0f : 1f;
            if (raceFinished)
            {
                ShowFinishPanel();
                StopRaceAudio();
            }

            AppendLog($"Load executado na Volta {lap} aos {FormatTime(elapsedTime)}");
            ShowMessage("Jogo carregado.");
            RefreshCheckpointVisuals();
            ClosePauseAfterSaveLoad();
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

        // Carrega a proxima pista configurada no Inspector.
        public void LoadNextTrack()
        {
            Time.timeScale = 1f;
            if (!string.IsNullOrWhiteSpace(nextTrackSceneName))
            {
                SceneManager.LoadScene(nextTrackSceneName);
            }
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

        // Formata segundos em mm:ss.cc para o cronometro do HUD.
        static string FormatTimeWithCentiseconds(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            int centiseconds = Mathf.FloorToInt((time * 100f) % 100f);
            return $"{minutes:00}:{seconds:00}.{centiseconds:00}";
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
