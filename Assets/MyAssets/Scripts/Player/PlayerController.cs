using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

/*
public class PlayerController : MonoBehaviour
{
    Custom_Actions input;
    NavMeshAgent agent;
    Animator animator;
    Camera cmCamera;

    [Header("Target Effect Settings")]
    [SerializeField] GameObject targetEffectPrefab;
    private GameObject currentInstantiatedEffect;  // Referencia al efecto instanciado actualmente en la escena
    private GameObject currentlySelectedObject;    // Referencia al GameObject que tiene el efecto de selección

    [Header("Movement Settings")]
    [SerializeField] ParticleSystem clickEffect;
    [SerializeField] LayerMask clickableLayers;
    private bool isRightClickPressed = false;
    float lookRotationSpeed = 8f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        input = new Custom_Actions();
        AssignInputs();
    }

    private void Update()
    {
        FaceTarget();
        if (isRightClickPressed)
        {
            ClickToMoveContinuous();
        }
        //SetAnimations();
    }
    private void OnEnable()
    {
        input.Enable(); // Enable the input system when the object is enabled
    }

    private void OnDisable()
    {
        input.Disable(); // Disable the input system when the object is disabled
    }
    void AssignInputs()
    {
        // --- Cambios aquí: Manejo de los eventos del clic derecho para el movimiento ---
        // Asignar el evento del clic derecho al contexto de la acción 'Move'
        // 'started' se dispara cuando el botón se presiona por primera vez.
        input.Main.Move.started += ctx => OnMoveStarted(ctx);
        // 'performed' se dispara continuamente mientras el botón está presionado Y/O al final del clic (dependiendo del tipo de control).
        // Lo usaremos para el primer click y luego la bandera para el arrastre.
        input.Main.Move.performed += ctx => OnMovePerformed(ctx);
        // 'canceled' se dispara cuando el botón se suelta.
        input.Main.Move.canceled += ctx => OnMoveCanceled(ctx);
        // --- Fin cambios ---

        input.Select.Select.performed += ctx => ClickToTarget();
    }

    private void OnMoveStarted(InputAction.CallbackContext context)
    {
        isRightClickPressed = true;
        // Opcional: Podrías hacer aquí un primer movimiento instantáneo al presionar
        // ClickToMoveContinuous(); // Ya se hará en performed si el control es un "Button"
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        // Este se dispara una vez para el clic simple y también si el "Interaction" del Input Action
        // es de tipo "Hold" y el botón se mantiene.
        // Lo usamos para asegurar que el movimiento inicial del clic se procese.
        ClickToMoveContinuous();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        isRightClickPressed = false;
    }

    // --- Fin nuevos métodos para el Input System ---

    void ClickToMoveContinuous()
    {
        RaycastHit hit;
        // Usamos Input.mousePosition directamente, ya que la acción de input ya está activa.
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100f, clickableLayers))
        {
            agent.SetDestination(hit.point);
            if (clickEffect != null)
            {
                // Instancia el efecto de click en la posición del hit con un pequeño offset vertical
                var effectInstance = Instantiate(clickEffect, hit.point + new Vector3(0, 0.1f, 0), clickEffect.transform.rotation);
                Destroy(effectInstance.gameObject, 0.2f);
            }
        }
    }
    void ClickToTarget()
    {
        RaycastHit hit;
        // Siempre usa Camera.main para el rayo de la cámara principal
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100f, clickableLayers))
        {
            // Verificamos si el objeto clickeado es diferente al objeto actualmente seleccionado
            if (hit.collider.gameObject != currentlySelectedObject)
            {
                // Paso 1: Si ya hay un efecto instanciado, destrúyelo del objeto anterior
                if (currentInstantiatedEffect != null)
                {
                    Destroy(currentInstantiatedEffect);
                    currentInstantiatedEffect = null; // Limpiar la referencia
                }

                // Paso 2: Instanciar el nuevo efecto en el objeto clickeado
                if (targetEffectPrefab != null) // Asegurarse de que el prefab está asignado
                {
                    // Instanciar el prefab. Es bueno hacerlo hijo del objeto clickeado para que se mueva con él.
                    currentInstantiatedEffect = Instantiate(targetEffectPrefab, hit.collider.transform.position, Quaternion.identity);
                    currentInstantiatedEffect.transform.SetParent(hit.collider.transform); // Hacerlo hijo del objeto clickeado

                    // Opcional: Ajustar la posición local si el efecto no encaja bien en el centro del objeto padre
                    currentInstantiatedEffect.transform.localPosition = Vector3.zero; // O un offset específico  
                    currentInstantiatedEffect.transform.localPosition += new Vector3(0, -0.4f, 0); // Ajuste para que este a los pies del objeto.
                }
                else
                {
                    Debug.LogWarning("¡targetEffectPrefab no asignado en el Inspector!");
                }

                // Paso 3: Actualizar la referencia al objeto actualmente seleccionado
                currentlySelectedObject = hit.collider.gameObject;
            }
            else
            {
                // El jugador hizo clic en el mismo objeto que ya está seleccionado.
                // Aquí, simplemente no hacemos nada según tus requisitos: "luego ya no si clickeas a ese objeto y tiene el prefab puesto que ya no instancie mas."
                Debug.Log($"Ya seleccionado: {hit.collider.gameObject.name}");
            }
        }
        else
        {
            // Si se hace clic en un área que no es "clickableLayers" o no golpea nada,
            // puedes decidir si quieres "deseleccionar" el objeto actual.
            // Por ahora, no lo haremos según tu descripción original.
            // Si quisieras deseleccionar al hacer clic en el vacío:
            /*
            if (currentInstantiatedEffect != null)
            {
                Destroy(currentInstantiatedEffect);
                currentInstantiatedEffect = null;
                currentlySelectedObject = null;
                Debug.Log("Deseleccionado.");
            }
            */
/*

            Debug.Log("Click fuera de una capa clickeable.");
        }
    }
    void FaceTarget()
    {
        if (agent.velocity.sqrMagnitude > 0.1f) // Solo si el agente se está moviendo
        {
            Vector3 lookDirection = agent.steeringTarget - transform.position;
            if (lookDirection != Vector3.zero)
            {
                lookDirection.y = 0; // Evita rotar en el eje X si el objetivo está arriba/abajo
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookRotationSpeed);
            }
        }
    }
    void SetAnimations()
    {
        if (agent.velocity.magnitude > 0.1f)
        {
            animator.Play("WALK");
        }
        else
        {
            animator.Play("IDLE");
        }
    }
}
*/

/*
public class PlayerController : MonoBehaviour
{
    Custom_Actions input; // Referencia a la clase generada por Input System
    NavMeshAgent agent;
    Animator animator;

    // --- CAMBIOS AQUÍ ---
    // Prefabs de los efectos de selección para objetos específicos (clic izquierdo)
    [Header("Select Click Effects")]
    public SelectEffectEntry[] selectEffectsByLayer; // Array para efectos de selección según la capa del objeto
    private Dictionary<int, ParticleSystem> layerToSelectEffectMap; // Para mapear Layers a ParticleSystems de selección
    // --- FIN CAMBIOS ---

    private GameObject currentInstantiatedSelectEffect; // Instancia actual del efecto de selección (el que queda sobre el objeto)
    private GameObject currentlySelectedObject; // El GameObject actualmente seleccionado por clic izquierdo

    [Header("Movement Settings")]
    [SerializeField] LayerMask clickableLayers; // Capas en las que el jugador puede hacer clic para moverse o seleccionar

    [Header("Move Click Effect")]
    public ParticleSystem movementClickEffect; // El efecto para clics con botón derecho (movimiento), shader si lo tienes en el prefab

    // NOTA: 'specificClickEffects' ya no se usa, lo usaremos para los efectos de SELECCIÓN.
    // Dejaré la variable para que no dé errores si la tenías configurada en el Inspector,
    // pero la renombraré y la moveré al contexto correcto.
    // private Dictionary<int, ParticleSystem> layerToClickEffectMap; // Este ya no es necesario aquí, ya que 'movementClickEffect' es único.

    float lookRotationSpeed = 8f;
    private bool isRightClickPressed = false; // Bandera para movimiento continuo con clic derecho

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        input = new Custom_Actions(); // Inicializa tu Input Action Map
        InitializeSelectEffectMap(); // Ahora inicializamos el mapa para los efectos de selección
    }

    private void OnEnable()
    {
        input.Enable(); // Habilita las acciones de Input
        AssignInputs(); // Asigna los callbacks de Input
    }

    private void OnDisable()
    {
        input.Disable(); // Deshabilita las acciones de Input
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
                    // Convertir el LayerMask a un valor de bit para usar como clave
                    // targetLayer.value ya debería ser el valor de bit si es una LayerMask de una sola capa
                    layerToSelectEffectMap[entry.targetLayer.value] = entry.effectPrefab;
                }
            }
        }
    }

    // Asigna los métodos a los eventos del Input System
    void AssignInputs()
    {
        input.Main.Move.started += ctx => OnMoveStarted(ctx);
        input.Main.Move.performed += ctx => OnMovePerformed(ctx);
        input.Main.Move.canceled += ctx => OnMoveCanceled(ctx);

        input.Select.Select.performed += ctx => ClickToTarget();
        input.Select.Interact.performed += ctx => OnInteractPressed();
    }

    private void Update()
    {
        FaceTarget(); // Mantiene al personaje mirando en la dirección del movimiento

        // Si el clic derecho está presionado, realiza el movimiento continuo
        if (isRightClickPressed)
        {
            ClickToMoveContinuous();
        }
    }

    // Callbacks para la acción de movimiento (clic derecho)
    private void OnMoveStarted(InputAction.CallbackContext context)
    {
        isRightClickPressed = true;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        // No necesitamos hacer nada aquí si ClickToMoveContinuous ya se llama en Update
        // si isRightClickPressed es true.
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        isRightClickPressed = false;
    }

    // Maneja el movimiento continuo al mantener presionado el clic derecho y el efecto de movimiento
    void ClickToMoveContinuous()
    {
        RaycastHit hit;
        // Lanza un rayo desde la posición del mouse, limitado por clickableLayers
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100f, clickableLayers))
        {
            agent.SetDestination(hit.point); // Establece el destino del NavMeshAgent

            // Instancia el efecto de movimiento (shader) en el punto clicado
            if (movementClickEffect != null)
            {
                // Instancia el efecto ligeramente por encima del punto de impacto para que sea visible
                Instantiate(movementClickEffect, hit.point + new Vector3(0, 0.1f, 0), movementClickEffect.transform.rotation);
            }
        }
    }

    // Maneja la selección de un objetivo al hacer clic izquierdo
    void ClickToTarget()
    {
        RaycastHit hit;
        // Lanza un rayo desde la posición del mouse, limitado por clickableLayers
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100f, clickableLayers))
        {
            // Evita seleccionarse a sí mismo
            if (hit.collider.gameObject == this.gameObject)
            {
                Debug.Log("Clic en el jugador. No se selecciona.");
                return;
            }
            // --- LÓGICA DE SELECCIÓN DE OBJETOS CON EFECTOS ESPECÍFICOS ---
            ParticleSystem effectToInstantiateForSelectionPrefab = null; // Prefab de ParticleSystem a instanciar

            // Obtener el valor de bit de la capa del objeto golpeado
            int hitLayerBitValue = 1 << hit.collider.gameObject.layer;

            // Intentar obtener un efecto específico para esta capa del diccionario de SELECCIÓN
            if (layerToSelectEffectMap.TryGetValue(hitLayerBitValue, out ParticleSystem specificSelectEffect))
            {
                effectToInstantiateForSelectionPrefab = specificSelectEffect;
            }

            // Si el objeto clicado es diferente al que ya está seleccionado O si no hay efecto de selección específico pero queremos quitar el anterior
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
                    Vector3 _rightPost = hit.collider.transform.position + new Vector3(0, -0.4f, 0); // Un pequeño offset para que el efecto no esté en el medio del objeto
                    currentInstantiatedSelectEffect = Instantiate(effectToInstantiateForSelectionPrefab.gameObject, _rightPost, Quaternion.identity);
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
        else // Si el clic izquierdo no golpea nada en las clickableLayers, deseleccionar
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

    // Maneja la interacción al presionar la tecla 'E' (sin cambios)
    void OnInteractPressed()
    {
        if (currentlySelectedObject != null)
        {
            IInteractable interactable = currentlySelectedObject.GetComponent<IInteractable>();

            if (interactable != null)
            {
                interactable.Interact(this.gameObject);
            }
            else
            {
                Debug.Log($"El objeto {currentlySelectedObject.name} no es interactuable.");
            }
        }
        else
        {
            Debug.Log("No hay ningún objeto seleccionado para interactuar.");
        }
    }

    // Hace que el personaje mire en la dirección de su movimiento (sin cambios)
    private void FaceTarget()
    {
        Vector3 horizontalVelocity = agent.velocity;
        horizontalVelocity.y = 0;

        if (horizontalVelocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookRotationSpeed);
        }
    }
}

// --- Clases Auxiliares ---

// Clase para definir entradas de efectos de SELECCIÓN específicos por capa
[System.Serializable]
public class SelectEffectEntry
{
    public LayerMask targetLayer; // La capa para la que este efecto de selección es específico
    public ParticleSystem effectPrefab; // El prefab del ParticleSystem asociado a esta capa
}
*/
public class PlayerController : MonoBehaviour
{
    Custom_Actions input; 
    NavMeshAgent agent;
    Animator animator;

    [Header("Interaction Settings")]
    [Tooltip("La distancia máxima a la que el jugador puede interactuar con un objeto seleccionado.")]
    public float interactionRange = 3f; // Distancia máxima para interactuar

    [Header("Movement Settings")]
    [SerializeField] LayerMask clickableLayers; // Capas en las que el jugador puede hacer clic para moverse o seleccionar

    [Header("Move Click Effect")]
    public ParticleSystem movementClickEffect; // El efecto para clics con botón derecho (movimiento), shader si lo tienes en el prefab

    float lookRotationSpeed = 8f;
    private bool isRightClickPressed = false; // Bandera para movimiento continuo con clic derecho

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        input = new Custom_Actions(); // Inicializa tu Input Action Map
    }

    private void OnEnable()
    {
        input.Enable(); // Habilita las acciones de Input
        AssignInputs(); // Asigna los callbacks de Input
    }

    private void OnDisable()
    {
        input.Disable(); // Deshabilita las acciones de Input
    }

    // Asigna los métodos a los eventos del Input System
    void AssignInputs()
    {
        input.Main.Move.started += ctx => OnMoveStarted(ctx);
       // input.Main.Move.performed += ctx => OnMovePerformed(ctx);
        input.Main.Move.canceled += ctx => OnMoveCanceled(ctx);

        // Ahora delegamos la llamada a SelectorManager.Instance.SelectTarget()
        input.Select.Select.performed += ctx => SelectorManager.Instance.SelectTarget();
        input.Select.Interact.performed += ctx => OnInteractPressed();
    }

    private void Update()
    {
        FaceTarget(); // Mantiene al personaje mirando en la dirección del movimiento

        // Si el clic derecho está presionado, realiza el movimiento continuo
        if (isRightClickPressed)
        {
            ClickToMoveContinuous();
        }
    }

    // Callbacks para la acción de movimiento (clic derecho)
    private void OnMoveStarted(InputAction.CallbackContext context)
    {
        SoundManager.Instance.PlaySounds(2); // Reproducimos el sonido de clic
        isRightClickPressed = true;
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        isRightClickPressed = false;
    }

    // Maneja el movimiento continuo al mantener presionado el clic derecho y el efecto de movimiento
    void ClickToMoveContinuous()
    {
        RaycastHit hit;
        // Lanza un rayo desde la posición del mouse, limitado por clickableLayers
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100f, clickableLayers))
        {
            agent.SetDestination(hit.point); // Establece el destino del NavMeshAgent

            // Instancia el efecto de movimiento (shader) en el punto clicado
            if (movementClickEffect != null)
            {
                // Instancia el efecto ligeramente por encima del punto de impacto para que sea visible
                // El ParticleSystem instanciado se destruirá a sí mismo si su 'Stop Action' está configurada a 'Destroy'.
                Instantiate(movementClickEffect, hit.point + new Vector3(0, 0.1f, 0), movementClickEffect.transform.rotation);
            }
        }
    }

    // Maneja la interacción al presionar la tecla 'E'
    void OnInteractPressed()
    {
        // Obtenemos el objeto actualmente seleccionado del SelectorManager
        GameObject currentlySelectedObject = SelectorManager.Instance.currentlySelectedObject;

        if (currentlySelectedObject != null)
        {
            // Calculamos la distancia entre el jugador y el objeto seleccionado.
            // Usamos la magnitud de la diferencia de los vectores de posición.
            float distance = Vector3.Distance(transform.position, currentlySelectedObject.transform.position);

            Debug.Log($"Distancia al objeto {currentlySelectedObject.name}: {distance} unidades.");

            // Si la distancia es menor o igual al rango de interacción
            if (distance <= interactionRange)
            {
                // Intentamos obtener un componente que implemente IInteractable
                IInteractable interactable = currentlySelectedObject.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    // Si el objeto es interactuable y está dentro del rango, interactuamos
                    SoundManager.Instance.PlaySounds(0); // Reproducimos el sonido de interacción
                    interactable.Interact(this.gameObject); // Pasamos el GameObject del jugador como interactor
                }
                else
                {
                    Debug.Log($"El objeto {currentlySelectedObject.name} no es interactuable.");
                }
            }
            else // El objeto está fuera del rango de interacción
            {
                Debug.Log($"El objeto {currentlySelectedObject.name} está demasiado lejos para interactuar. Necesitas acercarte.");
                SoundManager.Instance.PlaySounds(1);
            }
        }
        else
        {
            Debug.Log("No hay ningún objeto seleccionado para interactuar.");
        }
    }

    // Hace que el personaje mire en la dirección de su movimiento
    private void FaceTarget()
    {
        Vector3 horizontalVelocity = agent.velocity;
        horizontalVelocity.y = 0; // Ignora el componente Y para rotación horizontal

        if (horizontalVelocity.sqrMagnitude > 0.01f) // Solo si hay movimiento significativo
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookRotationSpeed);
        }
    }
}