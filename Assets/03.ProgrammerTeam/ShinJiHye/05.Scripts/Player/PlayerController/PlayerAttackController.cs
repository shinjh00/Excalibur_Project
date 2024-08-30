using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static UnityEditor.Progress;

/// <summary>
/// 플레이어 공격 컨트롤러
/// </summary>
public class PlayerAttackController : BaseController, IAttackable
{
    [SerializeField] protected float atkDamage;             // 공격력
    [SerializeField] protected float atkSpeed;              // 공격 속도
    [SerializeField] protected float atkRange;              // 공격 범위 (거리)
    [SerializeField] protected int atkAngle;                // 공격 범위 (각도)
    [SerializeField] protected float knockBackDistance;     // 넉백 거리
    [SerializeField] protected float invincibleTime = 0.5f;        // 무적 시간
    [SerializeField] protected float knockBackTime = 0.3f;        // 넉백 시간(움직이지 못함)

    [SerializeField] protected SpriteRenderer render;
    [SerializeField] protected SpriteRenderer effectRender;
    [SerializeField] protected HPBarUI hpBarUI;
    [SerializeField] public Material ownerMaterial;
    [SerializeField] protected AttackPoint attackPoint;
    [SerializeField] protected Animator attackEffect;
    [SerializeField] protected Vector3 mouseDir;
    [SerializeField] protected SkillInfoData skillInfo;
    [SerializeField] protected float cosAtkRange;
    [SerializeField] protected bool isAttack;


    // 공격 가능 객체 콜라이더들을 담을 배열 미리 선언
    [SerializeField] protected Collider2D[] damageableColliders = new Collider2D[20];
    // 범위 안에 들어와서 공격 가능한 객체들
    protected List<IDamageable> damageableList = new List<IDamageable>();
    protected List<IStateable> stateableList = new List<IStateable>();

    [SerializeField] protected LayerMask damageableMask;


    #region Property
    public int AtkAngle { get { return atkAngle; } }
    public float AtkDamage { get { return atkDamage; } set { atkDamage = value; photonView.RPC("UpdateAttackStatSnyc", RpcTarget.All, PlayerStat.AtkDamage, value); } }
    public float AtkSpeed { get { return atkSpeed; } set { atkSpeed = value; photonView.RPC("UpdateAttackStatSnyc", RpcTarget.All, PlayerStat.AtkSpeed, value); } }
    public float AtkRange { get { return atkRange; } }
    public AttackPoint AttackPoint { get { return attackPoint; } }
    public Animator AtkEffect => attackEffect;
    //public bool IsAttack { get { return isAttack; } }
    public LayerMask DamageableMask { get { return damageableMask; } }
    public Collider2D[] DamageableColliders { get { return damageableColliders; } }
    public List<IDamageable> DamageableList { get { return damageableList; } }
    public SpriteRenderer Renderer => render;
    #endregion

    /// <summary>
    /// 개발자 : 서보운
    /// <br/> AttackController에 대한 동기화 메소드들
    /// </summary>
    /// <param name="stat"></param>
    /// <param name="value"></param>
    [PunRPC]
    protected void UpdateAttackStatSnyc(PlayerStat stat, float value)
    {
        switch (stat)
        {
            case PlayerStat.AtkDamage:
                atkDamage = (float)value;
                break;
            case PlayerStat.AtkSpeed:
                atkSpeed = (float)value;
                break;
        }
    }

    /// <summary>
    /// 스탯 받아오기. Owner에 있는 클래스에서 받아올 예정
    /// OnChangeHp : 파이어베이스 연동용 이벤트
    /// </summary>
    protected override void GetStat()
    {
        atkDamage = owner.PlayerClassData.atkDamage;
        atkSpeed = owner.PlayerClassData.atkSpeed;
        atkRange = owner.PlayerClassData.atkRange;
        atkAngle = owner.PlayerClassData.atkAngle;
        atkSpeed = owner.PlayerClassData.atkSpeed;
        knockBackDistance = owner.PlayerClassData.knockBackDistance;
      //  knockBackSpeed = owner.PlayerClassData.knockBackSpeed;

    }

    protected virtual IEnumerator Start()
    {
        yield return new WaitUntil(() => (owner != null) && (owner.isSetting));

        attackPoint = GetComponentInChildren<AttackPoint>();
        cosAtkRange = Mathf.Cos((atkAngle * 1f) * Mathf.Deg2Rad);  // 공격 가능 각도 내적
        //isAttack = false;
        if (photonView.IsMine)
        {
            MatOwnering();
        }
        int id = 0;

        switch(owner.PlayerClassData.classType)
        {
            case ClassType.Warrior:
                id = 1002005;
                break;
            case ClassType.Wizard:
                id = 1002009;
                break;
            case ClassType.Archer:
                id = 1002013;
                break;
            case ClassType.Knight:
                id = 1002001;
                break;
            case ClassType.Excalibur:
                id = 1002021;
                break;
        }

        skillInfo = CsvParser.Instance.SkillDic[id];
        skillInfo.ReadEffectData();
    }
    public void MatOwnering()
    {
        render.material = ownerMaterial;
        hpBarUI.SetImage(ownerMaterial);
    }
    protected virtual void Update()
    {
        if (PhotonNetwork.InRoom && photonView.IsMine)
        {
            photonView.RPC("Flip", RpcTarget.All, attackPoint.CheckFlip());
        }
    }

    public void Attack()
    {
        if (attackEffect != null)
        {
            attackEffect.gameObject.SetActive(true);
        }
        StartCoroutine(AttackRoutine());
    }

    /// <summary>
    /// 범위 내 공격 가능한 객체 체크
    /// </summary>
    public virtual void CheckDamgeable()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        Debug.Log("playerAttackController");
        mouseDir = attackPoint.MouseDir;
        damageableList.Clear();
        stateableList.Clear();
        int overlapCount = Physics2D.OverlapCircleNonAlloc(transform.position, atkRange, damageableColliders, damageableMask);
        Debug.Log(overlapCount);
        // 마우스 커서와 공격 가능한 객체 위치의 각도 계산하여 범위 안에 있는 공격 가능한 객체들만 damageableList에 담기
        for (int i = 0; i < overlapCount; i++)
        {
            // 자신이 체크되는 거 방지
            if (damageableColliders[i].gameObject.Equals(gameObject))
            {
                continue;
            }
            // (플레이어 -> 타겟)의 방향 벡터
            Vector2 dirToTarget = (damageableColliders[i].transform.position - transform.position).normalized;

            // 벡터 내적 계산
            if (Vector3.Dot(mouseDir.normalized, dirToTarget) < (cosAtkRange))
            {
                Debug.DrawLine(transform.position, dirToTarget);
                continue;
            }
            // 공격 가능한 객체들 리스트에 담기
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
                Debug.Log(damageable);
                damageableList.Add(damageable);
            }
            if (stateable != null)
            {
                Debug.Log(stateable);
                stateableList.Add(stateable);
            }
        }
    }

    /// <summary>
    /// 데미지 주기
    /// </summary>
    /// <param name="damageableList"> 데미지를 받는 객체 리스트 </param>
    /// <param name="atkDamage"> 공격력 </param>
    public void InflictDamage(List<IDamageable> damageableList, float atkDamage)
    {
        foreach (var damageable in damageableList)
        {
            damageable.TakeDamage(atkDamage);
        }
    }

    /// <summary>
    /// 넉백 효과 주기
    /// </summary>
    public void KnockBack()
    {
        foreach (var damageable in damageableList)
        {
            damageable.TakeKnockBack(transform.position, knockBackDistance, knockBackDistance/knockBackTime);
        }
        foreach (var stateable in stateableList)
        {
            stateable.StateChange(PlayerState.Knockback, invincibleTime, knockBackTime, true, false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, atkRange);
    }

    [PunRPC]
    protected virtual void Flip(bool flip)
    {
        render.flipX = flip;
        if (effectRender != null)
        {
            effectRender.flipY = flip;
        }
    }

    public void SetMousePoiot(Vector3 mousePoint)
    {
        photonView.RPC("Snyc_MousePoint", RpcTarget.All, mousePoint);
    }

    [PunRPC]
    protected void Snyc_MousePoint(Vector3 mousePoint)
    {
        attackPoint.MouseDir = mousePoint;
    }


    #region Coroutine
    /// <summary>
    /// 플레이어 공격 루틴
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator AttackRoutine()
    {
        if (owner.PlayerClassData.classType.Equals(ClassType.Warrior))  // 전사 직업의 공격일 경우
        {
            yield return new WaitForSeconds(0.1f);
            attackEffect.SetBool("SkillStop", true);    // 무기 오브젝트 활성화 하기
        }

        owner.StateController.StateChange(PlayerState.AttackStart, 0, 0, true, false);

        // 공격 가능 객체 체크
        CheckDamgeable();

        // 애니메이션이 살짝 늦게 나오므로 딜레이 줌
        yield return new WaitForSeconds(0.18f);

        // 데미지 주기
        owner.photonView.RPC("PlaySound", RpcTarget.All, skillInfo.skEffect1_Data.mainSound);
        InflictDamage(damageableList, atkDamage);

        // 넉백 효과 주기
        KnockBack();

        // 공격 중일땐 (애니메이션 나오는 중일 땐) 공격 못하게 딜레이 줌
        yield return new WaitForSeconds(0.5f);
        owner.StateController.StateChange(PlayerState.AttackExit, 0, 0, true, false);
    }
    #endregion
}
