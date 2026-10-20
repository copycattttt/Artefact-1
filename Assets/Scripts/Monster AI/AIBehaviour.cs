using UnityEngine;

public class AIBehaviour : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float chaseSpeed;
    [SerializeField] private float walkSpeed;
    private float currentSpeed;

    void Start()
    {
        currentSpeed = walkSpeed;
    }

    void Update()
    {
        
    }
}
