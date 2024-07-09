using System.Collections.Generic;
using UnityEngine;

// Melee 클래스는 Weapon 클래스를 상속받아 근접 무기를 구현합니다.
public class Melee : Weapon
{
    private bool bEnable; //콤보 입력 체크
    private bool bExist;  // 콤보 입력 유무   
    private int index;    //콤보가 몇인지 //콤보의 인덱스

    protected Collider[] colliders; // 무기의 콜라이더 배열
    private List<GameObject> hittedList; // 타격한 오브젝트 리스트

    protected override void Awake()
    {
        base.Awake();
        colliders = GetComponentsInChildren<Collider>(); // 자식 콜라이더들을 가져옴
        hittedList = new List<GameObject>(); // 타격한 오브젝트 리스트 초기화
    }

    protected override void Start()
    {
        base.Start();
        End_Collision(); // 휘두를 때만 콜라이더를 켜고 끄기 위해 게임 시작 시 콜라이더를 꺼줌
    }

    // 콜라이더를 활성화하는 메서드 (애니메이션 이벤트로 호출됨)
    public virtual void Begin_Collision(AnimationEvent e)
    {
        foreach (Collider collider in colliders)
            collider.enabled = true; // 모든 콜라이더를 활성화
    }

    // 콜라이더를 비활성화하는 메서드
    public virtual void End_Collision()
    {
        foreach(Collider collider in colliders)
            collider.enabled = false; // 모든 콜라이더를 비활성화

        hittedList.Clear(); // 타격한 오브젝트 리스트 초기화
    }

    // 공격 키를 누를 때마다 호출되는 메서드
    public override void DoAction()
    {
        //if(공격 가능한 상태) // 공격 가능한 상태인지 체크
        if (bEnable) 
        {
            bEnable = false;
            bExist = true; //콤보공격을 할수 있게 true // 콤보 공격을 할 수 있게 설정
            return;
        }

        //공격 불가능한 상태라면 return
        if (state.IdleMode == false)
            return;

        base.DoAction(); // 기본 공격 실행
    }

    // 콤보를 시작하는 메서드
    public void Begin_Combo()
    {
        bEnable = true;
    }

    // 콤보를 종료하는 메서드
    public void End_Combo()
    {
        bEnable = false;
    }

    // 공격시작 시작하는 메서드
    public override void Begin_DoAction()
    {
        base.Begin_DoAction();

        if (bExist == false)
            return;

        bExist = false;

        index++;
        animator.SetTrigger("NextCombo"); // 다음 콤보를 위한 애니메이션 트리거 설정
        if (doActionDatas[index].bCanMove)
        {
            Move(); // 이동 메서드 호출
            return;
        }
        CheckStop(index); // 멈추는 조건 체크
    }

    // 공격을 종료하는 메서드
    public override void End_DoAction()
    {
        base.End_DoAction();

        index = 0;
        bEnable = false;
    }

    // 충돌이 발생했을 때 호출되는 메서드
    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == rootObject)
            return;

        if (hittedList.Contains(other.gameObject) == true)
            return;

        hittedList.Add(other.gameObject);

        //IDamagable damage = other.gameObject.GetComponent<IDamagable>();

        //if (damage == null)
        //    return;

        Vector3 hitPoint = Vector3.zero;

        Collider enableCollider = null;
        foreach(Collider collider in colliders)
        {
            if(collider.enabled == true)
            {
                enableCollider = collider;

                break;
            }
        }

        hitPoint = enableCollider.ClosestPoint(other.transform.position);
        hitPoint = other.transform.InverseTransformPoint(hitPoint);
    
        //GameObject temp = Instantiate<GameObject>(GameObject.CreatePrimitive(PrimitiveType.Sphere), other.transform, false);
        //temp.transform.localPosition = hitPoint;
        //temp.transform.localPosition = new Vector3(0.25f, 0.25f, 0.25f);

        //print(other.gameObject.name);
        //
        //damage.OnDamage(rootObject, this, hitPoint, doActionDatas[index]);
    }
}