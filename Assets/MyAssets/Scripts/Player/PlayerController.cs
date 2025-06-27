using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
public class PlayerController : MonoBehaviour
{
    Custom_Actions input;
    NavMeshAgent agent;
    Animator animator;
    Camera cmCamera;

    [Header("Target Effect Settings")]
    [SerializeField] GameObject targetEffectPrefab;
    private GameObject currentInstantiatedEffect;  // Referencia al efecto instanciado actualmente en la escena
    private GameObject currentlySelectedObject;    // Referencia al GameObject que tiene el efecto de selecci�n

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
        // --- Cambios aqu�: Manejo de los eventos del clic derecho para el movimiento ---
        // Asignar el evento del clic derecho al contexto de la acci�n 'Move'
        // 'started' se dispara cuando el bot�n se presiona por primera vez.
        input.Main.Move.started += ctx => OnMoveStarted(ctx);
        // 'performed' se dispara continuamente mientras el bot�n est� presionado Y/O al final del clic (dependiendo del tipo de control).
        // Lo usaremos para el primer click y luego la bandera para el arrastre.
        input.Main.Move.performed += ctx => OnMovePerformed(ctx);
        // 'canceled' se dispara cuando el bot�n se suelta.
        input.Main.Move.canceled += ctx => OnMoveCanceled(ctx);
        // --- Fin cambios ---

        input.Select.Select.performed += ctx => ClickToTarget();
    }

    private void OnMoveStarted(InputAction.CallbackContext context)
    {
        isRightClickPressed = true;
        // Opcional: Podr�as hacer aqu� un primer movimiento instant�neo al presionar
        // ClickToMoveContinuous(); // Ya se har� en performed si el control es un "Button"
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        // Este se dispara una vez para el clic simple y tambi�n si el "Interaction" del Input Action
        // es de tipo "Hold" y el bot�n se mantiene.
        // Lo usamos para asegurar que el movimiento inicial del clic se procese.
        ClickToMoveContinuous();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        isRightClickPressed = false;
    }

    // --- Fin nuevos m�todos para el Input System ---

    void ClickToMoveContinuous()
    {
        RaycastHit hit;
        // Usamos Input.mousePosition directamente, ya que la acci�n de input ya est� activa.
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100f, clickableLayers))
        {
            agent.SetDestination(hit.point);
            if (clickEffect != null)
            {
                // Instancia el efecto de click en la posici�n del hit con un peque�o offset vertical
                var effectInstance = Instantiate(clickEffect, hit.point + new Vector3(0, 0.1f, 0), clickEffect.transform.rotation);
                Destroy(effectInstance.gameObject, 0.2f);
            }
        }
    }
    void ClickToTarget()
    {
        RaycastHit hit;
        // Siempre usa Camera.main para el rayo de la c�mara principal
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100f, clickableLayers))
        {
            // Verificamos si el objeto clickeado es diferente al objeto actualmente seleccionado
            if (hit.collider.gameObject != currentlySelectedObject)
            {
                // Paso 1: Si ya hay un efecto instanciado, destr�yelo del objeto anterior
                if (currentInstantiatedEffect != null)
                {
                    Destroy(currentInstantiatedEffect);
                    currentInstantiatedEffect = null; // Limpiar la referencia
                }

                // Paso 2: Instanciar el nuevo efecto en el objeto clickeado
                if (targetEffectPrefab != null) // Asegurarse de que el prefab est� asignado
                {
                    // Instanciar el prefab. Es bueno hacerlo hijo del objeto clickeado para que se mueva con �l.
                    currentInstantiatedEffect = Instantiate(targetEffectPrefab, hit.collider.transform.position, Quaternion.identity);
                    currentInstantiatedEffect.transform.SetParent(hit.collider.transform); // Hacerlo hijo del objeto clickeado

                    // Opcional: Ajustar la posici�n local si el efecto no encaja bien en el centro del objeto padre
                    currentInstantiatedEffect.transform.localPosition = Vector3.zero; // O un offset espec�fico  
                    currentInstantiatedEffect.transform.localPosition += new Vector3(0, -0.4f, 0); // Ajuste para que este a los pies del objeto.
                }
                else
                {
                    Debug.LogWarning("�targetEffectPrefab no asignado en el Inspector!");
                }

                // Paso 3: Actualizar la referencia al objeto actualmente seleccionado
                currentlySelectedObject = hit.collider.gameObject;
            }
            else
            {
                // El jugador hizo clic en el mismo objeto que ya est� seleccionado.
                // Aqu�, simplemente no hacemos nada seg�n tus requisitos: "luego ya no si clickeas a ese objeto y tiene el prefab puesto que ya no instancie mas."
                Debug.Log($"Ya seleccionado: {hit.collider.gameObject.name}");
            }
        }
        else
        {
            // Si se hace clic en un �rea que no es "clickableLayers" o no golpea nada,
            // puedes decidir si quieres "deseleccionar" el objeto actual.
            // Por ahora, no lo haremos seg�n tu descripci�n original.
            // Si quisieras deseleccionar al hacer clic en el vac�o:
            /*
            if (currentInstantiatedEffect != null)
            {
                Destroy(currentInstantiatedEffect);
                currentInstantiatedEffect = null;
                currentlySelectedObject = null;
                Debug.Log("Deseleccionado.");
            }
            */
            Debug.Log("Click fuera de una capa clickeable.");
        }
    }
    void FaceTarget()
    {
        if (agent.velocity.sqrMagnitude > 0.1f) // Solo si el agente se est� moviendo
        {
            Vector3 lookDirection = agent.steeringTarget - transform.position;
            if (lookDirection != Vector3.zero)
            {
                lookDirection.y = 0; // Evita rotar en el eje X si el objetivo est� arriba/abajo
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
