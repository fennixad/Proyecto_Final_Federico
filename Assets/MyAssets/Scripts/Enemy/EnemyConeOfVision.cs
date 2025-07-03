using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class EnemyVisionController : MonoBehaviour
{
    [Header("Configuración del Cono de Visión")]
    [Tooltip("Referencia al GameObject del jugador.")]
    public GameObject player;
    [Range(0, 360)]
    [Tooltip("Ángulo total del cono de visión en grados.")]
    public float visionAngle = 90f; // Ángulo de visión del enemigo
    [Tooltip("Distancia máxima del cono de visión.")]
    public float visionDistance = 10f; // Distancia de visión del enemigo
    [Tooltip("Número de segmentos para dibujar el arco del cono. Más segmentos = más suave.")]
    public int coneSegments = 20; // Para la calidad del dibujo del Line Renderer
    [Tooltip("El material para el Line Renderer (debe permitir transparencia).")]
    public Material coneMaterial; // Nuevo: Material para el Line Renderer
    [Tooltip("Ancho de la línea del cono.")]
    public float coneLineWidth = 0.1f; // Ancho del Line Renderer

    [Header("Estado de Detección")]
    [Tooltip("Indica si el jugador ha sido detectado (dentro del ángulo y distancia).")]
    public bool playerDetected = false;
    [Tooltip("Indica si el jugador está dentro de la distancia de visión.")]
    public bool playerInVisionRange = false;

    private LineRenderer lineRenderer;

    void Awake()
    {
        // Obtener o añadir el Line Renderer
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // Configurar el Line Renderer
        lineRenderer.useWorldSpace = false; // Queremos que sea local al transform del enemigo
        lineRenderer.startWidth = coneLineWidth;
        lineRenderer.endWidth = coneLineWidth;
        if (coneMaterial != null)
        {
            lineRenderer.material = coneMaterial;
        }
        else
        {
            Debug.LogWarning("¡Material del cono no asignado! El cono no se dibujará correctamente.");
            // Asignar un material por defecto si no hay ninguno para evitar errores
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
        lineRenderer.positionCount = coneSegments + 2; // Dos puntos para el origen y los segmentos del arco

        // Ocultar el cono inicialmente si no queremos que se vea hasta que el juego empiece,
        // aunque el requerimiento es que se vea siempre, entonces lo dejamos activo.
        lineRenderer.enabled = true; // Asegurarse de que el Line Renderer esté habilitado
    }

    void Start()
    {
        // Verificar que se haya asignado el jugador
        if (player == null)
        {
            Debug.LogError("¡Jugador no asignado en EnemyVisionController para " + gameObject.name + "!");
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("No se pudo encontrar un GameObject con el tag 'Player'. Asigna el jugador manualmente.");
                enabled = false;
                lineRenderer.enabled = false; // Desactivar el Line Renderer si no hay jugador
                return;
            }
        }

        // Dibujar el cono inicial
        DrawVisionCone();
    }

    void Update()
    {
        CheckVisionCone(); // Llama a la función para verificar el cono de visión
        DrawVisionCone(); // Dibuja el cono en cada frame (por si el enemigo gira)
    }

    public void CheckVisionCone()
    {
        if (player == null || !player.activeInHierarchy)
        {
            playerDetected = false;
            playerInVisionRange = false;
            return;
        }

        Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;

        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);

        float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
        playerInVisionRange = (distanceToPlayer <= visionDistance);

        if (playerInVisionRange && angleToPlayer < (visionAngle * 0.5f))
        {
            RaycastHit hit;
            // Configura tus capas de obstáculos aquí. No incluyas la capa del jugador.
            LayerMask obstacleLayers = LayerMask.GetMask("Default", "Environment"); // ¡AJUSTA ESTO A TUS CAPAS REALES!

            if (Physics.Raycast(transform.position, dirToPlayer, out hit, visionDistance, obstacleLayers))
            {
                if (hit.collider.gameObject == player)
                {
                    playerDetected = true;
                }
                else
                {
                    playerDetected = false;
                }
            }
            else
            {
                playerDetected = true;
            }
        }
        else
        {
            playerDetected = false;
        }
    }

    /// <summary>
    /// Dibuja el cono de visión usando el Line Renderer.
    /// </summary>
    void DrawVisionCone()
    {
        if (lineRenderer == null) return;

        // El primer punto es el origen (la posición del enemigo)
        lineRenderer.SetPosition(0, Vector3.zero);

        // Calcular el ángulo inicial (la mitad izquierda del cono)
        float startAngle = -visionAngle / 2f;

        // Iterar para dibujar el arco
        for (int i = 0; i <= coneSegments; i++)
        {
            float currentAngle = startAngle + (i * (visionAngle / coneSegments));
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0); // Rotación en el eje Y (horizontal)
            Vector3 direction = rotation * Vector3.forward; // Dirección en el cono
            Vector3 point = direction * visionDistance; // Punto en el borde del cono

            // Ajustar el punto para que esté en el "suelo" (Y=0), si el enemigo está en el suelo.
            // Esto asume que el Line Renderer está en la misma posición Y que el enemigo.
            // Si quieres que el cono se proyecte en el suelo sin importar la altura del enemigo:
            // point.y = 0; // O la altura del NavMesh si es relevante

            lineRenderer.SetPosition(i + 1, point);
        }

        // El último punto conecta de vuelta al origen
        lineRenderer.SetPosition(coneSegments + 1, Vector3.zero);

        // Opcional: Cambiar el color del cono si el jugador está detectado
        if (playerDetected)
        {
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
        }
        else
        {
            // Un color por defecto transparente (ajústalo a tu gusto)
            lineRenderer.startColor = new Color(0, 1, 0, 0.2f); // Verde transparente
            lineRenderer.endColor = new Color(0, 1, 0, 0.2f);
        }
    }

    public bool IsPlayerDetected()
    {
        return playerDetected;
    }

    // El Gizmo ya no es tan crítico con el Line Renderer, pero puede seguir siendo útil para depuración.
    void OnDrawGizmosSelected()
    {
        if (transform == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * visionDistance);

        Vector3 leftRayRotation = Quaternion.Euler(0, -visionAngle * 0.5f, 0) * transform.forward;
        Vector3 rightRayRotation = Quaternion.Euler(0, visionAngle * 0.5f, 0) * transform.forward;

        Gizmos.DrawRay(transform.position, leftRayRotation * visionDistance);
        Gizmos.DrawRay(transform.position, rightRayRotation * visionDistance);

#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireArc(transform.position, transform.up, leftRayRotation, visionAngle, visionDistance);
#endif
    }
}
