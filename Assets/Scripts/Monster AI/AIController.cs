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

    [Header("Stage Control")]
    public bool FrontStage;
    [SerializeField] private bool pushTowardsPlayer;
    [SerializeField] private float playerRange;
    [SerializeField] private float minRangeFromPlayer;
    [SerializeField] private float frontStageTimerMax;
    [SerializeField] private float frontStageTimer;

    private void Start()
    {
        aiBehaviour = GetComponent<AIBehaviour>();
        agent = GetComponent<NavMeshAgent>();
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
                pushTowardsPlayer = true;
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
            possibleActions[3].score = 0;
            possibleActions[4].score = 0;
        }

        for (int i = 0; i < possibleActions.Length; i++)
        {
            float bestScore = 0;

            if (bestScore < possibleActions[i].score)
            {
                bestScore = possibleActions[i].score;
                bestOption = possibleActions[i].optionName;
            }
        }

        switch (bestOption)
        {
            case "Search":
                aiBehaviour.target = DecideNextLocation(aiBehaviour.randomPointRange);
                agent.SetDestination(aiBehaviour.target);
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
