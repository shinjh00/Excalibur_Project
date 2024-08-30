using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 기사 고유 스킬3 견뎌
/// <br/> 토글형 스킬(켜고 끌 수 있음). 딜레이 존재
/// <br/> 방어력이 증가(1.5배)하고, 공격력이 감소(0.5배)하며 넉백 저항이 생김
/// </summary>
public class KnightEndureSkill : MonoBehaviour
{
    bool isUse; // 사용 여부

    public bool IsUse { get { return isUse; } set { isUse = value; } }

    /// <summary>
    /// 견뎌 스킬 사용
    /// </summary>
    public void UsingEndureSkill(PlayerController player)
    {
        IsUse = true;
        // 1. 넉백 저항 켜기
        player.HealthController.photonView.RPC("SetKnockbackResist", Photon.Pun.RpcTarget.All, true);
        // 2. 공격력은 0.5배, 방어력은 1.5배 설정
        player.StatController.AddBuffStat(StatLevel.Atk, 0.5f);
        player.StatController.AddBuffStat(StatLevel.Def, 1.5f);
    }

    /// <summary>
    /// 견뎌 스킬 사용 해제
    /// </summary>
    public void DisableEndureSkill(PlayerController player)
    {
        IsUse = false;
        // 1. 넉백 저항 끄기
        player.HealthController.photonView.RPC("SetKnockbackResist", Photon.Pun.RpcTarget.All, false);
        // 2. 공격력 디버프, 방어 버프 해제
        player.StatController.RemoveBuffStat(StatLevel.Atk, 0.5f);
        player.StatController.RemoveBuffStat(StatLevel.Def, 1.5f);
    }
}
