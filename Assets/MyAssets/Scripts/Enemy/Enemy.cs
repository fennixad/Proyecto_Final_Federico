using NUnit.Framework;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemies : MonoBehaviour, IInteractable
{
    public static Enemies Instance { get; private set; } 
    private bool isStunned = false;

    [Header("Patrullaje")]
    [Tooltip("Lista de GameObjects (puntos) que el enemigo usará para patrullar.")]
    public List<GameObject> patrolPoints; 
    [Tooltip("Define si el patrullaje es secuencial (punto por punto) o aleatorio.")]
    public PatrolType patrolType = PatrolType.Sequential; // Tipo de patrullaje
    [Tooltip("Distancia mínima al objetivo para considerarlo 'alcanzado'.")]
    public float stoppingDistanceThreshold = 0.5f; // Umbral para saber si ha llegado al destino

    private int currentPatrolIndex = 0; // Para el patrullaje secuencial
    public EnemyState currentState;
    public NavMeshAgent navMeshAgent;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        currentState = EnemyState.Idle; 

        // Si hay puntos de patrullaje, iniciar el patrullaje
        if (patrolPoints != null && patrolPoints.Count > 0)
        {
            StateChange(EnemyState.Patrolling);
        }
        else
        {
            Debug.LogWarning($"El enemigo {gameObject.name} no tiene puntos de patrulla asignados. Permanece en Idle.");
        }
    }

    private void Update()
    {
        // Solo maneja la lógica de patrullaje si el enemigo está en estado de Patrullaje
        if (currentState == EnemyState.Patrolling && !isStunned)
        {
            CheckAndSetPatrolDestination();
        }
        // ... otras lógicas para otros estados
    }

    public void Interact(GameObject interactor)
    {
        if (EnemyState.Attacking == currentState) return; // Evita interacciones si está atacando
        Debug.Log($"Interactuando con el enemigo: {gameObject.name} desde {interactor.name}");
        if (!isStunned) // Solo aturdir si no está ya aturdido
        {
            isStunned = true;
            // Sonido de aturdimiento
            StateChange(EnemyState.Stunned); // Cambia el estado a Aturdido
        }
    }

    private void StateChange(EnemyState newState)
    {
        if (currentState == newState) return; // Evita cambios de estado redundantes

        currentState = newState;
        switch (newState)
        {
            case EnemyState.Idle:
                navMeshAgent.isStopped = true; // Detener al NavMeshAgent
                // Animacion IDLE
                Debug.Log("Enemigo en estado Idle.");
                break;
            case EnemyState.Patrolling:
                navMeshAgent.isStopped = false; // Reanudar al NavMeshAgent
                // No llamamos a Patrol() directamente aquí, se gestiona en Update() via CheckAndSetPatrolDestination
                Debug.Log("Enemigo en estado Patrullando.");
                break;
            case EnemyState.Attacking:
                navMeshAgent.isStopped = true; // Detener al NavMeshAgent para atacar
                Debug.Log("Enemigo en estado Attacking.");
                // Aquí iría la lógica de ataque
                break;
            case EnemyState.Stunned:
                navMeshAgent.isStopped = true; // Detener al NavMeshAgent al aturdir
                StartCoroutine(StunEnemyCoroutine()); // Inicia la corrutina para aturdir al enemigo
                Debug.Log("Enemigo en estado Aturdido.");
                break;
        }
    }

    /// <summary>
    /// Verifica si el agente ha llegado a su destino y, si es así, establece uno nuevo.
    /// </summary>
    void CheckAndSetPatrolDestination()
    {
        // Solo continuar si tenemos puntos de patrulla y el agente está activo y no está esperando una ruta
        if (patrolPoints == null || patrolPoints.Count == 0 || !navMeshAgent.isOnNavMesh)
        {
            StateChange(EnemyState.Idle); // Si no hay destinos o no está en el NavMesh, pasa a Idle
            return;
        }

        // Si el agente no tiene una ruta o ha llegado muy cerca de su destino actual
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + stoppingDistanceThreshold)
        {
            // Asegurarse de que el agente realmente ha parado si no hay más camino
            if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)
            {
                SetNextPatrolDestination(); // Establece el siguiente punto de patrulla
            }
        }
    }

    /// <summary>
    /// Establece el siguiente destino para el NavMeshAgent, según el tipo de patrullaje.
    /// </summary>
    void SetNextPatrolDestination()
    {
        if (patrolPoints.Count == 0) return;

        Vector3 targetPosition;

        if (patrolType == PatrolType.Sequential)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count; // Ciclo a través de los puntos
            targetPosition = patrolPoints[currentPatrolIndex].transform.position;
        }
        else // PatrolType.Random
        {
            int randomIndex = Random.Range(0, patrolPoints.Count); // GetRandomNumber(0, patrolPoints.Count - 1) si quieres usar tu método
            targetPosition = patrolPoints[randomIndex].transform.position;
        }

        navMeshAgent.SetDestination(targetPosition);
        Debug.Log($"Estableciendo destino de patrulla a: {targetPosition}");
    }


    IEnumerator StunEnemyCoroutine() // Renombrado para evitar conflicto con el método Interact
    {
        navMeshAgent.isStopped = true; // Asegurarse de que el agente se detiene
        yield return new WaitForSeconds(2f); // Espera 2 segundos
        isStunned = false;
        navMeshAgent.isStopped = false; // Reanudar al agente
        StateChange(EnemyState.Patrolling); // Vuelve al patrullaje después de aturdirse
    }

    // Enumeración para el estado del enemigo
    public enum EnemyState // Cambiado a public para poder ser visto por otros scripts si es necesario
    {
        Idle,
        Patrolling,
        Attacking,
        Stunned
    }

    // Enumeración para el tipo de patrullaje
    public enum PatrolType
    {
        Sequential,
        Random
    }
}
