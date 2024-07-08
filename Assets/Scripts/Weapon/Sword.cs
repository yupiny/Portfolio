using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : Melee
{
    [SerializeField]
    private string handName = "Hand_Sword";

    private Transform handTransform;

    protected override void Reset()
    {
        base.Reset();

        type = WeaponType.Sword;
    }

    protected override void Start()
    {
        base.Start();

        handTransform = rootObject.transform.FindChildByName(handName);
        Debug.Assert(handTransform != null);

        transform.SetParent(handTransform, false); // 만든걸 손에 붙힘
        gameObject.SetActive(false);
    }

    public override void Begin_Equip()
    {
        base.Begin_Equip();

        gameObject.SetActive(true);
    }

    public override void UnEquip()
    {
        base.UnEquip();

        gameObject.SetActive(false);
    }
}
