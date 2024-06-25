using UnityEngine;
using UnityEngine.InputSystem;

public class MoveComponent : MonoBehaviour
{
    [SerializeField]
    private float walkSpeed = 2.0f;

    [SerializeField]
    private float runSpeed = 4.0f;

    [SerializeField]
    private float sensitivitiy = 10.0f;

    [SerializeField]
    private float deadZone = 0.001f;

    private bool bCanMove = true;

    private Animator animator;
    private CharacterController controller;

    private Vector2 inputMove;
    private Vector2 currInputMove;

    public bool bRun;
    public void Move()
    {
        bCanMove = true;
    }
    public void Stop()
    {
        bCanMove = false;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");

        InputAction moveAction = actionMap.FindAction("Move");
        moveAction.performed += Input_Move_Performed;
        moveAction.canceled += Input_Move_Cancled;

        InputAction runAction = actionMap.FindAction("Run");
        runAction.started += Input_Run_Started;
        runAction.canceled += Input_Run_Cancled;
    }

    private void Input_Move_Performed(InputAction.CallbackContext context)
    {
        inputMove = context.ReadValue<Vector2>();
    }
    private void Input_Move_Cancled(InputAction.CallbackContext context)
    {
        inputMove = Vector3.zero;
    }

    private void Input_Run_Started(InputAction.CallbackContext context)
    {
        bRun = true;
    }

    private void Input_Run_Cancled(InputAction.CallbackContext context)
    {
        bRun = false;
    }

    private Vector2 velocity;
    private void Update()
    {
        currInputMove = Vector2.SmoothDamp(currInputMove, inputMove, ref velocity, 1.0f / sensitivitiy);

        if (bCanMove == false)
            return;

        Vector3 direction = Vector3.zero;

        float speed = bRun ? runSpeed : walkSpeed;
        if (currInputMove.magnitude > deadZone)
        {

            Debug.Log("speed" + bRun);

            direction = (Vector3.right * currInputMove.x) + (Vector3.forward * currInputMove.y);
            direction = direction.normalized * speed;
        }
        controller.Move(direction * Time.deltaTime);

        //if (weapon.UnarmedMode)
        {
            animator.SetFloat("SpeedY", direction.magnitude);

            //return;
        }

        //animator.SetFloat("SpeedX", currInputMove.x * speed);
        //animator.SetFloat("SpeedY", currInputMove.y * speed);
    }
}