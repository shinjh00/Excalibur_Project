using System;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필 / 플레이어의 상태를 구분하기 위한 열거값
/// </summary>
[Flags]
public enum PlayerState
{
    Idle = 1 << 0,
    Slow = 1 << 1,
    Sleep = 1 << 2,
    Move = 1 << 3,
    AttackStart = 1 << 4,
    OnAttack = 1 << 5,
    AttackExit = 1 << 6,
    SkillStart = 1 << 7,
    OnSkill = 1 << 8,
    SkillExit = 1 << 9,
    Silence = 1 << 10,
    Interact = 1 << 11,
    Puzzle = 1 << 12,
    UnAttackable = 1 << 13,
    Dead = 1 << 14,
    Knockback = 1 << 15,
    invincible = 1 << 16,
    Hit = 1 << 17,
    Metamorph = 1<< 18,
    Maintain = 1 << 19,
    Lobby = 1 << 20,
    SuperArmor = 1 << 21,
    Chat = 1 << 22,
    Groggy = 1 <<23
}
public enum DamageType
{
    Normal,
    Fixed
}
/// <summary>
/// 피격 인터페이스
/// </summary>
public interface IDamageable
{

    /// <summary>
    /// 데미지 받는 메소드
    /// </summary>
    /// <param name="damage"> 피해량 </param>
    public void TakeDamage(float damage,DamageType dmgType = DamageType.Normal);

    /// <summary>
    /// 넉백 메소드
    /// </summary>
    /// <param name="attackPos"> 공격 위치 </param>
    /// <param name="knockBackDistance"> 넉백 거리 </param>
    /// <param name="knockBackSpeed"> 넉백 속도 </param>
    public void TakeKnockBack(Vector3 attackPos, float knockBackDistance, float knockBackSpeed);
    /// <summary>
    /// 스킬에 의한 넉백 메소드
    /// </summary>
    /// <param name="mouseDir">스킬 목표 위치</param>
    /// <param name="attackPos">발사체 좌표</param>
    /// <param name="knockBackDistance">넉백 거리</param>
    /// <param name="knockBackSpeed">넉백 속도</param>
    public void TakeKnockBackFromSkill(Vector3 mouseDir, Vector3 attackPos, float knockBackDistance, float knockBackSpeed);

}

public interface IStateable
{
    public void StateChange(PlayerState state, object param, float time, bool add, bool fixThisState);
}
