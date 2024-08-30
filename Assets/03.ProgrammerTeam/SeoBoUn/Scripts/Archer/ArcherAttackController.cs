using Photon.Pun;
using System.Collections;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 아쳐용 어택 컨트롤러
/// </summary>
public class ArcherAttackController : PlayerAttackController
{
    /// <summary>
    /// 아쳐용 공격. 아쳐는 공격 범위가 사각형 박스
    /// 외부에서 실행할 애니메이션 이벤트
    /// </summary>

    RaycastHit2D[] hitInfos;
    Vector2 normalVec;
    private void Awake()
    {
        hitInfos = new RaycastHit2D[20];
    }

    protected override IEnumerator Start()
    {
        StartCoroutine(base.Start());
        yield return null;

        owner.StateController.HitEvent += SetAttackExit;
    }

    /// <summary>
    /// 아쳐는 공격할 때 직선으로 공격 -> 마우스를 바라보는 레이 방식으로 변환
    /// </summary>
    public override void CheckDamgeable()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        normalVec = (mousePos - (Vector2)transform.position).normalized;
        damageableList.Clear();
        stateableList.Clear();
        int raycastSize = Physics2D.RaycastNonAlloc(transform.position, normalVec, hitInfos, atkRange, damageableMask);

        for (int i = 0; i < raycastSize; i++)
        {
            if (hitInfos[i].collider.gameObject.Equals(owner.gameObject))
            {
                continue;
            }
            PlayerController targetPlayer = hitInfos[i].collider.GetComponent<PlayerController>();
            if (targetPlayer != null)
            {
                if (targetPlayer.photonView.Owner.IsTeammate())
                {
                    continue;
                }
            }
            IDamageable damagable = hitInfos[i].collider.GetComponent<IDamageable>();
            IStateable stateable = hitInfos[i].collider.GetComponent<IStateable>();
            
            if (damagable != null)
            {
                damageableList.Add(damagable);
            }
            if(stateable != null)
            {
                stateableList.Add(stateable);
            }
        }
        InflictDamage(damageableList, atkDamage);
        KnockBack();
    }

    protected override IEnumerator AttackRoutine()
    {
        //isAttack = true;
        owner.StateController.StateChange(PlayerState.AttackStart, 0, 0, true, false);

        owner.photonView.RPC("PlaySound", RpcTarget.All, skillInfo.skEffect1_Data.mainSound);
        yield return null;
    }

    /// <summary>
    /// 아쳐용 공격 끝.
    /// 외부에서 실행할 애니메이션 이벤트
    /// </summary>
    public void SetAttackExit()
    {
        //isAttack = false;
        owner.StateController.StateChange(PlayerState.AttackExit, 0, 0, true, false);
    }

}
