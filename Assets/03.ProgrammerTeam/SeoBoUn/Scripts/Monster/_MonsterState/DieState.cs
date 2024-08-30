using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 몬스터 사망 상태 클래스
/// </summary>
public class DieState : BaseState
{
    private Coroutine routine;

    /// <summary>
    /// 생성자
    /// </summary>
    /// <param name="owner">사용하는 주체의 몬스터</param>
    public DieState(BaseMonster owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        if(owner.CurState == MonsterState.Die)
        {
            return;
        }
        routine = owner.StartCoroutine(DieRoutine());
        owner.CurState = MonsterState.Die;
        owner.SetAnimator("Die");
        owner.Die();
        owner.AttackPoint.EndAttack();
        owner.photonView.RPC("CheckSound", Photon.Pun.RpcTarget.All);
    }

    /// <summary>
    /// 사망 루틴을 진행
    /// </summary>
    /// <returns></returns>
    IEnumerator DieRoutine()
    {
        yield return null;
    }
}
