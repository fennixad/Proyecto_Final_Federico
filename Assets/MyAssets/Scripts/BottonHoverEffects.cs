using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BottonHoverEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // El material con tu shader que se aplicará al pasar el ratón
    public Material hoverMaterial;

    // Guardaremos el material por defecto de la UI aquí.
    // Lo inicializaremos en Awake() con el material actual del Image.
    private Material defaultUIMaterial;

    private Image buttonImage;

    void Awake()
    {
        buttonImage = GetComponent<Image>();

        if (buttonImage == null)
        {
            enabled = false; // Desactiva este script si no encuentra Image
            return;
        }

        // Guardar el material que el botón usa por defecto (el de la UI estándar).
        // Este será el material al que regresaremos al quitar el hover.
        defaultUIMaterial = buttonImage.material;

        buttonImage.material = defaultUIMaterial; // Para garantizar que inicia con el material UI por defecto
    }

    // Este método se llama automáticamente por el sistema de eventos de Unity
    // cuando el puntero del ratón ENTRA en el área de este botón.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverMaterial != null)
        {
            // Aplica el material con tu shader al componente Image del botón
            buttonImage.material = hoverMaterial;
        }
        else
        {
            Debug.LogWarning($"El material de hover no está asignado para {gameObject.name}. Asegúrate de arrastrarlo al Inspector.");
        }
    }

    // Este método se llama automáticamente por el sistema de eventos de Unity
    // cuando el puntero del ratón SALE del área de este botón.
    public void OnPointerExit(PointerEventData eventData)
    {
        // Vuelve al material que el botón usaba por defecto (el de la UI estándar)
        // Esto "quita" el efecto del shader, regresando a la apariencia normal del botón.
        buttonImage.material = defaultUIMaterial;
    }
}
