using UnityEngine;

// �ൿ �����͸� �����ϴ� Ŭ����
[System.Serializable]
public class DoActionData
{
    public bool bCanMove; // �̵� ���� ����
    public float Power; // ���ݷ�
    public float Distance; // ���� �Ÿ�
    public int StopFrame; // ���ߴ� ������ ��

    public GameObject Particle; // ��ƼŬ ȿ��
    public Vector3 ParticlePositionOffset; // ��ƼŬ ��ġ ������
    public Vector3 ParticleScaleOffset = Vector3.one; // ��ƼŬ ������ ������

    public int HitImpactIndex; // �ǰ� ��� �ε��� (���� �������� �ٸ�) //�ǰݸ���� ���� �������� �� �ٸ��� �ϱ����ؼ�

    public GameObject HitParticle; // ��Ʈ ��ƼŬ ȿ��
    public Vector3 HitParticlePositionOffset; // ��Ʈ ��ƼŬ ��ġ ������
    public Vector3 HitParticleScaleOffset = Vector3.one; // ��Ʈ ��ƼŬ ������ ������
}

// ���� Ŭ���� (�߻� Ŭ����)
public abstract class Weapon : MonoBehaviour
{
    [SerializeField]
    protected WeaponType type; // ���� ����

    [SerializeField]
    protected DoActionData[] doActionDatas; // �ൿ ������ �迭

    public WeaponType Type { get => type; } // ���� ������ ��ȯ�ϴ� ������Ƽ

    protected GameObject rootObject; // ��Ʈ ������Ʈ
    protected Animator animator; // �ִϸ�����
    protected StateComponent state; // ���� ������Ʈ

    // �ʱ�ȭ �޼��� (�����Ϳ��� Reset ��ư�� ������ �� ȣ��)
    protected virtual void Reset()
    {

    }

    // Awake �޼��� (���� ������Ʈ�� Ȱ��ȭ�� �� ȣ��)
    protected virtual void Awake()
    {
        rootObject = transform.root.gameObject;
        Debug.Assert(rootObject != null); // ��Ʈ ������Ʈ�� null�� �ƴ��� Ȯ��

        state = rootObject.GetComponent<StateComponent>(); // ���� ������Ʈ �Ҵ�
        animator = rootObject.GetComponent<Animator>(); // �ִϸ����� �Ҵ�
    }

    // Start �޼��� (���� ���� �� ȣ��)
    protected virtual void Start()
    {

    }

    // Update �޼��� (�� �����Ӹ��� ȣ��)
    protected virtual void Update()
    {

    }

    // ���⸦ �����ϴ� �޼���
    public void Equip()
    {
        state.SetEquipMode(); // ���¸� ���� ���� ����
    }

    // ������ �����ϴ� �޼���
    public virtual void Begin_Equip()
    {

    }

    // ������ �����ϴ� �޼���
    public virtual void End_Equip()
    {
        state.SetIdleMode(); // ���¸� ��� ���� ����
    }

    // ���⸦ �����ϴ� �޼���
    public virtual void UnEquip()
    {
        state.SetIdleMode(); // ���¸� ��� ���� ����
    }

    // �ൿ�� �����ϴ� �޼���
    public virtual void DoAction()
    {
        state.SetActionMode(); // ���¸� �ൿ ���� ����
    }

    // �ൿ�� �����ϴ� �޼���
    public virtual void Begin_DoAction()
    {

    }

    // �ൿ�� �����ϴ� �޼���
    public virtual void End_DoAction()
    {
        state.SetIdleMode(); // ���¸� ��� ���� ����
        Move(); // �̵� �޼��� ȣ��
    }

    // ��ƼŬ�� ����ϴ� �޼���
    public virtual void Play_Particle()
    {

    }

    // �̵��� �����ϴ� �޼���
    protected void Move()
    {
        MoveComponent moving = rootObject.GetComponent<MoveComponent>(); // �̵� ������Ʈ �Ҵ�
        if (moving != null)
            moving.Move(); // �̵�
    }

    // ���ߴ� ������ Ȯ���ϴ� �޼���
    protected void CheckStop(int index)
    {
        if (doActionDatas[index].bCanMove == false)
        {
            MoveComponent moving = rootObject.GetComponent<MoveComponent>();
            if (moving != null)
                moving.Stop(); // �̵� ����
        }
    }
}