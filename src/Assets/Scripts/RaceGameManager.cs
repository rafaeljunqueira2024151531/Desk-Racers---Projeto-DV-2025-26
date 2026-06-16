using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DeskRacers
{
    public class RaceGameManager : MonoBehaviour
    {
        public DeskRacersCarController player;
        public Text speedText;
        public Text positionText;
        public Text lapText;
        public Text powerUpText;
        public Text messageText;
        public GameObject mainMenuPanel;
        public GameObject pausePanel;
        public GameObject creditsPanel;
        public Slider volumeSlider;
        public Slider sensitivitySlider;

        public int totalLaps = 3;
        public string currentTrack = "Setup Gamer";

        int lap = 1;
        float elapsed;
        bool paused;
        bool raceStarted;
        bool unlockAll;

        string SavePath => Path.Combine(Application.persistentDataPath, "deskracers_save.json");
        string LogPath => Path.Combine(Application.persistentDataPath, "log.txt");

        public int CurrentLap => lap;
        public float Elapsed => elapsed;

        void Start()
        {
            Time.timeScale = mainMenuPanel != null ? 0f : 1f;
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }

            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }

            if (creditsPanel != null)
            {
                creditsPanel.SetActive(false);
            }

            if (volumeSlider != null)
            {
                volumeSlider.value = AudioListener.volume;
                volumeSlider.onValueChanged.AddListener(value => AudioListener.volume = value);
            }

            if (sensitivitySlider != null && player != null)
            {
                sensitivitySlider.value = player.turnStrength;
                sensitivitySlider.onValueChanged.AddListener(value => player.turnStrength = value);
            }
        }

        void Update()
        {
            if (!paused)
            {
                elapsed += Time.deltaTime;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.escapeKey.wasPressedThisFrame)
                {
                    TogglePause();
                }

                if (keyboard.f2Key.wasPressedThisFrame)
                {
                    unlockAll = true;
                    ShowMessage("Cheat F2: todas as pistas desbloqueadas.");
                }
            }

            RefreshHud();
        }

        void RefreshHud()
        {
            if (player == null)
            {
                return;
            }

            if (speedText != null)
            {
                speedText.text = $"{player.SpeedKmh:000} cm/s";
            }

            if (positionText != null)
            {
                positionText.text = "1/4";
            }

            if (lapText != null)
            {
                lapText.text = $"{lap}/{totalLaps}";
            }

            if (powerUpText != null)
            {
                powerUpText.text = player.PowerUpName;
            }
        }

        public void TogglePause()
        {
            if (!raceStarted)
            {
                if (pausePanel != null)
                {
                    pausePanel.SetActive(false);
                }

                return;
            }

            paused = !paused;
            Time.timeScale = paused ? 0f : 1f;
            if (pausePanel != null)
            {
                pausePanel.SetActive(paused);
            }
        }

        public void StartRace()
        {
            raceStarted = true;
            paused = false;
            Time.timeScale = 1f;
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(false);
            }

            if (creditsPanel != null)
            {
                creditsPanel.SetActive(false);
            }
        }

        public void ShowCredits()
        {
            if (creditsPanel != null)
            {
                creditsPanel.SetActive(true);
            }
        }

        public void HideCredits()
        {
            if (creditsPanel != null)
            {
                creditsPanel.SetActive(false);
            }
        }

        public void RegisterLap()
        {
            lap++;
            if (lap > totalLaps)
            {
                lap = totalLaps;
                ShowMessage($"Corrida concluida em {FormatTime(elapsed)}");
            }
            else
            {
                ShowMessage($"Volta {lap}/{totalLaps}");
            }
        }

        public void SaveGame()
        {
            if (player == null)
            {
                return;
            }

            SaveData data = new SaveData
            {
                currentTrack = currentTrack,
                position = player.transform.position,
                rotation = player.transform.rotation,
                lap = lap,
                elapsed = elapsed,
                coins = player.Coins,
                unlockAll = unlockAll
            };

            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
            AppendLog($"Save executado na Volta {lap} aos {FormatTime(elapsed)}");
            ShowMessage("Jogo gravado.");
        }

        public void LoadGame()
        {
            if (!File.Exists(SavePath) || player == null)
            {
                ShowMessage("Ainda nao existe save.");
                return;
            }

            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            currentTrack = data.currentTrack;
            lap = Mathf.Clamp(data.lap, 1, totalLaps);
            elapsed = Mathf.Max(0f, data.elapsed);
            unlockAll = data.unlockAll;
            player.LoadState(data.position, data.rotation, data.coins);
            AppendLog($"Load executado na Volta {lap} aos {FormatTime(elapsed)}");
            ShowMessage("Jogo carregado.");
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        void AppendLog(string line)
        {
            File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {line}{Environment.NewLine}");
        }

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

        void ClearMessage()
        {
            if (messageText != null)
            {
                messageText.text = string.Empty;
            }
        }

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
        public string currentTrack;
        public Vector3 position;
        public Quaternion rotation;
        public int lap;
        public float elapsed;
        public int coins;
        public bool unlockAll;
    }
}
