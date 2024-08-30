using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// <br/> 엑스칼리버 캐릭터 전용 스킬 컨트롤러
/// 1, 2 스킬만 존재. 3스킬은 없음
/// </summary>
public class ExcaliburSkillController : PlayerSkillController
{
    [Tooltip("어택 포인트 할당 필요")]
    [SerializeField] Collider2D skill1AttackPoint;

    public Collider2D Skill1AttackPoint { get { return skill1AttackPoint; } }

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
            case ClassType.Excalibur:
                if (skillData[4] != null)
                    playerSkillData = skillData[4];
                break;
        }

        if (playerSkillData != null)
        {
            playerSkillDataIns = Instantiate(playerSkillData, transform.position, Quaternion.identity, transform);
            playerSkillDataIns.owner = owner;
            playerSkillDataIns.transform.parent = owner.transform;
            playerSkillDataIns.gameObject.SetActive(true);
        }

        return;

  /*      // 포톤 네트워크용 (프로퍼티로 플레이어 공용 스킬 설정)
        if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(DefinePropertyKey.COMMONSKILL1))
        {
            return;
        }

        curCommonSkill.Add(commonSkillDatas[(int)PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1)]);
        curCommonSkill.Add(commonSkillDatas[(int)PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2)]);

        commonSkillCoolTime = new float[curCommonSkill.Count];
        isCoolTime = new bool[curCommonSkill.Count];

        for (int i = 0; i < curCommonSkill.Count; i++)
        {   // 패시브 스킬은 미리 적용하기

            if (curCommonSkill[i].skillType == SkillType.Passive)
            {
                GameManager.Ins.Message("Passive Skill Activate");
                curCommonSkill[i].Execute(owner);
            }
            commonSkillCoolTime[i] = curCommonSkill[i].coolTime;
            isCoolTime[i] = false;
        }*/

    }

    [PunRPC]
    public override void GeneralSkill1()
    {   // 스킬 1. 엑스칼리버 유저는 스킬에 대한 해금 없이 바로 사용 가능
        if (owner.StateController.CurState.Contain(PlayerState.Dead))
        {
            return;
        }

        if (playerSkillDataIns != null)
        {
            playerSkillDataIns.Skill1_Ready(owner); ;
        }
    }

    [PunRPC]
    public override void GeneralSkill2()
    {   // 스킬 2. 엑스칼리버 유저는 스킬에 대한 해금 없이 바로 사용 가능
        if (owner.StateController.CurState.Contain(PlayerState.Dead))
        {   // 사망 시 사용 불가능
            return;
        }

        if (playerSkillDataIns != null)
        {
            playerSkillDataIns.Skill2_Ready(owner);
        }
    }
}
