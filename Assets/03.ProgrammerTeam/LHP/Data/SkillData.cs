using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필 / 각 직업에 스킬데이터를 할당시키기 위한 부모클래스
/// </summary>
[Serializable]
public abstract class SkillData : MonoBehaviour
{
    [SerializeField] public PlayerController owner;
    protected List<IDamageable> damageableList = new List<IDamageable>();
    protected List<IStateable> stateableList = new List<IStateable>();
    protected Dictionary<IDamageable, bool> isHit = new Dictionary<IDamageable, bool>();
    protected Collider2D[] damageableColliders = new Collider2D[20];


    protected int skill1id;
    protected int skill2id;
    protected int skill3id;


    [SerializeField] protected SkillInfoData skill1Data;
    [SerializeField] protected SkillInfoData skill2Data;
    [SerializeField] protected SkillInfoData skill3Data;






    [Tooltip("스킬1 선딜 최소치")]
    [SerializeField] protected float s1FirstDelayMin;
    [Tooltip("스킬1 선딜 최대치")]
    [SerializeField] protected float s1FirstDelayMax;
    [Tooltip("스킬1 후딜 최소치")]
    [SerializeField] protected float s1AfterDelayMin;
    [Tooltip("스킬1 후딜 최대치")]
    [SerializeField] protected float s1AfterDelayMax;

    [Tooltip("스킬2 선딜 최소치")]
    [SerializeField] protected float s2FirstDelayMin;
    [Tooltip("스킬2 선딜 최대치")]
    [SerializeField] protected float s2FirstDelayMax;
    [Tooltip("스킬2 후딜 최소치")]
    [SerializeField] protected float s2AfterDelayMin;
    [Tooltip("스킬2 후딜 최대치")]
    [SerializeField] protected float s2AfterDelayMax;

    [Tooltip("스킬3 선딜 최소치")]
    [SerializeField] protected float ultFirstDelayMin;
    [Tooltip("스킬3 선딜 최대치")]
    [SerializeField] protected float ultFirstDelayMax;
    [Tooltip("스킬3 후딜 최소치")]
    [SerializeField] protected float ultAfterDelayMin;
    [Tooltip("스킬3 후딜 최대치")]
    [SerializeField] protected float ultAfterDelayMax;


    protected Coroutine skillCoroutine;

    protected bool casting;

    public bool Casting { get { return casting; } set { casting = value; } }

    protected virtual void Start()
    {
        GetSkillId();
        skill1Data = CsvParser.Instance.SkillDic[skill1id];
        skill1Data.ReadEffectData();
        skill2Data = CsvParser.Instance.SkillDic[skill2id];
        skill2Data.ReadEffectData();

        if (owner.PlayerClassData.classType != ClassType.Excalibur)
        {
            skill3Data = CsvParser.Instance.SkillDic[skill3id];
            skill3Data.ReadEffectData();
        }

        s1FirstDelayMin = skill1Data.skEffect1_Data.minForeDelay;
        s1FirstDelayMax = skill1Data.skEffect1_Data.maxForeDelay;
        s1AfterDelayMin = skill1Data.skEffect1_Data.minAftDelay;
        s1AfterDelayMax = skill1Data.skEffect1_Data.maxAftDelay;
        s2FirstDelayMin = skill2Data.skEffect1_Data.minForeDelay;
        s2FirstDelayMax = skill2Data.skEffect1_Data.maxForeDelay;
        s2AfterDelayMin = skill2Data.skEffect1_Data.minAftDelay;
        s2AfterDelayMax = skill2Data.skEffect1_Data.maxAftDelay;


        if (owner.PlayerClassData.classType != ClassType.Excalibur)
        {
            ultFirstDelayMin = skill3Data.skEffect1_Data.minForeDelay;
            ultFirstDelayMax = skill3Data.skEffect1_Data.maxForeDelay;
            ultAfterDelayMin = skill3Data.skEffect1_Data.minAftDelay;
            ultAfterDelayMax = skill3Data.skEffect1_Data.maxAftDelay;
        }
    }

    public abstract void GetSkillId();
    /// <summary>
    /// 스킬 1 캐스팅유무를 구분
    /// </summary>
    /// <param name="player"></param>
    public abstract void Skill1_Ready(PlayerController player);
    /// <summary>
    /// 스킬 1 실행
    /// </summary>
    /// <param name="player"></param>
    public abstract void Skill1_Excute(PlayerController player, object parameter);
    /// <summary>
    /// 스킬 1이 구현될 메서드
    /// </summary>
    /// <param name="player"></param>
    public abstract void Skill1_Logic(PlayerController player, object parameter);
    /// <summary>
    /// 스킬 2 캐스팅유무를 구분
    /// </summary>
    /// <param name="player"></param>
    public abstract void Skill2_Ready(PlayerController player);
    /// <summary>
    /// 스킬 2 실행
    /// </summary>
    /// <param name="player"></param>
    public abstract void Skill2_Excute(object parameter);
    /// <summary>
    /// 스킬 2이 구현될 메서드
    /// </summary>
    /// <param name="player"></param>
    public abstract void Skill2_Logic(PlayerController player, object parameter);
    /// <summary>
    /// 궁극기의 캐스팅 유무 구분
    /// </summary>
    /// <param name="player"></param>
    public abstract void Ult_Ready(PlayerController player);
    /// <summary>
    /// 궁극기 실행
    /// </summary>
    /// <param name="player"></param>
    public abstract void Ult_Excute(object parameter);
    /// <summary>
    /// 궁극기가 구현될 메서드
    /// </summary>
    /// <param name="player"></param>
    /// <param name="pos"></param>
    public abstract void Ult_Logic(PlayerController player, object pos);

    /// <summary>
    /// 데미지를 주는 메서드
    /// </summary>
    /// <param name="damageableList">데미지를 받을 리스트</param>
    /// <param name="atkDamage">데미지수치</param>
    protected void InflictDamage(List<IDamageable> damageableList, float atkDamage)
    {
        foreach (var damageable in damageableList)
        {
            if (!isHit.ContainsKey(damageable))
            {
                damageable.TakeDamage(atkDamage);
                continue;
            }
            if (isHit[damageable])
            {
                Debug.Log("Already added Dictionary");
                continue;
            }
        }
    }
    /// <summary>
    /// 넉백을 가하는 메서드
    /// </summary>
    /// <param name="player">해당 지점으로부터 넉백을 가함</param>
    /// <param name="knockBackDistance">넉백 거리</param>
    /// <param name="knockBackSpeed">넉백 속도</param>
    protected void KnockBack(Transform player, float knockBackDistance, float knockBackSpeed)
    {

        foreach (var damageable in damageableList)
        {
            if (!isHit.ContainsKey(damageable))
            {
                damageable.TakeKnockBack(player.transform.position, knockBackDistance, knockBackSpeed);
                continue;
            }
            if (isHit[damageable])
            {
                continue;
            }
        }
    }
    /// <summary>
    /// 스킬을 활용한 넉백메서드. 넉백 위치가 플레이어로부터 일어나는게 아니라 스킬피격 위치에서 플레이어가 향하는 지점이 됨
    /// </summary>
    /// <param name="mouseDir">넉백 목표 위치</param>
    /// <param name="player">공격 시작 위치</param>
    /// <param name="knockBackDistance">넉백 거리</param>
    /// <param name="knockBackSpeed">넉백 속도</param>
    protected void KnockBackFromSkill(Vector3 mouseDir, Transform player, float knockBackDistance, float knockBackSpeed)
    {

        foreach (var damageable in damageableList)
        {
            if (!isHit.ContainsKey(damageable))
            {
                damageable.TakeKnockBackFromSkill(mouseDir, player.transform.position, knockBackDistance, knockBackSpeed);
                continue;
            }
            if (isHit[damageable])
            {
                continue;
            }

        }
    }
    /// <summary>
    /// 캐릭터의 상태를 넉백으로 바꿔줌 
    /// </summary>
    /// <param name="invincibleTime">무적시간</param>
    /// <param name="KnockBackTime">넉백시간(못움직이는 시간)</param>
    protected void KnockBackStateChange(float invincibleTime, float KnockBackTime)
    {
        foreach (var stateable in stateableList)
        {
            stateable.StateChange(PlayerState.Knockback, invincibleTime, KnockBackTime, true, false);
        }
    }
    /// <summary>
    /// 선딜과 후딜에 의한 캐릭터 상태변경과 스킬 구현
    /// </summary>
    /// <param name="firstDelay">선딜</param>
    /// <param name="afterDelay">후딜</param>
    /// <param name="skill">구현할 스킬 메서드</param>
    /// <returns></returns>
    public virtual IEnumerator SkillRoutine(float firstDelay, float afterDelay, Action skill)
    {
        owner.StateController.StateChange(PlayerState.SkillStart, 0, 0, true, false);
        owner.MoveSetter(false);
        owner.PlayerAnim.SetFloat("Move", 0);
        yield return new WaitForSeconds(firstDelay);
        skill();
        yield return new WaitForSeconds(afterDelay);
        owner.MoveSetter(true);
        owner.StateController.StateChange(PlayerState.SkillExit, 0, 0, true, false);

    }

    public void Hit()
    {
        if (skillCoroutine != null)
        {
            casting = false;
            StopCoroutine(skillCoroutine);
        }
        owner.SkillCut();
    }

}

