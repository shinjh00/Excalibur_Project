using Photon.Pun;
using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 베이스 몬스터 스크립트
/// 모든 몬스터는 해당 스크립트를 상속받아야 함.
/// </summary>
public class BaseMonster : MonoBehaviourPun, IDamageable
{
    protected MonsterStateMachine fsm;
    protected MonsterData monsterData;
    protected PlayerController targetPos;

    [Tooltip("플레이어 레이어 설정")]
    [SerializeField] protected LayerMask playerLayer;
    [Tooltip("몬스터의 애니메이터 설정")]
    [SerializeField] protected Animator animator;
    [Tooltip("몬스터 오브젝트의 콜라이더 설정(죽었을 때 트리거 설정용)")]
    [SerializeField] protected Collider2D monsterColliders;
    [Tooltip("디버깅 콜라이더(플레이어 감지용)")]
    [SerializeField] protected Collider2D[] colliders;
    [Tooltip("몬스터의 ID(기획서 참조)")]
    [SerializeField] protected int id;
    [Tooltip("몬스터의 현재 상태")]
    [SerializeField] protected MonsterState curState;
    [Tooltip("몬스터 스프라이트 이미지")]
    [SerializeField] protected SpriteRenderer spriteRender;
    [Tooltip("몬스터 경계 이미지 할당(?)")]
    [SerializeField] protected SpriteRenderer alertImage;
    [Tooltip("몬스터 추적 이미지 할당(!)")]
    [SerializeField] protected SpriteRenderer traceImage;
    [Tooltip("몬스터의 어택 포인트 스크립트를 가진 오브젝트 할당")]
    [SerializeField] protected MonsterAttackPoint attackPoint;
    [Tooltip("몬스터 시체 스크립트를 가진 오브젝트 할당")]
    [SerializeField] protected MonsterCorpseController corpseController;
    [Tooltip("몬스터 UI 할당 필요")]
    [SerializeField] protected MonsterUI monsterUI;
    [Tooltip("데미지 플로터 할당 필요")]
    [SerializeField] protected DamageFloater dmgFloater;

    [Tooltip("파이어베이스 상 아이디(에디터 할당 x)")]
    [SerializeField] protected int playID;
    protected float curDectionRange;
    protected bool isFsmStart;
    protected bool startTrace;
    protected bool isDamage;
    protected bool isPatrol;
    protected bool isDie;
    protected bool isCheck;
    protected bool isAttack;
    protected bool isAttacking;
    protected float cosRange;
    protected float attackCooltime;
    protected LayerMask obstacleLayer;
    protected Action<float, int> OnChangeHp;
    protected MonsterSpanwer.MonsterSpawnTable myTable;
    public float CurDectionRange { get { return curDectionRange; } set { curDectionRange = value; } }
    public bool StartTrace { get { return startTrace; } set { startTrace = value; } }
    public bool IsDamage { get { return isDamage; } set { isDamage = value; } }
    public bool IsPatrol { get { return isPatrol; } set { isPatrol = value; } }
    public bool IsAttack { get { return isAttack; } set { isAttack = value; } }
    public bool IsAttacking { get { return isAttacking; } set { isAttacking = value; } }
    public float CosRange { get { return cosRange; } }
    public float AttackCooltime { get { return attackCooltime; } set { attackCooltime = value; } }
    public MonsterState CurState { get { return curState; } set { curState = value; } }
    public PlayerController Target { get { return targetPos; } }
    public Transform TargetPos { get { return targetPos.transform; } }
    public MonsterData m_MonsterData { get { return monsterData; } }
    public Animator Animator { get { return animator; } }
    public SpriteRenderer SpriteRender { get { return spriteRender; } }
    public SpriteRenderer AlertImage { get { return alertImage; } set { alertImage.gameObject.SetActive(value); } }
    public SpriteRenderer TraceImage { get { return traceImage; } set { traceImage.gameObject.SetActive(value); } }
    public MonsterAttackPoint AttackPoint { get { return attackPoint; } }


    protected virtual void Awake()
    {
        cosRange = Mathf.Cos(90 * Mathf.Deg2Rad);
        obstacleLayer = LayerMask.GetMask(new string[3] { "Wall", "Monster", "Interact" });
    }

    protected virtual void Start()
    {
        attackPoint.SetOwner(this);
        dmgFloater = Instantiate(dmgFloater, transform.position, Quaternion.identity, transform);
        monsterUI.Init(monsterData.hp);
        photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
        // playID = FirebaseManager.Instance.Link.SetStructData(monsterData);
        // OnChangeHp += FirebaseManager.Instance.Link.ChangeMonsterHp;
    }
    public void TableSetting(MonsterSpanwer.MonsterSpawnTable table)
    {
        myTable = table;
    }
    /// <summary>
    /// 상태 머신에 의한 업데이트 실행
    /// </summary>
    protected void Update()
    {
        if (isDie)
        {
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < GameManager.Ins.playerList.Count; i++)
            {
                // 약 17? -> 가까이 오게되면 상태 머신을 실행
                if (!GameManager.Ins.playerList[i].IsDestroyed() && Vector2.SqrMagnitude(transform.position - GameManager.Ins.playerList[i].transform.position) < 300f)
                {
                    photonView.RPC("FsmActive", RpcTarget.MasterClient, true);
                    break;
                }
                else
                {
                    photonView.RPC("FsmActive", RpcTarget.MasterClient, false);
                }
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            if (!isFsmStart)
            {
                return;
            }
            fsm.Update();
        }
    }

    /// <summary>
    /// 상태 머신에 의한 업데이트 실행
    /// </summary>
    protected void FixedUpdate()
    {
        if (isDie)
        {
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < GameManager.Ins.playerList.Count; i++)
            {
                // 약 17? -> 가까이 오게되면 상태 머신을 실행
                if (!GameManager.Ins.playerList[i].IsDestroyed() && Vector2.SqrMagnitude(transform.position - GameManager.Ins.playerList[i].transform.position) < 300f)
                {
                    photonView.RPC("FsmActive", RpcTarget.MasterClient, true);
                    break;
                }
                else
                {
                    photonView.RPC("FsmActive", RpcTarget.MasterClient, false);
                }
            }
        }
        if (PhotonNetwork.IsMasterClient)
        {
            if (!isFsmStart)
            {
                return;
            }
            fsm.FixedUpdate();
        }
    }

    /// <summary>
    /// 데미지를 받는 메소드
    /// </summary>
    /// <param name="damage">피해량</param>
    public virtual void TakeDamage(float damage, DamageType dmgType = DamageType.Normal)
    {
        if (isDie)
        {   // 사망하였을 땐 보물상자의 데미지 메소드 실행
            corpseController.TakeDamage(damage);
            return;
        }
        photonView.RPC("DamageSync", RpcTarget.All, damage);
        OnChangeHp?.Invoke(monsterData.hp, playID);
        photonView.RPC("FloatingDamageMonster", RpcTarget.All, damage);
    }

    [PunRPC]
    protected void DamageSync(float damage)
    {
        StartCoroutine(monsterUI.HpRoutine(monsterData.hp, monsterData.hp - damage));

        monsterData.hp -= damage;
        isDamage = true;
    }

    /// <summary>
    /// 넉백 메소드
    /// </summary>
    /// <param name="attackPos"> 공격 위치 </param>
    /// <param name="knockBackDistance"> 넉백 거리 </param>
    /// <param name="knockBackSpeed"> 넉백 속도 </param>
    public void TakeKnockBack(Vector3 attackPos, float knockBackDistance, float knockBackSpeed)
    {
        if (isDie)
        {
            return;
        }

        Vector3 hitPos = transform.position;  // 맞은 애 위치
        Vector3 attackDir = (hitPos - attackPos).normalized;  // 넉백 될 방향
        Vector3 targetPos = hitPos + attackDir * knockBackDistance;  // 넉백 거리에 따른 이동 위치

        int layerMask = 1 << LayerMask.NameToLayer("Wall");
        RaycastHit2D hit = Physics2D.Raycast(hitPos, attackDir, knockBackDistance, layerMask);
        Debug.DrawRay(hitPos, attackDir * knockBackDistance, Color.red, 2.0f);

        if (hit)  // 벽에 레이가 맞았을 때
        {
            photonView.RPC("KnockBackSnyc", RpcTarget.MasterClient, hitPos, hit.point, knockBackSpeed);
        }
        else  // 벽에 레이가 안 맞았을 때
        {
            photonView.RPC("KnockBackSnyc", RpcTarget.MasterClient, hitPos, (Vector2)targetPos, knockBackSpeed);
        }
    }
    public void TakeKnockBackFromSkill(Vector3 mouseDir, Vector3 attackPos, float knockBackDistance, float knockBackSpeed)
    {
        if (isDie)
        {
            return;
        }
        Vector3 damagablePos = transform.position;
        Vector3 targetEndPos = mouseDir;
        Vector3 attackDir = (targetEndPos - attackPos).normalized;  // 넉백 될 방향
        Vector3 targetPos = damagablePos + attackDir * knockBackDistance;  // 넉백 거리에 따른 이동 위치

        int layerMask = 1 << LayerMask.NameToLayer("Wall");
        RaycastHit2D hit = Physics2D.Raycast(damagablePos, attackDir, knockBackDistance, layerMask);
        Debug.DrawRay(damagablePos, attackDir * knockBackDistance, Color.red, 2.0f);

        if (hit)  // 벽에 레이가 맞았을 때
        {
            photonView.RPC("KnockBackSnyc", RpcTarget.MasterClient, damagablePos, hit.point, knockBackSpeed);
        }
        else  // 벽에 레이가 안 맞았을 때
        {
            photonView.RPC("KnockBackSnyc", RpcTarget.MasterClient, damagablePos, (Vector2)targetPos, knockBackSpeed);
        }
    }
    [PunRPC]
    protected void KnockBackSnyc(Vector3 hitPos, Vector2 targetPos, float knockBackSpeed)
    {
        // transform.position = Vector2.MoveTowards(hitPos, targetPos, knockBackSpeed);
        StartCoroutine(KnockBackRoutine(hitPos, targetPos, knockBackSpeed));
    }

    /// <summary>
    /// 애니메이터 설정 및 동기화 메소드(trigger)
    /// </summary>
    /// <param name="triggerName">설정할 애니메이션 트리거</param>
    public void SetAnimator(string triggerName)
    {
        if (isFsmStart)
        {
            photonView.RPC("SetAnimatorTrigger", RpcTarget.All, triggerName);
        }
    }

    [PunRPC]
    protected void SetAnimatorTrigger(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    /// <summary>
    /// 애니메이터 설정 및 동기화 메소드(bool)
    /// </summary>
    /// <param name="boolName">설정할 파라미터</param>
    /// <param name="setBool">설정 값(bool)</param>
    public void SetAnimator(string boolName, bool setBool)
    {
        if (isFsmStart)
        {
            photonView.RPC("SetAnimatorBool", RpcTarget.All, boolName, setBool);
        }
    }

    [PunRPC]
    protected void SetAnimatorBool(string boolName, bool setBool)
    {
        animator.SetBool(boolName, setBool);
    }

    /// <summary>
    /// 개발자 : 서보운
    /// 플레이어 체크용 메소드.
    /// 플레이어가 탐지 범위인지 체크를 위한 메소드
    /// </summary>
    /// <returns>반환값은 범위 안에 플레이어가 있는지(bool)</returns>
    protected bool CheckPlayer()
    {
        bool isCheck = false;

        // 1. 횃불 탐지 거리로 측정함.
        int size = Physics2D.OverlapCircleNonAlloc(transform.position, monsterData.torchDetectionRange, colliders, playerLayer);

        for (int i = 0; i < size; i++)
        {
            PlayerController target = colliders[i].GetComponent<PlayerController>();

            if (target != null && !target.StateController.CurState.Contain(PlayerState.Dead | PlayerState.Metamorph | PlayerState.Sleep | PlayerState.Groggy))
            {   // 2. 타겟을 찾았다면 횃불 상태인지 검사
                if (target.visionController.IsTorch())
                {   // 2-1. 켰다면 찾은 것. 더 로직 검사할 이유는 없음
                    targetPos = target;
                    isCheck = true;
                    break;
                }
                else
                {   // 2-2. 꺼져 있었다면, 한번 더 거리 측정이 필요
                    float dist = Vector2.Distance(target.transform.position, transform.position);

                    if (dist <= curDectionRange)
                    {
                        targetPos = target;
                        isCheck = true;
                        break;
                    }
                }
            }
        }

        return isCheck;
    }

    /// <summary>
    /// 개발자 : 서보운
    /// 어택 범위 탐지용 메소드
    /// </summary>
    /// <returns>반환값은 타겟과 내 위치에 대하여 공격 가능한지(bool)</returns>
    protected bool CheckAttackRange()
    {
        return (!targetPos.StateController.CurState.Contain(PlayerState.Dead | PlayerState.Sleep | PlayerState.Metamorph | PlayerState.Groggy) &&
            Vector2.SqrMagnitude(TargetPos.position - transform.position) < (monsterData.attackRange * monsterData.attackRange + 0.002f));
    }

    /// <summary>
    /// 개발자 : 서보운
    /// 몬스터, 벽과 부딫혔을 때 비벼서 나가지는 현상 방지용
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (obstacleLayer.Contain(collision.gameObject.layer))
        {
            isPatrol = false;
        }
    }

    /// <summary>
    /// 어택 시작 메소드(애니메이션 이벤트)
    /// </summary>
    public void StartAttack()
    {
        attackPoint.StartAttack();
    }

    /// <summary>
    /// 어택 끝 메소드(애니메이션 이벤트)
    /// </summary>
    public void EndAttack()
    {
        attackPoint.EndAttack();
    }

    /// <summary>
    /// 어택 종료 상황(애니메이션 이벤트)
    /// </summary>
    public void EndAttacking()
    {
        attackPoint.EndAttaking();
    }

    /// <summary>
    /// 사망 처리 추가 필요
    /// 보물상자 스폰 및 관련된 내용들..
    /// </summary>
    public void Die()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            myTable.curCount--;
            myTable.spawner.MonsterDie(this, myTable.curCount < myTable.monsterMinCount);
            photonView.RPC("DieSnyc", RpcTarget.All);
        }
    }

    [PunRPC]
    protected void DieSnyc()
    {
        isDie = true;
        ChangeLayer(this.gameObject, 9);

        corpseController.enabled = true;
        corpseController.OnDie(monsterData.monsterBoxID);
        if(targetPos != null)
        {
            targetPos.StatController.GetExp(monsterData.exp);

            var allies = targetPos.photonView.Owner.AllyPlayers()
                             .Where(p => p != targetPos.photonView.Owner) // 본인 제외
                             .ToList();
            foreach (var p in allies)
            {
                PlayerController ally = p.GetPlayerController();
                Debug.Log($"targetPos Ally : {ally}");                //지 동료 플레이어가 한번 찍힘
                ally.photonView.RPC("GetExp", p, monsterData.exp);      //한번 찍혀야겠지? =>  p 라는 플레이어가 GetExp라는 메서드를 한번 실행함
            }
        }
        attackPoint.EndAttack();
        monsterColliders.isTrigger = true;
        attackPoint.enabled = false;

    }
    /// <summary>
     /// true면 레드팀에게 경험치 false면 블루팀에게 경험치
     /// </summary>
     /// <param name="red"></param>

    // 사망 시 레이어 변경
    private void ChangeLayer(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach(Transform child in obj.transform)
        {
            ChangeLayer(child.gameObject, layer);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, monsterData.detectionRange);

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, monsterData.attackRange);
    }

    /// <summary>
    /// 외부 동기화 함수(경계 이미지)
    /// </summary>
    /// <param name="value"></param>
    public void ChangeAlertSet(bool value)
    {
        photonView.RPC("ChangeAlertImage", RpcTarget.All, value);
    }
    [PunRPC]
    protected void ChangeAlertImage(bool value)
    {
        AlertImage.gameObject.SetActive(value);
    }

    /// <summary>
    /// 외부 동기화 메소드(추격 이미지)
    /// </summary>
    /// <param name="value"></param>
    public void ChangeTraceSet(bool value)
    {
        photonView.RPC("ChangeTraceImage", RpcTarget.All, value);
    }
    [PunRPC]
    protected void ChangeTraceImage(bool value)
    {
        TraceImage.gameObject.SetActive(value);
    }

    /// <summary>
    /// 외부 동기화 메소드(렌더러의 flip)
    /// </summary>
    /// <param name="value"></param>
    public void ChangeFlip(bool value)
    {
        photonView.RPC("Flip", RpcTarget.All, value);
    }

    [PunRPC]
    protected void Flip(bool value)
    {
        spriteRender.flipX = value;
    }

    [PunRPC]
    public void SetInteract(bool value)
    {
        corpseController.IsInteract = value;
    }

    [PunRPC]
    public void FloatingDamageMonster(float damage)
    {
        dmgFloater.transform.position = transform.position;
        dmgFloater.Floating(damage);
    }

    [PunRPC]
    public void TargetSnyc(int viewID)
    {   // 몬스터의 타겟 동기화(동일한 타겟 검색)
        targetPos = GameManager.Ins.FindPlayer(viewID);
    }

    /// <summary>
    /// 넉백 루틴
    /// </summary>
    /// <param name="hitPos"></param>
    /// <param name="targetPos"></param>
    /// <param name="knockBackSpeed"></param>
    /// <returns></returns>
    IEnumerator KnockBackRoutine(Vector2 hitPos, Vector2 targetPos, float knockBackSpeed)
    {
        float rate = 0f;
        float time = Vector2.Distance(hitPos, targetPos) / knockBackSpeed;

        while (rate < 1f)
        {
            rate += Time.deltaTime / time;

            if (rate > 1f)
            {
                rate = 1f;
            }

            transform.position = Vector2.Lerp(hitPos, targetPos, rate);

            yield return null;
        }
    }

    [PunRPC]
    public virtual void CheckSound()
    {

    }

    public void SetStatRate(MonsterStat stat, float rate)
    {
        switch (stat)
        {
            case MonsterStat.Hp:
                monsterData.hp = (monsterData.hp * rate);
                break;
            case MonsterStat.Atk:
                monsterData.damage = (monsterData.damage * rate);
                break;
        }
    }

    [PunRPC]
    protected void FsmActive(bool value)
    {
        isFsmStart = value;
    }
}

public enum MonsterStat
{
    Hp,
    Atk
}