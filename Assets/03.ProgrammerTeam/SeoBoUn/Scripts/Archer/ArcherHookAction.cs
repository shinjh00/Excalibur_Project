using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아쳐의 후크 액션(갈고리 중 무언가 부딫히면 데미지)
/// </summary>
public class ArcherHookAction : MonoBehaviour
{
    [SerializeField] PlayerController controller;
    [SerializeField] HookSkill skill;
    [SerializeField] LayerMask targetLayer;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] Collider2D hookCollider;

    /// <summary>
    /// 갈고리 시작
    /// </summary>
    public void StartHookAction(HookSkill skill)
    {
        this.skill = skill;
        hookCollider.enabled = true;
    }

    public void EndHookAction()
    {
        skill.IsCollider = true;
        hookCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!hookCollider.enabled)
        {
            return;
        }

        if(!controller.photonView.IsMine)
        {
            return;
        }

        if(targetLayer.Contain(collision.gameObject.layer))
        {
            if(collision.gameObject.Equals(this.gameObject))
            {
                return;
            }
            PlayerController targetPlayer = collision.GetComponent<PlayerController>();
            if (targetPlayer != null)
            {
                if (targetPlayer.photonView.Owner.IsTeammate())
                {
                    return ;
                }
            }
            EndHookAction();
            IDamageable damagable = collision.GetComponent<IDamageable>();
            IStateable stateable = collision.GetComponent<IStateable>();
            if (stateable != null)
            {
                Debug.Log(stateable);
                stateable.StateChange(PlayerState.Knockback, skill.InvTime, skill.KbTime + 0.2f, true, false);
            }
            if (damagable != null)
            {
                controller.SetAnimator("Roll");
                damagable.TakeKnockBack(transform.position, controller.PlayerClassData.knockBackDistance * 2, controller.PlayerClassData.knockBackSpeed);
                damagable.TakeDamage(controller.AttackController.AtkDamage * 1.8f);
                StartCoroutine(HookKnockBack(collision.transform.position));
            }
        }
    }

    private IEnumerator HookKnockBack(Vector3 dirPos)
    {
        Vector2 startPos = transform.position;
        Vector2 knockBackDir = (startPos - (Vector2)dirPos).normalized;

        // 너무 가까우면(0.5) 넉백을 실행하지 않음

        if (Physics2D.Raycast(startPos, knockBackDir, 1f, wallLayer))
        {
            yield break; 
        }

        Vector2 targetPos = startPos + knockBackDir * controller.PlayerClassData.knockBackDistance * 2;  // 넉백 거리에 따른 이동 위치

        RaycastHit2D hit = Physics2D.Raycast(startPos, knockBackDir, controller.PlayerClassData.knockBackDistance * 2, wallLayer);

        if(hit)
        {
            targetPos = hit.point;
        }

        float rate = 0;
        float time = Vector2.Distance(startPos, targetPos) / controller.PlayerClassData.knockBackSpeed;

        while(rate < 1f)
        {
            rate += Time.deltaTime / time;
            if (rate > 1f)
            {
                rate = 1f;
            }
            controller.transform.position = Vector2.Lerp(startPos, targetPos, rate);
            yield return null;
        }


        hookCollider.enabled = false;
    }
}
