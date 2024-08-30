using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

/// <summary>
/// 개발자 : 신지혜 / 기사 클래스 기본 공격 컨트롤러
/// </summary>
public class KnightAttackController : PlayerAttackController
{
    [Tooltip("Spear 할당")]
    [SerializeField] Spear spear;

    [Space(5)]
    [Header("Growth Rate")]
    [Tooltip("공격력 성장수치")]
    [SerializeField] float atkDamageRate;
    [Tooltip("공격속도 성장수치")]
    [SerializeField] float atkSpeedRate;
    [Tooltip("체력 성장수치")]
    [SerializeField] float hpRate;
    [Tooltip("방어력 성장수치")]
    [SerializeField] float defenceRate;
    [Tooltip("이동속도 성장수치")]
    [SerializeField] float moveSpeedRate;

    [Space(5)]
    [Header("Attack Datas")]
    [Tooltip("기본 공격 계수")]
    [SerializeField] float atkCoefficient;
    [Tooltip("히트박스 크기 (가로)")]
    [SerializeField] float hitboxWidth;
    [Tooltip("히트박스 크기 (세로)")]
    [SerializeField] float hitboxHeight;
    [Tooltip("넉백 거리")]
    [SerializeField] float knockBackCoefficient;

    Vector3 hitboxCenter;           // 히트박스 중앙
    Vector3 hitboxSize;             // 히트박스 크기

    AudioSource knightAudioSource;

    public Spear Spear { get { return spear; } set { spear = value; } }
    public AudioSource KnightAudioSource { get { return knightAudioSource; } set { knightAudioSource = value; } }

    protected override void GetStat()
    {
        base.GetStat();
        atkDamage = owner.PlayerClassData.atkDamage * atkCoefficient;
        knockBackDistance = owner.PlayerClassData.knockBackDistance * 0.5f;
    }

    protected override IEnumerator Start()
    {
        StartCoroutine(base.Start());
        yield return null;
        if(photonView.IsMine)
        {
            owner.PlayerBgEffectAnim.gameObject.GetComponent<SpriteRenderer>().material = ownerMaterial;
        }
        knightAudioSource = owner.audioSource;
    }

    /// <summary>
    /// 기사 - 공격 가능 객체 체크
    /// 사각형 히트박스 (3*2)
    /// </summary>
    public override void CheckDamgeable()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        damageableList.Clear();
        stateableList.Clear();
        SetHitbox();
        // 히트박스 중심점, 크기, 회전을 기준으로 OverlapBox
        int overlapCount = Physics2D.OverlapBoxNonAlloc(hitboxCenter, hitboxSize, 0, damageableColliders, damageableMask);

        for (int i = 0; i < overlapCount; i++)
        {
            if (damageableColliders[i].gameObject.Equals(gameObject))
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
            IDamageable damageable = damageableColliders[i].GetComponent<IDamageable>();
            IStateable stateable = damageableColliders[i].GetComponent<IStateable>();

            if (damageable != null)
            {
                damageableList.Add(damageable);
            }
            if (stateable != null)
            {
                stateableList.Add(stateable);
            }
        }
    }

    /// <summary>
    /// 기사 - 공격 루틴
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator AttackRoutine()
    {
        //isAttack = true;
        if(owner != null)
        owner.StateController.StateChange(PlayerState.AttackStart, 0, 0, true, false);

        //spear.SpearAnim.SetTrigger("Attack");

        CheckDamgeable();

        // 기사 스킬 지속 시간
        yield return new WaitForSeconds(0.5f);

        owner.photonView.RPC("PlaySound", RpcTarget.All, skillInfo.skEffect1_Data.mainSound);
        InflictDamage(damageableList, atkDamage);

        KnockBack();

        owner.StateController.StateChange(PlayerState.AttackExit, 0, 0, true, false);
        //isAttack = false;
    }

    /// <summary>
    /// 히트박스 수치 설정
    /// </summary>
    private void SetHitbox()
    {
        hitboxSize = new Vector3(hitboxWidth, hitboxHeight, 0);  // 히트박스 크기
        hitboxCenter = spear.SpearPoint.transform.position;  // 히트박스 중심점
    }

    /// <summary>
    /// 히트박스 Gizmos를 그리는 메소드
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (spear == null || spear.SpearPoint == null)
        {
            return;
        }

        // Gizmos 색상 설정
        Gizmos.color = Color.red;

        // 히트박스를 나타내는 사각형 그리기
        Gizmos.DrawCube(hitboxCenter, hitboxSize);
    }


    #region hitboxCorners
    /*private void Awake()
    {
        hitboxCorners = new Vector3[4];  // 히트박스 코너 4개
    }

    /// <summary>
    /// 히트박스 코너 위치 계산 메소드
    /// </summary>
    public void CalcHitboxCorners()
    {
        // 히트박스의 반 크기
        Vector3 halfSize = new Vector3(hitboxSize.x / 2f, hitboxSize.y / 2f, 0);

        // 회전 행렬 계산
        Quaternion rotation = Quaternion.Euler(0, 0, hitboxAngle);

        // 히트박스의 각 코너 위치 계산
        hitboxCorners[0] = hitboxCenter + rotation * new Vector3(halfSize.x, halfSize.y, 0);   // Top Right
        hitboxCorners[1] = hitboxCenter + rotation * new Vector3(halfSize.x, -halfSize.y, 0);   // Bottom Right
        hitboxCorners[2] = hitboxCenter + rotation * new Vector3(-halfSize.x, -halfSize.y, 0);   // Bottom Left
        hitboxCorners[3] = hitboxCenter + rotation * new Vector3(-halfSize.x, halfSize.y, 0);   // Top Left
    }

    private void SetHitbox()
    {
        hitboxAngle = Mathf.Atan2(attackPoint.MouseDir.y, attackPoint.MouseDir.x) * Mathf.Rad2Deg;  // 방향 벡터에서 회전 각도 계산
        CalcHitboxCorners();
    }

    private void OnDrawGizmosSelected()
    {
        if (hitboxCorners == null || hitboxCorners.Length != 4)
        {
            return;
        }

        for (int i = 0; i < hitboxCorners.Length; i++)
        {
            Gizmos.DrawLine(hitboxCorners[i], hitboxCorners[(i + 1) % hitboxCorners.Length]);
        }
    }*/
    #endregion

}
