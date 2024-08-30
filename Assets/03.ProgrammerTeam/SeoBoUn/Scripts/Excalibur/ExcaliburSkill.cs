using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// <br/> 엑스칼리버의 스킬 데이터
/// </summary>

public class ExcaliburSkill : SkillData
{
    [Header("Skill1")]
    [Tooltip("Skill1 이동 속도(기본 계수는 0.5)")]
    [SerializeField] float skill1MoveFactor;
    ExcaliburAttackPoint skill1AttackPoint;

    [Space(5)]
    [Header("Skill2")]
    [Tooltip("Skill1 이동 속도(기본 계수는 0)")]
    [SerializeField] float skill2MoveFactor;

    [SerializeField] RangeChecker rangeIns;
    RangeChecker ins;

    int curSkillNum;

    float curMoveFactor;    // 이동 계수. 움직이면서 작동해야 하는 1번 스킬에 해당

    protected override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// ExcaliburSkill1. 전술 횡베기
    /// </summary>
    /// <param name="player"></param>
    public override void Skill1_Ready(PlayerController player)
    {
        owner.AttackController.enabled = false;
        curMoveFactor = skill1MoveFactor;
        SetData();
        curSkillNum = 1;

        float s1ForeDelay = Utils.CalculateDelay(skill1Data.skEffect1_Data.minForeDelay, skill1Data.skEffect1_Data.maxForeDelay, owner.AttackController.AtkSpeed);
        float s1AfterDelay = Utils.CalculateDelay(skill1Data.skEffect1_Data.minAftDelay, skill1Data.skEffect1_Data.maxAftDelay, owner.AttackController.AtkSpeed);
        skillCoroutine = StartCoroutine(SkillRoutine(s1ForeDelay, s1AfterDelay, () => Skill1_Logic(owner, 0f)));
    }

    public override void Skill1_Excute(PlayerController player, object parameter)
    {

    }

    public override void Skill1_Logic(PlayerController player, object parameter)
    {
        skill1AttackPoint.Skill1();
    }

    /// <summary>
    /// ExcaliburSkill2. 빛이 되어라
    /// </summary>
    /// <param name="parameter"></param>
    public override void Skill2_Ready(PlayerController player)
    {
        owner.AttackController.enabled = false;
        SetData();
        if (owner.photonView.IsMine)
        {
            if (!casting)
            {
                casting = true;
                ins = Instantiate(rangeIns, owner.transform.position, Quaternion.identity);
                Vector3 rangeScale = new Vector3(1f, skill2Data.skEffect2_Data.skillRange);
                float inRangeScale = 1f;
                ins.InitSkill(RangeType.NonTarget, rangeScale, inRangeScale, this, player);
                ins.AddSkill((direction) => owner.photonView.RPC("Skill2_Excute", RpcTarget.All, direction));
                curSkillNum = 2;
            }
            else
            {
                if (ins != null)
                    ins.Cancle();
                owner.AttackController.enabled = true;
                casting = false;
            }
        }
    }
    public override void Skill2_Excute(object parameter)
    {
        float s2ForeDelay = Utils.CalculateDelay(skill2Data.skEffect1_Data.minForeDelay, skill2Data.skEffect1_Data.maxForeDelay, owner.AttackController.AtkSpeed);
        float s2AfterDelay = Utils.CalculateDelay(skill2Data.skEffect1_Data.minAftDelay, skill2Data.skEffect1_Data.maxAftDelay, owner.AttackController.AtkSpeed);
        curMoveFactor = skill2MoveFactor;
        skillCoroutine = StartCoroutine(SkillRoutine(s2ForeDelay, s2AfterDelay, () => Skill2_Logic(owner, 3f)));
    }

    public override void Skill2_Logic(PlayerController player, object parameter)
    {
        skill1AttackPoint.Skill2(skill2Data.skEffect1_Data.susTime);
    }


    // 엑스칼리버는 Ult 스킬 미존재
    public override void Ult_Excute(object parameter)
    {
        return;
    }

    public override void Ult_Logic(PlayerController player, object pos)
    {
        return;
    }

    public override void Ult_Ready(PlayerController player)
    {
        return;
    }

    public override IEnumerator SkillRoutine(float firstDelay, float afterDelay, Action skill)
    {
        if (owner.photonView.IsMine)
        {
            owner.StatController.AddBuffStat(StatLevel.MoveSpeed, curMoveFactor);
        }
        owner.StateController.StateChange(PlayerState.SkillStart, 0, 0, true, false);
        switch(curSkillNum)
        {
            case 1:
                SoundManager.instance.PlaySFX(skill1Data.skEffect1_Data.beforeSound, owner.audioSource);
                break;
            case 2:
                SoundManager.instance.PlaySFX(skill2Data.skEffect1_Data.beforeSound, owner.audioSource);
                break;
        }
        yield return new WaitForSeconds(firstDelay);
        switch (curSkillNum)
        {
            case 1:
                SoundManager.instance.PlaySFX(skill1Data.skEffect1_Data.mainSound, owner.audioSource);
                break;
            case 2:
                SoundManager.instance.PlaySFX(skill2Data.skEffect1_Data.mainSound, owner.audioSource);
                break;
        }
        skill();
        yield return new WaitForSeconds(afterDelay);
        switch (curSkillNum)
        {
            case 1:
                SoundManager.instance.PlaySFX(skill1Data.skEffect1_Data.afterSound, owner.audioSource);
                break;
            case 2:
                SoundManager.instance.PlaySFX(skill2Data.skEffect1_Data.afterSound, owner.audioSource);
                break;
        }
        if (owner.photonView.IsMine)
        {
            owner.StatController.RemoveBuffStat(StatLevel.MoveSpeed, curMoveFactor);
        }
        owner.StateController.StateChange(PlayerState.SkillExit, 0, 0, true, false);
    }

    private void SetData()
    {
        if (skill1AttackPoint == null)
        {
            skill1AttackPoint = owner.AttackController.AttackPoint as ExcaliburAttackPoint;
            skill1AttackPoint.IsMoving += UsingSkill2;

            skill1AttackPoint.SetInfo(0,
                owner.AttackController.AtkDamage * skill1Data.skEffect1_Data.dmgFactor,
                skill1Data.skEffect1_Data.knockbackDis,
                owner.PlayerClassData.knockBackSpeed,
                skill1Data.skEffect1_Data.unbeatableTime,
                skill1Data.skEffect1_Data.knockbackTime);

            skill1AttackPoint.SetInfo(1,
                owner.AttackController.AtkDamage * skill2Data.skEffect1_Data.dmgFactor,
                skill2Data.skEffect1_Data.knockbackDis,
                owner.PlayerClassData.knockBackSpeed,
                skill2Data.skEffect1_Data.unbeatableTime,
                skill2Data.skEffect1_Data.knockbackTime);
        }
    }

    public override void GetSkillId()
    {
        skill1id = 1002022;
        skill2id = 1002023;
    }

    public void UsingSkill2(bool value)
    {
        if(!value)
        {
            owner.StatController.AddBuffStat(StatLevel.MoveSpeed, 0.01f);
        }
        else
        {
            owner.StatController.RemoveBuffStat(StatLevel.MoveSpeed, 0.01f);
        }
    }
}
