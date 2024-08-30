using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 개발자 : 서보운
/// 몬스터의 추격 상태
/// </summary>
public class TraceState : BaseState
{
    private Coroutine coroutine;
    private LayerMask wallMask;

    public TraceState(BaseMonster owner)
    {
        this.owner = owner;
        wallMask = LayerMask.GetMask("Wall");
    }

    public override void Enter()
    {
        owner.CurDectionRange = owner.m_MonsterData.torchDetectionRange;
        owner.EndAttack();
        owner.ChangeTraceSet(true);
        owner.SetAnimator("Move", true);
        owner.CurState = MonsterState.Trace;
        if (PhotonNetwork.IsMasterClient)
        {
            owner.photonView.RPC("TargetSnyc", RpcTarget.All, owner.Target.photonView.ViewID);
        }
        coroutine = owner.StartCoroutine(AttackRoutine());
    }

    public override void FixedUpdate()
    {
        Vector2 dir = Vector2.zero;
        if (owner.Target != null)
        {
            dir = (owner.TargetPos.position - owner.transform.position).normalized;

            owner.transform.Translate(dir * owner.m_MonsterData.moveSpeed * Time.fixedDeltaTime);

            RaycastHit2D hit = Physics2D.Raycast(owner.transform.position, dir, 2f, wallMask);
            dir = dir + (Vector2)owner.transform.up * 2f;

            if (Vector2.Dot(owner.gameObject.transform.right, dir) > owner.CosRange)
            {
                owner.ChangeFlip(false);
            }
            else
            {
                owner.ChangeFlip(true);
            }
        }
    }

    IEnumerator AttackRoutine()
    {   // TODO... AttackDelay 필요함

        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            owner.AttackCooltime += 0.1f;
        }
    }

    public override void Exit()
    {
        owner.CurDectionRange = owner.m_MonsterData.detectionRange;
        owner.ChangeTraceSet(false);
        owner.SetAnimator("Move", false);
        if (coroutine != null)
        {
            owner.StopCoroutine(coroutine);
        }
    }
}
