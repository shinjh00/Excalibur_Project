using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 아쳐용 어택 애니메이션 스크립트
/// </summary>
public class ArcherAttackAnim : MonoBehaviour
{
    [Tooltip("어택 컨트롤러 할당 필요")]
    [SerializeField] ArcherAttackController attackController;
    [Tooltip("어택 이펙트 할당 필요")]
    [SerializeField] AttackEffect attackEffect;
    [Tooltip("어택 포인트 할당 필요")]
    [SerializeField] AttackPoint attackPoint;

    /// <summary>
    /// 외부에서 어택을 시작할 때 무기 애니메이션도 동시에 동작하도록 만든 메소드
    /// <br/>애니메이션 이벤트 할당 필요
    /// </summary>
    public void EnterAttack()
    {
        attackPoint.StartWeaponAnim();
    }

    /// <summary>
    /// 외부에서 데미지를 줄 때 시작할 애니메이션 이벤트
    /// <br/>애니메이션 이벤트 할당 필요
    /// </summary>
    public void StartAttack()
    {
        attackController.CheckDamgeable();
    }

    /// <summary>
    /// 외부에서 공격이 끝났을 때 시작할 애니메이션 이벤트
    /// <br/>애니메이션 이벤트 할당 필요
    /// </summary>
    public void EndAttack()
    {
        attackController.SetAttackExit();
    }


}
