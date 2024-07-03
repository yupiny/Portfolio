using UnityEngine;

[System.Serializable]
public class DoActionData
{
    public bool bCanMove;
    public float Power;
    public float Distance;
    public int StopFrame;

    public GameObject Particle;
    public Vector3 ParticlePositionOffset;
    public Vector3 ParticleScaleOffset = Vector3.one;

    public int HitImpactIndex; //피격모션을 무기 종류별로 다 다르게 하기위해서

    public GameObject HitParticle;
    public Vector3 HitParticlePositionOffset;
    public Vector3 HitParticleScaleOffset = Vector3.one;
}

public abstract class Weapon : MonoBehaviour
{
    [SerializeField]
    protected WeaponType type;

    [SerializeField]
    protected DoActionData[] doActionDatas;

    public WeaponType Type { get => type; }

    protected GameObject rootObject;

    protected Animator animator;
    protected StateComponent state;

    protected virtual void Reset()
    {

    }


    protected virtual void Awake()
    {
        rootObject = transform.root.gameObject;
        Debug.Assert(rootObject != null);
        
        state = rootObject.GetComponent<StateComponent>();
        animator= rootObject.GetComponent<Animator>();
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {

    }

    public void Equip()
    {
        state.SetEquipMode();
    }

    public virtual void Begin_Equip()
    {
       
    }

    public virtual void End_Equip()
    {
        state.SetIdleMode();
    }

    public virtual void UnEquip()
    {
        state.SetIdleMode();
    }

    public virtual void DoAction()
    {
        state.SetActionMode();

    }

    public virtual void Begin_DoAction()
    {

    }


    public virtual void End_DoAction()
    {
        state.SetIdleMode();
        Move();
    }

    public virtual void Play_Particle()
    {

    }
    protected void Move()
    {
        MoveComponent moving = rootObject.GetComponent<MoveComponent>();
        if (moving != null)
            moving.Move();
    }
    protected void CheckStop(int index)
    {
        if (doActionDatas[index].bCanMove == false)
        {
            MoveComponent moving = rootObject.GetComponent<MoveComponent>();
            if (moving != null)
                moving.Stop();
        }
    }
}