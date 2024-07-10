using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sword Ŭ������ Melee Ŭ������ ��ӹ޾� ���� ������
public class Sword : Melee
{
    // �� ������Ʈ�� �̸��� �����ϴ� ����
    [SerializeField]
    private string swordHandName = "Hand_Sword";

    // ���� ���� ���� Transform�� �����ϴ� ����.
    private Transform handTransform;

    // Reset �޼���� Unity �����Ϳ��� Reset ��ư�� ������ �� ȣ��
    // ���⼭�� base Ŭ������ Reset �޼��带 ȣ���ϰ�, ���� Ÿ���� ������ ����
    protected override void Reset()
    {
        base.Reset();
        type = WeaponType.Sword;
    }

    // Start �޼���� Unity ������ ���� ������Ʈ�� Ȱ��ȭ�� �� ȣ��
    protected override void Start()
    {
        base.Start();

        // handName�� �ش��ϴ� �̸��� ���� �ڽ��� ã�� handTransform�� ����
        handTransform = rootObject.transform.FindChildByName(swordHandName);
        Debug.Assert(handTransform != null); // handTransform�� null�� �ƴ��� Ȯ��
        transform.SetParent(handTransform, false); // ���� ���� Transform�� handTransform�� �ڽ����� �����ϰ�, ���� ��ǥ�� ����

        // �� ���� ������Ʈ ��Ȱ��ȭ
        gameObject.SetActive(false);
    }

    // Begin_Equip �޼���� ���� ������ �� ȣ��
    public override void Begin_Equip()
    {
        base.Begin_Equip();

        // �� ���� ������Ʈ Ȱ��ȭ
        gameObject.SetActive(true);
    }

    // UnEquip �޼���� ���� ������ �� ȣ��
    public override void UnEquip()
    {
        base.UnEquip();

        // �� ���� ������Ʈ�� ��Ȱ��ȭ
        gameObject.SetActive(false);
    }
}