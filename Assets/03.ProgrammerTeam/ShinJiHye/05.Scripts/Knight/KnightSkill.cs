using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 개발자 : 신지혜 / 기사 스킬 데이터
/// </summary>
public class KnightSkill : SkillData
{
    KnightAttackController knightAttackController;
    Animator playerAnim;
    Spear spear;

    [Header("Skill1")]
    [Tooltip("스킬 데미지 계수")]
    [SerializeField] float skill1DamageCoefficient;
    [Tooltip("스킬 선딜레이")]
    [SerializeField] float skill1FirstDelay;
    [Tooltip("스킬 후딜레이")]
    [SerializeField] float skill1AfterDelay;
    [Tooltip("스킬 사정거리")]
    [SerializeField] float skill1Range;
    [Tooltip("스킬 투사체 속도")]
    [SerializeField] float skill1SpearSpeed;
    [Tooltip("히트박스 범위 (가로)")]
    [SerializeField] float skill1HitboxWidth;
    [Tooltip("히트박스 범위 (세로)")]
    [SerializeField] float skill1HitboxHeight;
    [Tooltip("넉백 거리")]
    [SerializeField] float skill1KnockBackDis;
    [Tooltip("넉백 시간")]
    [SerializeField] float skill1KnockBackTime;

    [Tooltip("스킬 1을 위한 오브젝트(에디터 할당 필요 x)")]
    [SerializeField] KnightThrowSkill knightThrowSkill;

    Vector3 mouseDir;
    Vector3 targetPos;  // 스킬1 목표 위치
    float targetAngle;  // 스킬1 목표 각도

    [Space(5)]
    [Header("Skill2")]
    [Tooltip("스킬 계수")]
    [SerializeField] float skill2DamageCoefficient;
    [Tooltip("스킬 이동거리")]
    [SerializeField] float skill2MoveDistance;
    [Tooltip("스킬 돌격 속도")]
    [SerializeField] float skill2MoveSpeed;
    [Tooltip("히트박스 범위 (가로)")]
    [SerializeField] float skill2HitboxWidth;
    [Tooltip("히트박스 범위 (세로)")]
    [SerializeField] float skill2HitboxHeight;
    [Tooltip("넉백 거리")]
    [SerializeField] float skill2KnockBackDis;
    [Tooltip("넉백 시간")]
    [SerializeField] float skill2KnockBackTime;

    [Tooltip("스킬 2 사거리 측정용(에디터 할당 필요)")]
    [SerializeField] RangeChecker checkerPrefab;
    [Tooltip("스킬 2를 위한 오브젝트(에디터 할당 필요 x)")]
    [SerializeField] KnightRushSkill knightRushSkill;

    RangeChecker ins;   // 임시 프리팹

    [Space(5)]
    [Header("Ult")]
    [Tooltip("스킬 공격력 계수")]
    [SerializeField] float skill3AtkDamageCoefficient;
    [Tooltip("스킬 방어력 계수")]
    [SerializeField] float skill3DefenseCoefficient;
    [Tooltip("스킬 선딜레이")]
    [SerializeField] float skill3FirstDelay;
    [Tooltip("스킬 3을 위한 오브젝트(에디터 할당 필요)")]
    [SerializeField] KnightEndureSkill knightEndureSkill;

    protected override void Start()
    {
        GetSkillId();
        skill1Data = CsvParser.Instance.SkillDic[skill1id];
        skill1Data.ReadEffectData();
        skill2Data = CsvParser.Instance.SkillDic[skill2id];
        skill2Data.ReadEffectData();
        skill3Data = CsvParser.Instance.SkillDic[skill3id];
        skill3Data.ReadEffectData();

        skill1Range = skill1Data.skEffect1_Data.skillRange;
        skill1FirstDelay = skill1Data.skEffect1_Data.minForeDelay;
        skill1AfterDelay = skill1Data.skEffect1_Data.minAftDelay;
        skill1DamageCoefficient = skill1Data.skEffect1_Data.dmgFactor;

        skill2MoveSpeed = skill2Data.skEffect1_Data.skillSpd;
        skill2MoveDistance = skill2Data.skEffect1_Data.skillRange;
        skill2KnockBackDis = skill2Data.skEffect1_Data.knockbackDis;
        skill2KnockBackTime = skill2Data.skEffect1_Data.knockbackTime;
        skill2HitboxHeight = 1.0f;
        s2FirstDelayMin = skill2Data.skEffect1_Data.minForeDelay;
        s2FirstDelayMax = skill2Data.skEffect1_Data.maxForeDelay;
        s2AfterDelayMin = skill2Data.skEffect1_Data.minAftDelay;
        s2AfterDelayMax = skill2Data.skEffect1_Data.maxAftDelay;
        skill2DamageCoefficient = skill2Data.skEffect1_Data.dmgFactor;

        ultAfterDelayMin = skill3Data.skEffect1_Data.minAftDelay;
        ultAfterDelayMax = skill3Data.skEffect1_Data.maxAftDelay;

        base.Start();
        if (knightAttackController == null)
        {
            knightAttackController = owner.GetComponent<KnightAttackController>();
        }
        if (playerAnim == null)
        {
            playerAnim = owner.PlayerAnim;
        }
        if (spear == null)
        {
            spear = knightAttackController.Spear;
        }
        if (knightThrowSkill == null)
        {
            knightThrowSkill = owner.gameObject.GetComponent<KnightThrowSkill>();
        }
        if (knightRushSkill == null)
        {
            knightRushSkill = owner.gameObject.GetComponent<KnightRushSkill>();
        }
        knightRushSkill.SetInit(owner.MoveController.MoveSpeed * skill2MoveSpeed, skill2MoveDistance, skill2KnockBackDis * owner.PlayerClassData.knockBackDistance, skill2KnockBackTime);
    }

    #region Skill1
    /// <summary>
    /// 기사 고유 스킬 1 - 창던지기 만발사수
    /// </summary>
    /// <param name="player"></param>
    public override void Skill1_Ready(PlayerController player)
    {
        // 1. 스킬 사용 중 어택은 불가능
        //owner.AttackController.enabled = false;
        knightAttackController.enabled = false;
        if (owner.photonView.IsMine)
        {
            if (!casting)    // 캐스팅 상태가 아니라면
            {
                // 캐스팅 시작
                casting = true;

                mouseDir = knightAttackController.AttackPoint.MouseDir;
                Vector3 targetDir = mouseDir.normalized;  // 마우스 방향 벡터
                targetPos = owner.transform.position + (targetDir * skill1Range);  // 마우스 방향에 따른 목표 위치

                owner.photonView.RPC("Skill1_Excute", RpcTarget.All, targetPos);        
            }
            else
            {
                owner.AttackController.enabled = true;
                casting = false;
            }
        }
    }

    public override void Skill1_Excute(PlayerController player, object parameter)
    {
        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
        }
        skillCoroutine = StartCoroutine(SkillRoutine(skill1FirstDelay, skill1AfterDelay, () => Skill1_Logic(owner, parameter)));
    }

    public override void Skill1_Logic(PlayerController player, object parameter)
    {
        // 1. 애니메이션 있으면 세팅
        playerAnim.SetTrigger("Skill1");
        player.MoveController.enabled = false;
        knightAttackController.AttackPoint.IsAttacking = true;
        knightThrowSkill.SetInfo(knightAttackController, spear, owner.AttackController.AtkDamage * skill1DamageCoefficient);
        knightThrowSkill.isThrowing = true;
        knightThrowSkill.ThrowSpear(targetPos);
    }
    #endregion

    #region Skill2
    /// <summary>
    /// 기사 고유 스킬 2 - 돌격 앞으로
    /// </summary>
    /// <param name="player"></param>
    public override void Skill2_Ready(PlayerController player)
    {
        owner.AttackController.enabled = false;
        if(owner.photonView.IsMine)
        {
            if(!casting)
            {
                casting = true;

                ins = Instantiate(checkerPrefab, owner.transform.position, Quaternion.identity);
                Vector3 rangeScale = new Vector3(skill2MoveDistance, skill2HitboxHeight);
                float inRangeScale = 1f;
                ins.InitSkill(RangeType.NonTarget, rangeScale, inRangeScale, this, player);
                ins.AddSkill((direction) => owner.photonView.RPC("Skill2_Excute", RpcTarget.All, direction));
            }
            else
            {
                if(ins != null)
                {
                    ins.Cancle();
                }
                owner.AttackController.enabled = true;
                casting = false;
            }
        }
    }

    public override void Skill2_Excute(object parameter)
    {
        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
        }
        float s2FirstDelay = Utils.CalculateDelay(s2FirstDelayMin, s2FirstDelayMax, owner.AttackController.AtkSpeed);
        float s2AfterDelay = Utils.CalculateDelay(s2AfterDelayMin, s2AfterDelayMax, owner.AttackController.AtkSpeed);
        skillCoroutine = StartCoroutine(SkillRoutine(s2FirstDelay, s2AfterDelay, () => Skill2_Logic(owner, parameter)));
    }

    public override void Skill2_Logic(PlayerController player, object parameter)
    {
        // 애니메이션 세팅 필요
        playerAnim.SetTrigger("Skill2");
        knightRushSkill.SetInfo(skill2DamageCoefficient * player.AttackController.AtkDamage);
        SoundManager.instance.PlaySFX(1650020, owner.audioSource);
        knightRushSkill.UseRushSkill(player, parameter);
    }
    #endregion

    #region Skill3
    /// <summary>
    /// 기사 고유 스킬3 - 견뎌
    /// </summary>
    /// <param name="player"></param>
    public override void Ult_Ready(PlayerController player)
    {
        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
        }
        // TODO.. 애니메이션 있다면 추가로 작성
        float ultFirstDelay = Utils.CalculateDelay(ultAfterDelayMin, ultAfterDelayMax, owner.AttackController.AtkSpeed);
        float ultAfterDelay = 0f;
        skillCoroutine = StartCoroutine(SkillRoutine(ultFirstDelay, ultAfterDelay, () => Ult_Logic(owner, 0f)));
    }

    public override void Ult_Excute(object parameter)
    {

    }

    public override void Ult_Logic(PlayerController player, object pos)
    {
        if(knightEndureSkill.IsUse)
        {   // 사용 중이었다면 끄기
            SoundManager.instance.PlaySFX(1650021, owner.audioSource);
            knightEndureSkill.DisableEndureSkill(player);
        }
        else
        {   // 아니면 켜기

            SoundManager.instance.PlaySFX(1650022, owner.audioSource);
            knightEndureSkill.UsingEndureSkill(player);
        }
    }

    public override void GetSkillId()
    {
        skill1id = 1002002;
        skill2id = 1002003;
        skill3id = 1002004;
    }
    #endregion
}
