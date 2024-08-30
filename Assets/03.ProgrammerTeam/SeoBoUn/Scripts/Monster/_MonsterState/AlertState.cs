using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 서보운
/// 몬스터의 경계상태
/// 3초간 경계 및 이미지 띄우기
/// </summary>
public class AlertState : BaseState
{
    private Coroutine coroutine;

    public AlertState(BaseMonster owner)
    {
        this.owner = owner;
    }

    /// <summary>
    /// 진입 시 경계 이미지 띄우고 경계 코루틴을 진행
    /// </summary>
    public override void Enter()
    {
        owner.ChangeAlertSet(true);
        coroutine = owner.StartCoroutine(AlertCoroutine());
        owner.SetAnimator("Idle", true);
        owner.CurState = MonsterState.Alert;
    }

    public override void Exit()
    {
        owner.ChangeAlertSet(false);
        if (coroutine != null)
        {
            owner.StopCoroutine(coroutine);
        }
        owner.SetAnimator("Idle", false);
    }

    /// <summary>
    /// 개발자 : 서보운
    /// 3초간 경계상태를 확인하기 위한 메소드
    /// </summary>
    /// <returns></returns>
    private IEnumerator AlertCoroutine()
    {
        float alertTime = 0f;
        owner.StartTrace = false;

        while (alertTime < owner.m_MonsterData.detectionTime && owner.TargetPos != null)
        {
            alertTime += 0.1f;

            Vector2 dir = (owner.TargetPos.position - owner.transform.position).normalized;

            if (Vector2.Dot(owner.gameObject.transform.right, dir) > owner.CosRange)
            {
                owner.ChangeFlip(false);
            }
            else
            {
                owner.ChangeFlip(true);
            }

            yield return new WaitForSeconds(0.1f);
        }
        owner.StartTrace = true;
    }
}
