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
