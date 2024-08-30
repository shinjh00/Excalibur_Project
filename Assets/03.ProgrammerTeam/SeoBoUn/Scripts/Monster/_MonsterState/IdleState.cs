using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// IdleState 제작
/// </summary>
public class IdleState : BaseState
{
    private Coroutine routine;

    /// <summary>
    /// 생성자
    /// </summary>
    /// <param name="owner">사용하는 주체의 몬스터</param>
    public IdleState(BaseMonster owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        owner.ChangeAlertSet(true);
        owner.SetAnimator("Idle", true);
        owner.CurState = MonsterState.Idle;
        routine = owner.StartCoroutine(StartPatrol());
    }

    public override void Exit()
    {
        owner.ChangeAlertSet(false);
        owner.SetAnimator("Idle", false);
        if (routine != null)
        {
            owner.StopCoroutine(routine);
        }
    }

    IEnumerator StartPatrol()
    {
        owner.IsPatrol = false;
        yield return new WaitForSeconds(3f);

        owner.IsPatrol = true;
    }
}
