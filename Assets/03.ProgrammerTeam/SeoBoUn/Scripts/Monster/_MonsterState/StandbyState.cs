using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 몬스터의 대기 상태(미믹용으로 이용예정)
/// </summary>
public class StandbyState : BaseState
{
    public StandbyState(BaseMonster owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        owner.CurState = MonsterState.Standby;
        owner.SetAnimator("Standby");
    }

    public override void Exit()
    {
    }
}
