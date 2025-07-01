using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BottonHoverEffects : MonoBehaviour
{
    // El material con tu shader que se aplicará al pasar el ratón
    public Material hoverMaterial;

    // Opcional: Si tu botón tiene una imagen (Sprite) que quieres mantener visible,
    // puedes guardar una referencia a su material por defecto (el de la UI).
    private Material defaultUIMaterial;

    private Image buttonImage; // Referencia al componente Image del botón

    void Awake()
    {
        // Obtener la referencia al componente Image en el mismo GameObject
        buttonImage = GetComponent<Image>();
        if (buttonImage == null)
        {
            Debug.LogError("ButtonHoverEffect requiere un componente Image en el mismo GameObject.");
            enabled = false; // Desactiva este script si no encuentra Image
            return;
        }

        // Guardar el material que el botón usa por defecto (el de la UI estándar)
        // antes de que lo cambiemos. Así podemos volver a él.
        defaultUIMaterial = buttonImage.material;

        // Opcional: Asegurarse de que el material inicial sea el predeterminado de UI
        // Esto es útil si accidentalmente arrastraste un material al Image en el Inspector.
        buttonImage.material = defaultUIMaterial;
    }

    // Este método se llama automáticamente por el sistema de eventos de Unity
    // cuando el puntero del ratón ENTRA en el área de este botón.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverMaterial != null)
        {
            // Aplica el material con tu shader al componente Image del botón
            buttonImage.material = hoverMaterial;
            Debug.Log($"Puntero entró en {gameObject.name}. Aplicando material de hover.");
        }
        else
        {
            Debug.LogWarning($"El material de hover no está asignado para {gameObject.name}.");
        }
    }

    // Este método se llama automáticamente por el sistema de eventos de Unity
    // cuando el puntero del ratón SALE del área de este botón.
    public void OnPointerExit(PointerEventData eventData)
    {
        // Vuelve al material que el botón usaba por defecto (el de la UI estándar)
        // Esto "quita" el efecto del shader.
        buttonImage.material = defaultUIMaterial;
        Debug.Log($"Puntero salió de {gameObject.name}. Volviendo al material por defecto.");
    }
}
