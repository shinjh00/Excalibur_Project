using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자: 이예린 / 공격 이펙트 관련 클래스
/// </summary>
public class AttackEffect : MonoBehaviour
{
    [SerializeField] protected AttackPoint attackPoint;
    [SerializeField] Animator effectAnimator;

    /// <summary>
    /// 공격 이팩트 애니메이션 시작할 때 해야할 작업 진행하는 메소드
    /// </summary>
    public void StartEffect()
    {
        if (effectAnimator != null)
        {
            effectAnimator.SetTrigger("Attack");
        }
        transform.parent.rotation = attackPoint.transform.rotation;
    }

    /// <summary>
    /// 공격 이팩트 애니메이션 끝날 때 해야할 작업 진행하는 메소드
    /// </summary>
    public void EndEffect()
    {
        attackPoint.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 실제 데미지를 주는 메소드
    /// 애니메이션 이벤트에 할당 필요
    /// </summary>
    public void AttackDamage()
    {
        attackPoint.Attack();
    }

    public void Skill1()
    {
        Debug.Log("스킬1");
        if (effectAnimator != null)
        {
            Debug.Log("스킬1 트리거");
            effectAnimator.SetTrigger("Skill1");
        }
    }

    public void Skill2()
    {
        Debug.Log("스킬2");
        if(effectAnimator != null)
        {
            Debug.Log("스킬2 트리거");
            effectAnimator.SetTrigger("Skill2");
        }
    }

    public void Skill3()
    {
        if(effectAnimator != null)
        {
            effectAnimator.SetTrigger("Skill3");
        }
    }
}
