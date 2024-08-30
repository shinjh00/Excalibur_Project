using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

/// <summary>
/// 개발자 : 서보운
/// <br/> 공용스킬 팬던트
/// <br/>팬던트 사망 상태에서 자동으로 사용 쿨타임은 1분
/// <br/>팬던트 스킬이 있으면 사망 상태에 이르렀을 때 5초 이후 15퍼센트의 체력을 회복하며 부활
/// .부활 후 2초간 무적
/// </summary>
public class CommonSkill_Pendant : CommonSkillData
{
    [Tooltip("회복량(0.15 -> 15%, 0.25 -> 25%)")]
    public float healAmount;

    bool isUse;
    public bool IsUse { get { return isUse; } }

    Coroutine executeRoutine;

    public override void SetData()
    {
        skillInfo = CsvParser.Instance.SkillDic[id];
        skillInfo.ReadEffectData();

    }

    public override void Execute(PlayerController controller)
    {
        executeRoutine = controller.StartCoroutine(ExecuteRoutine(controller, () => (controller.HealthController.Hp <= 0)));
    }

    IEnumerator ExecuteRoutine(PlayerController controller, Func<bool> UseCheck)
    {
        isUse = true;
        yield return new WaitUntil(() => UseCheck());
        Debug.Log($"{controller.name}의 부활 펜던트 사용");
        controller.SkillController.enabled = false;
        yield return new WaitForSeconds(5f);
        controller.SetAnimator("Revive");
        SoundManager.instance.PlaySFX(skillInfo.skEffect1_Data.mainSound, controller.audioSource);
        controller.AllControllerStart();
        controller.HealthController.Hp += (controller.HealthController.MaxHp * healAmount);
        controller.MoveController.MovingStop();
        controller.StateController.StateChange(PlayerState.Dead, 0, 0, false, false);
        controller.StateController.StateChange(PlayerState.Groggy, 0, 0, false, false);

        isUse = false;
    }
}
