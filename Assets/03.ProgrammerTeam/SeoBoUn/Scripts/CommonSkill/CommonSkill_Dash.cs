using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// <br/> 공용스킬 대쉬
/// <br/> 플레이어가 사용하면 마우스 방향을 향해 4칸 거리 순간이동(쿨타임 10초)
/// </summary>
public class CommonSkill_Dash : CommonSkillData
{
    [Tooltip("대쉬 거리")]
    public float dashDistance;
    [Tooltip("대쉬 스피드")]
    public float dashSpeed;
    [Tooltip("장애물(벽, 트랩) 레이어")]
    public LayerMask wallLayer;
    Coroutine dashRoutine;

    public override void SetData()
    {
        skillInfo = CsvParser.Instance.SkillDic[id];
        skillInfo.ReadEffectData();

        dashDistance = skillInfo.skEffect1_Data.skillRange;
        dashSpeed = skillInfo.skEffect1_Data.skillSpd;
        coolTime = skillInfo.coolTime;
    }

    public override void Execute(PlayerController controller)
    {
        // 입력 방향으로 이동
        SoundManager.instance.PlaySFX(skillInfo.skEffect1_Data.mainSound, controller.audioSource);

        Vector2 moveDir = controller.MoveController.MoveDir;
        if (moveDir == Vector2.zero)
        {   // 만약 입력이 없었다면 마우스 방향으로 대쉬
            moveDir = controller.AttackController.AttackPoint.MouseDir.normalized;
        }
        Vector2 targetPos = Vector2.zero;

        RaycastHit2D hitInfo = Physics2D.Raycast(controller.transform.position, moveDir, dashDistance, wallLayer);

        if(hitInfo)
        {
            if (wallLayer.Contain(hitInfo.collider.gameObject.layer))
            {
                targetPos = hitInfo.point - (moveDir * 0.5f);
            }
            else
            {
                targetPos = hitInfo.point;
            }

        }
        else
        {
            targetPos = (Vector2)controller.transform.position + moveDir * dashDistance;
        }

        // controller.transform.position = targetPos;

        controller.MoveController.IsDash = true;
        dashRoutine = controller.StartCoroutine(DashRoutine(controller, targetPos));
    }

    /// <summary>
    /// 대쉬 루틴
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    IEnumerator DashRoutine(PlayerController controller, Vector2 targetPos)
    {
        Vector2 startPos = controller.transform.position;

        float rate = 0f;
        float time = Vector2.Distance(startPos, targetPos) / dashSpeed;

        controller.MoveController.IsDash = true;
        while ( rate < 1f )
        {
            rate += Time.deltaTime / time;

            if(rate > 1f)
            {
                rate = 1f;
            }

            controller.transform.position = Vector2.Lerp(startPos, targetPos, rate);

            yield return null;
        }

        controller.MoveController.IsDash = false;
    }
}
