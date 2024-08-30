using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearPoint : MonoBehaviour
{
    [Tooltip("Knight 할당")]
    [SerializeField] KnightThrowSkill skill;

    /// <summary>
    /// 창에 IDamageable이 Trigger됐을 때의 메소드
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!skill.isThrowing)
        {   // 스킬이 사용되지 않은 경우
            return;
        }

        if(skill.targetCollision != null)
        {   // 누구 하나가 잡힌 경우
            return;
        }

        if (collision.gameObject.Equals(skill.gameObject))
        {   // 내가 잡힌 경우
            return;
        }
        PlayerController targetPlayer = collision.GetComponent<PlayerController>();
        if (targetPlayer != null)
        {
            if (targetPlayer.photonView.Owner.IsTeammate())
            {
                return;
            }
        }
        if (!skill.damageableMask.Contain(collision.gameObject.layer))
        {   // 데미지를 받는 경우가 아닌 경우
            return;
        }

        if (collision != null)
        {
            skill.targetCollision = collision.transform;
            skill.isGrapped = true;

            if (!skill.CheckOwner())
            {
                return;
            }
            IDamageable damagable = collision.gameObject.GetComponent<IDamageable>();
            damagable.TakeDamage(skill.damage);
            IStateable statable = collision.gameObject.GetComponent<IStateable>();
            if (statable != null)
            {
                statable.StateChange(PlayerState.Knockback, 0.3f, 0.5f, true, false);
            }
        }
    }
}
