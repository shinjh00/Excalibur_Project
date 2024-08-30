using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 개발자 : 서보운
/// 몬스터의 어택 상태에 대한 스크립트
/// </summary>
public class AttackState : BaseState
{
    private Coroutine coroutine;

    public AttackState(BaseMonster owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        if (owner.CurState == MonsterState.Attack)
        {
            return;
        }
        owner.CurState = MonsterState.Attack;
        owner.SetAnimator("Idle", true);
        coroutine = owner.StartCoroutine(AttackRoutine());
    }

    public override void Exit()
    {
        if (coroutine != null)
        {
            owner.StopCoroutine(coroutine);
        }
        owner.EndAttack();
        owner.IsAttack = false;
        owner.IsAttacking = false;
        owner.SetAnimator("Idle", false);
    }

    IEnumerator AttackRoutine()
    {   // TODO... AttackDelay 필요함

        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if (owner.AttackCooltime > 1.5f)
            {
                owner.IsAttacking = true;
                if (!owner.Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                {
                    owner.SetAnimator("Attack");
                    owner.photonView.RPC("CheckSound", Photon.Pun.RpcTarget.All);
                }
                owner.AttackCooltime = 0f;
            }
            else
            {
                owner.AttackCooltime += 0.1f;
            }
        }
    }
}
