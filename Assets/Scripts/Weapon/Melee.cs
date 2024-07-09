using System.Collections.Generic;
using UnityEngine;

// Melee Ŭ������ Weapon Ŭ������ ��ӹ޾� ���� ���⸦ �����մϴ�.
public class Melee : Weapon
{
    private bool bEnable; //�޺� �Է� üũ
    private bool bExist;  // �޺� �Է� ����   
    private int index;    //�޺��� ������ //�޺��� �ε���

    protected Collider[] colliders; // ������ �ݶ��̴� �迭
    private List<GameObject> hittedList; // Ÿ���� ������Ʈ ����Ʈ

    protected override void Awake()
    {
        base.Awake();
        colliders = GetComponentsInChildren<Collider>(); // �ڽ� �ݶ��̴����� ������
        hittedList = new List<GameObject>(); // Ÿ���� ������Ʈ ����Ʈ �ʱ�ȭ
    }

    protected override void Start()
    {
        base.Start();
        End_Collision(); // �ֵθ� ���� �ݶ��̴��� �Ѱ� ���� ���� ���� ���� �� �ݶ��̴��� ����
    }

    // �ݶ��̴��� Ȱ��ȭ�ϴ� �޼��� (�ִϸ��̼� �̺�Ʈ�� ȣ���)
    public virtual void Begin_Collision(AnimationEvent e)
    {
        foreach (Collider collider in colliders)
            collider.enabled = true; // ��� �ݶ��̴��� Ȱ��ȭ
    }

    // �ݶ��̴��� ��Ȱ��ȭ�ϴ� �޼���
    public virtual void End_Collision()
    {
        foreach(Collider collider in colliders)
            collider.enabled = false; // ��� �ݶ��̴��� ��Ȱ��ȭ

        hittedList.Clear(); // Ÿ���� ������Ʈ ����Ʈ �ʱ�ȭ
    }

    // ���� Ű�� ���� ������ ȣ��Ǵ� �޼���
    public override void DoAction()
    {
        //if(���� ������ ����) // ���� ������ �������� üũ
        if (bEnable) 
        {
            bEnable = false;
            bExist = true; //�޺������� �Ҽ� �ְ� true // �޺� ������ �� �� �ְ� ����
            return;
        }

        //���� �Ұ����� ���¶�� return
        if (state.IdleMode == false)
            return;

        base.DoAction(); // �⺻ ���� ����
    }

    // �޺��� �����ϴ� �޼���
    public void Begin_Combo()
    {
        bEnable = true;
    }

    // �޺��� �����ϴ� �޼���
    public void End_Combo()
    {
        bEnable = false;
    }

    // ���ݽ��� �����ϴ� �޼���
    public override void Begin_DoAction()
    {
        base.Begin_DoAction();

        if (bExist == false)
            return;

        bExist = false;

        index++;
        animator.SetTrigger("NextCombo"); // ���� �޺��� ���� �ִϸ��̼� Ʈ���� ����
        if (doActionDatas[index].bCanMove)
        {
            Move(); // �̵� �޼��� ȣ��
            return;
        }
        CheckStop(index); // ���ߴ� ���� üũ
    }

    // ������ �����ϴ� �޼���
    public override void End_DoAction()
    {
        base.End_DoAction();

        index = 0;
        bEnable = false;
    }

    // �浹�� �߻����� �� ȣ��Ǵ� �޼���
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