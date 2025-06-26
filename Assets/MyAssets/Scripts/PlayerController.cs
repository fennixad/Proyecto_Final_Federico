using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
public class PlayerController : MonoBehaviour
{
    Custom_Actions input;
    NavMeshAgent agent;
    Animator animator;
    Camera cmCamera;
    [SerializeField] GameObject targetEffect;
    [Header("Movement Settings")]
    [SerializeField] ParticleSystem clickEffect;
    [SerializeField] LayerMask clickableLayers;

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
        input.Main.Move.performed += ctx => ClickToMove();
        input.Select.Select.performed += ctx => ClickToTarget();
        input.Enable(); // Enable the input system
    }

    void ClickToMove()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100f, clickableLayers))
        {
            agent.SetDestination(hit.point);
            if (clickEffect != null)
            {
                Instantiate(clickEffect, hit.point += new Vector3(0, 0.1f, 0), clickEffect.transform.rotation);
            }
        }
    }
    void ClickToTarget()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100f, clickableLayers))
        {
            if (hit.collider.transform.CompareTag("SelectOne"))
            {
                Instantiate(targetEffect,hit.collider.transform.position, Quaternion.identity);
                Debug.Log("Clicked on ClickOne object");
            }
            else if (hit.collider.transform.CompareTag("SelectTwo"))
            {
                Debug.Log("Clicked on ClickTwo object");
            }
            else
            {
                Debug.Log("Clicked on an object that is not SelectOne or SelectTwo");
            }
        }
    }
    void FaceTarget()
    {
        Vector3 direction = (agent.destination - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, lookRotationSpeed * Time.deltaTime);
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
