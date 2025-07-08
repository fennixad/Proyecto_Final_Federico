
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

/*
public class Enemies : MonoBehaviour, IInteractable
{
    // --- Variables de Configuración ---
    [Header("Patrullaje")]
    [Tooltip("Puntos a seguir en el patrullaje.")]
    public List<GameObject> patrolPoints;
    [Tooltip("Tipo de patrullaje: secuencial o aleatorio.")]
    public PatrolType patrolType = PatrolType.Sequential;
    [Tooltip("Distancia mínima para considerar que se ha llegado al punto de patrulla.")]
    public float stoppingDistanceThreshold = 0.5f;

    [Header("Tiempo de Espera en Patrulla")]
    [Tooltip("Habilita o deshabilita el tiempo de espera en los puntos de patrulla.")]
    public bool enablePatrolIdleTime = false;
    [Tooltip("Define si el tiempo de espera es fijo o aleatorio entre un rango.")]
    public PatrolIdleMode patrolIdleMode = PatrolIdleMode.Fixed;
    [Tooltip("Tiempo de espera fijo que el enemigo se quedará en Idle en un punto de patrulla (si el modo es Fijo).")]
    [Min(0f)] public float fixedPatrolIdleTime = 2f;
    [Tooltip("Tiempo mínimo que el enemigo se quedará en Idle en un punto de patrulla (si el modo es Aleatorio).")]
    [Min(0f)] public float minRandomPatrolIdleTime = 1f;
    [Tooltip("Tiempo máximo que el enemigo se quedará en Idle en un punto de patrulla (si el modo es Aleatorio).")]
    [Min(0f)] public float maxRandomPatrolIdleTime = 3f;

    [Header("Detección y Persecución")]
    [Tooltip("Distancia a la que el enemigo pierde de vista al jugador.")]
    public float loseTargetDistance = 15f;

    [Header("Material del Cono de Visión")]
    [Tooltip("Renderizador de malla para el cono de visión.")]
    public MeshRenderer coneMeshRenderer;
    [Tooltip("Material del cono cuando el enemigo está en estado normal (idle/patrulla).")]
    public Material normalConeMaterial;
    [Tooltip("Material del cono cuando el enemigo ha detectado al jugador.")]
    public Material detectedConeMaterial;

    // --- Variables Internas ---
    private bool isStunned = false;
    private int currentPatrolIndex = 0;
    public EnemyState currentState;
    private NavMeshAgent navMeshAgent;
    private FieldOfView fieldOfView;
    private float currentPatrolIdleDuration;
    private Animator anim;

    // --- Ciclo de Vida del Script ---
    private void Awake()
    {
        anim = transform.GetChild(0).GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        fieldOfView = GetComponent<FieldOfView>();
        if (fieldOfView == null) Debug.LogError("FieldOfView not found on " + name);
        if (coneMeshRenderer == null) coneMeshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        currentState = EnemyState.Idle;
        StateChange(patrolPoints != null && patrolPoints.Count > 0 ? EnemyState.Patrolling : EnemyState.Idle);
    }

    private void Update()
    {
        if (isStunned) return;

        switch (currentState)
        {
            case EnemyState.Idle:
                // Si el enemigo está en Idle (ya sea por patrulla o sin puntos) y ve al jugador, cambia a Atacando.
                // La espera de patrulla se maneja en la corrutina PatrolIdleCoroutine.
                if (fieldOfView != null && fieldOfView.canSeeTarget)
                {
                    StateChange(EnemyState.Chasing);
                }
                break;

            case EnemyState.Patrolling:
                // Si el FieldOfView detecta al objetivo, cambia a estado de ataque
                if (fieldOfView != null && fieldOfView.canSeeTarget)
                {
                    StateChange(EnemyState.Chasing);
                }
                // Si está patrullando y no detecta, comprueba y establece el siguiente destino
                else
                {
                    CheckAndSetPatrolDestination();
                }
                break;

            case EnemyState.Chasing:
                // Si pierde de vista al objetivo o este se aleja demasiado, vuelve a patrullar
                if (fieldOfView == null || !fieldOfView.canSeeTarget ||
                    (fieldOfView.currentTarget != null && Vector3.Distance(transform.position, fieldOfView.currentTarget.position) > loseTargetDistance))
                {
                    StateChange(EnemyState.Patrolling);
                }
                // Si sigue viendo al objetivo, actualiza constantemente su destino a la posición del jugador
                else if (fieldOfView.currentTarget != null && navMeshAgent.isOnNavMesh)
                {
                    navMeshAgent.SetDestination(fieldOfView.currentTarget.position);
                }
                break;
        }
    }

    // --- Métodos de Interacción y Cambio de Estado ---
    public void Interact(GameObject interactor)
    {
        if (currentState == EnemyState.Chasing || isStunned) return;
        StateChange(EnemyState.Stunned);
    }

    private void StateChange(EnemyState newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        // Detener todas las corrutinas activas al cambiar de estado para evitar comportamientos no deseados
        StopAllCoroutines();

        // Cambiar el material del cono de visión según el estado
        if (coneMeshRenderer != null)
        {
            Material[] mats = coneMeshRenderer.materials;
            mats[0] = (newState == EnemyState.Chasing && detectedConeMaterial != null) ? detectedConeMaterial : normalConeMaterial;
            coneMeshRenderer.materials = mats;
        }

        // Acciones específicas al cambiar de estado
        switch (newState)
        {
            case EnemyState.Idle:
                navMeshAgent.isStopped = true; // Detiene al NavMeshAgent
                // Si la espera de patrulla está habilitada, calcula la duración y empieza la espera
                if (enablePatrolIdleTime)
                {
                    // Calcula la duración según el modo seleccionado (fijo o aleatorio)
                    currentPatrolIdleDuration = (patrolIdleMode == PatrolIdleMode.Fixed) ?
                                                fixedPatrolIdleTime :
                                                Random.Range(minRandomPatrolIdleTime, maxRandomPatrolIdleTime);
                    anim.SetFloat("Speed", 0f); // Asegura que la animación esté en Idle
                    StartCoroutine(PatrolIdleCoroutine(currentPatrolIdleDuration));
                }
                break;
            case EnemyState.Patrolling:
                navMeshAgent.isStopped = false; // Permite que el NavMeshAgent se mueva
                anim.SetFloat("Speed", 0.4f); // Asegura que la animación esté en Patrullando
                navMeshAgent.speed = 1.5f; // Ajusta la velocidad del agente de patrullaje
                if (patrolPoints != null && patrolPoints.Count > 0 && navMeshAgent.isOnNavMesh)
                    SetNextPatrolDestination();
                else StateChange(EnemyState.Idle); // Si no hay puntos, vuelve a Idle
                break;
            case EnemyState.Chasing:
                navMeshAgent.isStopped = false;
                anim.SetFloat("Speed", 1f); // Asegura que la animación esté en Atacando
                navMeshAgent.speed = 2.5f;
                if (fieldOfView?.currentTarget != null && navMeshAgent.isOnNavMesh)
                    navMeshAgent.SetDestination(fieldOfView.currentTarget.position);
                break;
            case EnemyState.Stunned:
                anim.SetFloat("Speed", 0f); // Asegura que la animación esté en Idle
                anim.SetBool("Stunned", true); // Activa la animación de aturdimiento
                navMeshAgent.isStopped = true;
                StartCoroutine(StunEnemyCoroutine());
                break;
        }
    }

    // --- Lógica de Patrullaje ---
    void CheckAndSetPatrolDestination()
    {
        if (patrolPoints == null || patrolPoints.Count == 0 || !navMeshAgent.isOnNavMesh)
        {
            StateChange(EnemyState.Idle);
            return;
        }

        // Si el agente está cerca de su destino actual y ha llegado
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + stoppingDistanceThreshold &&
            (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f))
        {
            // Si el tiempo de espera está habilitado, cambia a Idle para esperar.
            // De lo contrario, va directamente al siguiente punto.
            if (enablePatrolIdleTime)
            {
                StateChange(EnemyState.Idle);
            }
            else
            {
                SetNextPatrolDestination();
            }
        }
    }

    void SetNextPatrolDestination()
    {
        if (patrolPoints.Count == 0) return;

        Vector3 targetPosition = (patrolType == PatrolType.Sequential) ?
                                 patrolPoints[++currentPatrolIndex % patrolPoints.Count].transform.position :
                                 patrolPoints[Random.Range(0, patrolPoints.Count)].transform.position;

        navMeshAgent.SetDestination(targetPosition);
    }

    // --- Corrutinas ---
    IEnumerator PatrolIdleCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        // Después del tiempo de espera, si no ha detectado al jugador, sigue patrullando
        if (!fieldOfView.canSeeTarget)
        {
            StateChange(EnemyState.Patrolling);
        }
        // Si el jugador fue detectado durante el idle, el Update ya lo habría cambiado a Attacking.
    }

    IEnumerator StunEnemyCoroutine()
    {
        yield return new WaitForSeconds(0.5f); // Espera breve antes de aplicar el aturdimiento
        anim.SetBool("Stunned", false); // Activa la animación de aturdimiento
        anim.SetBool("Stunning", true); // Activa la animación de aturdimiento
        yield return new WaitForSeconds(3.5f);
        isStunned = false;
        anim.SetBool("Stunning", false); // Desactiva la animación de aturdimiento
        StateChange(patrolPoints != null && patrolPoints.Count > 0 ? EnemyState.Patrolling : EnemyState.Idle);
    }

    // --- Enumeraciones ---
    public enum EnemyState { Idle, Patrolling, Chasing, Stunned, Attacking }
    public enum PatrolType { Sequential, Random }
    public enum PatrolIdleMode { Fixed, Random } // Definición de la nueva enumeración
}
*/
public class Enemies : MonoBehaviour, IInteractable
{
    [Header("Patrullaje")]
    public List<GameObject> patrolPoints;
    public PatrolType patrolType = PatrolType.Sequential;
    public float stoppingDistanceThreshold = 0.5f;

    [Header("Tiempo de Espera en Patrulla")]
    public bool enablePatrolIdleTime = false;
    public PatrolIdleMode patrolIdleMode = PatrolIdleMode.Fixed;
    [Min(0f)] public float fixedPatrolIdleTime = 2f;
    [Min(0f)] public float minRandomPatrolIdleTime = 1f;
    [Min(0f)] public float maxRandomPatrolIdleTime = 3f;

    [Header("Detección y Persecución")]
    public float loseTargetDistance = 15f;
    [Tooltip("Distancia a la que el enemigo ejecuta un ataque cuerpo a cuerpo.")]
    public float meleeAttackDistance = 2f;

    [Header("Material del Cono de Visión")]
    public MeshRenderer coneMeshRenderer;
    public Material normalConeMaterial;
    public Material detectedConeMaterial;

    private bool isStunned = false;
    private int currentPatrolIndex = 0;
    public EnemyState currentState;
    private NavMeshAgent navMeshAgent;
    private FieldOfView fieldOfView;
    private float currentPatrolIdleDuration;
    private Animator anim;

    private void Awake()
    {
        anim = transform.GetChild(0).GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        fieldOfView = GetComponent<FieldOfView>();
        if (fieldOfView == null) Debug.LogError("FieldOfView not found on " + name);
        if (coneMeshRenderer == null) coneMeshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        currentState = EnemyState.Idle;
        StateChange(patrolPoints != null && patrolPoints.Count > 0 ? EnemyState.Patrolling : EnemyState.Idle);
    }

    private void Update()
    {
        if (isStunned) return;

        switch (currentState)
        {
            case EnemyState.Idle: HandleIdleUpdate(); break;
            case EnemyState.Patrolling: HandlePatrollingUpdate(); break;
            case EnemyState.Chasing: HandleChasingUpdate(); break;
            case EnemyState.Attacking: HandleAttackingUpdate(); break;
        }
    }

    private void HandleIdleUpdate()
    {
        if (fieldOfView != null && fieldOfView.canSeeTarget)
            StateChange(EnemyState.Chasing);
    }

    private void HandlePatrollingUpdate()
    {
        if (fieldOfView != null && fieldOfView.canSeeTarget)
            StateChange(EnemyState.Chasing);
        else
            CheckAndSetPatrolDestination();
    }

    private void HandleChasingUpdate()
    {
        if (fieldOfView == null || !fieldOfView.canSeeTarget ||
            (fieldOfView.currentTarget != null &&
             Vector3.Distance(transform.position, fieldOfView.currentTarget.position) > loseTargetDistance))
        {
            StateChange(EnemyState.Patrolling);
        }
        else if (fieldOfView.currentTarget != null && navMeshAgent.isOnNavMesh)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, fieldOfView.currentTarget.position);

            if (distanceToPlayer <= meleeAttackDistance)
            {
                StateChange(EnemyState.Attacking);
            }
            else
            {
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(fieldOfView.currentTarget.position);
            }
        }
    }

    private void HandleAttackingUpdate()
    {
        // Aquí no hacemos nada por frame.
        // Si quieres que mire al jugador sin rotar físicamente, puedes usar transform.LookAt con constraints.
    }

    public void Interact(GameObject interactor)
    {
        if (currentState == EnemyState.Chasing || isStunned) return;
        StateChange(EnemyState.Stunned);
    }

    private void StateChange(EnemyState newState)
    {
        if (currentState == newState) return;

        StopAllCoroutines();
        currentState = newState;
        UpdateConeMaterial(newState);

        switch (newState)
        {
            case EnemyState.Idle: EnterIdleState(); break;
            case EnemyState.Patrolling: EnterPatrollingState(); break;
            case EnemyState.Chasing: EnterChasingState(); break;
            case EnemyState.Stunned: EnterStunnedState(); break;
            case EnemyState.Attacking: EnterAttackingState(); break;
        }
    }

    private void UpdateConeMaterial(EnemyState state)
    {
        if (coneMeshRenderer != null)
        {
            Material[] mats = coneMeshRenderer.materials;
            mats[0] = (state == EnemyState.Chasing && detectedConeMaterial != null)
                ? detectedConeMaterial
                : normalConeMaterial;
            coneMeshRenderer.materials = mats;
        }
    }

    private void EnterIdleState()
    {
        navMeshAgent.isStopped = true;
        anim.SetFloat("Speed", 0f);

        if (enablePatrolIdleTime)
        {
            currentPatrolIdleDuration = (patrolIdleMode == PatrolIdleMode.Fixed) ?
                                         fixedPatrolIdleTime :
                                         Random.Range(minRandomPatrolIdleTime, maxRandomPatrolIdleTime);

            StartCoroutine(PatrolIdleCoroutine(currentPatrolIdleDuration));
        }
    }

    private void EnterPatrollingState()
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = 1.5f;
        anim.SetFloat("Speed", 0.4f);

        if (patrolPoints != null && patrolPoints.Count > 0 && navMeshAgent.isOnNavMesh)
            SetNextPatrolDestination();
        else
            StateChange(EnemyState.Idle);
    }

    private void EnterChasingState()
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = 2.5f;
        anim.SetFloat("Speed", 1f);

        if (fieldOfView?.currentTarget != null && navMeshAgent.isOnNavMesh)
            navMeshAgent.SetDestination(fieldOfView.currentTarget.position);
    }

    private void EnterAttackingState()
    {
        navMeshAgent.isStopped = true;
        anim.SetFloat("Speed", 0f);
        anim.SetTrigger("Attack");

        StartCoroutine(AttackAndGameOverCoroutine());
    }

    private void EnterStunnedState()
    {
        navMeshAgent.isStopped = true;
        anim.SetFloat("Speed", 0f);
        anim.SetBool("Stunned", true);
        StartCoroutine(StunEnemyCoroutine());
    }

    void CheckAndSetPatrolDestination()
    {
        if (patrolPoints == null || patrolPoints.Count == 0 || !navMeshAgent.isOnNavMesh)
        {
            StateChange(EnemyState.Idle);
            return;
        }

        if (!navMeshAgent.pathPending &&
            navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + stoppingDistanceThreshold &&
            (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f))
        {
            if (enablePatrolIdleTime)
                StateChange(EnemyState.Idle);
            else
                SetNextPatrolDestination();
        }
    }

    void SetNextPatrolDestination()
    {
        if (patrolPoints.Count == 0) return;

        Vector3 targetPosition = (patrolType == PatrolType.Sequential) ?
                                 patrolPoints[++currentPatrolIndex % patrolPoints.Count].transform.position :
                                 patrolPoints[Random.Range(0, patrolPoints.Count)].transform.position;

        navMeshAgent.SetDestination(targetPosition);
    }

    IEnumerator PatrolIdleCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (!fieldOfView.canSeeTarget)
            StateChange(EnemyState.Patrolling);
    }

    IEnumerator StunEnemyCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        anim.SetBool("Stunned", false);
        anim.SetBool("Stunning", true);
        yield return new WaitForSeconds(3.5f);
        isStunned = false;
        anim.SetBool("Stunning", false);
        StateChange(patrolPoints != null && patrolPoints.Count > 0 ? EnemyState.Patrolling : EnemyState.Idle);
    }

    IEnumerator AttackAndGameOverCoroutine()
    {
        yield return new WaitForSeconds(1f); // Ajusta según la duración de la animación
        SceneManager.LoadScene(2);
    }

    public enum EnemyState { Idle, Patrolling, Chasing, Stunned, Attacking }
    public enum PatrolType { Sequential, Random }
    public enum PatrolIdleMode { Fixed, Random }
}