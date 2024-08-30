using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 서보운
/// <br/>공용스킬 선택UI
/// </summary>
public class CommonSkillSelectUI : MonoBehaviour
{
    [SerializeField] Button dashSkillButton;
    [SerializeField] Button healSkillButton;
    [SerializeField] Button pendantSkillButton;
    [SerializeField] Button ambushSkillButton;

    [SerializeField] Button checkButton;

    List<CommonSkill> commonSkills;

    private void Awake()
    {
        commonSkills = new List<CommonSkill>();

        dashSkillButton.onClick.AddListener(SetDashSkill);
        healSkillButton.onClick.AddListener(SetHealSkill);
        pendantSkillButton.onClick.AddListener(SetPendantSkill);
        ambushSkillButton.onClick.AddListener(SetAmbushSkill);

        checkButton.onClick.AddListener(SetSkillComplete);

        SetDashSkill();
        SetHealSkill();
        SetSkillComplete();
    }

    /// <summary>
    /// 공용 스킬 선택 완료 메소드
    /// </summary>
    private void SetSkillComplete()
    {
        if(commonSkills.Count != 2)
        {
            return;
        }

        PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1, commonSkills[0]);
        PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2, commonSkills[1]);
        this.gameObject.SetActive(false);
    }

    private void SetDashSkill()
    {
        AddCommonSkill(CommonSkill.Dash);
    }

    private void SetHealSkill()
    {
        AddCommonSkill(CommonSkill.Heal);
    }

    private void SetPendantSkill()
    {
        AddCommonSkill(CommonSkill.Pendant);
    }

    private void SetAmbushSkill()
    {
        AddCommonSkill(CommonSkill.Ambush);
    }

    private void AddCommonSkill(CommonSkill curSkill)
    {
        if(commonSkills.Count == 2)
        {   // 2개의 스킬을 선택했을 때
            Debug.Log("전부 선택 완료");
            return;
        }

        if(commonSkills.Contains(curSkill))
        {
            Debug.Log("중복된 스킬");
            return;
        }

        commonSkills.Add(curSkill);
    }
}
