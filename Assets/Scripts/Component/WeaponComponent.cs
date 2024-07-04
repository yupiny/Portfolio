using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public enum WeaponType
{
    Unarmed = 0, Fist, Sword, Hammer, FireBall, Max
}

public class WeaponComponent : MonoBehaviour
{
    [SerializeField]
    private GameObject[] originPrefabs;

    private Animator animator;

    private StateComponent state;

    private WeaponType type = WeaponType.Unarmed;

    public event Action<WeaponType, WeaponType> OnWeaponTypeChanged;

    public bool UnarmedMode { get => type == WeaponType.Unarmed; }
    public bool FistMode { get => type == WeaponType.Fist; }
    public bool SwordMode { get => type == WeaponType.Sword; }
    public bool HammerMode { get => type == WeaponType.Hammer; }
    public bool FireBallMode { get => type == WeaponType.FireBall; }

    private void Awake()
    {
        state = GetComponent<StateComponent>();
        animator= GetComponent<Animator>();
    }

    private Dictionary<WeaponType, Weapon> weaponTable;

    private void Start()
    {
        weaponTable= new Dictionary<WeaponType, Weapon>();

        for(int i =0; i<(int)WeaponType.Max; i++)
        {
            weaponTable.Add((WeaponType)i, null);
        }

        for(int i=0; i<originPrefabs.Length; i++)
        {
            GameObject obj = Instantiate<GameObject>(originPrefabs[i], transform);
            Weapon weapon = obj.GetComponent<Weapon>();

            obj.name = weapon.Type.ToString();

            weaponTable[weapon.Type] = weapon;
        }
    }

    public void SetFistMode() //2번을 누를때마다 호출
    {
        if (state.IdleMode == false)
            return;

        SetMode(WeaponType.Fist);
    }

    public void SetSwordMode() //2번을 누를때마다 호출
    {
        if (state.IdleMode == false)
            return;

        SetMode(WeaponType.Sword);
    }

    public void SetHammerMode() //3번을 누를때마다 호출
    {
        if (state.IdleMode == false)
            return;

        SetMode(WeaponType.Hammer);
    }

    public void SetFireBallMode() //3번을 누를때마다 호출
    {
        if (state.IdleMode == false)
            return;

        SetMode(WeaponType.FireBall);
    }

    private void SetUnarmedMode()
    {
        if(state.IdleMode == false)
            return;

        animator.SetInteger("WeaponType", 0); // 0 == (int)WeaponType.Unarmed

        //TODO : UnEquip
        if (weaponTable[type] != null)
        {
            weaponTable[type].UnEquip();
        }
        

        ChangeType(WeaponType.Unarmed);
    }

    private void SetMode(WeaponType type)
    {
        if(this.type == type)
        {
            SetUnarmedMode(); //장착해제 코드 호출

            return;
        }
        else if(UnarmedMode == false)
        {
            //TODO : 해당 무기해제 다른 무기를 장착하기 위함임
            weaponTable[this.type].UnEquip();
        }

        if (weaponTable[type] == null)
        {
            SetUnarmedMode();

            return;
        }

        animator.SetBool("IsEquipping", true);
        animator.SetInteger("WeaponType", (int)type);

        weaponTable[type].Equip(); //TODO : 무기 장착

        ChangeType(type);
    }

    private void ChangeType(WeaponType type)
    {
        if (this.type == type)
            return;

        WeaponType prevType = this.type;
        this.type = type;

        OnWeaponTypeChanged?.Invoke(prevType, type);
    }

    public void Begin_Equip()
    {
        weaponTable[type].Begin_Equip();
    }

    public void End_Equip()
    {
        animator.SetBool("IsEquipping", false);
        weaponTable[type].End_Equip();
    }

    public void DoAction()
    {
        if (weaponTable[type] == null)
            return;

        animator.SetBool("IsAction", true);
        weaponTable[type].DoAction();
    }

    private void Begin_DoAction()//공격중 입력이 또 들어오면 콤보공격함
                                 //콤보를 이을지 말지 결정 / 호출순서 3
    {
        weaponTable[type].Begin_DoAction();
    }

    private void End_DoAction() //공격 끝날때 호출 / 호출순서 4
    {
        animator.SetBool("IsAction", false);
        weaponTable[type].End_DoAction();
    }

    private void Begin_Combo() //콤보 이을지 말지 결정 / 호출순서 1
    {
        Melee melee = weaponTable[type] as Melee;
        melee?.Begin_Combo();
    }

    private void End_Combo() //콤보 중일때 콤보 불가능하게 설정 / 호출순서 2
    {
        Melee melee = weaponTable[type] as Melee;
        melee?.End_Combo();
    }

    private void Begin_Collision(AnimationEvent e)
    {
        Melee melee = weaponTable[type] as Melee;
        melee?.Begin_Collision(e);
    }
    private void End_Collision()
    {
        Melee melee = weaponTable[type] as Melee;
        melee?.End_Collision();
    }

    private void Play_DoAction_Particle()
    {
        weaponTable[type].Play_Particle();
    }
}