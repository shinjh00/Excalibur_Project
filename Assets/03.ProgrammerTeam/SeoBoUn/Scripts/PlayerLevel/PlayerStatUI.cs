using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 서보운
/// <br/> 임시 플레이어 스탯 표현용 UI
/// </summary>
public class PlayerStatUI : MonoBehaviour
{
    [Tooltip("Hp 숫자 텍스트 (에디터 할당 필요)")]
    [SerializeField] TMP_Text hpText;
    [Tooltip("MaxHp 숫자 텍스트 (에디터 할당 필요")]
    [SerializeField] TMP_Text maxHpText;    
    [Tooltip("Atk 숫자 텍스트 (에디터 할당 필요)")]
    [SerializeField] TMP_Text atkText;
    [Tooltip("AtkSpd 숫자 텍스트 (에디터 할당 필요)")]
    [SerializeField] TMP_Text atkSpdText;
    [Tooltip("Def 숫자 텍스트 (에디터 할당 필요)")]
    [SerializeField] TMP_Text defText;
    [Tooltip("Spd 숫자 텍스트 (에디터 할당 필요)")]
    [SerializeField] TMP_Text spdText;
    [Tooltip("RemainPoint 숫자 텍스트 (에디터 할당 필요)")]
    [SerializeField] TMP_Text remainPointText;
    [Tooltip("레벨 텍스트(에디터 할당 필요")]
    [SerializeField] TMP_Text levelText;

    [Tooltip("Hp 능력치업 버튼 (에디터 할당 필요)")]
    [SerializeField] Button hpUpButton;
    [Tooltip("Atk 능력치업 버튼 (에디터 할당 필요)")]
    [SerializeField] Button atkUpButton;
    [Tooltip("AtkSpd 능력치업 버튼 (에디터 할당 필요)")]
    [SerializeField] Button atkSpdUpButton;
    [Tooltip("Def 능력치업 버튼 (에디터 할당 필요)")]
    [SerializeField] Button defUpButton;

    public Func<PlayerStat> PointUp;
    public Action StatUpEvent;

    private void Awake()
    {
        hpUpButton.onClick.AddListener(LevelUpHp);
        atkUpButton.onClick.AddListener(LevelUpAtk);
        atkSpdUpButton.onClick.AddListener(LevelUpAtkSpd);
        defUpButton.onClick.AddListener(LevelUpDef);
    }

    /// <summary>
    /// UI변경용 메소드.
    /// </summary>
    /// <param name="curData"></param>
    /// <param name="remainPoint"></param>
    public void ChangeUI(LevelData curData, int remainPoint)
    {
        maxHpText.text = $"{curData.maxHp}";
        atkText.text = $"{curData.atk}";
        atkSpdText.text = $"{curData.atkSpeed:F2}";
        defText.text = $"{curData.def}";
        spdText.text = $"{curData.moveSpeed}";
        remainPointText.text = $"{remainPoint}";
    }

    public void ChangeHpText(float hp)
    {
        hpText.text = $"{hp:F0}";
    }

    public void ChangeLevel(int level)
    {
        levelText.text = $"LV. {level}";
    }

    /// <summary>
    /// 버튼이 눌렸을 때 호출될 메소드
    /// 포인트를 소모해서 능력치를 업그레이드하는 메소드들
    /// </summary>
    private void LevelUpHp()
    {
        PointUp += (() => PlayerStat.Hp);
        StatUpEvent?.Invoke();
    }

    private void LevelUpAtk()
    {
        PointUp += (() => PlayerStat.AtkDamage);
        StatUpEvent?.Invoke();
    }

    private void LevelUpAtkSpd()
    {
        PointUp += (() => PlayerStat.AtkSpeed);
        StatUpEvent?.Invoke();
    }

    private void LevelUpDef()
    {
        PointUp += (() => PlayerStat.Defense);
        StatUpEvent?.Invoke();
    }
}
