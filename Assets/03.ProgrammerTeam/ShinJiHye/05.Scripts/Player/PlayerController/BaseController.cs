using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 베이스 컨트롤러. PlayerController는 다음 클래스를 상속받아야 함.
/// </summary>
public class BaseController : MonoBehaviourPun
{
    [SerializeField] protected PlayerController owner;

    /// <summary>
    /// 컨트롤러 설정 및 스탯 설정
    /// </summary>
    /// <param name="owner"> 플레이어 종합 컨트롤러 </param>
    public void SetOwner(PlayerController owner)
    {
        this.owner = owner;
        GetStat();
    }

    /// <summary>
    /// owner에 있는 플레이어 데이터 참조용 메소드
    /// virtual 함수를 구현해야 함.
    /// </summary>
    protected virtual void GetStat()
    {
        Debug.Log("자식에서 해당 내용을 구현하지 않음.");
    }
}

/// <summary>
/// 플레이어 스탯에 대한 열거형
/// <br/>Hp, MaxHp, Defense, AtkDamage, AtkSpeed, AtkRange, AtkAngle, MoveSpeed, Exp
/// </summary>
public enum PlayerStat
{
    Hp,
    MaxHp,
    Defense,
    AtkDamage,
    AtkSpeed,
    AtkRange,
    AtkAngle,
    MoveSpeed,
    Exp
}