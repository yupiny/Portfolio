using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sword 클래스는 Melee 클래스를 상속받아 검을 구현함
public class Sword : Melee
{
    // 손 오브젝트의 이름을 저장하는 변수
    [SerializeField]
    private string swordHandName = "Hand_Sword";

    // 검이 붙을 손의 Transform을 저장하는 변수.
    private Transform handTransform;

    // Reset 메서드는 Unity 에디터에서 Reset 버튼을 눌렀을 때 호출
    // 여기서는 base 클래스의 Reset 메서드를 호출하고, 무기 타입을 검으로 설정
    protected override void Reset()
    {
        base.Reset();
        type = WeaponType.Sword;
    }

    // Start 메서드는 Unity 엔진이 게임 오브젝트를 활성화할 때 호출
    protected override void Start()
    {
        base.Start();

        // handName에 해당하는 이름을 가진 자식을 찾아 handTransform에 저장
        handTransform = rootObject.transform.FindChildByName(swordHandName);
        Debug.Assert(handTransform != null); // handTransform이 null이 아닌지 확인
        transform.SetParent(handTransform, false); // 현재 검의 Transform을 handTransform의 자식으로 설정하고, 로컬 좌표를 유지

        // 검 게임 오브젝트 비활성화
        gameObject.SetActive(false);
    }

    // Begin_Equip 메서드는 검을 장착할 때 호출
    public override void Begin_Equip()
    {
        base.Begin_Equip();

        // 검 게임 오브젝트 활성화
        gameObject.SetActive(true);
    }

    // UnEquip 메서드는 검을 해제할 때 호출
    public override void UnEquip()
    {
        base.UnEquip();

        // 검 게임 오브젝트를 비활성화
        gameObject.SetActive(false);
    }
}