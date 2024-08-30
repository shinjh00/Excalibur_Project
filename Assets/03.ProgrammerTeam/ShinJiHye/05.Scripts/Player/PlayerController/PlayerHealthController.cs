using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 플레이어의 체력(피격, 사망상태)과 관련된 컨트롤러
/// </summary>
public class PlayerHealthController : BaseController, IDamageable
{
    [SerializeField] Rigidbody2D rigid;
    [SerializeField] float maxHp;
    [SerializeField] float hp;
    [SerializeField] float atkRange;
    [SerializeField] float defense;
    float finalDamage;

    int layerMask;

    public UnityEvent OnHit;
    public UnityEvent OnDie;
    public Action<Vector3> OnExcaliburDie;

    public HPBarUI hpBarUI;
    public Action<float> OnChangeHp;
    [Tooltip("LHP - Prefabs - DamageFloater 할당 필요함")]
    [SerializeField] DamageFloater dmgFloater;

    // 넉백 저항(true : 저항 중, false : 아님)
    bool isKnockbackResistance;
    Coroutine knockBackRoutine;
    #region Property 
    public float MaxHp { get { return maxHp; } set { if (PhotonNetwork.InRoom) photonView.RPC("UpdateHealthStatSnyc", RpcTarget.All, PlayerStat.MaxHp, value); } }
    public float Hp { get { return hp; } set { value = Mathf.Clamp(value, 0, MaxHp); if (PhotonNetwork.InRoom) photonView.RPC("UpdateHealthStatSnyc", RpcTarget.All, PlayerStat.Hp, value); OnChangeHp?.Invoke(value); } }
    public float Defense { get { return defense; } set { defense = value; if (PhotonNetwork.InRoom) photonView.RPC("UpdateHealthStatSnyc", RpcTarget.All, PlayerStat.Defense, value); } }
    public bool IsKnockbackResistance { get { return isKnockbackResistance; } set { isKnockbackResistance = value; } }
    #endregion

    /// <summary>
    /// 개발자 : 서보운
    /// <br/> HealthController에 대한 동기화 메소드들
    /// </summary>
    /// <param name="stat"></param>
    /// <param name="value"></param>
    [PunRPC]
    private void UpdateHealthStatSnyc(PlayerStat stat, float value)
    {
        switch (stat)
        {
            case PlayerStat.Hp:
                photonView.RPC("ChangeHpBarUI", RpcTarget.All, hp, value);
                hp = (float)value;
                break;
            case PlayerStat.MaxHp:
                if (maxHp < value)
                {   // 증가하는 상황

                    // 1. 비율 찾기

                    float ratio = (float)(hp / maxHp);

                    // 2. 비율 만큼의 회복량
                    int curHpPlus = (int)((value - maxHp) * ratio);

                    maxHp = (float)value;
                    if (hpBarUI != null)
                    {
                        hpBarUI.ChangeMaxHp(value);
                    }
                    Hp += curHpPlus;
                }
                else
                {   // 감소하는 상황

                    // 1. 비율 찾기
                    float ratio = (float)(hp / maxHp);

                    // 2. 비율 만큼의 회복량
                    int curHpMinus = (int)((maxHp - value) * ratio);

                    maxHp = (float)value;
                    if (hpBarUI != null)
                    {
                        hpBarUI.ChangeMaxHp(value);
                    }
                    Hp -= curHpMinus;
                }
                break;
            case PlayerStat.Defense:
                defense = (float)value;
                break;
        }
    }


    /// <summary>
    /// 스탯 받아오기. Owner에 있는 클래스에서 받아올 예정
    /// OnChangeHp : 파이어베이스 연동용 이벤트
    /// </summary>
    protected override void GetStat()
    {
        maxHp = owner.PlayerClassData.maxHp;
        hp = owner.PlayerClassData.hp;
        if (PhotonNetwork.InRoom)
        {
            photonView.RPC("UpdateHealthStatSnyc", RpcTarget.All, PlayerStat.Hp, hp);
            photonView.RPC("UpdateHealthStatSnyc", RpcTarget.All, PlayerStat.MaxHp, maxHp);
        }
        // OnChangeHp += FirebaseManager.Instance.Link.ChagneHp;
        OnChangeHp += owner.StatController.PlayerStatUI.ChangeHpText;
        hpBarUI.Init(Hp);
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => (owner != null) && (owner.isSetting));

        Hp = MaxHp;
        layerMask = 1 << LayerMask.NameToLayer("Wall");
        hpBarUI = GetComponentInChildren<HPBarUI>();
        dmgFloater = Resources.Load<DamageFloater>("6.Prefab/Etc/DamageFloater");
        dmgFloater = Instantiate(dmgFloater, transform.position, Quaternion.identity, transform);
    }

    public Vector3 SetPos()
    {
        return transform.position;
    }

    /// <summary>
    /// 실제 데미지 메소드
    /// </summary>
    /// <param name="damage">피해량</param>
    public void TakeDamage(float damage, DamageType dmgType = DamageType.Normal)
    {
        if (owner.StateController.CurState.Contain(PlayerState.Dead))
        {
            GameManager.Ins.Message($"{owner.photonView.OwnerActorNr}player is Dead");
            return;
        }
        if (owner.StateController.CurState.Contain(PlayerState.Groggy))
        {
            GameManager.Ins.Message($"{owner.photonView.OwnerActorNr}player is Groggy");
            return;
        }

        if (owner.StateController.CurState.Contain(PlayerState.invincible))
        {   // 무적 상태일 땐 플로팅 데미지가 뜨지 않아야 함.
            // photonView.RPC("FloatingDamageRPC", RpcTarget.All,(float)-1);
            GameManager.Ins.Message($"{owner.photonView.OwnerActorNr}player is invincible");
            return;
        }

        if (PhotonNetwork.LocalPlayer.GetProperty<bool>(DefinePropertyKey.OUTGAME))
        {
            finalDamage = 0.1f;
        }
        else
        {
            if (dmgType == DamageType.Fixed)
            {
                finalDamage = damage; // 고정 데미지일 때 방어력 무시
            }
            else
            {
                finalDamage = (damage) * (100 / (100 + defense)); // 일반 데미지일 때 방어력 적용
            }
        }

        Hp -= finalDamage;
        owner.photonView.RPC("PlaySound", RpcTarget.All, owner.FieldData.attackSoundID);
        photonView.RPC("FloatingDamageRPC", RpcTarget.All, finalDamage);

        if (Hp <= 0)
        {
            Hp = 0;
            photonView.RPC("PlayDieEvent", RpcTarget.All);
        }
        else if (Hp > 0)
        {
            photonView.RPC("PlayHitEvent", RpcTarget.All);
        }
    }

    /// <summary>
    /// 넉백 메소드
    /// </summary>
    /// <param name="attackPos"> 공격 위치 </param>
    /// <param name="knockBackDistance"> 넉백 거리 </param>
    /// <param name="knockBackSpeed"> 넉백 속도 </param>
    public void TakeKnockBack(Vector3 attackPos, float knockBackDistance, float knockBackSpeed)
    {
        if (isKnockbackResistance || owner.StateController.CurState.Contain(PlayerState.SuperArmor))
        {   // 넉백 저항 상태라면 + 슈퍼아머 상태라면
            return;
        }
        Vector3 hitPos = transform.position;  // 맞은 애 위치
        Vector3 attackDir = (hitPos - attackPos).normalized;  // 넉백 될 방향

        // 너무 가까우면(0.5) 넉백을 실행하지 않음

        if (Physics2D.Raycast(hitPos, attackDir, 1f, layerMask))
        {
            Debug.Log("벽이다!");
            return;
        }

        Vector3 targetPos = hitPos + attackDir * knockBackDistance;  // 넉백 거리에 따른 이동 위치

        RaycastHit2D hit = Physics2D.Raycast(hitPos, attackDir, knockBackDistance, layerMask);

        if (hit)  // 벽에 레이가 맞았을 때
        {
            photonView.RPC("KnockBackSnyc", RpcTarget.All, (Vector2)hitPos, hit.point, knockBackSpeed);
            // transform.position = Vector3.Lerp(hitPos, hit.point, 1f);
            //transform.position = Vector2.MoveTowards(hitPos, hit.point, knockBackSpeed * Time.deltaTime);
        }
        else  // 벽에 레이가 안 맞았을 때
        {
            photonView.RPC("KnockBackSnyc", RpcTarget.All, (Vector2)hitPos, (Vector2)targetPos, knockBackSpeed);
            //transform.position = Vector3.Lerp(hitPos, targetPos, 1f);
            //transform.position = Vector2.MoveTowards(hitPos, targetPos, knockBackSpeed * Time.deltaTime);
        }
    }

    [PunRPC]
    private void KnockBackSnyc(Vector2 hitPos, Vector2 targetPos, float knockBackSpeed)
    {
        if ((owner.StateController.CurState.Contain(PlayerState.SuperArmor)))
        {
            return;
        }

        //transform.position = Vector2.MoveTowards(hitPos, targetPos, knockBackSpeed);
        knockBackRoutine = StartCoroutine(KnockBackRoutine(hitPos, targetPos, knockBackSpeed));
    }

    [PunRPC]
    private void PlayHitEvent()
    {
        if (owner.StateController.CurState.Contain(PlayerState.invincible | PlayerState.Dead))
        {
            Debug.Log("Owner Is Dead In RPC");
            return;
        }
        //hpBarUI.ChangeHP(Hp);
        OnHit?.Invoke();
    }

    [PunRPC]
    private void PlayDieEvent()
    {
        //hpBarUI.ChangeHP(Hp);
        OnDie?.Invoke();

        if (owner.PlayerClassData.classType == ClassType.Excalibur)
        {
            OnExcaliburDie?.Invoke(transform.position);
        }
    }
    [PunRPC]
    void FloatingDamageRPC(float damage)
    {
        dmgFloater.transform.position = transform.position;
        dmgFloater.Floating(damage);
    }
    public void FloatingMessage(string message, MessageType type,float value = 0)
    {
        dmgFloater.transform.position = (Vector2)transform.position + new Vector2(0, value);
        Color color = type switch
        {
            MessageType.Exp => Color.green,
            MessageType.Level => Color.yellow,
            _ => Color.white
        };
        dmgFloater.Floating(message, color);
    }
    [PunRPC]
    void ChangeHpBarUI(float hp, float target)
    {
        if (hp == 0 && target == 0)
        {
            return;
        }

        StartCoroutine(hpBarUI.HpRoutine(hp, target));
    }

    public void TakeKnockBackFromSkill(Vector3 mouseDir, Vector3 attackPos, float knockBackDistance, float knockBackSpeed)
    {
        if (isKnockbackResistance)
        {   // 넉백 저항 상태라면
            return;
        }

        if (owner.StateController.CurState.Contain(PlayerState.invincible | PlayerState.SuperArmor)) //무적 시 리턴
        {
            return;
        }
        Vector3 targetDir = (mouseDir - attackPos).normalized;
        // 너무 가까우면(0.5) 넉백을 실행하지 않음

        if (Physics2D.Raycast(attackPos, targetDir, 1f, layerMask))
        {
            Debug.Log("벽이다!");
            return;
        }

        Vector3 targetPos = attackPos + targetDir * knockBackDistance;  // 넉백 거리에 따른 이동 위치

        RaycastHit2D hit = Physics2D.Raycast(attackPos, targetDir, knockBackDistance, layerMask);

        if (hit)  // 벽에 레이가 맞았을 때
        {
            photonView.RPC("KnockBackSnyc", RpcTarget.All, (Vector2)attackPos, hit.point, knockBackSpeed);
            // transform.position = Vector3.Lerp(hitPos, hit.point, 1f);
            //transform.position = Vector2.MoveTowards(hitPos, hit.point, knockBackSpeed * Time.deltaTime);
        }
        else  // 벽에 레이가 안 맞았을 때
        {
            photonView.RPC("KnockBackSnyc", RpcTarget.All, (Vector2)attackPos, (Vector2)targetPos, knockBackSpeed);
            //transform.position = Vector3.Lerp(hitPos, targetPos, 1f);
            //transform.position = Vector2.MoveTowards(hitPos, targetPos, knockBackSpeed * Time.deltaTime);
        }
    }

    [PunRPC]
    public void StartSetDamageColorRed()
    {
        StartCoroutine(SetDamageColorRed());
    }

    IEnumerator SetDamageColorRed()
    {
        owner.AttackController.Renderer.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        owner.AttackController.Renderer.color = Color.white;
    }

    /// <summary>
    /// 개발자 : 서보운
    /// 외부에서 히트 이벤트에 추가하기 위한 메소드
    /// </summary>
    /// <param name="hitEvent"></param>
    public void SetHitEvent(UnityAction hitEvent)
    {
        OnHit.AddListener(hitEvent);
    }

    public void RemoveHitEvent(UnityAction hitEvent)
    {
        OnHit.RemoveListener(hitEvent);
    }

    /// <summary>
    /// 개발자 : 서보운
    /// <br/> 넉백 시간과 스피드에 따른 넉백 루틴
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

    /// <summary>
    /// 개발자 : 서보운
    /// <br/>나이트용 넉백 면역 동기화 메소드
    /// </summary>
    /// <param name="value"></param>
    [PunRPC]
    public void SetKnockbackResist(bool value)
    {
        isKnockbackResistance = value;
        owner.PlayerBgEffectAnim.gameObject.SetActive(value);
    }
    /// <summary>
    /// 개발자 : 이형필
    /// <br/>퍼즐3 발동시 이전 넉백루틴 캔슬 후 순간이동하기 위한 메서드
    /// </summary>
    public void knockBackCancle()
    {
        if (knockBackRoutine != null)
        {
            Debug.Log("knockBackCancle");
            StopCoroutine(knockBackRoutine);
        }

    }
}


