using UnityEngine;

public class Staff : Melee
{
    [SerializeField]
    private string staffHandName = "Hand_Staff";

    private Transform staffHandTransform;

    protected override void Reset()
    {
        base.Reset();
        type = WeaponType.Staff;
    }

    protected override void Start()
    {
        base.Start();

        staffHandTransform = rootObject.transform.FindChildByName(staffHandName);
        Debug.Assert(staffHandTransform != null);
        transform.SetParent(staffHandTransform, false);

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