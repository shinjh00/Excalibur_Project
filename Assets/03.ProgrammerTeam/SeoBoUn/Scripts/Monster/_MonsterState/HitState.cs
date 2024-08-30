using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 개발자 : 서보운
/// 몬스터의 피격 상태
/// </summary>
public class HitState : BaseState
{
    Coroutine coroutine;

    public HitState(BaseMonster owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        if (owner.CurState == MonsterState.Hit)
        {
            return;
        }
        owner.CurState = MonsterState.Hit;
        
        if (!owner.Animator.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
        {   // 예외처리. 몬스터가 피격 상태일 때 중복해서 피격이 들어가는 경우를 방지함
            owner.SetAnimator("Hit");
        }
        coroutine = owner.StartCoroutine(HitRoutine());
        owner.photonView.RPC("CheckSound", Photon.Pun.RpcTarget.All);
    }

    IEnumerator HitRoutine()
    {
        yield return new WaitForSeconds(0.33f);
        owner.IsDamage = false;
    }
}
