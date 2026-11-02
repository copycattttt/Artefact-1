using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float gravity;

    [SerializeField] private float LookSensitivity;
    [SerializeField] private float LookAngleLimit;

    private Camera cam;
    private CharacterController characterController;

    private InputAction move;

    private Vector3 moveDirection;
    private float lookAngle;

    private void Start()
    {
        cam = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();

        move = InputSystem.actions.FindAction("Movement");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        Vector2 moveVector = move.ReadValue<Vector2>();
        Vector2 mouseDelta = new Vector2(Mouse.current.delta.x.ReadValue(), Mouse.current.delta.y.ReadValue());

        MovePlayer(moveVector);
        MoveCamera(mouseDelta);
    }

    private void MovePlayer(Vector2 moveVector)
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float oldY = moveDirection.y;

        Vector2 newSpeed = new Vector2(moveVector.y * speed, moveVector.x * speed);

        moveDirection = (forward * newSpeed.x) + (right * newSpeed.y);
        
        if(!characterController.isGrounded)
        {
            moveDirection.y -= gravity *Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void MoveCamera(Vector2 mouseDelta)
    {
        lookAngle += -mouseDelta.y * LookSensitivity;
        lookAngle = Mathf.Clamp(lookAngle, -LookAngleLimit, LookAngleLimit);

        cam.transform.localRotation = Quaternion.Euler(lookAngle, 0, 0);
        transform.rotation *= Quaternion.Euler(0, mouseDelta.x * LookSensitivity, 0);
    }
}
