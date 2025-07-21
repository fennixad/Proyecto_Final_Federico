using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfView : MonoBehaviour
{
    // --- Variables de Configuración ---
    [Header("Forma de la visión")]
    [Tooltip("Grados totales del cono de visión.")]
    [Range(0f, 360f)] public float viewAngle = 90f;
    [Tooltip("Radio máximo de detección (distancia horizontal en el suelo).")]
    public float viewRadius = 6f;

    [Header("Resolución de la malla")]
    [Tooltip("Define la suavidad del cono de visión. A mayor valor, más detalle.")]
    [Min(2)] public int meshResolution = 40;

    [Header("Puntos de origen")]
    [Tooltip("Offset vertical desde la posición del enemigo para lanzar los Raycast de detección (ej. altura de los ojos/pecho).")]
    public float detectionRaycastHeightOffset = 1.0f;
    [Tooltip("Altura Y LOCAL donde se dibujará la base del cono visual (ej. 0 para los pies si el pivote está allí).")]
    public float visualConeLocalBaseY = 0.0f;

    [Header("Detección")]
    [Tooltip("Capa (Layer) de los objetos que el enemigo debe detectar (ej. el jugador).")]
    public LayerMask targetMask;
    [Tooltip("Capa (Layer) de los objetos que bloquean la visión del enemigo (ej. paredes, cajas).")]
    public LayerMask obstacleMask;
    [Tooltip("Intervalo en segundos entre cada barrido de detección para optimizar el rendimiento.")]
    public float detectionInterval = 0.2f;

    // --- Estado de la Detección (Propiedades públicas) ---
    public bool canSeeTarget { get; private set; } // Indica si el enemigo puede ver actualmente a un objetivo
    public Transform currentTarget { get; private set; } // Referencia al Transform del objetivo actualmente detectado

    // --- Componentes Internos ---
    private MeshFilter viewMeshFilter;
    private Mesh viewMesh;

    // --- Ciclo de Vida del Script ---
    void Awake()
    {
        viewMeshFilter = GetComponent<MeshFilter>();
        viewMesh = new Mesh { name = "View Mesh" };
        viewMeshFilter.mesh = viewMesh;
    }

    void OnEnable() => StartCoroutine(FindTargetsWithDelay()); // Inicia la corrutina de detección al activarse

    IEnumerator FindTargetsWithDelay()
    {
        var wait = new WaitForSeconds(detectionInterval);
        while (enabled)
        {
            yield return wait; // Espera el intervalo
            FindVisibleTargets(); // Ejecuta la lógica de detección
        }
    }

    void LateUpdate() => DrawFieldOfView(); // Dibuja el cono de visión después de todos los Updates

    // --- Lógica de Detección ---
    void FindVisibleTargets()
    {
        bool targetFoundThisScan = false; // Bandera para saber si se encontró un objetivo en esta pasada de escaneo
        Transform foundTargetTransform = null; // Almacena el Transform del objetivo encontrado

        // Origen de los raycasts de detección (desde la altura definida)
        Vector3 detectionOrigin = transform.position + Vector3.up * detectionRaycastHeightOffset;

        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;

        // Barrido de raycasts en el ángulo de visión
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = -viewAngle * 0.5f + stepAngleSize * i + transform.eulerAngles.y;
            Vector3 dir = DirFromAngle(angle, true); // Dirección horizontal del rayo

            RaycastHit hit;
            // Lanza el raycast desde el origen de detección hasta el viewRadius, considerando obstáculos y objetivos
            if (Physics.Raycast(detectionOrigin, dir, out hit, viewRadius, obstacleMask | targetMask))
            {
                // Si el objeto golpeado pertenece a la capa de objetivos
                if (((1 << hit.collider.gameObject.layer) & targetMask) != 0)
                {
                    targetFoundThisScan = true; // Se encontró un objetivo
                    foundTargetTransform = hit.collider.transform; // Guarda su referencia
                    break; // Salir del bucle: ya se encontró el objetivo (asumimos un solo jugador)
                }
            }
        }

        // Actualiza las propiedades públicas después de completar todo el barrido
        canSeeTarget = targetFoundThisScan;
        currentTarget = foundTargetTransform; // Asigna null si no se encontró nada
    }

    // --- Generación del Cono Visual ---
    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        var viewPoints = new List<Vector3>();
        // Origen global del cono visual (en la altura base definida)
        Vector3 visualConeOriginGlobal = transform.position + transform.up * visualConeLocalBaseY;

        // Obtiene los puntos que definen el borde del cono
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = -viewAngle * 0.5f + stepAngleSize * i + transform.eulerAngles.y;
            viewPoints.Add(ViewCast(angle, visualConeOriginGlobal).point);
        }

        // Crea la malla del cono a partir de los puntos
        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = new Vector3(0, visualConeLocalBaseY, 0); // Vértice central del cono (local)
        for (int i = 0; i < viewPoints.Count; i++)
        {
            Vector3 localPoint = transform.InverseTransformPoint(viewPoints[i]);
            localPoint.y = visualConeLocalBaseY; // Asegura que la malla esté plana en la base Y
            vertices[i + 1] = localPoint;

            // Define los triángulos de la malla
            if (i < viewPoints.Count - 1)
            {
                int triIndex = i * 3;
                triangles[triIndex] = 0;
                triangles[triIndex + 1] = i + 1;
                triangles[triIndex + 2] = i + 2;
            }
        }
        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
        viewMesh.RecalculateBounds();
    }

    // Lanza un raycast para determinar la distancia de un punto en el cono
    ViewCastInfo ViewCast(float globalAngle, Vector3 rayOrigin)
    {
        Vector3 dir = DirFromAngle(globalAngle, true); // Dirección del rayo
        RaycastHit hit;
        // Si el rayo golpea un obstáculo, devuelve el punto de colisión
        if (Physics.Raycast(rayOrigin, dir, out hit, viewRadius, obstacleMask))
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        // Si no golpea nada, devuelve un punto al final del radio de visión
        return new ViewCastInfo(false, rayOrigin + dir * viewRadius, viewRadius, globalAngle);
    }

    // Calcula un vector de dirección a partir de un ángulo en grados
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y; // Convertir a ángulo global si es necesario
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    // Estructura para almacenar información de cada raycast del cono
    public struct ViewCastInfo
    {
        public bool hit; // Si el rayo golpeó algo
        public Vector3 point; // El punto golpeado o el final del rayo
        public float distance; // Distancia recorrida por el rayo
        public float angle; // Ángulo del rayo
        public ViewCastInfo(bool hit, Vector3 point, float distance, float angle)
        {
            this.hit = hit;
            this.point = point;
            this.distance = distance;
            this.angle = angle;
        }
    }
}
*/
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfView : MonoBehaviour
{
    // --- Variables de Configuración ---
    [Header("Forma de la visión")]
    [Tooltip("Grados totales del cono de visión.")]
    [Range(0f, 360f)] public float viewAngle = 90f;
    [Tooltip("Radio máximo de detección (distancia horizontal en el suelo).")]
    public float viewRadius = 6f;

    [Header("Resolución de la malla")]
    [Tooltip("Define la suavidad del cono de visión. A mayor valor, más detalle.")]
    [Min(2)] public int meshResolution = 40;

    [Header("Puntos de origen")]
    [Tooltip("Offset vertical desde la posición del enemigo para lanzar los Raycast de detección (ej. altura de los ojos/pecho).")]
    public float detectionRaycastHeightOffset = 1.0f;
    [Tooltip("Altura Y LOCAL donde se dibujará la base del cono visual (ej. 0 para los pies si el pivote está allí).")]
    public float visualConeLocalBaseY = 0.0f;

    [Header("Detección")]
    [Tooltip("Capa (Layer) de los objetos que el enemigo debe detectar (ej. el jugador).")]
    public LayerMask targetMask;
    [Tooltip("Capa (Layer) de los objetos que bloquean la visión del enemigo (ej. paredes, cajas).")]
    public LayerMask obstacleMask;
    [Tooltip("Intervalo en segundos entre cada barrido de detección para optimizar el rendimiento.")]
    public float detectionInterval = 0.2f;

    // --- Optimización de la detección ---
    [Tooltip("Número máximo de colliders a detectar con OverlapSphere. Evita asignaciones de memoria extra.")]
    public int maxOverlapColliders = 5; // Limita los resultados del OverlapSphere
    private Collider[] overlapResults; // Array para reutilizar en OverlapSphereNonAlloc

    // --- Optimización de la malla ---
    [Tooltip("Intervalo en segundos entre cada actualización de la malla visual. 0 para actualizar cada frame.")]
    public float meshUpdateInterval = 0.1f; // Actualizar la malla menos frecuentemente

    // --- Estado de la Detección (Propiedades públicas) ---
    public bool canSeeTarget { get; private set; }
    public Transform currentTarget { get; private set; }

    // --- Componentes Internos ---
    private MeshFilter viewMeshFilter;
    private Mesh viewMesh;

    // --- Variables de control de Corrutinas ---
    private Coroutine findTargetsCoroutine;
    private Coroutine drawMeshCoroutine;


    void Awake()
    {
        viewMeshFilter = GetComponent<MeshFilter>();
        viewMesh = new Mesh { name = "View Mesh" };
        viewMeshFilter.mesh = viewMesh;

        // Inicializa el array para OverlapSphereNonAlloc
        overlapResults = new Collider[maxOverlapColliders];
    }

    void OnEnable()
    {
        // Detener corrutinas antiguas si el script fue deshabilitado y vuelto a habilitar
        if (findTargetsCoroutine != null) StopCoroutine(findTargetsCoroutine);
        if (drawMeshCoroutine != null) StopCoroutine(drawMeshCoroutine);

        findTargetsCoroutine = StartCoroutine(FindTargetsWithDelay());
        if (meshUpdateInterval > 0)
        {
            drawMeshCoroutine = StartCoroutine(DrawFieldOfViewWithDelay());
        }
    }

    void OnDisable()
    {
        // Asegúrate de detener las corrutinas cuando el GameObject se deshabilita
        if (findTargetsCoroutine != null) StopCoroutine(findTargetsCoroutine);
        if (drawMeshCoroutine != null) StopCoroutine(drawMeshCoroutine);
    }

    // --- Corrutina para detección de objetivos (Optimizada) ---
    IEnumerator FindTargetsWithDelay()
    {
        var wait = new WaitForSeconds(detectionInterval);
        while (enabled)
        {
            yield return wait;
            FindVisibleTargets();
        }
    }

    // --- Corrutina para dibujar el cono visual (Optimizada) ---
    IEnumerator DrawFieldOfViewWithDelay()
    {
        var wait = new WaitForSeconds(meshUpdateInterval);
        while (enabled)
        {
            DrawFieldOfViewMesh(); // Renombrado para evitar confusión con el método original
            yield return wait;
        }
    }

    // --- Lógica de Detección (Optimizada) ---
    void FindVisibleTargets()
    {
        bool targetFoundThisScan = false;
        Transform foundTargetTransform = null;
        Vector3 detectionOrigin = transform.position + Vector3.up * detectionRaycastHeightOffset;

        // --- Optimización: Usar OverlapSphereNonAlloc para una detección inicial barata ---
        // Esto es mucho más eficiente que 40 Raycasts para ver si hay ALGO cerca.
        int numColliders = Physics.OverlapSphereNonAlloc(
            detectionOrigin,
            viewRadius,
            overlapResults,
            targetMask
        );

        if (numColliders > 0)
        {
            // Hay uno o más posibles objetivos dentro del radio
            for (int i = 0; i < numColliders; i++)
            {
                Transform potentialTarget = overlapResults[i].transform;
                // Vector del enemigo al objetivo potencial
                Vector3 dirToTarget = (potentialTarget.position + Vector3.up * detectionRaycastHeightOffset - detectionOrigin).normalized;

                // 1. Comprobar si el objetivo está dentro del ángulo de visión
                if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
                {
                    float distToTarget = Vector3.Distance(detectionOrigin, potentialTarget.position + Vector3.up * detectionRaycastHeightOffset);
                    RaycastHit hit;
                    // 2. Lanzar un solo Raycast preciso para verificar si hay obstáculos en medio
                    if (Physics.Raycast(detectionOrigin, dirToTarget, out hit, distToTarget, obstacleMask | targetMask))
                    {
                        // Si el raycast golpea el objetivo, y no un obstáculo
                        if (hit.collider.transform == potentialTarget)
                        {
                            targetFoundThisScan = true;
                            foundTargetTransform = potentialTarget;
                            break; // Se encontró el objetivo más cercano y visible, salir
                        }
                    }
                }
            }
        }

        canSeeTarget = targetFoundThisScan;
        currentTarget = foundTargetTransform; // Asigna null si no se encontró nada
    }

    // --- Generación del Cono Visual (Ahora en su propia corrutina o Update si meshUpdateInterval es 0) ---
    // Renombrado de DrawFieldOfView a DrawFieldOfViewMesh para evitar conflicto si meshUpdateInterval es 0 y se llama desde LateUpdate
    void DrawFieldOfViewMesh()
    {
        // Si meshUpdateInterval es 0, este método se llamaría cada LateUpdate().
        // Si meshUpdateInterval > 0, se llamará a través de la corrutina DrawFieldOfViewWithDelay().

        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        var viewPoints = new List<Vector3>();
        Vector3 visualConeOriginGlobal = transform.position + transform.up * visualConeLocalBaseY;

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = -viewAngle * 0.5f + stepAngleSize * i + transform.eulerAngles.y;
            viewPoints.Add(ViewCast(angle, visualConeOriginGlobal).point);
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = new Vector3(0, visualConeLocalBaseY, 0); // Vértice central del cono (local)
        for (int i = 0; i < viewPoints.Count; i++)
        {
            Vector3 localPoint = transform.InverseTransformPoint(viewPoints[i]);
            localPoint.y = visualConeLocalBaseY; // Asegura que la malla esté plana en la base Y
            vertices[i + 1] = localPoint;

            if (i < viewPoints.Count - 1)
            {
                int triIndex = i * 3;
                triangles[triIndex] = 0;
                triangles[triIndex + 1] = i + 1;
                triangles[triIndex + 2] = i + 2;
            }
        }
        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
        viewMesh.RecalculateBounds();
    }

    // Lanza un raycast para determinar la distancia de un punto en el cono (este sigue siendo para la malla visual)
    ViewCastInfo ViewCast(float globalAngle, Vector3 rayOrigin)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;
        // Solo considera obstáculos para el dibujo del cono
        if (Physics.Raycast(rayOrigin, dir, out hit, viewRadius, obstacleMask))
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        return new ViewCastInfo(false, rayOrigin + dir * viewRadius, viewRadius, globalAngle);
    }

    // Calcula un vector de dirección a partir de un ángulo en grados
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    // Estructura para almacenar información de cada raycast del cono
    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;
        public ViewCastInfo(bool hit, Vector3 point, float distance, float angle)
        {
            this.hit = hit;
            this.point = point;
            this.distance = distance;
            this.angle = angle;
        }
    }

    // --- Lógica para dibujar el cono si meshUpdateInterval es 0 ---
    // Si meshUpdateInterval es 0, el cono se actualizará en cada LateUpdate.
    // Si meshUpdateInterval > 0, el método DrawFieldOfViewWithDelay() lo manejará.
    void LateUpdate()
    {
        if (meshUpdateInterval == 0)
        {
            DrawFieldOfViewMesh();
        }
    }
}