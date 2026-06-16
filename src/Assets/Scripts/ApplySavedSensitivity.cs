using UnityEngine;

namespace DeskRacers
{
    public class ApplySavedSensitivity : MonoBehaviour
    {
        public ArcadeCarController car;

        // Aplica ao carro a sensibilidade escolhida no menu.
        void Start()
        {
            if (car != null && PlayerPrefs.HasKey("Sensitivity"))
            {
                car.turnSpeed = PlayerPrefs.GetFloat("Sensitivity");
            }
        }
    }
}
