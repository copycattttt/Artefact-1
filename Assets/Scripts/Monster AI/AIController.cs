using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR;
using static UnityEngine.GraphicsBuffer;

public class AIController : MonoBehaviour
{
    private AIBehaviour aiBehaviour;
    private NavMeshAgent agent;

    public Transform player;

    [Header("Score system")]
    [SerializeField] private options[] possibleActions;
    [SerializeField] private string bestOption;
    [SerializeField] private bool isChasing;

    [Header("Tunnels")]
    [SerializeField] private int maxTunnels;

    [Header("Stage Control")]
    public bool FrontStage;
    [SerializeField] private bool pushTowardsPlayer;
    [SerializeField] private float pushTowardsCDReset;
    private float pushTowardsCoolDown;
    [SerializeField] private float playerRange;
    [SerializeField] private float minRangeFromPlayer;
    [SerializeField] private float frontStageTimerMax;
    [SerializeField] private float frontStageTimer;
    private bool preDecided;

    private void Start()
    {
        aiBehaviour = GetComponent<AIBehaviour>();
        agent = GetComponent<NavMeshAgent>();

        pushTowardsCoolDown = pushTowardsCDReset;
    }

    private void Update()
    {
        //handle switching between frontstage and backstage
        if (FrontStage)
        {
            //handles pushing the ai into an area near the player
            float distance = Vector3.Distance(agent.transform.position, player.position);

            if (distance > playerRange)
            {
                if (pushTowardsCoolDown <= 0)
                {
                    pushTowardsPlayer = true;
                    pushTowardsCoolDown = pushTowardsCDReset;
                }
                else
                {
                    pushTowardsCoolDown -= 1 * Time.deltaTime;
                }
            }
            else
            {
                pushTowardsPlayer = false;

                //timer for switching out of frontstage, ran in this part so its only changed when in the frontstage radius round the player
                frontStageTimer += 1 * Time.deltaTime;

                if (frontStageTimer >= frontStageTimerMax)
                {
                    frontStageTimer = frontStageTimerMax;
                    FrontStage = false;
                }
            }
        }
        else
        {
            //timer for switching to frontstage
            frontStageTimer -= 1 * Time.deltaTime;

            if (frontStageTimer <= 0)
            {
                frontStageTimer = 0;
                FrontStage = true;
            }
        }
    }

    //Scoring system functions
    public void ChooseAction()
    {
        //Buckets tasks based on if the player is being chased
        if (isChasing)
        {
            possibleActions[0].score = 0;
            possibleActions[1].score = 0;
            possibleActions[2].score = 0;
        }
        else
        {
            possibleActions[0].score = ScoreSearching();
            possibleActions[1].score = ScoreTunnel();
            possibleActions[2].score = ScoreMakeTunnel();
            possibleActions[3].score = 0;
            possibleActions[4].score = 0;
        }

        float bestScore = 0;

        for (int i = 0; i < possibleActions.Length; i++)
        {
            if (bestScore < possibleActions[i].score)
            {
                bestScore = possibleActions[i].score;
                bestOption = possibleActions[i].optionName;
            }
        }

        switch (bestOption)
        {
            case "Search":
                if (!preDecided)
                {
                    aiBehaviour.target = DecideNextLocation(aiBehaviour.randomPointRange);
                    agent.SetDestination(aiBehaviour.target);
                }
                else
                {
                    agent.SetDestination(aiBehaviour.target);
                    preDecided = false;
                }
                break;
            case "Tunnel":
                break;
            case "MakeTunnel":
                break;
            case "ChargePlayer":
                break;
            case "CutOffPlayer":
                break;
            default:
                break;
        }
    }

    private float ScoreTunnel()
    {
        float newScore = 0f;
        float smallestDist = 0f;

        if (!FrontStage)
        {
            newScore = 100f;
        }
        else if (pushTowardsPlayer)
        {
            float playerTunnelDist = 0f;

            //distance to closest tunnel, closest tunnel to player, distance to next location
            preDecided = true;
            float newPointDist = Vector3.Distance(agent.transform.position, aiBehaviour.target);

            for (int i = 0; i < aiBehaviour.tunnelHoles.Length; i++)
            {
                float distance = Vector3.Distance(player.transform.position, aiBehaviour.tunnelHoles[i].transform.position);

                if (playerTunnelDist == 0)
                {
                    playerTunnelDist = distance;
                }
                else if (distance < playerTunnelDist)
                {
                    playerTunnelDist = distance;
                }
            }

            if (playerTunnelDist * 1.5f < Vector3.Distance(agent.transform.position, aiBehaviour.closestTunnel.position))
            {
                newScore = 5f;
            }
        }
        else
        {
            //distance to closest tunnel, time in area
            smallestDist = Vector3.Distance(agent.transform.position, aiBehaviour.closestTunnel.position);

            float distValue = smallestDist * 0.00025f;
            newScore = aiBehaviour.areaTime * distValue;
        }

        return newScore;
    }

    private float ScoreMakeTunnel()
    {
        float newScore = 0f;
        float smallestDist = 0f;

        if (aiBehaviour.tunnelHoles.Length < maxTunnels)
        {
            smallestDist = Vector3.Distance(agent.transform.position, aiBehaviour.closestTunnel.position);

            float tunnelsLeft = maxTunnels - aiBehaviour.tunnelHoles.Length;
            float distValue = smallestDist * 0.025f;

            newScore = tunnelsLeft / distValue;
        }

        return newScore;
    }

    private float ScoreSearching()
    {
        float newScore =  2 * frontStageTimer;

        return newScore;
    }

    //Random movement functions
    public Vector3 DecideNextLocation(float range)
    {
        Vector3 target = transform.position;

        if (pushTowardsPlayer == false)
        {
            target = RandomNavmeshLocation(range, transform.position);
        }
        else
        {
            target = RandomNavmeshLocation(playerRange, player.position);

            float distance = Vector3.Distance(player.position, target);

            while (distance < minRangeFromPlayer)
            {
                target = RandomNavmeshLocation(playerRange, player.position);
                distance = Vector3.Distance(agent.transform.position, target);

                if (distance > minRangeFromPlayer)
                {
                    break;
                }
            }
        }

        return target;
    }

    public Vector3 RandomNavmeshLocation(float radius, Vector3 basePosition)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += basePosition;
        NavMeshHit hit;
        Vector3 finalPosition = transform.position;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }
        return finalPosition;
    }

    [System.Serializable] public struct options
    {
        public string optionName;
        public float score;
    }
}
