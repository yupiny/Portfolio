using UnityEngine;

public class Staff : Melee
{
    [SerializeField]
    private string handName = "";

    private Transform staffHandTransform;

    protected override void Reset()
    {
        base.Reset();
        type = WeaponType.Staff;
    }

    protected override void Start()
    {
        base.Start();

    }
}