using UnityEngine;
using UnityEngine.AI;

public class AIBehaviour : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float chaseSpeed;
    [SerializeField] private float walkSpeed;
    private float currentSpeed;

    [Header("AI")]
    [SerializeField] private Transform target;
    [SerializeField] private bool hasLOS;
    private NavMeshAgent agent;
    private float distance;

    void Start()
    {
        currentSpeed = walkSpeed;
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        distance = Vector3.Distance(agent.transform.position, target.position);

        if (hasLOS)
        {
            agent.destination = target.position;
        }
    }
}
