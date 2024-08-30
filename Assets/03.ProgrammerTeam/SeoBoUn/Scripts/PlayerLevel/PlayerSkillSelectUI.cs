using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 서보운
/// <br/> 스킬 선택용 UI
/// </summary>
public class PlayerSkillSelectUI : MonoBehaviour
{
    [SerializeField] Button QSkillButton;
    [SerializeField] Button ESkillButton;
    [SerializeField] Button RSkillButton;

    [SerializeField] TMP_Text remainSkillPoint;

    public Func<PlayerStatController.SkillButton> PointUp;
    public Action UnLockSkillEvent;

    private void Awake()
    {
        QSkillButton.onClick.AddListener(UnLockQSkill);
        ESkillButton.onClick.AddListener(UnLockESkill);
        RSkillButton.onClick.AddListener(UnLockRSkill);
    }

    private void UnLockQSkill()
    {
        PointUp += (() => PlayerStatController.SkillButton.Q);
        UnLockSkillEvent?.Invoke();
    }

    private void UnLockESkill()
    {
        PointUp += (() => PlayerStatController.SkillButton.E);
        UnLockSkillEvent?.Invoke();
    }

    private void UnLockRSkill()
    {
        PointUp += (() => PlayerStatController.SkillButton.R);
        UnLockSkillEvent?.Invoke();
    }

    public void OnChangePoint(int remainPoint)
    {
        remainSkillPoint.text = $"{remainPoint}";
    }

    /// <summary>
    /// 특정 스킬이 해금되었을 때 그 버튼은 더이상 눌리지 못하도록 변경
    /// </summary>
    /// <param name="targetButton"></param>
    public void OnChangeUnLock(PlayerStatController.SkillButton targetButton, int remainPoint)
    {
        switch(targetButton)
        {
            case PlayerStatController.SkillButton.Q:
                QSkillButton.interactable = false;
                break;
            case PlayerStatController.SkillButton.E:
                ESkillButton.interactable = false;
                break;
            case PlayerStatController.SkillButton.R:
                RSkillButton.interactable = false;
                break;
        }

        remainSkillPoint.text = $"{remainPoint}";
    }
}
