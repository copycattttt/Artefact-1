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
    public float stoppingRange;
    public float randomPointRange;

    [Header("Tunnels")]
    public GameObject[] tunnelHoles;
    [SerializeField] private GameObject tunnelHolePrefab;
    public Transform closestTunnel;
    private Transform oldClosestTunnel;
    [SerializeField] private float tunnelTravelTime;
    public float areaTime;

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

        distance = Vector3.Distance(transform.position, target);

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

        float smallestDist = 0f;

        for (int i = 0; i < tunnelHoles.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, tunnelHoles[i].transform.position);

            if (smallestDist == 0)
            {
                smallestDist = distance;
                closestTunnel = tunnelHoles[i].transform;
            }
            else if (distance < smallestDist)
            {
                smallestDist = distance;
                closestTunnel = tunnelHoles[i].transform;
            }
        }

        if (closestTunnel == oldClosestTunnel)
        {
            areaTime += 1 * Time.deltaTime;
        }
        else
        {
            areaTime = 0f;
            oldClosestTunnel = closestTunnel;
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
