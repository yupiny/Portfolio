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

    private StateComponent state;
    private WeaponComponent weapon;



    private void Awake()
    {
        animator = GetComponent<Animator>();
        state = GetComponent<StateComponent>();
        weapon = GetComponent<WeaponComponent>();

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

        actionMap.FindAction("Staff").started += context =>
        {
            weapon.SetStaffMode();
        };
    }
}
