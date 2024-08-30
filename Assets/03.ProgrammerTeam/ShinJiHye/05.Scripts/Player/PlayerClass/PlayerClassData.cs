using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerClassData", menuName = "Data/PlayerClass")]
public class PlayerClassData : ScriptableObject
{
    [Header("Info")]

    [Tooltip("직업(클래스)")]
    public ClassType classType;

    [Tooltip("설명")]
    [TextArea(3, 5)]
    public string description;


    [Header("Stats")]

    [Tooltip("이동 속도")]
    public float moveSpeed;

    [Tooltip("공격력")]
    public float atkDamage;

    [Tooltip("공격 속도")]
    public float atkSpeed;

    [Tooltip("공격 범위 (플레이어로부터의 거리)")]
    public float atkRange;

    [Range(45, 90)]
    [Tooltip("공격 범위 (마우스 위치를 중심으로 양쪽으로 펼쳐진 부채꼴의 각도)")]
    public int atkAngle;

    [Tooltip("넉백 시킬 때의 넉백 거리")]
    public float knockBackDistance;

    [Tooltip("넉백 시킬 때의 넉백 속도")]
    public float knockBackSpeed;

    [Tooltip("최대 체력")]
    public float maxHp;

    [Tooltip("체력")]
    public float hp;

    [Tooltip("방어력")]
    public float defense;

    [Tooltip("민첩")]
    public int dex;


    [Header("Status")]

    [Tooltip("공격 유형 (근거리/원거리, 단타/광역)")]
    public AttackType attackType;

    [Tooltip("현재 레벨")]
    public int level;

    [Tooltip("경험치")]
    public float exp;

    [Tooltip("코인")]
    public int coin;

    [Tooltip("속한 팀 (팀전 시)")]
    public Team team;

    [Tooltip("해당 클래스의 전체 레벨")]
    public PlayerClassData[] ClassLvData;


    [Header("Value")]

    [Tooltip("플레이어의 콜라이더 반지름")]
    public float playerColRadius;
}

public enum ClassType { Warrior, Knight, Wizard, Archer, Excalibur }

[Flags]
public enum AttackType
{
    None = 0,
    Melee = 1,      // 근거리
    Ranged = 2,     // 원거리
    ST = 4,         // 단일 (Single Target Damage)
    AoE = 8         // 광역 (Area of Effect Damage)
}

public enum Team { Yellow, Red, Green, Blue }


