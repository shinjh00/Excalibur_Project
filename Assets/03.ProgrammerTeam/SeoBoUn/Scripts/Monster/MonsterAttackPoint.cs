using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// <br/>몬스터 어택 포인트에 대한 스크립트
/// </summary>
public class MonsterAttackPoint : MonoBehaviour
{
    [SerializeField] Collider2D rightAttackCollider;
    [SerializeField] Collider2D leftAttackCollider;
    [SerializeField] Collider2D curCollider;
    [SerializeField] BaseMonster owner;
    [SerializeField] LayerMask playerLayer;

    public void SetOwner(BaseMonster owner)
    {
        this.owner = owner;
        curCollider = leftAttackCollider;
    }

    /// <summary>
    /// 공격을 시작할 메소드
    /// 어택 중임을 표현함과 동시에 왼쪽, 오른쪽 중 flip상태에 따른 콜라이더 활성화
    /// </summary>
    public void StartAttack()
    {
        if(owner.SpriteRender.flipX)
        {
            curCollider = leftAttackCollider;
        }
        else
        {
            curCollider = rightAttackCollider;
        }
        owner.IsAttack = true;
        curCollider.enabled = true;
    }

    /// <summary>
    /// 공격을 종료할 메소드
    /// 모든 공격 콜라이더를 끄기
    /// </summary>
    public void EndAttack()
    {
        owner.IsAttack = false;
        leftAttackCollider.enabled = false;
        rightAttackCollider.enabled = false;
        curCollider.enabled = false;
    }

    public void EndAttaking()
    {
        owner.IsAttacking = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            if(!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            if (owner.Target != null && owner.Target.StateController.CurState.Contain(PlayerState.invincible))
            {   // 중복 방지
                return;
            }

            if(owner.CurState == MonsterState.Die)
            {   // 사망시 공격 불가능
                return;
            }

            IDamageable target = other.gameObject.GetComponent<IDamageable>();
            IStateable targetS = other.gameObject.GetComponent<IStateable>();
            target?.TakeDamage(owner.m_MonsterData.damage);
            target?.TakeKnockBack(transform.position, 3f, 7f);
            targetS?.StateChange(PlayerState.Knockback, 0.5f, 0.3f, true, false);
        }
    }
}
