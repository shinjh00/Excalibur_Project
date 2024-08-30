using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 개발자 : 서보운
/// 아쳐용 스킬 컨트롤러
/// <br/> 2, 4, 7 레벨에 스킬을 하나씩 해금할 예정
/// </summary>
public class ArcherSkillController : PlayerSkillController
{
    ArcherHookAction hookAction;
    ArcherBoomAction boomAction;

    [SerializeField] Animator sk1_obj;

    private void Start()
    {
        hookAction = GetComponent<ArcherHookAction>();
        boomAction = GetComponent<ArcherBoomAction>();
    }

    protected override void GetStat()
    {
        curCommonSkill = new List<CommonSkillData>();

        switch (owner.PlayerClassData.classType)
        {
            case ClassType.Warrior:
                if (skillData[0] != null)
                    playerSkillData = skillData[0];
                break;
            case ClassType.Archer:
                if (skillData[1] != null)
                    playerSkillData = skillData[1];
                break;
            case ClassType.Wizard:
                if (skillData[2] != null)
                    playerSkillData = skillData[2];
                break;
            case ClassType.Knight:
                if (skillData[3] != null)
                    playerSkillData = skillData[3];
                break;
        }

        if (playerSkillData != null)
        {
            playerSkillDataIns = Instantiate(playerSkillData, transform.position, Quaternion.identity, transform);
            playerSkillDataIns.owner = owner;
            playerSkillDataIns.transform.parent = owner.transform;
            playerSkillDataIns.gameObject.SetActive(true);
        }

        // 포톤 네트워크용 (프로퍼티로 플레이어 공용 스킬 설정)
        if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(DefinePropertyKey.COMMONSKILL1))
        {
            return;
        }

        curCommonSkill.Add(commonSkillDatas[(int)PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1)]);
        curCommonSkill.Add(commonSkillDatas[(int)PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2)]);

        commonSkillCoolTime = new float[curCommonSkill.Count];
        isCoolTime = new bool[curCommonSkill.Count];

        for(int i = 0; i < curCommonSkill.Count; i++)
        {   // 패시브 스킬은 미리 적용하기
            curCommonSkill[i].SetData();

            if (curCommonSkill[i].skillType == SkillType.Passive)
            {
                GameManager.Ins.Message("Passive Skill Activate");
                curCommonSkill[i].Execute(owner);
            }
            commonSkillCoolTime[i] = curCommonSkill[i].coolTime;
            isCoolTime[i] = false;
        }

        // playerSkillDataIns.GetComponent<ArcherSkill>().InitSkillSet();
    }
    [PunRPC]
    public override void GeneralSkill1()
    {
        if(owner.StateController.CurState.Contain(PlayerState.Dead))
        {
            return;
        }

        
       //if (!owner.StatController.UnLockSkills[(int)PlayerStatController.SkillButton.Q])
       //{
       //    return;
       //}
        
        if (playerSkillDataIns != null)
        {
            playerSkillDataIns.Skill1_Ready(owner); ;

        }
    }

    [PunRPC]
    public override void GeneralSkill2()
    {
        if (owner.StateController.CurState.Contain(PlayerState.Dead))
        {   // 사망 시 사용 불가능
            return;
        }

        
       //if (!owner.StatController.UnLockSkills[(int)PlayerStatController.SkillButton.E])
       //{
       //    return;
       //}
        
        if (playerSkillDataIns != null && !boomAction.IsUse)
        {
            Debug.Log("사용 중이 아님");
            playerSkillDataIns.Skill2_Ready(owner);
        }
    }

    [PunRPC]
    public override void Skill2_Excute(object parameter)
    {
        if (owner.StateController.CurState.Contain(PlayerState.Dead))
        {   // 사망 시 사용 불가능
            return;
        }

        if (playerSkillDataIns != null && !boomAction.IsUse)
        {
            playerSkillDataIns.Skill2_Excute(parameter);
        }
    }

    [PunRPC]
    public override void SuperSkill()
    {
        if (owner.StateController.CurState.Contain(PlayerState.Dead))
        {
            return;
        }

        
       //if (!owner.StatController.UnLockSkills[(int)PlayerStatController.SkillButton.R])
       //{
       //    return;
       //}
        
        if (playerSkillDataIns != null)
        {
            playerSkillDataIns.Ult_Ready(owner);
        }
    }

    [PunRPC]
    public override void CommonSkill1()
    {
        if (owner.StateController.CurState.Contain(PlayerState.Dead))
        {   // 사망 시 사용 불가능
            return;
        }

        if (curCommonSkill[0].skillType == SkillType.Passive)
        {   // 패시브는 사용 불가능
            return;
        }
        if (isCoolTime[0])
        {   // 쿨타임중 사용 불가능
            Debug.Log("쿨타임 중");
            return;
        }

        curCommonSkill[0].Execute(owner);
        StartCoroutine(SkillCoolTime(0));
    }
    [PunRPC]
    public override void CommonSkill2()
    {
        if (owner.StateController.CurState.Contain(PlayerState.Dead))
        {   // 사망 시 사용 불가능
            return;
        }

        if (curCommonSkill[1].skillType == SkillType.Passive)
        {   // 패시브는 사용 불가능
            return;
        }
        if (isCoolTime[1])
        {   // 쿨타임중 사용 불가능
            Debug.Log("쿨타임 중");
            return;
        }

        curCommonSkill[1].Execute(owner);
        StartCoroutine(SkillCoolTime(1));
    }

    IEnumerator SkillCoolTime(int index)
    {

       if (owner.photonView.IsMine)
        {

            isCoolTime[index] = true;
            while (true)
            {
                skillUI.SkillCoolTime_UI((SkillUI.SkillType)index, (int)commonSkillCoolTime[index]);
                yield return new WaitForSeconds(1f);
                commonSkillCoolTime[index]--;

                if (commonSkillCoolTime[index] == 0)
                {
                    isCoolTime[index] = false;
                    skillUI.SkillCoolTime_UI((SkillUI.SkillType)index, (int)commonSkillCoolTime[index]);
                    break;
                }
            }
            commonSkillCoolTime[index] = curCommonSkill[index].coolTime;
        }

    }


}
