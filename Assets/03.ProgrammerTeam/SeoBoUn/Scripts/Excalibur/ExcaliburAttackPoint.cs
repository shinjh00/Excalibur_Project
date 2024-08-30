using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// <br/> 엑스칼리버용 공격 포인트 스크립트
/// </summary>
public class ExcaliburAttackPoint : AttackPoint
{
    [Tooltip("빔 렌더러(에디터 할당 필요)")]
    [SerializeField] LineRenderer beamLine;
    [Tooltip("빔 콜라이더(에디터 할당 필요)")]
    [SerializeField] EdgeCollider2D beamCollider;
    [Tooltip("빔 모으는 렌더러(에디터 할딩 필요 및 이펙트 추가 필요)")]
    [SerializeField] Animator beamAnimator;
    [SerializeField] Animator buffAnimator;
    [Tooltip("엑스칼리버 어택 컨트롤러(에디터 할당 필요)")]
    [SerializeField] ExcaliburAttackController excaliburAttackController;
    // Hit 판정용 리스트
    List<IDamageable> hitList;

    [Tooltip("스킬1 휘두르는 시간(초 단위로 입력 필요)")]
    [SerializeField] float swingTime;
    [Tooltip("스킬2 빔 속도(1 : 1배, 5 : 5배...)")]
    [SerializeField] float beamSpeed;
    [Tooltip("디버깅용 현재 데미지(에디터 할당 x)")]
    [SerializeField] int curSkillNum;

    float[] damage = new float[2];
    float[] knockBackDis = new float[2];
    float[] knockBackSpeed = new float[2];
    float[] invTime = new float[2];
    float[] kbTime = new float[2];

    public Action<bool> IsMoving;
    public Animator BuffAnim { get { return buffAnimator; } }

    float rate = 0f;

    protected override void Awake()
    {
        base.Awake();

        beamLine.positionCount = 2;
        beamLine.enabled = false;
        beamCollider.enabled = false;
        buffAnimator.enabled = false;
        hitList = new List<IDamageable>();

        playerLight.SetClass(ClassType.Excalibur);
    }

    public void SetInfo(int num, float dmg, float knockDis, float knockSpeed, float invTime, float kbTime)
    {
        damage[num] = dmg;
        knockBackDis[num] = knockDis;
        knockBackSpeed[num] = knockSpeed;
        this.invTime[num] = invTime;
        this.kbTime[num] = kbTime;
    }

    protected override void Update()
    {
        if (playerPv.IsMine)
        {
            if (isAttacking)
            {
                return;
            }

            float distanceToMouse = Vector3.Distance(transform.position, _camera.ScreenToWorldPoint(Input.mousePosition));

            if (distanceToMouse > range)
            {
                Vector3 targetDir = (_camera.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
                targetDir.z = 0;
                mouseDir = Vector3.Lerp(mouseDir, targetDir, Time.deltaTime * smoothingFactor); // smoothingFactor는 부드럽게 하는 정도를 조절

                float dis = Vector3.Distance(targetDir, mouseDir);

                if (dis > smfRange)
                {
                    smoothingFactor = 20f;
                }
                else
                    smoothingFactor = 1f;

                transform.up = mouseDir;
             //   attackController.SetMousePoiot(mouseDir);
                RotateCornLight();
            }
        }
    }

    public void Skill1()
    {
        isAttacking = true;
        curSkillNum = 0;
        StartCoroutine(Skill1Routine());
    }

    IEnumerator Skill1Routine()
    {
        hitList.Clear();
        rate = 0f;

        Vector3 start = transform.eulerAngles;

        Vector3 target = start;
        target.z += 150f;

        // Quaternion target = Quaternion.Euler(0, 0, transform.rotation.z + 150f);

        while (rate < swingTime)
        {
            rate += 0.01f;

            transform.eulerAngles = Vector3.Lerp(start, target, rate / swingTime);

            yield return new WaitForSeconds(0.01f);
        }

        isAttacking = false;
        excaliburAttackController.enabled = true;
    }

    public void Skill2(float skillTime)
    {
        curSkillNum = 1;
        StartCoroutine(Skill2Routine(skillTime));
    }

    IEnumerator Skill2Routine(float skillTime)
    {
        // 충전 1.5초
        // 발사 1초(시야각까지) -> 시야각 고정
        IsMoving?.Invoke(false);
        hitList.Clear();
        beamAnimator.SetTrigger("Charging");
        yield return new WaitForSeconds(1.5f);
        beamAnimator.SetTrigger("Shoot");
        isAttacking = true;
        beamLine.enabled = true;
        beamCollider.enabled = true;
        attackController.SetMousePoiot(mouseDir);
        float rate = 0f;

        Vector2 startPos = excaliburAttackController.transform.position;
        Vector2 endPos = excaliburAttackController.transform.position + mouseDir * 20f;
        Vector2 targetVec = Vector2.zero;
        beamLine.SetPosition(0, startPos);
        beamLine.SetPosition(1, startPos);

        while (rate < 1f)
        {
            rate += Time.deltaTime * beamSpeed;
            if (rate > 1f)
            {
                rate = 1f;
            }
            targetVec = Vector3.Lerp(startPos, endPos, rate);

            beamLine.SetPosition(1, targetVec);

            yield return null;
        }

        yield return new WaitForSeconds(skillTime);

        IsMoving?.Invoke(true);
        isAttacking = false;
        beamLine.enabled = false;
        beamCollider.enabled = false;
        excaliburAttackController.enabled = true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isAttacking)
        {
            return;
        }

        if (!excaliburAttackController.DamageableMask.Contain(collision.gameObject.layer))
        {
            return;
        }

        if(!playerPv.IsMine)
        {
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
        IDamageable damagable = collision.GetComponent<IDamageable>();
        IStateable stateable = collision.GetComponent<IStateable>();

        if (curSkillNum == 0)
        {   // 스킬1은 한대만 때리자
            if (damagable != null && !hitList.Contains(damagable))
            {
                hitList.Add(damagable);
                damagable.TakeDamage(damage[curSkillNum]);
                damagable.TakeKnockBack(transform.position, knockBackDis[curSkillNum], knockBackSpeed[curSkillNum]);
            }
            if (stateable != null)
            {
                stateable.StateChange(PlayerState.Knockback, invTime[curSkillNum], kbTime[curSkillNum], true, false);
            }
        }
        else
        {   // 스킬2는 중복해서 빔 쏘자
            if (damagable != null)
            {
                damagable.TakeDamage(damage[curSkillNum]);
                damagable.TakeKnockBack(transform.position, knockBackDis[curSkillNum], knockBackSpeed[curSkillNum]);
            }
            if (stateable != null)
            {
                stateable.StateChange(PlayerState.Knockback, invTime[curSkillNum], kbTime[curSkillNum], true, false);
            }
        }

    }
}
