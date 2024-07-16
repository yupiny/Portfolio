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

    [SerializeField]
    private string followTargetName = "FollowTarget";

    [SerializeField]
    private Vector2 mouseSensitivity = new Vector2(0.5f, 0.5f); //마우스의 속도

    [SerializeField]
    private Vector2 limitPitchAngle = new Vector2(20, 340);

    [SerializeField]
    private float mouseRotationLerp = 0.25f; //보간 값

    private bool bCanMove = true;
    private Animator animator;

    private bool bRun;

    private Vector2 inputMove;
    public Vector2 MoveValue { get => inputMove; }
    private Vector2 currInputMove;

    private Vector2 inputLook;

    private WeaponComponent weapon;
    private StateComponent state;
    private Renderer[] renderers; //캐릭터 메쉬배열로 저장

    [SerializeField]
    private GameObject TelpoEffect;

    /// <summary>
    /// Evade시 변경된 회전 값을 복구하기 위한 prev 값 저장
    /// </summary>
    //private Quaternion? evadeRotation = null;

    private Transform followTargetTransform;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        weapon = GetComponent<WeaponComponent>();

        state = GetComponent<StateComponent>();
        state.OnStateTypeChanged += OnStateTypeChanged;

        Awake_BindPlayerInput();
    }

    private void Awake_BindPlayerInput()
    {   
        // 1. PlayerInput 컴포넌트를 가져오기
        // 2. PlayerInput 컴포넌트의 ActionMap 가져오기
        // 3. ActionMap에서 Binding한 것 가져오기
        // 4. Bind의 event에 메서드 연결하기

        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");

        InputAction moveAction = actionMap.FindAction("Move");
        moveAction.performed += Input_Move_Performed;
        moveAction.canceled += Input_Move_Cancled;

        InputAction lookAction = actionMap.FindAction("Look");
        lookAction.performed += Input_Look_Performed;
        lookAction.canceled += Input_Look_Cancled;

        InputAction runAction = actionMap.FindAction("Run");
        runAction.started += Input_Run_Started;
        runAction.canceled += Input_Run_Cancled;
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

    private void Input_Look_Performed(InputAction.CallbackContext context)
    {
        inputLook = context.ReadValue<Vector2>();
    }

    private void Input_Look_Cancled(InputAction.CallbackContext context)
    {
        inputLook = Vector2.zero;
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        followTargetTransform = transform.FindChildByName(followTargetName);
    }

    private Vector2 velocity;
    private Quaternion rotation;
    private void Update()
    {
        currInputMove = Vector2.SmoothDamp(currInputMove, inputMove, ref velocity, 1.0f / sensitivity);

        if (bCanMove == false)
            return;


        rotation *= Quaternion.AngleAxis(inputLook.x * mouseSensitivity.x, Vector3.up); //x방향회전
        rotation *= Quaternion.AngleAxis(-inputLook.y * mouseSensitivity.y, Vector3.right); //y방향회전
        followTargetTransform.rotation = rotation;

        Vector3 angles = followTargetTransform.localEulerAngles;
        angles.z = 0.0f;

        // 회전 각 제한
        float xAngle = followTargetTransform.localEulerAngles.x;

        // 180도면 짐벌락 걸릴 가능성있어서 배제
        if (xAngle < 180.0f && xAngle > limitPitchAngle.x)
            angles.x = limitPitchAngle.x;
        else if (xAngle > 180.0f && xAngle < limitPitchAngle.y)
            angles.x = limitPitchAngle.y;

        followTargetTransform.localEulerAngles = angles;

        // 부드러운 화면 전환을 위한 보정
        rotation = Quaternion.Lerp(followTargetTransform.rotation, rotation, mouseRotationLerp * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        followTargetTransform.localEulerAngles = new Vector3(angles.x, 0, 0);


        Vector3 direction = Vector3.zero;

        float speed = bRun ? runSpeed : walkSpeed;
        if (currInputMove.magnitude > deadZone)
        {
            direction = (Vector3.right * currInputMove.x) + (Vector3.forward * currInputMove.y);
            direction = direction.normalized * speed;
        }

        transform.Translate(direction * Time.deltaTime);

        if (weapon.UnarmedMode)
        {
            animator.SetFloat("SpeedY", direction.magnitude);

            return;
        }

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
        EvadeDirection telpoDirection = EvadeDirection.Forward;

        // 키 입력이 없는 경우
        if (value.y == 0.0f)
        {
            direction = EvadeDirection.Forward;
            telpoDirection = EvadeDirection.Forward;

            if (value.x < 0.0f) //왼쪽
            {
                direction = EvadeDirection.Left;
                telpoDirection = EvadeDirection.Left;
            }
            else if (value.x > 0.0f) //오른쪽
            {
                direction = EvadeDirection.Right;
                telpoDirection = EvadeDirection.Right;
            }
        }
        // 앞 입력 시
        else if (value.y >= 0.0f)
        {
            direction = EvadeDirection.Forward;
            telpoDirection = EvadeDirection.Forward;
            if (value.x < 0.0f) //왼쪽 앞 대각선
            {
                //evadeRotation = transform.rotation;
                //transform.Rotate(Vector3.up, -45.0f);
                telpoDirection = EvadeDirection.forwardLeft;
            }
            else if (value.x > 0.0f) //오른쪽 앞 대각선
            {
                //evadeRotation = transform.rotation;
                //transform.Rotate(Vector3.up, 45.0f);
                telpoDirection = EvadeDirection.forwardRight;
            }
        }
        else
        {
            direction = EvadeDirection.Backward;
            telpoDirection = EvadeDirection.Backward;
            if (value.x < 0.0f) //왼쪽 뒤 대각선
            {
                //evadeRotation = transform.rotation;
                //transform.Rotate(Vector3.up, 45.0f);
                telpoDirection = EvadeDirection.backwardLeft;
            }
            else if (value.x > 0.0f) //오른쪽 뒤 대각선
            {
                //evadeRotation = transform.rotation;
                //transform.Rotate(Vector3.up, -45.0f);
                telpoDirection = EvadeDirection.backwardRight;
            }
        }
        animator.SetInteger("Direction", (int)direction);
        animator.SetTrigger("Evade");

        //만약 스태프라면
        if (animator.GetInteger("WeaponType") == (int)WeaponType.Staff)
        {
            StaffEvade(telpoDirection);
        }
    }

    [SerializeField]
    private float telpoDistance = 5.0f;
    private void StaffEvade(EvadeDirection telpoDirection)
    {
        // 1. 방향 받았으니까, 목적지를 구한다
        // 0 : 앞, 1 : 뒤, 2: 왼쪽, 3: 오른쪽
        Vector3 destnation = transform.position;

        if (telpoDirection == EvadeDirection.Forward)
            destnation += new Vector3(0, 0, +telpoDistance);
        
        if (telpoDirection == EvadeDirection.Backward)
            destnation += new Vector3(0, 0, -telpoDistance);

        if (telpoDirection == EvadeDirection.Right)
            destnation += new Vector3(+telpoDistance, 0, 0);

        if (telpoDirection == EvadeDirection.Left)
            destnation += new Vector3(-telpoDistance, 0, 0);

        if (telpoDirection == EvadeDirection.forwardLeft)
            destnation += new Vector3(-4, 0, +4);

        if (telpoDirection == EvadeDirection.forwardRight)
            destnation += new Vector3(+4, 0, +4);

        if (telpoDirection == EvadeDirection.backwardLeft)
            destnation += new Vector3(-4, 0, -4);

        if (telpoDirection == EvadeDirection.backwardRight)
            destnation += new Vector3(+4, 0, -4);

        Vector3 playerPosition = transform.position;
        // 파티클 생성 코드 작성할 것 함수 X
        GameObject telpoEffect = GameObject.Instantiate<GameObject>(TelpoEffect, transform.position, Quaternion.identity);
        telpoEffect.transform.position += new Vector3(0, 0.05f, 0);

        StartCoroutine(performTelpo(destnation));
    }


    IEnumerator performTelpo(Vector3 dest)
    {
        sensitivity = 50f;
        yield return new WaitForSeconds(0.05f);
        renderers = GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in renderers)
        {   
            //메쉬 끄기
            renderer.enabled = false;
        }

        while(Vector3.Distance(transform.position, dest) > 0.05f) // 만약 목적지에 근접하면 코루틴 탈출
        {
            //자기위치, 목표위치, 이동속도 줘서 자기위치에 넣어줌(이동)
            transform.position = Vector3.MoveTowards(transform.position, dest, 100.0f * Time.deltaTime); // moveToward를 통해서 위치를 얻은걸로 이동
            yield return null; //1프레임 멈춤
        }


        foreach (Renderer renderer in renderers)
        {
            //메쉬 전부 키기
            renderer.enabled = true;
        }


        yield return new WaitForSeconds(0.3f);

        animator.SetTrigger("ReturnToBlendTree");

        animator.SetFloat("SpeedX", 0);
        animator.SetFloat("SpeedY", 0);
        //Invoke("Move", 0.5f);
        Move();
        //Invoke("End_Evade", 0.5f);
        End_Evade();
        sensitivity = 30f;
    }



    public void Evade()
    {
        End_Evade();
    }
    private void End_Evade()
    {
        //if (evadeRotation.HasValue)
        //    StartCoroutine(Reset_EvadeRotation());

        state.SetIdleMode();
    }

    //private IEnumerator Reset_EvadeRotation()
    //{
    //    float delta = 0.0f;

    //    while (true)
    //    {
    //        float angle = Quaternion.Angle(transform.rotation, evadeRotation.Value);
    //        if (angle < 2.0f)
    //            break;

    //        delta += Time.deltaTime * 50f;
    //        Quaternion rotate = Quaternion.RotateTowards(transform.rotation, evadeRotation.Value, delta);
    //        transform.rotation = rotate;

    //        yield return new WaitForFixedUpdate();
    //    }

    //    transform.rotation = evadeRotation.Value;
    //}

    private void OnGUI()
    {
        GUI.color = Color.red;
        GUILayout.Label(inputMove.ToString());
        //GUILayout.Label(currInputMove.ToString());
    }
}
