using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Data.Common;
using Photon.Realtime;

/// <summary>
/// 개발자 : 서보운
/// <br/> 아쳐의 스킬 데이터
/// </summary>
[Serializable]
public class ArcherSkill : SkillData
{
    Vector3 targetDir;
    [Header("Skill1")]
    [Tooltip("skill1 프리팹 할당 필요")]
    [SerializeField] ArrowSkill arrow;

    [Space(5)]
    [Header("Skill2")]
    [Tooltip("skill2 프리팹 할당 필요")]
    [SerializeField] BoomArrowSkill boomArrow;
    [SerializeField] bool isUse;


    [Space(5)]
    [Header("Ult")]
    [Tooltip("후크 스킬 프리팹 할당 필요")]
    [SerializeField] HookSkill hookSkill;

    RangeChecker ins;
    [SerializeField] RangeChecker rangeIns;
    int curSkillNum;

    #region Skill1
    /// <summary>
    /// ArcherSkill1. 열발같은 한발
    /// </summary>
    /// <param name="player"></param>
    public override void Skill1_Ready(PlayerController player)
    {
        // 1. 스킬 사용 중 어택은 불가능
        owner.AttackController.enabled = false;
        if(owner.photonView.IsMine)
        {
            if(!casting)    // 캐스팅 상태가 아니라면
            {
                // 캐스팅 시작
                casting = true;
                // 발사될 위치 선정을 위해 rangeIns 생성하고,
                ins = Instantiate(rangeIns, owner.transform.position, Quaternion.identity);
                // 범위는 너비 * 범위(1 * 4)
                Vector3 rangeScale = new Vector3(2f, skill1Data.skEffect1_Data.skillRange);
                float inRangeScale = 1f;
                ins.InitSkill(RangeType.NonTarget, rangeScale, inRangeScale, this, player);
                ins.AddSkill((direction) => owner.photonView.RPC("Skill1_Excute", RpcTarget.All, direction));
            }
            else
            {
                if(ins != null)
                {
                    ins.Cancle();
                }
                owner.AttackController.enabled = true;
                casting = false;
            }
        }
    }

    public override void Skill1_Excute(PlayerController player, object parameter)
    {
        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
        }
        curSkillNum = 1;
        float s1FirstDelay = Utils.CalculateDelay(s1FirstDelayMin, s1FirstDelayMax, owner.AttackController.AtkSpeed);
        float s1AfterDelay = Utils.CalculateDelay(s1AfterDelayMin, s1AfterDelayMax, owner.AttackController.AtkSpeed);
        skillCoroutine = StartCoroutine(SkillRoutine(s1FirstDelay, s1AfterDelay, () => Skill1_Logic(owner, parameter)));
    }

    public override void Skill1_Logic(PlayerController player, object parameter)
    {
        // 애니메이션 세팅
        // player.PlayerAnim.SetTrigger("Skill1");

        ArrowSkill instance = Instantiate(arrow, player.transform.position + (Vector3)parameter, Quaternion.identity).GetComponent<ArrowSkill>();
        instance.transform.up = (Vector3)parameter;
        instance.SetInit(skill1Data.skEffect1_Data.skillRange, skill1Data.skEffect1_Data.knockbackDis, skill1Data.skEffect1_Data.knockbackDis / skill1Data.skEffect1_Data.knockbackTime  , skill1Data.skEffect1_Data.skillSpd , skill1Data.skEffect1_Data.unbeatableTime, skill1Data.skEffect1_Data.knockbackTime);
        instance.SetInfo(parameter, player, player.AttackController.AtkDamage * skill1Data.skEffect1_Data.dmgFactor);
        StartCoroutine(instance.ArrowCreateRoutine());
    }
    #endregion

    #region Skill2
    /// <summary>
    /// ArcherSkill2. 한발에 두방
    /// </summary>
    /// <param name="player"></param>
    public override void Skill2_Ready(PlayerController player)
    {
        // 1. 사용한 적 있으며, 만약 적중해서 폭탄이 부착중이라면
        if (!isUse)
        {   // 2. 스킬 사용 중 어택은 불가능
            owner.AttackController.enabled = false;
            if (owner.photonView.IsMine)
            {
                if (!casting)    // 캐스팅 상태가 아니라면
                {
                    // 캐스팅 시작
                    casting = true;
                    // 발사될 위치 선정을 위해 rangeIns 생성하고,
                    ins = Instantiate(rangeIns, owner.transform.position, Quaternion.identity);
                    // 범위는 너비 * 범위(1 * 4)
                    Vector3 rangeScale = new Vector3(2f, skill2Data.skEffect1_Data.skillRange);
                    float inRangeScale = 1f;
                    ins.InitSkill(RangeType.NonTarget, rangeScale, inRangeScale, this, player);
                    ins.AddSkill((direction) => owner.photonView.RPC("Skill2_Excute", RpcTarget.All, direction));
                }
                else
                {
                    if (ins != null)
                    {
                        ins.Cancle();
                    }
                    owner.AttackController.enabled = true;
                    casting = false;
                }
            }
        }
    }

    public override void Skill2_Excute(object parameter)
    {
        if(skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
        }
        curSkillNum = 2;
        float s2FirstDelay = Utils.CalculateDelay(s2FirstDelayMin, s2FirstDelayMax, owner.AttackController.AtkSpeed);
        float s2AfterDelay = Utils.CalculateDelay(s2AfterDelayMin, s2AfterDelayMax, owner.AttackController.AtkSpeed);
        skillCoroutine = StartCoroutine(SkillRoutine(s2FirstDelay, s2AfterDelay, () => Skill2_Logic(owner, parameter)));
    }

    public override void Skill2_Logic(PlayerController player, object parameter)
    {
        // player.PlayerAnim.SetTrigger("Skill2");

        BoomArrowSkill instance = Instantiate(boomArrow, player.transform.position + (Vector3)parameter, Quaternion.identity).GetComponent<BoomArrowSkill>();
        instance.transform.up = (Vector3)parameter;
        instance.SetInit(skill2Data.skEffect1_Data.skillRange, skill2Data.skEffect1_Data.knockbackDis, skill2Data.skEffect1_Data.knockbackDis / skill2Data.skEffect1_Data.knockbackTime, skill2Data.skEffect1_Data.skillSpd, skill2Data.skEffect1_Data.unbeatableTime, skill2Data.skEffect1_Data.knockbackTime);
        instance.SetInfo(parameter, player, player.AttackController.AtkDamage * skill2Data.skEffect1_Data.dmgFactor);
        StartCoroutine(instance.BoomArrowCreateRoutine());
    }
    #endregion
    public override void Ult_Ready(PlayerController player)
    {
        // 1. 스킬 사용 중 어택은 불가능
        owner.AttackController.enabled = false;
        if (owner.photonView.IsMine)
        {
            if (!casting)    // 캐스팅 상태가 아니라면
            {
                // 캐스팅 시작
                casting = true;
                // 발사될 위치 선정을 위해 rangeIns 생성하고,
                ins = Instantiate(rangeIns, owner.transform.position, Quaternion.identity);
                // 범위는 너비 * 범위(1 * 4)
                Vector3 rangeScale = new Vector3(2f, skill3Data.skEffect1_Data.skillRange);
                float inRangeScale = 1f;
                ins.InitSkill(RangeType.NonTarget, rangeScale, inRangeScale, this, player);
                ins.AddSkill((direction) => owner.photonView.RPC("Ult_Excute", RpcTarget.All, direction));
            }
            else

            {
                if (ins != null)
                {
                    ins.Cancle();
                }
                owner.AttackController.enabled = true;
                casting = false;
            }
        }
    }

    public override void Ult_Excute(object parameter)
    {
        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
        }
        curSkillNum = 3;
        float ultFirstDelay = Utils.CalculateDelay(ultFirstDelayMin, ultFirstDelayMax, owner.AttackController.AtkSpeed);
        float ultAfterDelay = Utils.CalculateDelay(ultAfterDelayMin, ultAfterDelayMax, owner.AttackController.AtkSpeed);
        skillCoroutine = StartCoroutine(SkillRoutine(ultFirstDelay, ultAfterDelay, () => Ult_Logic(owner, parameter)));
    }

    public override void Ult_Logic(PlayerController player, object pos)
    {
        HookSkill instance = Instantiate(hookSkill, player.transform.position + (Vector3)pos, Quaternion.identity);

        instance.transform.up = (Vector3)pos;
        instance.SetInit(skill3Data.skEffect1_Data.skillRange, skill3Data.skEffect1_Data.knockbackDis, skill3Data.skEffect1_Data.knockbackDis / skill3Data.skEffect1_Data.knockbackTime, skill3Data.skEffect1_Data.skillSpd , skill3Data.skEffect1_Data.unbeatableTime, skill3Data.skEffect1_Data.knockbackTime);
        instance.SetInfo(pos, player, player.AttackController.AtkDamage * skill3Data.skEffect1_Data.dmgFactor);
        StartCoroutine(instance.HookRoutine());
    }

    public override IEnumerator SkillRoutine(float firstDelay, float afterDelay, Action skill)
    {
        owner.StateController.StateChange(PlayerState.SkillStart, 0, 0, true, false);
        owner.MoveSetter(false);
        owner.PlayerAnim.SetFloat("Move", 0);
        owner.PlayerAnim.SetTrigger("Skill2");
        switch(curSkillNum)
        {
            case 1:
                owner.AttackController.AttackPoint.Effect.Skill1();
                SoundManager.instance.PlaySFX(skill1Data.skEffect1_Data.beforeSound, owner.audioSource);
                break;
            case 2:
                owner.AttackController.AttackPoint.Effect.Skill2();
                SoundManager.instance.PlaySFX(skill2Data.skEffect1_Data.beforeSound, owner.audioSource);
                break;
            case 3:
                owner.AttackController.AttackPoint.Effect.Skill3();
                SoundManager.instance.PlaySFX(skill3Data.skEffect1_Data.beforeSound, owner.audioSource);
                break;
        }
        yield return new WaitForSeconds(firstDelay);
        skill();
        switch (curSkillNum)
        {
            case 1:
                SoundManager.instance.PlaySFX(skill1Data.skEffect1_Data.mainSound, owner.audioSource);
                break;
            case 2:
                SoundManager.instance.PlaySFX(skill2Data.skEffect1_Data.mainSound, owner.audioSource);
                break;
            case 3:
                SoundManager.instance.PlaySFX(skill3Data.skEffect1_Data.mainSound, owner.audioSource);
                break;
        }
        yield return new WaitForSeconds(afterDelay);
        switch (curSkillNum)
        {
            case 1:
                SoundManager.instance.PlaySFX(skill1Data.skEffect1_Data.afterSound, owner.audioSource);
                break;
            case 2:
                SoundManager.instance.PlaySFX(skill2Data.skEffect1_Data.afterSound, owner.audioSource);
                break;
            case 3:
                SoundManager.instance.PlaySFX(skill3Data.skEffect1_Data.afterSound, owner.audioSource);
                break;
        }
        owner.MoveSetter(true);
        owner.StateController.StateChange(PlayerState.SkillExit, 0, 0, true, false);
    }

    public override void GetSkillId()
    {
        skill1id = 1002014;
        skill2id = 1002015;
        skill3id = 1002016;
    }
}
