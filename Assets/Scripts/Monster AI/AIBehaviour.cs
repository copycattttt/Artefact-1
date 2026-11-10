using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class AIBehaviour : MonoBehaviour
{
    private AIController aiController;
    [SerializeField] private GameObject model;

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
    public Transform targetTunnel;
    public Transform closestTunnel;
    private Transform oldClosestTunnel;
    private int tunnelMode;
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

        distance = Vector3.Distance(transform.position, agent.destination);

        //checks if it can see the player
        if (hasLOS)
        {
            target = aiController.player.position;
            agent.SetDestination(target);
            StartChaseBehaviour();
        }
        else
        {         
            if (distance < stoppingRange && tunnelMode == 0)
            {
                EndChaseBehaviour();

                aiController.ChooseAction();
            }
            else if (distance < stoppingRange && tunnelMode == 1)
            {
                model.SetActive(false);
                Invoke("TunnelLogic", tunnelTravelTime);
                tunnelMode = 2;
            }
        }

        float smallestDist = 0f;

        //keeps track of closest tunnel
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

        //handles incrementing and reseting area time
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

    //logic for the tunnel system
    public void StartTunnel()
    {
        agent.SetDestination(closestTunnel.position);
        tunnelMode = 1;
    }

    private void TunnelLogic()
    {
        if (targetTunnel == null)
        {
            int randTunnel = UnityEngine.Random.Range(0, tunnelHoles.Length);
            targetTunnel = tunnelHoles[randTunnel].transform;

            while (targetTunnel == closestTunnel)
            {
                randTunnel = UnityEngine.Random.Range(0, tunnelHoles.Length);
                targetTunnel = tunnelHoles[randTunnel].transform;

                if (targetTunnel != closestTunnel)
                {
                    break;
                }
            }

            agent.SetDestination(targetTunnel.position);
        }

        //moves player to the target tunnel
        transform.position = targetTunnel.position;

        //prevents weird infinite tunnel loop
        closestTunnel = targetTunnel;

        //makes the monster visible and resets everything out of tunnelMode
        model.SetActive(true);
        tunnelMode = 0;
        targetTunnel = null;

        //prevents ai from heading back to where it was patrolling before
        target = transform.position;
        agent.SetDestination(target);
    }

    //logic for making tunnels
    public void MakeTunnel()
    {

    }

    //handles which behaviours to use
    public void StartChaseBehaviour()
    {
        currentSpeed = chaseSpeed;
    }

    public void EndChaseBehaviour()
    {
        currentSpeed = walkSpeed;
    }
}
