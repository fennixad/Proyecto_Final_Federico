using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    public Material defaultMaterial; // El material que el botón usa normalmente
    public Material hoverMaterial;   // El material con tu shader que se aplica al pasar el ratón
    private Image buttonImage;

    public Button butonStart;
    public Button butonExit;
    public Texture2D defaultCursor;
    public Texture2D hoverCursor;
    public Vector2 hotSpot = Vector2.zero; // El punto "caliente" del puntero (donde registra el clic)
    public LayerMask excludeFromClickSoundLayers;

    void Start()
    {
        // Opcional: Define el hotSpot si tu puntero no es el típico (0,0) que es la esquina superior izquierda.
        // Por ejemplo, para centrarlo si tu cursorTexture es 64x64, usarías new Vector2(32, 32).
        // hotSpot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);

        Cursor.SetCursor(defaultCursor, hotSpot, CursorMode.Auto);
    }
    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Paso 1: Comprobar si el clic está sobre un elemento de UI
            // EventSystem.current.IsPointerOverGameObject() detecta si el puntero está sobre cualquier elemento de UI.
            // Para UI en pantalla táctil o con varios EventSystems, podrías necesitar pasar el ID del puntero.
            // Para el mouse, el ID -1 funciona.
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                // El clic fue sobre un elemento de UI (botón, slider, etc.)
                // No reproducimos el sonido general de clic.
               // Debug.Log("Clic en UI. No se reproduce sonido general.");
                return; // Salir de la función Update
            }

            // Paso 2: Comprobar si el clic golpeó un GameObject de la escena (usando un Raycast)
            // Lanza un rayo desde la posición del mouse en la pantalla
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            // Si el rayo golpea algo (y no está en las capas a excluir)
            // Si excludeFromClickSoundLayers está configurado, el rayo ignorará esas capas.
            // Si *no* está configurado (todo en "Nothing"), golpeará cualquier cosa,
            // y solo reproduciremos el sonido si no golpea nada.
            if (Physics.Raycast(ray, out hit, 100f, ~excludeFromClickSoundLayers)) // ~ para invertir la LayerMask
            {
                // El clic golpeó un GameObject 3D de la escena que no está en las capas a excluir.
                // Asumimos que estos GameObjects tienen sus propios sonidos o no queremos sonido general.
                //Debug.Log($"Clic en GameObject 3D: {hit.collider.name}. No se reproduce sonido general.");
                return; // Salir de la función Update
            }

            // Si el código llega aquí, significa que el clic NO fue sobre UI
            // Y NO golpeó un GameObject 3D de la escena (o solo golpeó GameObjects que queremos incluir en el sonido general)
            // Por lo tanto, el clic fue en "vacío" o en un elemento de fondo no interactuable.
            PlayClickSound();
        }
    }
    void OnDisable()
    {
        // Opcional: Si quieres que el puntero vuelva al predeterminado de Windows al deshabilitar el script o el objeto.
        // Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
    void PlayClickSound()
    {
        SoundManager.Instance.PlaySounds(1);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverMaterial != null)
        {
            buttonImage.material = hoverMaterial; // Aplica el material de hover
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        buttonImage.material = defaultMaterial; // Vuelve al material por defecto
    }
}
