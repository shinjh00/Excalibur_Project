using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 서보운
/// <br/> 공용스킬 선택버튼에 대한 스크립트
/// </summary>
public class CommonSkillButtons : MonoBehaviour
{
    [Tooltip("대쉬 버튼 에디터 할당 필요")]
    [SerializeField] Button dashSkillButton;
    [Tooltip("힐 버튼 에디터 할당 필요")]
    [SerializeField] Button healSkillButton;
    [Tooltip("팬던트 버튼 에디터 할당 필요")]
    [SerializeField] Button pendantSkillButton;
    [Tooltip("매복 버튼 에디터 할당 필요")]
    [SerializeField] Button ambushSkillButton;

    [Tooltip("몇 번째 공용 스킬을 선택하는지(1, 2)")]
    [SerializeField] int setCommon;

    [Tooltip("선택한 스킬의 텍스트(에디터 할당 필요)")]
    [SerializeField] TMP_Text selectSkillText;
    [Tooltip("변경할 스킬의 텍스트(에디터 할당 필요)")]
    [SerializeField] TMP_Text otherSkillText;

    private void Awake()
    {
        dashSkillButton.onClick.AddListener(SetDashSkill);
        healSkillButton.onClick.AddListener(SetHealSkill);
        pendantSkillButton.onClick.AddListener(SetPendantSkill);
        ambushSkillButton.onClick.AddListener(SetAmbushSkill);
    }

    private void SetDashSkill()
    {
        switch(setCommon)
        {
            case 1: // 1번 스킬 설정
                if(PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2) == CommonSkill.Dash)
                {   // 2가 대쉬였으면 2를 1의 공용스킬로 변경
                    CommonSkill targetSkill = PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1);

                    PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2, targetSkill);
                    SetSkillText(targetSkill);
                }
                PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1, CommonSkill.Dash);
                break;
            case 2:
                if (PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1) == CommonSkill.Dash)
                {
                    CommonSkill targetSkill = PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2);

                    PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1, targetSkill);
                    SetSkillText(targetSkill);
                }
                PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2, CommonSkill.Dash);
                break;
        }

        selectSkillText.text = "Dash";
        gameObject.SetActive(false);
    }

    private void SetHealSkill()
    {
        switch (setCommon)
        {
            case 1:
                if (PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2) == CommonSkill.Heal)
                {
                    CommonSkill targetSkill = PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1);

                    PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2, targetSkill);
                    SetSkillText(targetSkill);
                }
                PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1, CommonSkill.Heal);
                break;
            case 2:
                if (PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1) == CommonSkill.Heal)
                {
                    CommonSkill targetSkill = PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2);

                    PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1, targetSkill);
                    SetSkillText(targetSkill);
                }
                PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2, CommonSkill.Heal);
                break;
        }

        selectSkillText.text = "Heal";
        gameObject.SetActive(false);
    }

    private void SetPendantSkill()
    {
        switch (setCommon)
        {
            case 1:
                if (PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2) == CommonSkill.Pendant)
                {
                    CommonSkill targetSkill = PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1);

                    PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2, targetSkill);
                    SetSkillText(targetSkill);
                }
                PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1, CommonSkill.Pendant);
                break;
            case 2:
                if (PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1) == CommonSkill.Pendant)
                {
                    CommonSkill targetSkill = PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2);

                    PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1, targetSkill);
                    SetSkillText(targetSkill);
                }
                PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2, CommonSkill.Pendant);
                break;
        }

        selectSkillText.text = "Pendant";
        gameObject.SetActive(false);
    }

    private void SetAmbushSkill()
    {
        switch (setCommon)
        {
            case 1:
                if (PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2) == CommonSkill.Ambush)
                {
                    CommonSkill targetSkill = PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1);

                    PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2, targetSkill);
                    SetSkillText(targetSkill);
                }
                PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1, CommonSkill.Ambush);
                break;
            case 2:
                if (PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1) == CommonSkill.Ambush)
                {
                    CommonSkill targetSkill = PhotonNetwork.LocalPlayer.GetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2);

                    PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1, targetSkill);
                    SetSkillText(targetSkill);
                }
                PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2, CommonSkill.Ambush);
                break;
        }

        selectSkillText.text = "Ambush";
        gameObject.SetActive(false);
    }

    public void SetSkillText(CommonSkill curSkill)
    {
        switch(curSkill)
        {
            case CommonSkill.Dash:
                otherSkillText.text = "Dash";
                break;
            case CommonSkill.Pendant:
                otherSkillText.text = "Pendant";
                break;
            case CommonSkill.Heal:
                otherSkillText.text = "Heal";
                break;
            case CommonSkill.Ambush:
                otherSkillText.text = "Ambush";
                break;
        }
    }
}
