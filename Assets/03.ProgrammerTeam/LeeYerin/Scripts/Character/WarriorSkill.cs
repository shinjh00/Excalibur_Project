using Photon.Pun;
using System.Collections;
using UnityEngine;

/// <summary>
/// 개발자: 이예린 / 전사 스킬 구현
/// </summary>
public class WarriorSkill : SkillData
{
    [SerializeField] LayerMask layerMask;

    [Header("Skill1")]
    float s1FirstDelay;
    float s1AfterDelay;
    Vector2 s1OneTwoRange = new Vector2(3f, 1.5f);  // 1타, 2타 피격 범위 사이즈
    Vector2 s1ThreeRange = new Vector2(3f, 2f);     // 3타 피격 범위 사이즈
    Vector2 s1RangePivot = new Vector2(1.5f, 0f);

    float rushRange;
    Vector2 s1Range;
    int clickCount = 0;
    float stat = 0f;
    Coroutine slash;
    Coroutine ultRoutine;

    [Header("Skill2")]
    [SerializeField] float s2KnokBackDis;
    [SerializeField] float s2KnockBackSpeed;
    float s2FirstDelay;
    float s2AfterDelay;
    Vector2 s2Range = new Vector2(3.5f, 1f);    // 피격 범위 사이즈
    Vector2 s2RangePivot = new Vector2(1.75f, 0f);


    [Header("Ult")]
    [SerializeField] float ultKnokBackDis;
    [SerializeField] float ultKnockBackSpeed;
    float ultFirstDelay;
    float ultAfterDelay;
    Vector2 ultRange = new Vector2(1f, 1f);    // 피격 범위 사이즈
    float moveSpeed;    // 플레이어의 이동 속도
    Vector2 atkPos;   // 마우스 위치
    Animator effectAnimator;    // 이팩트 애니메이터

    protected override void Start()
    {
        GetSkillId();
        skill1Data = CsvParser.Instance.SkillDic[skill1id];
        skill1Data.ReadEffectData();
        skill2Data = CsvParser.Instance.SkillDic[skill2id];
        skill2Data.ReadEffectData();
        skill3Data = CsvParser.Instance.SkillDic[skill3id];
        skill3Data.ReadEffectData();

        moveSpeed = owner.PlayerClassData.moveSpeed;
        effectAnimator = owner.AttackController.AtkEffect;
    }

    #region Skill 1
    public override void Skill1_Ready(PlayerController player)
    {
        owner.AttackController.enabled = false;
        if (owner.photonView.IsMine)
        {
            if (!casting)
            {
                casting = true;
                atkPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                owner.photonView.RPC("Skill1_Excute", RpcTarget.All, atkPos);
            }
        }
    }

    public override void Skill1_Excute(PlayerController player, object parameter)
    {
        owner.PlayerAnim.SetBool("Skill1Click", true);
        owner.PlayerAnim.SetTrigger("Skill1");  // 스킬1 애니메이션 실행

        if (parameter != null)
        {
            atkPos = (Vector2)parameter;
        }
        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
        }

        switch (++clickCount)
        {
            case 0:
                Debug.Log("잘못된 clickCount 들어옴.");
                break;
            case 3:
                // 선딜레이 계산
                s1FirstDelay = calculateDelays(skill1Data.skEffect3_Data.minForeDelay, skill1Data.skEffect3_Data.maxForeDelay);
                // 후딜레이 계산
                s1AfterDelay = calculateDelays(skill1Data.skEffect3_Data.minAftDelay, skill1Data.skEffect3_Data.maxAftDelay);
                rushRange = skill1Data.skEffect3_Data.skillRange; // 돌진 거리
                s1Range = s1ThreeRange; // 피격 범위
                break;
            default:
                // 선딜레이 계산
                s1FirstDelay = calculateDelays(skill1Data.skEffect1_Data.minForeDelay, skill1Data.skEffect1_Data.maxForeDelay);
                // 후딜레이 계산
                s1AfterDelay = calculateDelays(skill1Data.skEffect1_Data.minAftDelay, skill1Data.skEffect1_Data.maxAftDelay);
                rushRange = skill1Data.skEffect1_Data.skillRange;    // 돌진 거리
                s1Range = s1OneTwoRange;    // 피격 범위
                break;
        }

        owner.PlayerAnim.SetFloat("Skill1Stat", stat);
        skillCoroutine = StartCoroutine(SkillRoutine(s1FirstDelay, 0, () => Skill1_Logic(owner, 1)));
    }

    public override void Skill1_Logic(PlayerController player, object parameter)
    {
        if (slash != null)
        {
            StopCoroutine(slash);
        }
        slash = StartCoroutine(comboSlash());
    }

    /// <summary>
    /// 스킬1(연속 베기) 실행하는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator comboSlash()
    {
        Vector2 playerPos = owner.transform.position;   // 플레이어의 스킬 시작 위치
        Vector2 dir = (atkPos - playerPos).normalized;  // 스킬 공격 방향

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;    // 방향 벡터의 각도를 라디안에서 도로 변환
        Vector2 rotatedPivot = Quaternion.Euler(0, 0, angle) * s1RangePivot;    // s2RangePivot을 dir 방향으로 회전시킨 벡터
        Vector2 rangeCenter = playerPos + rotatedPivot;     // 회전된 피벗을 플레이어 위치에 더하여 공격 범위의 중심 계산

        owner.PlayerAnim.SetFloat("Skill1Stat", stat += 0.125f);  // 공격 모션 실행
        SoundManager.instance.PlaySFX(1650013, owner.audioSource);  // 사운드 실행
        // 공격 이팩트 실행
        if (clickCount == 3)    // 3타라면
        {
            effectAnimator.SetTrigger("Skill1_3"); 
        }
        else if (clickCount == 2)
        {
            effectAnimator.SetTrigger("Skill1_2");
        }
        else
        {
            effectAnimator.SetTrigger("Skill1_1");
        }
        effectAnimator.SetBool("SkillStop", false);

        do
        {
            if (Physics2D.Raycast(owner.transform.position, dir, 1f, layerMask))
            {
                Debug.Log("벽이다!");
                break;
            }

            owner.transform.position += (Vector3)dir * moveSpeed * 2f * Time.deltaTime; // 이동속도의 2배 속도로 이동

            damageableList.Clear(); // 피격 대상 리스트 초기화
            stateableList.Clear();
            IStateable stateable = null;
            // 공격 범위 내의 충돌체 수집
            if (owner.photonView.IsMine)
            {
                int overlapCount = Physics2D.OverlapBoxNonAlloc(rangeCenter, s1Range, angle, damageableColliders, owner.AttackController.DamageableMask);

                for (int i = 0; i < overlapCount; i++)
                {
                    // 플레이어 제외
                    if (damageableColliders[i].gameObject.Equals(owner.gameObject))
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

                    if (damageable != null) // 충돌체가 공격 가능한 충돌체인 경우
                    {
                        damageableList.Add(damageable); // 피격할 충돌체 리스트에 추가
                    }
                    stateable = damageableColliders[i].GetComponent<IStateable>();

                }

            }
            if (stateable != null)
            {
                stateableList.Add(stateable);
            }

            // CSV 내용 추가
            float dmgFactor = 1f;   // 데미지 계수. 1타, 2타, 3타 전부 다름.
            float kbDis = 1f;       // 넉백 거리. 1타, 2타, 3타 전부 다름.
            float kbTime = 1f;      // 넉백 시간. 1타, 2타, 3타 전부 다름.
            float invTime = 1f;     // 무적 시간. 1타, 2타, 3타 전부 다름.
            switch(clickCount)
            {
                case 1:     // 스킬효과 1
                    dmgFactor = skill1Data.skEffect1_Data.dmgFactor;
                    kbDis = skill1Data.skEffect1_Data.knockbackDis;
                    kbTime = skill1Data.skEffect1_Data.knockbackTime;
                    invTime = skill1Data.skEffect1_Data.unbeatableTime;
                    break;
                case 2:     // 스킬효과 2
                    dmgFactor = skill1Data.skEffect2_Data.dmgFactor;
                    kbDis = skill1Data.skEffect2_Data.knockbackDis;
                    kbTime = skill1Data.skEffect2_Data.knockbackTime;
                    invTime = skill1Data.skEffect2_Data.unbeatableTime;
                    break;
                case 3:     // 스킬효과 3
                    dmgFactor = skill1Data.skEffect3_Data.dmgFactor;
                    kbDis = skill1Data.skEffect3_Data.knockbackDis;
                    kbTime = skill1Data.skEffect3_Data.knockbackTime;
                    invTime = skill1Data.skEffect3_Data.unbeatableTime;
                    break;
            }

            InflictDamage(damageableList, owner.AttackController.AtkDamage * dmgFactor);    // 리스트 안에 있는 충돌체에 데미지 가해줌
            KnockBack(owner.transform, kbDis, kbDis / kbTime);   // 넉백
            KnockBackStateChange(invTime, kbTime);
            isHit.AddDamagable(damageableList);
            yield return null;
        } while ((owner.transform.position - (Vector3)playerPos).magnitude < rushRange); // 돌진 거리만큼 이동할 때까지
        isHit.Clear();
        owner.MoveSetter(false);

        effectAnimator.SetBool("SkillStop", true);  // 무기 오브젝트 활성화

        owner.PlayerAnim.SetFloat("Skill1Stat", stat += 0.125f);  // 후딜레이 모션 실행
        yield return new WaitForSeconds(s1AfterDelay);
        stat += 0.125f;
        owner.PlayerAnim.SetBool("Skill1Click", false);
        owner.MoveSetter(true);
        owner.AttackController.enabled = true;
        casting = false;
        if (clickCount != 3)    // 3타가 아니라면
        {
            Debug.Log("Click!");
            yield return new WaitForSeconds(3f);    // 다시 Q 누르기 3초 기다리기
        }
        Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
        // 3초 안에 누르지 못했다면 전부 리셋
        StopSkill();
    }
    #endregion

    #region Skill 2
    public override void Skill2_Ready(PlayerController player)
    {
        owner.AttackController.enabled = false;
        if (owner.photonView.IsMine)
        {
            if (!casting)
            {
                casting = true;
                atkPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                owner.photonView.RPC("Skill2_Excute", RpcTarget.All, atkPos);
            }
        }
    }

    public override void Skill2_Excute(object parameter)
    {
        owner.PlayerAnim.SetTrigger("Skill2");  // 스킬2 애니메이션 실행

        if (parameter != null)
        {
            atkPos = (Vector2)parameter;
        }

        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
        }

        // 선딜레이 시간 계산
        s2FirstDelay = calculateDelays(skill2Data.skEffect1_Data.minForeDelay, skill2Data.skEffect1_Data.maxForeDelay);

        // 후딜레이 시간 계산
        s2AfterDelay = calculateDelays(skill2Data.skEffect1_Data.minAftDelay, skill2Data.skEffect1_Data.maxAftDelay);

        owner.PlayerAnim.SetFloat("Skill2Stat", 0f);
        skillCoroutine = StartCoroutine(SkillRoutine(s2FirstDelay, 0, () => Skill2_Logic(owner, 1)));
    }

    public override void Skill2_Logic(PlayerController player, object parameter)
    {
        StartCoroutine(StabCombo());
    }

    /// <summary>
    /// 스킬2(연속 찌르기) 실행하는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator StabCombo()
    {
        owner.StatController.AddBuffStat(StatLevel.MoveSpeed, 0.001f);
        Vector2 playerPos = owner.transform.position;   // 플레이어의 스킬 시작 위치
        Vector2 dir = (atkPos - playerPos).normalized;  // 스킬 공격 방향

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;    // 방향 벡터의 각도를 라디안에서 도로 변환
        Vector2 rotatedPivot = Quaternion.Euler(0, 0, angle) * s2RangePivot;    // s2RangePivot을 dir 방향으로 회전시킨 벡터
        Vector2 rangeCenter = playerPos + rotatedPivot;     // 회전된 피벗을 플레이어 위치에 더하여 공격 범위의 중심 계산

        owner.PlayerAnim.SetFloat("Skill2Stat", 0.5f);  // 공격 애니메이션 실행
        SoundManager.instance.PlaySFX(1650014, owner.audioSource);
        // 공격 이팩트 애니메이션 실행
        effectAnimator.gameObject.SetActive(true);
        effectAnimator.SetTrigger("Skill2");
        effectAnimator.SetBool("SkillStop", false);

        float time = 0f;
        damageableList.Clear(); // 피격 대상 리스트 초기화
        stateableList.Clear();
        IStateable stateable = null;
            while (time < 0.7f) // 0.7초 동안 공격 진행
            {
            // 공격 범위 내의 충돌체 수집
            if (owner.photonView.IsMine)
            {
                int overlapCount = Physics2D.OverlapBoxNonAlloc(rangeCenter, s2Range, angle, damageableColliders, owner.AttackController.DamageableMask);

                for (int i = 0; i < overlapCount; i++)
                {
                    // 플레이어 제외
                    if (damageableColliders[i].gameObject.Equals(owner.gameObject))
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

                    if (damageable != null) // 충돌체가 공격 가능한 충돌체인 경우
                    {
                        damageableList.Add(damageable); // 피격할 충돌체 리스트에 추가
                    }
                    stateable = damageableColliders[i].GetComponent<IStateable>();

                }

                if (stateable != null)
                {
                    stateableList.Add(stateable);
                }
                InflictDamage(damageableList, owner.AttackController.AtkDamage * skill2Data.skEffect1_Data.dmgFactor);    // 리스트 안에 있는 충돌체에 데미지 가해줌
                KnockBackStateChange(skill2Data.skEffect1_Data.unbeatableTime, skill2Data.skEffect1_Data.knockbackTime);
                isHit.AddDamagable(damageableList);
            }
            time += 0.1f;
            yield return new WaitForSeconds(0.1f);
             }
        isHit.Clear();
        KnockBack(owner.transform, skill2Data.skEffect1_Data.knockbackDis, skill2Data.skEffect1_Data.knockbackDis / skill2Data.skEffect1_Data.knockbackTime);   // 넉백

        // 공격 이팩트 애니메이션 종료
        effectAnimator.SetBool("SkillStop", true); ;
        owner.PlayerAnim.SetFloat("Skill2Stat", 0.99f); // 후딜레이 애니메이션 출력
        yield return new WaitForSeconds(s2AfterDelay);  // 후딜레이 진행
        owner.PlayerAnim.SetFloat("Skill2Stat", 1f);    // 스킬2 종료
        owner.MoveController.SetMoveDir(Vector2.zero);
        owner.AttackController.enabled = true;  // 일반 공격 활성화
        casting = false;
        owner.StatController.RemoveBuffStat(StatLevel.MoveSpeed, 0.001f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;  // Gizmos의 색상을 마젠타로 설정
        Vector3 direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;  // 마우스 위치로부터 방향 벡터 계산 및 정규화
        Vector3 playerPos = owner.transform.position;   // 플레이어의 스킬 시작 위치
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;  // 방향 벡터의 각도를 라디안에서 도로 변환
        Vector3 rotatedPivot = Quaternion.Euler(0, 0, angle) * s2RangePivot;  // s2RangePivot을 방향 각도만큼 회전
        Vector3 boxCenter = playerPos + rotatedPivot;  // 회전된 피벗을 플레이어 위치에 더하여 Gizmos의 중심 계산
        Gizmos.matrix = Matrix4x4.TRS(boxCenter, Quaternion.Euler(0, 0, angle), Vector3.one);  // Gizmos의 변환 행렬 설정 (위치, 회전, 스케일)
        Gizmos.DrawWireCube(Vector3.zero, s2Range);  // 변환 행렬을 기준으로 WireCube 그리기
    }

    #endregion

    #region Ult
    public override void Ult_Ready(PlayerController player)
    {
        owner.AttackController.enabled = false;
        if (owner.photonView.IsMine)
        {
            if (!casting)
            {
                casting = true;
                atkPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                owner.photonView.RPC("Ult_Excute", RpcTarget.All, atkPos);
            }
        }
    }

    public override void Ult_Excute(object parameter)
    {
        owner.PlayerAnim.SetTrigger("Ult");

        atkPos = (Vector2)parameter;

        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
        }

        // 선딜레이 시간 계산
        ultFirstDelay = calculateDelays(skill3Data.skEffect1_Data.minForeDelay, skill3Data.skEffect1_Data.maxForeDelay);

        // 후딜레이 시간 계산
        ultAfterDelay = calculateDelays(skill3Data.skEffect1_Data.minAftDelay, skill3Data.skEffect1_Data.maxAftDelay);

        owner.PlayerAnim.SetFloat("UltStat", 0);
        owner.audioSource.pitch = ultFirstDelay;    // 공격속도에 맞게 재생 속도 변경
        SoundManager.instance.PlaySFX(1650015, owner.audioSource);  // 사운드 실행
        skillCoroutine = StartCoroutine(SkillRoutine(ultFirstDelay, 0f, () => Ult_Logic(owner, 1)));
    }

    public override void Ult_Logic(PlayerController player, object pos)
    {
        ultRoutine = StartCoroutine(DashAttack());
    }

    /// <summary>
    /// 궁극기 실행하는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator DashAttack()
    {
        owner.Rigid.bodyType = RigidbodyType2D.Kinematic;
        owner.PlayerAnim.SetFloat("UltStat", 0.25f);    // 공격 모션으로

        Vector2 playerPos = owner.transform.position;   // 플레이어의 스킬 시작 위치
        Vector2 dir = (atkPos - playerPos).normalized;  // 스킬 공격 방향

        effectAnimator.SetTrigger("Skill3"); // 공격 이팩트 실행
        // effectAnimator.SetTrigger("SkillOn"); // 무기 오브젝트 비활성화
        effectAnimator.SetBool("SkillStop", false);

        // 플레이어 돌진 및 피격
        do
        {
            if (Physics2D.Raycast(owner.transform.position, dir, 1f, layerMask))
            {
                Debug.Log("벽이다!");
                break;
            }
            owner.transform.position += (Vector3)dir * moveSpeed * 5f * Time.deltaTime; // 공격 방향으로 원래 움직임의 5배의 속도로 이동

            damageableList.Clear();
            stateableList.Clear();
            // 피격 범위 내 충돌체 검사
            IStateable stateable = null;
            if (owner.photonView.IsMine)
            {
                int overlapCount = Physics2D.OverlapBoxNonAlloc(owner.transform.position, ultRange, 0, damageableColliders, owner.AttackController.DamageableMask);
                for (int i = 0; i < overlapCount; i++)
                {
                    // 플레이어 제외
                    if (damageableColliders[i].gameObject.Equals(owner.gameObject))
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

                    if (damageable != null) // 충돌체가 공격 가능한 충돌체인 경우
                    {
                        damageableList.Add(damageable); // 피격할 충돌체 리스트에 추가
                    }
                    stateable = damageableColliders[i].GetComponent<IStateable>();
                }

            }
            if (stateable != null)
            {
                stateableList.Add(stateable);
            }


            InflictDamage(damageableList, owner.AttackController.AtkDamage * skill3Data.skEffect1_Data.dmgFactor);    // 리스트 안에 있는 충돌체에 데미지 가해줌
            KnockBack(owner.transform, skill3Data.skEffect1_Data.knockbackDis, skill3Data.skEffect1_Data.knockbackDis / skill3Data.skEffect1_Data.knockbackTime);   // 넉백
            KnockBackStateChange(skill3Data.skEffect1_Data.unbeatableTime, skill3Data.skEffect1_Data.knockbackTime);
            isHit.AddDamagable(damageableList);
            yield return null;
        } while ((owner.transform.position - (Vector3)playerPos).magnitude < 6f);    // 플레이어가 정해진 거리만큼 돌진할 때까지
        isHit.Clear();



        owner.PlayerAnim.SetFloat("UltStat", 0.5f); // 공격 모션 마무리
        yield return new WaitForSeconds(0.2f);
        owner.MoveSetter(false);
        owner.PlayerAnim.SetFloat("UltStat", 0.99f);   // 후딜레이 애니메이션으로
        SoundManager.instance.PlaySFX(1650016, owner.audioSource);  // 사운드 실행
        effectAnimator.SetBool("SkillStop", true);  // 무기 오브젝트 활성화
        yield return new WaitForSeconds(ultAfterDelay);
        owner.PlayerAnim.SetFloat("UltStat", 1f);
        owner.MoveSetter(true);
        owner.AttackController.enabled = true;
        casting = false;
        owner.Rigid.bodyType = RigidbodyType2D.Dynamic;
    }
    #endregion

    /// <summary>
    /// 딜레이 계산하는 메소드
    /// </summary>
    /// <param name="maxDelay">최대 딜레이</param>
    /// <param name="miniDelay">최소 딜레이</param>
    /// <returns></returns>
    private float calculateDelays(float maxDelay, float miniDelay)
    {
        float delay = maxDelay - (owner.AttackController.AtkSpeed - 0.7f);
        if (delay < miniDelay) // 최소 딜레이보다 작을 시
        {
            delay = miniDelay;
        }

        return delay;
    }
    public void StopSkill()
    {
        if (slash != null)
        {
            StopCoroutine(slash);
        }
        if(ultRoutine != null)
        {
            StopCoroutine(ultRoutine);
        }
        owner.PlayerAnim.SetFloat("Skill1Stat", stat = 0f);
        clickCount = 0;
        slash = null;
        effectAnimator.SetBool("SkillStop", true);
        owner.AttackController.enabled = true;
        casting = false;
        owner.Rigid.bodyType = RigidbodyType2D.Dynamic;
        owner.MoveSetter(true);
    }

    public override void GetSkillId()
    {
        skill1id = 1002006;
        skill2id = 1002007;
        skill3id = 1002008;
    }
}
