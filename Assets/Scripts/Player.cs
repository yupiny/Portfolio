using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

using StateType = StateComponent.StateType;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(StateComponent))]
[RequireComponent(typeof(MoveComponent))]
[RequireComponent(typeof(WeaponComponent))]
public class Player : MonoBehaviour
{
    private Animator animator;

    private MoveComponent moving;
    private StateComponent state;
    private WeaponComponent weapon;

    /// <summary>
    /// Evade시 변경된 회전 값을 복구하기 위한 prev 값 저장
    /// </summary>
    private Quaternion? evadeRotation = null;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        state = GetComponent<StateComponent>();
        moving = GetComponent<MoveComponent>();
        weapon = GetComponent<WeaponComponent>();

        state.OnStateTypeChanged += OnStateTypeChanged;

        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");

        actionMap.FindAction("Evade").started += context =>
        {
            /*if (weapon.UnarmedMode == false)
                return;*/
            /*if (state.IdleMode == false)
                return;*/

            state.SetEvadeMode();
        };

        actionMap.FindAction("Sword").started += context =>
        {
            weapon.SetSwordMode();
        };


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
        Vector2 value = moving.MoveValue;
        EvadeDirection direction = EvadeDirection.Forward;

        // 키 입력이 없는 경우
        if (value.y == 0.0f)
        {
            direction = EvadeDirection.Forward;

            if (value.x < 0.0f)
            {
                direction = EvadeDirection.Left;
            }
            else if (value.x > 0.0f)
            {
                direction = EvadeDirection.Right;
            }
        }
        // 앞 입력 시
        else if (value.y >= 0.0f)
        {
            direction = EvadeDirection.Forward;
            if (value.x < 0.0f)
            {
                evadeRotation = transform.rotation;
                transform.Rotate(Vector3.up, -45.0f);
            }
            else if (value.x > 0.0f)
            {
                evadeRotation = transform.rotation;
                transform.Rotate(Vector3.up, 45.0f);
            }
        }
        else
        {
            direction = EvadeDirection.Backward;
            if (value.x < 0.0f)
            {
                evadeRotation = transform.rotation;
                transform.Rotate(Vector3.up, 45.0f);
            }
            else if (value.x > 0.0f)
            {
                evadeRotation = transform.rotation;
                transform.Rotate(Vector3.up, -45.0f);
            }
        }
        animator.SetInteger("Direction", (int)direction);
        animator.SetTrigger("Evade");
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
}
