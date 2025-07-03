using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*
public class SelectorManager : MonoBehaviour
{
    public static SelectorManager Instance { get; private set; }
    void Awake()
    {
        Instance = this;
    }
}
*/
public class SelectorManager : MonoBehaviour
{
    // Patrón Singleton para acceder fácilmente a este manager desde otros scripts.
    // Garantiza que solo haya una instancia de SelectorManager en la escena.
    public static SelectorManager Instance { get; private set; }

    // Prefabs de los efectos de selección para objetos específicos (clic izquierdo)
    [Header("Efectos de Selección (Clic Izquierdo)")]
    [Tooltip("Define los diferentes efectos de partículas para cada capa de objeto seleccionable.")]
    public SelectEffectEntry[] selectEffectsByLayer;
    private Dictionary<int, ParticleSystem> layerToSelectEffectMap; // Para mapear Layers a ParticleSystems de selección

    [Tooltip("La instancia actual del efecto de selección que se muestra sobre el objeto seleccionado.")]
    private GameObject currentInstantiatedSelectEffect;

    [Tooltip("El GameObject que está actualmente seleccionado por el jugador.")]
    public GameObject currentlySelectedObject { get; private set; } // Hacemos esta propiedad pública para que PlayerController pueda leerla.

    [Header("Configuración General de Selección")]
    [Tooltip("Capas de GameObjects que pueden ser seleccionados con un clic izquierdo.")]
    public LayerMask selectableLayers; // Renombramos de 'clickableLayers' a 'selectableLayers' para mayor claridad.

    private void Awake()
    {
        // Implementación del Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destruye esta instancia si ya existe otra
        }
        else
        {
            Instance = this; // Establece esta instancia como la única
        }

        InitializeSelectEffectMap(); // Inicializa el mapa para los efectos de selección
    }

    // Método para inicializar el diccionario de efectos de SELECCIÓN
    private void InitializeSelectEffectMap()
    {
        layerToSelectEffectMap = new Dictionary<int, ParticleSystem>();
        if (selectEffectsByLayer != null)
        {
            foreach (var entry in selectEffectsByLayer)
            {
                if (entry.effectPrefab != null)
                {
                    // Convertir el LayerMask a un valor de bit para usar como clave en el diccionario
                    // targetLayer.value ya debería ser el valor de bit si es una LayerMask de una sola capa
                    layerToSelectEffectMap[entry.targetLayer.value] = entry.effectPrefab;
                }
            }
        }
    }

    /// <summary>
    /// Maneja la lógica de selección de un objetivo al hacer clic izquierdo.
    /// Es llamada desde PlayerController.
    /// </summary>
    public void SelectTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        // Dibuja un rayo azul en el Editor para depuración (visible en la vista de escena)
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.blue, 1f);

        // Lanza un rayo desde la posición del mouse, limitado por las capas seleccionables
        if (Physics.Raycast(ray, out hit, 100f, selectableLayers))
        {
            // Evita seleccionarse a sí mismo (asumiendo que el SelectorManager no está en el Player)
            // Si el PlayerController también pudiera ser targeteado, necesitarías una forma de excluirlo aquí.
            // Para mantener la lógica separada, esta verificación se eliminará o ajustará si el player puede ser targeteado.
            // if (hit.collider.gameObject == PlayerController.Instance.gameObject) 
            // {
            //     Debug.Log("Clic en el jugador. No se selecciona.");
            //     return;
            // }

            // LÓGICA DE SELECCIÓN DE OBJETOS CON EFECTOS ESPECÍFICOS
            ParticleSystem effectToInstantiateForSelectionPrefab = null; // Prefab de ParticleSystem a instanciar
            SoundManager.Instance.PlaySounds(3); // Reproducir sonido de clic al seleccionar un objeto
            // Obtener el valor de bit de la capa del objeto golpeado
            int hitLayerBitValue = 1 << hit.collider.gameObject.layer;

            // Intentar obtener un efecto específico para esta capa del diccionario de SELECCIÓN
            if (layerToSelectEffectMap.TryGetValue(hitLayerBitValue, out ParticleSystem specificSelectEffect))
            {
                effectToInstantiateForSelectionPrefab = specificSelectEffect;
            }

            // Si el objeto clicado es diferente al que ya está seleccionado, o si queremos deseleccionar el actual
            if (hit.collider.gameObject != currentlySelectedObject)
            {
                // Destruir el efecto de selección anterior si existe
                if (currentInstantiatedSelectEffect != null)
                {
                    Destroy(currentInstantiatedSelectEffect);
                    currentInstantiatedSelectEffect = null;
                }

                // Instanciar el nuevo efecto de selección si se encontró uno para esta capa
                if (effectToInstantiateForSelectionPrefab != null)
                {
                    // Accedemos al .gameObject del ParticleSystem prefab para instanciarlo.
                    // Ajustamos el offset Y para que el efecto no esté en el medio del objeto.
                    Vector3 effectPosition = hit.collider.transform.position + new Vector3(0, -0.4f, 0);
                    currentInstantiatedSelectEffect = Instantiate(effectToInstantiateForSelectionPrefab.gameObject, effectPosition, Quaternion.identity);
                    currentInstantiatedSelectEffect.transform.SetParent(hit.collider.transform);
                    Debug.Log($"Seleccionado: {hit.collider.gameObject.name} con efecto específico.");
                }
                else
                {
                    // Si no hay un efecto específico configurado para esta capa, pero clicamos en un nuevo objeto,
                    // aún necesitamos deseleccionar el anterior.
                    Debug.Log($"Seleccionado: {hit.collider.gameObject.name}. No hay efecto de selección específico para esta capa.");
                }
                currentlySelectedObject = hit.collider.gameObject; // Actualiza el objeto seleccionado
            }
            else
            {
                Debug.Log($"Ya seleccionado: {hit.collider.gameObject.name}");
            }
        }
        else // Si el clic izquierdo no golpea nada en las capas seleccionables, deseleccionar
        {
            if (currentInstantiatedSelectEffect != null)
            {
                Destroy(currentInstantiatedSelectEffect);
                currentInstantiatedSelectEffect = null;
                currentlySelectedObject = null;
                Debug.Log("Deseleccionado al hacer clic en el vacío.");
            }
        }
    }
}

// Clase Auxiliar para definir entradas de efectos de SELECCIÓN específicos por capa
[System.Serializable]
public class SelectEffectEntry
{
    public LayerMask targetLayer; // La capa para la que este efecto de selección es específico
    public ParticleSystem effectPrefab; // El prefab del ParticleSystem asociado a esta capa
}