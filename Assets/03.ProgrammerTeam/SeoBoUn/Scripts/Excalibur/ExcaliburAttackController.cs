using Photon.Pun;
using System.Collections;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// <br/> 엑스칼리버 캐릭터 전용 공격 스크립트
/// </summary>
public class ExcaliburAttackController : PlayerAttackController
{
    // 공격 선딜레이(기본 1초)
    [SerializeField] float foreDelay;
    // 공격 후딜레이(기본 1초)
    [SerializeField] float afterDelay;

    [Tooltip("어택 포인트용 렌더러(에디터 할당 필요)")]
    [SerializeField] SpriteRenderer attackRangeSprite;

    protected override IEnumerator Start()
    {
        StartCoroutine(base.Start());
        yield return null;
        attackRangeSprite.gameObject.SetActive(false);

        skillInfo = CsvParser.Instance.SkillDic[1002021];
        skillInfo.ReadEffectData();
    }

    /// <summary>
    /// 3 * 4의 반원을 그려야 함.
    /// 선딜 1초, 후딜 1초 공격 범위도 체크해야 함(계수는 1.0)
    /// </summary>
    public override void CheckDamgeable()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        mouseDir = attackPoint.MouseDir;
        damageableList.Clear();
        stateableList.Clear();

        int overlapCount = Physics2D.OverlapCircleNonAlloc(transform.position, atkRange, damageableColliders, damageableMask);
        for (int i = 0; i < overlapCount; i++)
        {
            if (damageableColliders[i].gameObject.Equals(gameObject))
            {
                continue;
            }

            Vector2 dirToTarget = (damageableColliders[i].transform.position - transform.position).normalized;

            if (Vector3.Dot(mouseDir.normalized, dirToTarget) < (cosAtkRange))
            {
                continue;
            }
            PlayerController targetPlayer = damageableColliders[i].GetComponent<PlayerController>();
            if (targetPlayer != null)
            {
                if (targetPlayer.photonView.Owner.IsTeammate())
                {
                    continue;
                }
            }
            IDamageable damagable = damageableColliders[i].GetComponent<IDamageable>();
            IStateable stateable = damageableColliders[i].GetComponent<IStateable>();

            if (damagable != null)
            {
                damageableList.Add(damagable);
            }
            if (stateable != null)
            {
                stateableList.Add(stateable);
            }

        }
    }

    protected override IEnumerator AttackRoutine()
    {
        // 공격 시작 시 1초 선딜레이 -> 선딜레이 동안에 붉은색 반원으로 공격 범위 표시 및 이동 불가능
        owner.StatController.AddBuffStat(StatLevel.MoveSpeed, 0.001f);
        owner.StateController.StateChange(PlayerState.AttackStart, 0, 0, true, false);
        
        attackRangeSprite.gameObject.SetActive(true);
        yield return new WaitForSeconds(foreDelay);
        owner.photonView.RPC("PlaySound", RpcTarget.All, skillInfo.skEffect1_Data.mainSound);
        CheckDamgeable();

        InflictDamage(damageableList, atkDamage);
        KnockBack();
        attackRangeSprite.gameObject.SetActive(false);

        yield return new WaitForSeconds(afterDelay);

        owner.StatController.RemoveBuffStat(StatLevel.MoveSpeed, 0.001f);
        owner.StateController.StateChange(PlayerState.AttackExit, 0, 0, true, false);
    }
}
