using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// <br/> 공용스킬 회복
/// <br/> 10초에 걸쳐 전체 체력의 25퍼센트에 해당하는 체력을 회복
/// <br/> 10초동안 공격을 받지 않을 경우 추가로 10퍼센트의 체력을 한번에 회복(쿨타임 1분)
/// </summary>
public class CommonSkill_Heal : CommonSkillData
{
    [Tooltip("힐에 대한 총 시간(초)")]
    public int healTime;
    [Tooltip("총 회복량(0.25 -> 25%, 0.5 -> 50%")]
    public float healPercent;
    [Tooltip("한번에 회복할 체력량(0.1 -> 10%)")]
    public float healRecoveryOnce;

    bool isHited;

    Coroutine healRoutine;

    public override void SetData()
    {
        skillInfo = CsvParser.Instance.SkillDic[id];
        skillInfo.ReadEffectData();

        healPercent = skillInfo.skEffect1_Data.hpFactor;
        healRecoveryOnce = skillInfo.skEffect2_Data.hpFactor;
        coolTime = skillInfo.coolTime;
    }

    public override void Execute(PlayerController controller)
    {
        healRoutine = controller.StartCoroutine(HealRoutine(controller));
    }

    /// <summary>
    /// 힐 기능 루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator HealRoutine(PlayerController controller)
    {
        int healCount = 0;
        // 한번에 회복량 0.25 / 10 -> 0.025
        float healAmount = healPercent / healTime;
        isHited = false;

        controller.HealthController.SetHitEvent(OnHit);

        while(healCount < healTime)
        {
            controller.HealthController.Hp += (controller.HealthController.MaxHp * healAmount);
            healCount++;
            SoundManager.instance.PlaySFX(skillInfo.skEffect1_Data.mainSound, controller.audioSource);
            yield return new WaitForSeconds(1f);
        }

        controller.HealthController.RemoveHitEvent(OnHit);

        if (isHited)
        {   // 맞았다면
            yield break;
        }
        SoundManager.instance.PlaySFX(skillInfo.skEffect2_Data.mainSound, controller.audioSource);

        controller.HealthController.Hp += (controller.HealthController.MaxHp * healRecoveryOnce);
    }

    private void OnHit()
    {
        isHited = true;
    }
}
