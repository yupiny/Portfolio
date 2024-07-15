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
    private Renderer[] renderers; //캐릭터 메쉬배열로 저장

    [SerializeField]
    private GameObject TelpoEffect;

    /// <summary>
    /// Evade시 변경된 회전 값을 복구하기 위한 prev 값 저장
    /// </summary>
    private Quaternion? evadeRotation = null;
    private void Awake()
    {
        animator = GetComponent<Animator>();

        // 1. PlayerInput 컴포넌트를 가져오기
        // 2. PlayerInput 컴포넌트의 ActionMap 가져오기
        // 3. ActionMap에서 Binding한 것 가져오기
        // 4. Bind의 event에 메서드 연결하기
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


    //플레이어 상태가 변경되면 호출됨.
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

        // 키 입력이 없는 경우
        if (value.y == 0.0f)
        {
            direction = EvadeDirection.Forward;

            if (value.x < 0.0f) //왼쪽
            {
                direction = EvadeDirection.Left;
            }
            else if (value.x > 0.0f) //오른쪽
            {
                direction = EvadeDirection.Right;
            }
        }
        // 앞 입력 시
        else if (value.y >= 0.0f)
        {
            direction = EvadeDirection.Forward;
            if (value.x < 0.0f) //왼쪽 앞 대각선
            {
                evadeRotation = transform.rotation;
                transform.Rotate(Vector3.up, -45.0f);
            }
            else if (value.x > 0.0f) //오른쪽 앞 대각선
            {
                evadeRotation = transform.rotation;
                transform.Rotate(Vector3.up, 45.0f);
            }
        }
        else
        {
            direction = EvadeDirection.Backward;
            if (value.x < 0.0f) //왼쪽 뒤 대각선
            {
                evadeRotation = transform.rotation;
                transform.Rotate(Vector3.up, 45.0f);
            }
            else if (value.x > 0.0f) //오른쪽 뒤 대각선
            {
                evadeRotation = transform.rotation;
                transform.Rotate(Vector3.up, -45.0f);
            }
        }
        animator.SetInteger("Direction", (int)direction);
        animator.SetTrigger("Evade");

        //만약 스태프라면
        if (animator.GetInteger("WeaponType") == (int)WeaponType.Staff)
        {
            StaffEvade(direction);
        }
    }

    [SerializeField]
    private float telpoDistance = 5.0f;
    private void StaffEvade(EvadeDirection direction)
    {
        // 1. 방향 받았으니까, 목적지를 구한다
        // 0 : 앞, 1 : 뒤, 2: 왼쪽, 3: 오른쪽
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
        // 파티클 생성 코드 작성할 것 함수 X
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
            //메쉬 끄기
            renderer.enabled = false;
        }

        while(Vector3.Distance(transform.position, dest) > 0.01f) // 만약 목적지에 근접하면 코루틴 탈출
        {
            //자기위치, 목표위치, 이동속도 줘서 자기위치에 넣어줌(이동)
            transform.position = Vector3.MoveTowards(transform.position, dest, 10.0f * Time.deltaTime); // moveToward를 통해서 위치를 얻은걸로 이동
            yield return null; //1프레임 멈춤
        }


        foreach (Renderer renderer in renderers)
        {
            //메쉬 전부 키기
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
