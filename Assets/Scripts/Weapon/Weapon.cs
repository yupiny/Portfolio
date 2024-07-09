using UnityEngine;

// 행동 데이터를 정의하는 클래스
[System.Serializable]
public class DoActionData
{
    public bool bCanMove; // 이동 가능 여부
    public float Power; // 공격력
    public float Distance; // 공격 거리
    public int StopFrame; // 멈추는 프레임 수

    public GameObject Particle; // 파티클 효과
    public Vector3 ParticlePositionOffset; // 파티클 위치 오프셋
    public Vector3 ParticleScaleOffset = Vector3.one; // 파티클 스케일 오프셋

    public int HitImpactIndex; // 피격 모션 인덱스 (무기 종류별로 다름) //피격모션을 무기 종류별로 다 다르게 하기위해서

    public GameObject HitParticle; // 히트 파티클 효과
    public Vector3 HitParticlePositionOffset; // 히트 파티클 위치 오프셋
    public Vector3 HitParticleScaleOffset = Vector3.one; // 히트 파티클 스케일 오프셋
}

// 무기 클래스 (추상 클래스)
public abstract class Weapon : MonoBehaviour
{
    [SerializeField]
    protected WeaponType type; // 무기 유형

    [SerializeField]
    protected DoActionData[] doActionDatas; // 행동 데이터 배열

    public WeaponType Type { get => type; } // 무기 유형을 반환하는 프로퍼티

    protected GameObject rootObject; // 루트 오브젝트
    protected Animator animator; // 애니메이터
    protected StateComponent state; // 상태 컴포넌트

    // 초기화 메서드 (에디터에서 Reset 버튼을 눌렀을 때 호출)
    protected virtual void Reset()
    {

    }

    // Awake 메서드 (게임 오브젝트가 활성화될 때 호출)
    protected virtual void Awake()
    {
        rootObject = transform.root.gameObject;
        Debug.Assert(rootObject != null); // 루트 오브젝트가 null이 아닌지 확인

        state = rootObject.GetComponent<StateComponent>(); // 상태 컴포넌트 할당
        animator = rootObject.GetComponent<Animator>(); // 애니메이터 할당
    }

    // Start 메서드 (게임 시작 시 호출)
    protected virtual void Start()
    {

    }

    // Update 메서드 (매 프레임마다 호출)
    protected virtual void Update()
    {

    }

    // 무기를 장착하는 메서드
    public void Equip()
    {
        state.SetEquipMode(); // 상태를 장착 모드로 설정
    }

    // 장착을 시작하는 메서드
    public virtual void Begin_Equip()
    {

    }

    // 장착을 종료하는 메서드
    public virtual void End_Equip()
    {
        state.SetIdleMode(); // 상태를 대기 모드로 설정
    }

    // 무기를 해제하는 메서드
    public virtual void UnEquip()
    {
        state.SetIdleMode(); // 상태를 대기 모드로 설정
    }

    // 행동을 수행하는 메서드
    public virtual void DoAction()
    {
        state.SetActionMode(); // 상태를 행동 모드로 설정
    }

    // 행동을 시작하는 메서드
    public virtual void Begin_DoAction()
    {

    }

    // 행동을 종료하는 메서드
    public virtual void End_DoAction()
    {
        state.SetIdleMode(); // 상태를 대기 모드로 설정
        Move(); // 이동 메서드 호출
    }

    // 파티클을 재생하는 메서드
    public virtual void Play_Particle()
    {

    }

    // 이동을 수행하는 메서드
    protected void Move()
    {
        MoveComponent moving = rootObject.GetComponent<MoveComponent>(); // 이동 컴포넌트 할당
        if (moving != null)
            moving.Move(); // 이동
    }

    // 멈추는 조건을 확인하는 메서드
    protected void CheckStop(int index)
    {
        if (doActionDatas[index].bCanMove == false)
        {
            MoveComponent moving = rootObject.GetComponent<MoveComponent>();
            if (moving != null)
                moving.Stop(); // 이동 멈춤
        }
    }
}