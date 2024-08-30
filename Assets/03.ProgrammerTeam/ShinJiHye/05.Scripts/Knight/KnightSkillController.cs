using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 신지혜 / 기사 스킬 컨트롤러
/// </summary>
public class KnightSkillController : PlayerSkillController
{
    [PunRPC]
    public override void GeneralSkill1()
    {
        if (owner.StateController.CurState.Contain(PlayerState.Dead))
        {   // 사망 시 사용 불가능
            return;
        }
        
        if (!owner.StatController.UnLockSkills[(int)PlayerStatController.SkillButton.Q])
        {
            return;
        }
        
        if (playerSkillDataIns != null)
        {
            playerSkillDataIns.Skill1_Ready(owner);
        }
    }

    [PunRPC]
    public override void GeneralSkill2()
    {
        if (owner.StateController.CurState.Contain(PlayerState.Dead))
        {   // 사망 시 사용 불가능
            return;
        }
        
        if (!owner.StatController.UnLockSkills[(int)PlayerStatController.SkillButton.E])
        {
            return;
        }
        
        if (playerSkillDataIns != null)
        {
            playerSkillDataIns.Skill2_Ready(owner);
        }
    }

    [PunRPC]
    public override void SuperSkill()
    {
        if (owner.StateController.CurState.Contain(PlayerState.Dead))
        {   // 사망 시 사용 불가능
            return;
        }
        
        if (!owner.StatController.UnLockSkills[(int)PlayerStatController.SkillButton.R])
        {
            return;
        }
        
        if (playerSkillDataIns != null)
        {
            playerSkillDataIns.Ult_Ready(owner);
        }
    }
}
