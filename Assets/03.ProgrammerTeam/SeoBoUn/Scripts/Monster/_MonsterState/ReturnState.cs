using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 몬스터의 다시 돌아가는 상태
/// <br/>현재는 사용하지 않는 스크립트
/// </summary>
public class ReturnState : BaseState
{
    public ReturnState(BaseMonster owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        owner.SetAnimator("Move", true);
        owner.CurState = MonsterState.Return;
    }

    public override void Exit()
    {
        owner.SetAnimator("Move", false);
    }
}
