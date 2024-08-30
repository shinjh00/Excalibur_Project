using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// <br/> 공용스킬 데이터
/// <br/> 공용 스킬이 공통으로 가질 데이터를 가질 클래스
/// 기본은 쿨타임과 실행 가능(매개변수는 controller)기능 구현
/// </summary>
public abstract class CommonSkillData : MonoBehaviour
{
    [Tooltip("스킬의 쿨타임 입력(초)")]
    public float coolTime;
    [Tooltip("스킬의 사용 타입(Active : 직접 사용, Passive : 자동으로 사용)")]
    public SkillType skillType;

    [Tooltip("공용 스킬 데이터 정보 표시")]
    [SerializeField] protected SkillInfoData skillInfo;
    [SerializeField] public int id;

    public abstract void SetData();

    public abstract void Execute(PlayerController controller);
}

/// <summary>
/// 공용 스킬을 표현할 열거형
/// <br/> Dash, Pendant, Heal
/// </summary>
public enum CommonSkill
{
    Dash,
    Pendant,
    Heal,
    Ambush,
    Size
}

/// <summary>
/// 스킬의 타입(Active, Passive)
/// </summary>
public enum SkillType
{
    Active,
    Passive,
    Size
}