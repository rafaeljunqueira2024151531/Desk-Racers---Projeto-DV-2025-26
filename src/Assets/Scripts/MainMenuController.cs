using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DeskRacers
{
    public class MainMenuController : MonoBehaviour
    {
        public GameObject mainPanel;
        public GameObject trackPanel;
        public GameObject optionsPanel;
        public GameObject creditsPanel;
        public Slider volumeSlider;
        public GameObject firstMainButton;
        public GameObject firstTrackButton;
        public Button[] lockedTrackButtons;
        public GameObject[] lockedTrackMarkers;

        // Prepara paineis e valores iniciais do menu.
        void Start()
        {
            ShowMainPanel();

            if (optionsPanel != null)
            {
                optionsPanel.SetActive(false);
            }

            if (creditsPanel != null)
            {
                creditsPanel.SetActive(false);
            }

            if (volumeSlider != null)
            {
                float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
                AudioListener.volume = savedVolume;
                volumeSlider.SetValueWithoutNotify(savedVolume);
                volumeSlider.onValueChanged.AddListener(SetVolume);
            }

            ApplyTrackUnlockState();
        }

        // Permite usar F2 no menu para desbloquear todas as pistas.
        void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame))
            {
                PlayerPrefs.SetInt("UnlockAllTracks", 1);
                PlayerPrefs.Save();
                ApplyTrackUnlockState();
            }

            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                Back();
            }

            Gamepad pad = Gamepad.current;
            if (pad != null && pad.buttonEast.wasPressedThisFrame)
            {
                Back();
            }
        }

        // Abre uma scene de pista pelo nome.
        public void PlayTrack(string sceneName)
        {
            if (sceneName == "Track_Kitchen" && !IsKitchenUnlocked())
            {
                return;
            }

            SceneManager.LoadScene(sceneName);
        }

        // Mostra o painel principal do menu.
        public void ShowMainPanel()
        {
            SetPanelActive(mainPanel, true);
            SetPanelActive(trackPanel, false);
            SetPanelActive(optionsPanel, false);
            SetPanelActive(creditsPanel, false);
            SelectUiObject(firstMainButton);
        }

        // Abre o painel com as pistas disponiveis.
        public void ShowTrackPanel()
        {
            ApplyTrackUnlockState();
            SetPanelActive(mainPanel, false);
            SetPanelActive(trackPanel, true);
            SetPanelActive(optionsPanel, false);
            SetPanelActive(creditsPanel, false);
            SelectUiObject(firstTrackButton);
        }

        // Abre o painel de opcoes do menu.
        public void ShowOptionsPanel()
        {
            SetPanelActive(mainPanel, false);
            SetPanelActive(trackPanel, false);
            SetPanelActive(optionsPanel, true);
            SetPanelActive(creditsPanel, false);
            SelectUiObject(volumeSlider != null ? volumeSlider.gameObject : null);
        }

        // Abre o painel de creditos sem deixar o menu principal selecionavel.
        public void ShowCreditsPanel()
        {
            SetPanelActive(mainPanel, false);
            SetPanelActive(trackPanel, false);
            SetPanelActive(optionsPanel, false);
            SetPanelActive(creditsPanel, true);
            SelectUiObject(null);
        }

        // Volta ao painel principal a partir de submenus.
        public void Back()
        {
            if ((trackPanel != null && trackPanel.activeSelf) || (optionsPanel != null && optionsPanel.activeSelf) || (creditsPanel != null && creditsPanel.activeSelf))
            {
                ShowMainPanel();
            }
        }

        // Mostra ou esconde o painel de opcoes.
        public void ToggleOptions()
        {
            if (optionsPanel != null && optionsPanel.activeSelf)
            {
                ShowMainPanel();
            }
            else
            {
                ShowOptionsPanel();
            }
        }

        // Mostra ou esconde o painel de creditos.
        public void ToggleCredits()
        {
            if (creditsPanel != null && creditsPanel.activeSelf)
            {
                ShowMainPanel();
            }
            else
            {
                ShowCreditsPanel();
            }
        }

        // Ajusta o volume global do jogo.
        public void SetVolume(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            AudioListener.volume = clampedValue;
            PlayerPrefs.SetFloat("MasterVolume", clampedValue);
        }

        // Actualiza botoes/avisos das pistas bloqueadas no menu principal.
        void ApplyTrackUnlockState()
        {
            bool unlocked = IsKitchenUnlocked();

            foreach (Button trackButton in lockedTrackButtons)
            {
                if (trackButton != null)
                {
                    trackButton.interactable = unlocked;
                }
            }

            foreach (GameObject marker in lockedTrackMarkers)
            {
                if (marker != null)
                {
                    marker.SetActive(!unlocked);
                }
            }
        }

        // Verifica se a segunda pista ja pode ser escolhida.
        bool IsKitchenUnlocked()
        {
            return PlayerPrefs.GetInt("UnlockAllTracks", 0) == 1 || PlayerPrefs.GetInt("KitchenUnlocked", 0) == 1;
        }

        // Liga/desliga um painel se ele existir.
        void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }

        // Seleciona o primeiro elemento para comando/teclado.
        void SelectUiObject(GameObject target)
        {
            if (EventSystem.current == null)
            {
                return;
            }

            EventSystem.current.SetSelectedGameObject(null);
            if (target == null || !target.activeInHierarchy)
            {
                return;
            }

            EventSystem.current.SetSelectedGameObject(target);
        }

        // Fecha o jogo na build final.
        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
