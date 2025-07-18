using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

/*
public class PlayerController : MonoBehaviour
{
    // Asegúrate de que Custom_Actions esté generado correctamente por Unity Input System
    Custom_Actions input;
    NavMeshAgent agent;
    Animator animator;

    [Header("Interaction Settings")]
    [Tooltip("La distancia máxima a la que el jugador puede interactuar con un objeto seleccionado.")]
    public float interactionRange = 3f; // Distancia máxima para interactuar

    [Header("Movement Settings")]
    [SerializeField] LayerMask clickableLayers; // Capas en las que el jugador puede hacer clic para moverse o seleccionar

    [Header("Move Click Effect")]
    public ParticleSystem movementClickEffect; // El efecto para clics con botón derecho (movimiento)

    [Header("Player Speeds")]
    [Tooltip("Velocidad de movimiento normal del jugador.")]
    public float normalMoveSpeed = 3f;
    [Tooltip("Velocidad de movimiento del jugador al agacharse.")]
    public float crouchMoveSpeed = 1.5f;

    float lookRotationSpeed = 8f;
    private bool isRightClickPressed = false; // Bandera para movimiento continuo con clic derecho

    // --- Variables de estado internas para mejor control ---
    private bool isCrouching = false; // Nuevo: Para rastrear el estado de agacharse
    private float currentAgentSpeed; // Nuevo: Para saber la velocidad actual del NavMeshAgent

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // Asegúrate de que el Animator esté en el primer hijo del transform
        animator = transform.GetChild(0).GetComponent<Animator>();
        input = new Custom_Actions(); // Inicializa tu Input Action Map
    }

    private void OnEnable()
    {
        input.Enable(); // Habilita las acciones de Input
        AssignInputs(); // Asigna los callbacks de Input
    }

    private void OnDisable()
    {
        input.Disable(); //Deshabilita el Input Map!
        // Desuscribir todos los eventos
        // al destruir el GameObject o deshabilitar el script.
        input.Main.Move.started -= OnMoveStarted; 
        input.Main.Move.canceled -= OnMoveCanceled; 
        input.Select.Select.performed -= SelectorManager.Instance.SelectTarget;
        input.Select.Interact.performed -= OnInteractPressed;
        input.Anim.Crouch.performed -= Crouch;
        input.Anim.Crouch.canceled -= StandUp;
    }

    // Asigna los métodos a los eventos del Input System
    void AssignInputs()
    {
        input.Main.Move.started += ctx => OnMoveStarted(ctx);
        input.Main.Move.canceled += ctx => OnMoveCanceled(ctx);
        input.Select.Select.performed += SelectorManager.Instance.SelectTarget;
        input.Select.Interact.performed += OnInteractPressed;
        input.Anim.Crouch.performed += Crouch;
        input.Anim.Crouch.canceled += StandUp;
    }

    private void Start()
    {
        // Establecer la velocidad inicial del agente y la animación al inicio
        currentAgentSpeed = normalMoveSpeed;
        agent.speed = currentAgentSpeed;
        animator.SetFloat("Speed", 0f); // Empieza en Idle
        animator.SetBool("IsCrouching", false); // No agachado al inicio
    }

    private void Update()
    {
        FaceTarget(); // Mantiene al personaje mirando en la dirección del movimiento

        // Si el clic derecho está presionado, realiza el movimiento continuo
        if (isRightClickPressed)
        {
            ClickToMoveContinuous();
        }

        // Actualizar la velocidad de la animación en base a la velocidad actual del NavMeshAgent
        // Esto es crucial para tu Blend Tree de movimiento (Idle/Running)
        float currentSpeedMagnitude = agent.velocity.magnitude / currentAgentSpeed; // Normalizar la velocidad
        // Si el agente está casi detenido, forzar la velocidad de animación a 0
        if (agent.remainingDistance < agent.stoppingDistance + 0.1f && !agent.pathPending)
        {
            currentSpeedMagnitude = 0f;
        }

        // Interpolar suavemente el valor de Speed para el Animator para transiciones más fluidas
        animator.SetFloat("Speed", Mathf.Lerp(animator.GetFloat("Speed"), currentSpeedMagnitude, Time.deltaTime * 10f));
    }

    // Callbacks para la acción de movimiento (clic derecho)
    private void OnMoveStarted(InputAction.CallbackContext context)
    {
        isRightClickPressed = true;
        SoundManager.Instance.PlaySounds(2); // Reproducimos el sonido de clic
        // La velocidad del agente se establecerá en ClickToMoveContinuous o en Crouch/StandUp
        // La animación de movimiento se actualizará en Update con la magnitud de la velocidad del agente
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        isRightClickPressed = false;
        // Cuando se suelta el clic derecho, el agente se detendrá una vez que llegue a su destino
        // La animación pasará a Idle automáticamente por el chequeo de agent.velocity.magnitude en Update
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
                // El efecto se instanciará una vez por cada frame que se mantenga pulsado,
                // considera instanciarlo solo al "started" o usar un cooldown si es muy denso.
                // Sin embargo, si el shader es ligero y se destruye solo, puede estar bien.
                Instantiate(movementClickEffect, hit.point + new Vector3(0, 0.1f, 0), movementClickEffect.transform.rotation);
            }
        }
    }

    // Maneja la interacción al presionar la tecla 'E'
    void OnInteractPressed(InputAction.CallbackContext context)
    {
        GameObject currentlySelectedObject = SelectorManager.Instance.currentlySelectedObject;

        if (currentlySelectedObject != null)
        {
            float distance = Vector3.Distance(transform.position, currentlySelectedObject.transform.position);

            if (distance <= interactionRange)
            {
                IInteractable interactable = currentlySelectedObject.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    SoundManager.Instance.PlaySounds(0);               
                    interactable.Interact(currentlySelectedObject);
                    if (currentlySelectedObject.CompareTag("Enemy")) animator.SetTrigger("Attack"); // Si es un enemigo, activa el ataque
                    else animator.SetTrigger("Interact"); // Si es otro objeto, activa la interacción
                }
                else
                {
                    Debug.Log($"El objeto {currentlySelectedObject.name} no es interactuable.");
                }
            }
            else
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

        // Solo si hay movimiento significativo o el agente tiene un destino establecido
        if (agent.hasPath || horizontalVelocity.sqrMagnitude > 0.01f)
        {
            // Calcula la rotación hacia la dirección del movimiento
            Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity.normalized); // Usar normalized para evitar problemas con magnitud cercana a cero
            // Aplica una interpolación suave para la rotación
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookRotationSpeed);
        }
    }

    private void Crouch(InputAction.CallbackContext context)
    {
        if (isCrouching) return; // Ya está agachado

        isCrouching = true;
        animator.SetBool("IsCrouching", true); // Activa la animación de agacharse
        currentAgentSpeed = crouchMoveSpeed; // Actualiza la velocidad base
        agent.speed = currentAgentSpeed; // Aplica la nueva velocidad al NavMeshAgent
    }

    private void StandUp(InputAction.CallbackContext context)
    {
        if (!isCrouching) return; // Ya está de pie

        isCrouching = false;
        animator.SetBool("IsCrouching", false); // Desactiva la animación de agacharse
        currentAgentSpeed = normalMoveSpeed; // Restaura la velocidad base
        agent.speed = currentAgentSpeed; // Aplica la nueva velocidad al NavMeshAgent
    }
}
*/
public class PlayerController : MonoBehaviour
{
    // Asegúrate de que Custom_Actions esté generado correctamente por Unity Input System
    Custom_Actions input;
    NavMeshAgent agent;
    Animator animator;

    [Header("Interaction Settings")]
    [Tooltip("La distancia máxima a la que el jugador puede interactuar con un objeto seleccionado.")]
    public float interactionRange = 3f; // Distancia máxima para interactuar

    [Header("Movement Settings")]
    [SerializeField] LayerMask clickableLayers; // Capas en las que el jugador puede hacer clic para moverse o seleccionar

    [Header("Move Click Effect")]
    public ParticleSystem movementClickEffect; // El efecto para clics con botón derecho (movimiento)

    [Header("Player Speeds")]
    [Tooltip("Velocidad de movimiento normal del jugador.")]
    public float normalMoveSpeed = 3f;
    [Tooltip("Velocidad de movimiento del jugador al agacharse.")]
    public float crouchMoveSpeed = 1.5f;

    float lookRotationSpeed = 8f;
    private bool isRightClickPressed = false; // Bandera para movimiento continuo con clic derecho

    // --- Variables de estado internas para mejor control ---
    private bool isCrouching = false; // Para rastrear el estado de agacharse
    private float currentAgentSpeed; // Para saber la velocidad actual del NavMeshAgent

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // Asegúrate de que el Animator esté en el primer hijo del transform
        // O si está en el mismo GameObject, usa GetComponent<Animator>()
        animator = transform.GetChild(0).GetComponent<Animator>();
        input = new Custom_Actions(); // Inicializa tu Input Action Map
    }

    private void Start()
    {
        if (SelectorManager.Instance != null) // ¡Siempre verifica!
        {
            input.Select.Select.performed += SelectorManager.Instance.SelectTarget;
        }
        // Establecer la velocidad inicial del agente y la animación al inicio
        currentAgentSpeed = normalMoveSpeed;
        agent.speed = currentAgentSpeed;
        animator.SetFloat("Speed", 0f); // Empieza en Idle
        animator.SetBool("IsCrouching", false); // No agachado al inicio
    }

    private void OnEnable()
    {
        input.Enable(); // Habilita las acciones de Input

        // --- Suscripción de Callbacks de Input (MÉTODO DIRECTO) ---
        // Este es el método preferido para evitar el ArgumentException
        input.Main.Move.started += OnMoveStarted; // Se suscribe al método OnMoveStarted
        input.Main.Move.canceled += OnMoveCanceled; // Se suscribe al método OnMoveCanceled

        // Si SelectorManager.Instance.SelectTarget() ya es un método con el Context, es correcto así
        //input.Select.Select.performed += SelectorManager.Instance.SelectTarget;

        input.Select.Interact.performed += OnInteractPressed;
        input.Anim.Crouch.performed += Crouch;
        input.Anim.Crouch.canceled += StandUp;
    }

    private void OnDisable()
    {
        input.Disable(); // Deshabilita el Input Map

        // --- Desuscripción de Callbacks de Input (MÉTODO DIRECTO) ---
        // Deben coincidir exactamente con la suscripción en OnEnable()
        input.Main.Move.started -= OnMoveStarted;
        input.Main.Move.canceled -= OnMoveCanceled;
        input.Select.Select.performed -= SelectorManager.Instance.SelectTarget;
        input.Select.Interact.performed -= OnInteractPressed;
        input.Anim.Crouch.performed -= Crouch;
        input.Anim.Crouch.canceled -= StandUp;
    }

    // --- ELIMINADO: Ya NO necesitamos el método AssignInputs() ---
    // Este método es lo que causaba el problema de la doble suscripción con lambdas
    /*
    void AssignInputs()
    {
        input.Main.Move.started += ctx => OnMoveStarted(ctx);
        input.Main.Move.canceled += ctx => OnMoveCanceled(ctx);
        input.Select.Select.performed += SelectorManager.Instance.SelectTarget;
        input.Select.Interact.performed += OnInteractPressed;
        input.Anim.Crouch.performed += Crouch;
        input.Anim.Crouch.canceled += StandUp;
    }
    */

    private void Update()
    {
        FaceTarget(); // Mantiene al personaje mirando en la dirección del movimiento

        // Si el clic derecho está presionado, realiza el movimiento continuo
        if (isRightClickPressed)
        {
            ClickToMoveContinuous();
        }

        // Actualizar la velocidad de la animación en base a la velocidad actual del NavMeshAgent
        // Esto es crucial para tu Blend Tree de movimiento (Idle/Running)
        float currentSpeedMagnitude = agent.velocity.magnitude / currentAgentSpeed; // Normalizar la velocidad
        // Si el agente está casi detenido, forzar la velocidad de animación a 0
        if (agent.remainingDistance < agent.stoppingDistance + 0.1f && !agent.pathPending)
        {
            currentSpeedMagnitude = 0f;
        }

        // Interpolar suavemente el valor de Speed para el Animator para transiciones más fluidas
        animator.SetFloat("Speed", Mathf.Lerp(animator.GetFloat("Speed"), currentSpeedMagnitude, Time.deltaTime * 10f));
    }

    // Callbacks para la acción de movimiento (clic derecho)
    private void OnMoveStarted(InputAction.CallbackContext context)
    {
        isRightClickPressed = true;
        SoundManager.Instance.PlaySounds(2); // Reproducimos el sonido de clic
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
                // Considera usar un cooldown o instanciar solo OnMoveStarted si esto crea demasiadas partículas.
                Instantiate(movementClickEffect, hit.point + new Vector3(0, 0.1f, 0), movementClickEffect.transform.rotation);
            }
        }
    }

    // Maneja la interacción al presionar la tecla 'E'
    void OnInteractPressed(InputAction.CallbackContext context)
    {
        GameObject currentlySelectedObject = SelectorManager.Instance.currentlySelectedObject;

        if (currentlySelectedObject != null)
        {
            float distance = Vector3.Distance(transform.position, currentlySelectedObject.transform.position);

            if (distance <= interactionRange)
            {
                IInteractable interactable = currentlySelectedObject.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    SoundManager.Instance.PlaySounds(0);
                    interactable.Interact(currentlySelectedObject);
                    if (currentlySelectedObject.CompareTag("Enemy")) animator.SetTrigger("Attack"); // Si es un enemigo, activa el ataque
                    else animator.SetTrigger("Interact"); // Si es otro objeto, activa la interacción
                }
                else
                {
                    Debug.Log($"El objeto {currentlySelectedObject.name} no es interactuable.");
                }
            }
            else
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

        // Solo si hay movimiento significativo o el agente tiene un destino establecido
        if (agent.hasPath || horizontalVelocity.sqrMagnitude > 0.01f)
        {
            // Calcula la rotación hacia la dirección del movimiento
            Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity.normalized);
            // Aplica una interpolación suave para la rotación
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookRotationSpeed);
        }
    }

    private void Crouch(InputAction.CallbackContext context)
    {
        if (isCrouching) return; // Ya está agachado

        isCrouching = true;
        animator.SetBool("IsCrouching", true); // Activa la animación de agacharse
        currentAgentSpeed = crouchMoveSpeed; // Actualiza la velocidad base
        agent.speed = currentAgentSpeed; // Aplica la nueva velocidad al NavMeshAgent
    }

    private void StandUp(InputAction.CallbackContext context)
    {
        if (!isCrouching) return; // Ya está de pie

        isCrouching = false;
        animator.SetBool("IsCrouching", false); // Desactiva la animación de agacharse
        currentAgentSpeed = normalMoveSpeed; // Restaura la velocidad base
        agent.speed = currentAgentSpeed; // Aplica la nueva velocidad al NavMeshAgent
    }
}