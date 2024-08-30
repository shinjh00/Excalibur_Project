using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 스킬 컨트롤러
/// </summary>
public class PlayerSkillController : BaseController,IPunObservable
{
    public List<SkillData> skillData;
    public SkillData playerSkillData;
    [Tooltip("공용 스킬 리스트(에디터 할당 필요)")]
    public List<CommonSkillData> commonSkillDatas;

    protected float[] commonSkillCoolTime;
    protected bool[] isCoolTime;
    public SkillUI skillUI;

    public SkillData playerSkillDataIns = null;
    // 실제 플레이어가 사용할 공용스킬 리스트
    [SerializeField] protected List<CommonSkillData> curCommonSkill;
    public List<CommonSkillData> CurCommonSkill {  get { return curCommonSkill; } }

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
            playerSkillDataIns.gameObject.SetActive(true);
            owner.StateController.HitEvent += playerSkillDataIns.Hit;
        }
        
        // 포톤 네트워크용 (프로퍼티로 플레이어 공용 스킬 설정)
        if(!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(DefinePropertyKey.COMMONSKILL1))
        {
            return;
        }
        
        curCommonSkill.Add(commonSkillDatas[(int)PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1)]);
        curCommonSkill.Add(commonSkillDatas[(int)PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2)]);

        commonSkillCoolTime = new float[curCommonSkill.Count];
        isCoolTime = new bool[curCommonSkill.Count];

        int index = 0;
        foreach (CommonSkillData curSkill in curCommonSkill)
        {   // 패시브 스킬은 미리 적용하기
            curSkill.SetData();

            if(curSkill.skillType == SkillType.Passive)
            {
                curSkill.Execute(owner);
            }
            commonSkillCoolTime[index] = curSkill.coolTime;
            isCoolTime[index++] = false;
        }
    }
    [PunRPC]
    public virtual void CommonSkill1()
    {
        if(owner.PlayerClassData.classType == ClassType.Excalibur)
        {   // 엑스칼리버 유저는 공용스킬 사용 불가능
            return;
        }

        if (owner.StateController.CurState.Contain(PlayerState.Dead))
        {   // 사망 시 사용 불가능
            return;
        }
        if (curCommonSkill == null || curCommonSkill.Count ==0|| curCommonSkill[0].skillType == SkillType.Passive)
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
    public virtual void CommonSkill2()
    {
        if (owner.PlayerClassData.classType == ClassType.Excalibur)
        {   // 엑스칼리버 유저는 공용스킬 사용 불가능
            return;
        }

        if (owner.StateController.CurState.Contain(PlayerState.Dead))
        {   // 사망 시 사용 불가능
            return;
        }
        if (curCommonSkill == null||curCommonSkill.Count ==0 || curCommonSkill[1].skillType == SkillType.Passive)
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
    [PunRPC]
    public virtual void GeneralSkill1()
    {
        if (owner.StateController.CurState.Contain(PlayerState.Dead))
        {   // 사망 시 사용 불가능
            return;
        }
        
        if (!owner.StatController.UnLockSkills[(int)PlayerStatController.SkillButton.Q] && !PhotonNetwork.LocalPlayer.GetProperty<bool>(DefinePropertyKey.OUTGAME))
        {
            return;
        }
        
        if (playerSkillDataIns != null)
        {
            playerSkillDataIns.Skill1_Ready(owner); ;
        }
    }
    [PunRPC]
    public virtual void GeneralSkill2()
    {
        if (owner.StateController.CurState.Contain(PlayerState.Dead))
        {   // 사망 시 사용 불가능
            return;
        }
        
        if (!owner.StatController.UnLockSkills[(int)PlayerStatController.SkillButton.E] && !PhotonNetwork.LocalPlayer.GetProperty<bool>(DefinePropertyKey.OUTGAME))
        {
            return;
        }
        
        if (playerSkillDataIns != null)
        {
            playerSkillDataIns.Skill2_Ready(owner);
        }
    }
    [PunRPC]
    public virtual void SuperSkill()
    {
        if (owner.StateController.CurState.Contain(PlayerState.Dead))
        {   // 사망 시 사용 불가능
            return;
        }
        
        if (!owner.StatController.UnLockSkills[(int)PlayerStatController.SkillButton.R] && !PhotonNetwork.LocalPlayer.GetProperty<bool>(DefinePropertyKey.OUTGAME))
        {
            return;
        }
        
        if (playerSkillDataIns != null)
        {
            playerSkillDataIns.Ult_Ready(owner);
        }
    }
    [PunRPC]
    public virtual void Skill1_Excute(object parameter)
    {
        if (playerSkillDataIns != null)
        {
            playerSkillDataIns.Skill1_Excute(owner, parameter);
        }
    }
    [PunRPC]
    public virtual void Skill2_Excute(object parameter)
    {
        if (playerSkillDataIns != null)
        {
            playerSkillDataIns.Skill2_Excute(parameter);
        }
    }
    [PunRPC]
    public virtual void Ult_Excute(object parameter)
    {
        if (playerSkillDataIns != null)
        {
            playerSkillDataIns.Ult_Excute(parameter);
        }
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isCoolTime);
        }
        else
        {
            isCoolTime = (bool[])stream.ReceiveNext();
        }
    }
}
