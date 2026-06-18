using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DeskRacers
{
    public class UIButtonSelectionVisual : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public float selectedScale = 1.08f;
        public Color normalColor = Color.white;
        public Color selectedColor = new Color(1f, 0.88f, 0.2f);

        Vector3 startScale;
        Image image;

        // Guarda a escala e a imagem original do botao.
        void Awake()
        {
            startScale = transform.localScale;
            image = GetComponent<Image>();
            ApplySelected(false);
        }

        // Aplica destaque quando o botao e seleccionado por comando/teclado.
        public void OnSelect(BaseEventData eventData)
        {
            ApplySelected(true);
        }

        // Remove destaque quando outro botao e seleccionado.
        public void OnDeselect(BaseEventData eventData)
        {
            ApplySelected(false);
        }

        // Aplica destaque quando o rato passa por cima.
        public void OnPointerEnter(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
            ApplySelected(true);
        }

        // Remove destaque quando o rato sai e o botao nao esta seleccionado.
        public void OnPointerExit(PointerEventData eventData)
        {
            if (EventSystem.current.currentSelectedGameObject != gameObject)
            {
                ApplySelected(false);
            }
        }

        // Muda escala e cor do botao.
        void ApplySelected(bool selected)
        {
            transform.localScale = selected ? startScale * selectedScale : startScale;
            if (image != null)
            {
                image.color = selected ? selectedColor : normalColor;
            }
        }
    }
}
