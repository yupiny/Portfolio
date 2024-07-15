using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static StateComponent;

public enum EvadeDirection
{
    Forward, Backward, Left, Right, forwardRight, forwardLeft, backwardLeft, backwardRight
}

public class MoveComponent : MonoBehaviour
{
    [SerializeField]
    private float walkSpeed = 2.0f;

    [SerializeField]
    private float runSpeed = 4.0f;

    [SerializeField]
    private float sensitivity = 30.0f;

    [SerializeField]
    private float deadZone = 0.001f;

    private bool bCanMove = true;
    private Animator animator;

    private Vector2 inputMove;
    public Vector2 MoveValue { get => inputMove; }
    private Vector2 currInputMove;
    private bool bRun;
    private Vector2 velocity;

    private StateComponent state;
    private Renderer[] renderers; //ĳ���� �޽��迭�� ����

    [SerializeField]
    private GameObject TelpoEffect;

    /// <summary>
    /// Evade�� ����� ȸ�� ���� �����ϱ� ���� prev �� ����
    /// </summary>
    private Quaternion? evadeRotation = null;
    private void Awake()
    {
        animator = GetComponent<Animator>();

        // 1. PlayerInput ������Ʈ�� ��������
        // 2. PlayerInput ������Ʈ�� ActionMap ��������
        // 3. ActionMap���� Binding�� �� ��������
        // 4. Bind�� event�� �޼��� �����ϱ�
        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");

        InputAction moveAction = actionMap.FindAction("Move");
        moveAction.performed += Input_Move_Performed;
        moveAction.canceled += Input_Move_Cancled;

        InputAction runAction = actionMap.FindAction("Run");
        runAction.started += Input_Run_Started;
        runAction.canceled += Input_Run_Cancled;

        state = GetComponent<StateComponent>();
        state.OnStateTypeChanged += OnStateTypeChanged;
    }

    public void Move()
    {
        bCanMove = true;
    }
    public void Stop()
    {
        bCanMove = false;
    }

    private void Input_Move_Performed(InputAction.CallbackContext context)
    {
        inputMove = context.ReadValue<Vector2>();
    }

    private void Input_Move_Cancled(InputAction.CallbackContext context)
    {
        inputMove = Vector2.zero;
    }

    private void Input_Run_Started(InputAction.CallbackContext context)
    {
        bRun = true;
    }

    private void Input_Run_Cancled(InputAction.CallbackContext context)
    {
        bRun = false;
    }

    private void Update()
    {
        currInputMove = Vector2.SmoothDamp(currInputMove, inputMove, ref velocity, 1.0f / sensitivity);

        if (bCanMove == false)
            return;

        Vector3 direction = Vector3.zero;

        float speed = bRun ? runSpeed : walkSpeed;
        if (currInputMove.magnitude > deadZone)
        {
            direction = (Vector3.right * currInputMove.x) + (Vector3.forward * currInputMove.y);
            direction = direction.normalized * speed;
        }

        transform.Translate(direction * Time.deltaTime);

        //if (weapon.UnarmedMode)
        //{
            //animator.SetFloat("SpeedY", direction.magnitude);

        //    return;
        // }

        animator.SetFloat("SpeedX", currInputMove.x * speed);
        animator.SetFloat("SpeedY", currInputMove.y * speed);
    }


    //�÷��̾� ���°� ����Ǹ� ȣ���.
    private void OnStateTypeChanged(StateType prevType, StateType newType)
    {
        switch (newType)
        {
            case StateType.Evade:
                {
                    ExecuteEvade();
                }
                break;
        }
    }

    private void ExecuteEvade()
    {
        Vector2 value = MoveValue;
        EvadeDirection direction = EvadeDirection.Forward;

        // Ű �Է��� ���� ���
        if (value.y == 0.0f)
        {
            direction = EvadeDirection.Forward;

            if (value.x < 0.0f) //����
            {
                direction = EvadeDirection.Left;
            }
            else if (value.x > 0.0f) //������
            {
                direction = EvadeDirection.Right;
            }
        }
        // �� �Է� ��
        else if (value.y >= 0.0f)
        {
            direction = EvadeDirection.Forward;
            if (value.x < 0.0f) //���� �� �밢��
            {
                evadeRotation = transform.rotation;
                transform.Rotate(Vector3.up, -45.0f);
            }
            else if (value.x > 0.0f) //������ �� �밢��
            {
                evadeRotation = transform.rotation;
                transform.Rotate(Vector3.up, 45.0f);
            }
        }
        else
        {
            direction = EvadeDirection.Backward;
            if (value.x < 0.0f) //���� �� �밢��
            {
                evadeRotation = transform.rotation;
                transform.Rotate(Vector3.up, 45.0f);
            }
            else if (value.x > 0.0f) //������ �� �밢��
            {
                evadeRotation = transform.rotation;
                transform.Rotate(Vector3.up, -45.0f);
            }
        }
        animator.SetInteger("Direction", (int)direction);
        animator.SetTrigger("Evade");

        //���� ���������
        if (animator.GetInteger("WeaponType") == (int)WeaponType.Staff)
        {
            StaffEvade(direction);
        }
    }

    [SerializeField]
    private float telpoDistance = 5.0f;
    private void StaffEvade(EvadeDirection direction)
    {
        // 1. ���� �޾����ϱ�, �������� ���Ѵ�
        // 0 : ��, 1 : ��, 2: ����, 3: ������
        Vector3 destnation = transform.position;

        if (direction == EvadeDirection.Forward)
            destnation += new Vector3(0, 0, +telpoDistance);
        
        if (direction == EvadeDirection.Backward)
            destnation += new Vector3(0, 0, -telpoDistance);

        if (direction == EvadeDirection.Right)
            destnation += new Vector3(+telpoDistance, 0, 0);

        if (direction == EvadeDirection.Left)
            destnation += new Vector3(-telpoDistance, 0, 0);

        Vector3 playerPosition = transform.position;
        // ��ƼŬ ���� �ڵ� �ۼ��� �� �Լ� X
        GameObject telpoEffect = GameObject.Instantiate<GameObject>(TelpoEffect, transform.position, Quaternion.identity);
        Destroy(telpoEffect, 2f);

        StartCoroutine(performTelpo(destnation));
    }


    IEnumerator performTelpo(Vector3 dest)
    {
        sensitivity = 50f;
        //yield return new WaitForSeconds(0.1f);
        renderers = GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in renderers)
        {   
            //�޽� ����
            renderer.enabled = false;
        }

        while(Vector3.Distance(transform.position, dest) > 0.01f) // ���� �������� �����ϸ� �ڷ�ƾ Ż��
        {
            //�ڱ���ġ, ��ǥ��ġ, �̵��ӵ� �༭ �ڱ���ġ�� �־���(�̵�)
            transform.position = Vector3.MoveTowards(transform.position, dest, 10.0f * Time.deltaTime); // moveToward�� ���ؼ� ��ġ�� �����ɷ� �̵�
            yield return null; //1������ ����
        }


        foreach (Renderer renderer in renderers)
        {
            //�޽� ���� Ű��
            renderer.enabled = true;
        }

        
        animator.SetTrigger("ReturnToBlendTree");
        Move();
        End_Evade();
        sensitivity = 30f;
    }



    public void Evade()
    {
        End_Evade();
    }
    private void End_Evade()
    {
        if (evadeRotation.HasValue)
            StartCoroutine(Reset_EvadeRotation());

        state.SetIdleMode();
    }

    private IEnumerator Reset_EvadeRotation()
    {
        float delta = 0.0f;

        while (true)
        {
            float angle = Quaternion.Angle(transform.rotation, evadeRotation.Value);
            if (angle < 2.0f)
                break;

            delta += Time.deltaTime * 50f;
            Quaternion rotate = Quaternion.RotateTowards(transform.rotation, evadeRotation.Value, delta);
            transform.rotation = rotate;

            yield return new WaitForFixedUpdate();
        }

        transform.rotation = evadeRotation.Value;
    }

    private void OnGUI()
    {
        GUI.color = Color.red;
        GUILayout.Label(inputMove.ToString());
        //GUILayout.Label(currInputMove.ToString());
    }
}
