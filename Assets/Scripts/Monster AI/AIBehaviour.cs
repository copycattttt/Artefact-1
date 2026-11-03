using UnityEngine;
using UnityEngine.AI;

public class AIBehaviour : MonoBehaviour
{
    private AIController aiController;

    [Header("Stats")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float chaseSpeed;
    [SerializeField] private float angularBase;
    private float currentSpeed;

    [Header("AI")]
    public Vector3 target;
    [SerializeField] private bool hasLOS;
    private NavMeshAgent agent;
    private float distance;

    [Header("Random Movement")]
    [SerializeField] private float stoppingRange;
    public float randomPointRange;

    [Header("Tunnels")]
    [SerializeField] private GameObject tunnelHolePrefab;
    [SerializeField] private GameObject[] tunnelHoles;
    [SerializeField] private GameObject currentTunnel;
    [SerializeField] private float tunnelTravelTime;

    void Start()
    {
        aiController = GetComponent<AIController>();
        agent = GetComponent<NavMeshAgent>();

        target = aiController.DecideNextLocation(randomPointRange);
        agent.SetDestination(target);

        currentSpeed = walkSpeed;
    }

    void Update()
    {
        //handle the speed of the player
        agent.speed = currentSpeed;
        agent.angularSpeed = angularBase * currentSpeed;

        distance = Vector3.Distance(agent.transform.position, target);

        if (hasLOS)
        {
            target = aiController.player.position;
            agent.destination = target;
            StartChaseBehaviour();
        }
        else
        {         
            if (distance < stoppingRange)
            {
                EndChaseBehaviour();

                aiController.ChooseAction();
            }
        }
    }

    public void StartChaseBehaviour()
    {
        currentSpeed = chaseSpeed;
    }

    public void EndChaseBehaviour()
    {
        currentSpeed = walkSpeed;
    }
}
