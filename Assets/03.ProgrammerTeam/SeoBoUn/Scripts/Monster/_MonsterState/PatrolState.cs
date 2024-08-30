using System.Collections;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 몬스터의 정찰 상태
/// </summary>
public class PatrolState : BaseState
{
    private Coroutine coroutine;
    private LayerMask wallMask;
    private LayerMask obstacleMask;

    public PatrolState(BaseMonster owner)
    {
        this.owner = owner;
        wallMask = LayerMask.GetMask("Wall");
        obstacleMask = LayerMask.GetMask(new string[3] { "Wall", "Interact", "Monster" });
    }

    public override void Enter()
    {
        coroutine = owner.StartCoroutine(MoveRoutine());
        owner.CurState = MonsterState.Patrol;
    }

    public override void Exit()
    {
        owner.SetAnimator("Move", false);
        owner.SetAnimator("Idle", false);
        if (coroutine != null)
        {
            owner.StopCoroutine(coroutine);
        }
    }

    IEnumerator MoveRoutine()
    {
        while (true)
        {
            Vector2 curPos = owner.transform.position;
            Vector2 randPos = (Vector2)owner.transform.position + Random.insideUnitCircle * Random.Range(3f, 5f);
            Vector3 targetPos = Vector3.zero;

            Vector2 moveDir = (randPos - curPos).normalized;
            float distance = Vector2.Distance(randPos, curPos);

            RaycastHit2D hit = Physics2D.Raycast(curPos, moveDir, distance, wallMask);
            if (hit)
            {
                targetPos = hit.point - moveDir * Random.Range(1, 4);
            }
            else
            {
                targetPos = randPos;
            }

            // 오른쪽과 해당 위치 포인트에 대한 내적값이 90도의 내적값보다 크다? -> 각도가 더 작다 -> 
            if (Vector2.Dot(owner.gameObject.transform.right, targetPos - owner.gameObject.transform.right) > owner.CosRange)
            {
                owner.ChangeFlip(false);
            }
            else
            {
                owner.ChangeFlip(true);
            }

            owner.SetAnimator("Idle", false);
            owner.SetAnimator("Move", true);
            
            float rate = 0f;
            float time = Vector3.Distance(curPos, targetPos) / owner.m_MonsterData.moveSpeed;
            while (rate < 1f)
            {
                rate += Time.fixedDeltaTime / time;
                if(rate > 1f)
                {
                    rate = 1f;
                }
                owner.transform.position = Vector2.Lerp(curPos, targetPos, rate);
                yield return new WaitForSeconds(0.05f);

                if(Physics2D.Raycast(owner.transform.position, moveDir, 0.2f, wallMask))
                {
                    break;
                }
            }
            owner.SetAnimator("Move", false);
            owner.SetAnimator("Idle", true);
            yield return new WaitForSeconds(2f);
        }
    }
}
