using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DeskRacers
{
    public class MainMenuController : MonoBehaviour
    {
        public GameObject optionsPanel;
        public GameObject creditsPanel;
        public Slider volumeSlider;
        public Slider sensitivitySlider;

        // Prepara paineis e valores iniciais do menu.
        void Start()
        {
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
                volumeSlider.value = AudioListener.volume;
            }
        }

        // Abre uma scene de pista pelo nome.
        public void PlayTrack(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        // Mostra ou esconde o painel de opcoes.
        public void ToggleOptions()
        {
            if (optionsPanel != null)
            {
                optionsPanel.SetActive(!optionsPanel.activeSelf);
            }
        }

        // Mostra ou esconde o painel de creditos.
        public void ToggleCredits()
        {
            if (creditsPanel != null)
            {
                creditsPanel.SetActive(!creditsPanel.activeSelf);
            }
        }

        // Ajusta o volume global do jogo.
        public void SetVolume(float value)
        {
            AudioListener.volume = value;
        }

        // Guarda a sensibilidade para as pistas usarem se quiseres expandir.
        public void SetSensitivity(float value)
        {
            PlayerPrefs.SetFloat("Sensitivity", value);
        }

        // Fecha o jogo na build final.
        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
