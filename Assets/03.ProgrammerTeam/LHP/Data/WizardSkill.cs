using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필 / 마법사의 스킬 데이터
/// </summary>
[Serializable]
public class WizardSkill : SkillData
{
    Vector3 targetDir;


    [SerializeField] RangeChecker rangeIns;

    [Header("Skill1")]
    [SerializeField] Icycle icycle;

    [Space(10)]
    [Header("Skill2")]

    [Space(10)]
    [Header("Ult")]
    [SerializeField] Vector2 range;
    [SerializeField] float inRange;
    [SerializeField] float waitTime;
    [SerializeField] int attackCount;
    [SerializeField] WizardUlt ult;




    RangeChecker ins;

    #region Skill1
    public override void Skill1_Ready(PlayerController player)
    {
        owner.AttackController.enabled = false;
        if (owner.photonView.IsMine)
        {
            if (!casting)
            {
                casting = true;
                ins = Instantiate(rangeIns, owner.transform.position, Quaternion.identity);
                Vector3 rangeScale = new Vector3(icycle.thisCol.size.x, skill1Data.skEffect1_Data.skillRange*0.5f);
                Debug.Log(rangeScale);
                float inRangeScale = 1f;
                ins.InitSkill(RangeType.NonTarget, rangeScale, inRangeScale, this, player);
                ins.AddSkill((direction) => owner.photonView.RPC("Skill1_Excute", RpcTarget.All, direction));

            }
            else
            {
                if (ins != null)
                    ins.Cancle();
                owner.AttackController.enabled = true;
                casting = false;
            }
        }


    }
    public override void Skill1_Excute(PlayerController player,object parameter )
    {
        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
        }
        Debug.Log("test");
        s1FirstDelay = Utils.CalculateDelay(s1FirstDelayMin, s1FirstDelayMax, owner.AttackController.AtkSpeed);
        float s1AfterDelay = Utils.CalculateDelay(s1AfterDelayMin, s1AfterDelayMax, owner.AttackController.AtkSpeed);
        owner.PlayerAnim.speed = 1 / s1FirstDelay+ s1AfterDelay;
        skillCoroutine = StartCoroutine(SkillRoutine(s1FirstDelay, s1AfterDelay, () => Skill1_Logic(owner, parameter)));
        player.PlayerAnim.SetTrigger("Skill2");
        player.AttackController.AttackPoint.WeaponAnim("Attack");
    }
    float s1FirstDelay;
    public override void Skill1_Logic(PlayerController player, object parameter)
    {

        Icycle ins = Instantiate(icycle, player.transform.position + (Vector3)parameter, Quaternion.identity).GetComponent<Icycle>();
        ins.transform.up = (Vector3)parameter;
        ins.delay = s1FirstDelay;
        ins.owner = player;
        ins.damage = player.FactorCalculate(skill1Data.skEffect1_Data.dmgFactor);
        ins.knockBackDis = skill1Data.skEffect1_Data.knockbackDis;
        ins.knockBackSpeed = skill1Data.skEffect1_Data.knockbackTime;
        ins.shootArriveTime = skill1Data.skEffect1_Data.susTime;
        Debug.Log(skill1Data.skEffect1_Data.susTime);
        Debug.Log(skill1Data.skEffect1_Data.knockbackDis);
        Debug.Log(skill1Data.skEffect1_Data.knockbackTime);
        Debug.Log(skill1Data.skEffect1_Data.unbeatableTime);
        ins.shootRange = skill1Data.skEffect1_Data.skillRange;

        ins.invTime = skill1Data.skEffect1_Data.unbeatableTime;
        ins.kbTime = skill1Data.skEffect1_Data.knockbackTime;
        StartCoroutine(ins.IceCreateRoutine());

    }
    #endregion
    #region Skill2
    public override void Skill2_Ready(PlayerController player)
    {
        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
        }

        float t = player.PlayerAnim.GetCurrentAnimatorStateInfo(0).length;
        float s2FirstDelay = Utils.CalculateDelay(s2FirstDelayMin, s2FirstDelayMax, owner.AttackController.AtkSpeed);
        float s2AfterDelay = Utils.CalculateDelay(s2AfterDelayMin, s2AfterDelayMax, owner.AttackController.AtkSpeed);
        owner.PlayerAnim.speed = 1 / s2FirstDelay;
        skillCoroutine = StartCoroutine(SkillRoutine(s2FirstDelay, s2AfterDelay, () => Skill2_Logic(owner, 0f)));
        player.PlayerAnim.SetTrigger("Skill1");
        player.AttackController.AttackPoint.WeaponAnim("Strike");



    }
    IEnumerator EffectRoutine(float t)
    {
        yield return new WaitForSeconds(t);
        owner.AttackController.AttackPoint.EffectAnim("Skill1");
    }
    public override void Skill2_Excute(object parameter)
    {
        
    }
    public override void Skill2_Logic(PlayerController player,object parameter)
    {
        targetDir = player.transform.position + player.AttackController.AttackPoint.MouseDir.normalized;
        damageableList.Clear();
        stateableList.Clear();
        IStateable stateable = null;
        #region CheckLogic
        if (owner.photonView.IsMine)
        {                                                                                           //공격범위 csv에 없어서 일반 공격범위로 임시사용
            int overlapCount = Physics2D.OverlapBoxNonAlloc(targetDir, new Vector2(player.AttackController.AtkRange, player.AttackController.AtkRange), player.AttackController.AttackPoint.transform.rotation.eulerAngles.z,
            damageableColliders, player.AttackController.DamageableMask);

            // 마우스 커서와 공격 가능한 객체 위치의 각도 계산하여 범위 안에 있는 공격 가능한 객체들만 damageableList에 담기
            for (int i = 0; i < overlapCount; i++)
            {
                // 자신이 체크되는 거 방지
                if (damageableColliders[i].gameObject.Equals(player.gameObject))
                {
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
                if (damageable != null)
                {
                    damageableList.Add(damageable);
                }
                stateable = damageableColliders[i].GetComponent<IStateable>();
            }
        }
        if (stateable != null)
        {
            stateableList.Add(stateable);
        }
        if(damageableList.Count >0)
        {
            StartCoroutine(EffectRoutine(0.13f));
        }
        #endregion
        InflictDamage(damageableList, player.FactorCalculate(skill2Data.skEffect1_Data.dmgFactor));
        KnockBack(owner.transform, skill2Data.skEffect1_Data.knockbackDis, skill2Data.skEffect1_Data.knockbackDis/skill2Data.skEffect1_Data.knockbackTime);   // 넉백
        Debug.Log($"dis : {skill2Data.skEffect1_Data.knockbackDis},time : {skill2Data.skEffect1_Data.knockbackTime}");
        KnockBackStateChange(skill2Data.skEffect1_Data.unbeatableTime, skill2Data.skEffect1_Data.knockbackTime);
       
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
                ins = Instantiate(rangeIns, owner.transform.position, Quaternion.identity);
                ins.InitSkill(RangeType.AoE, range, inRange, this, player);
                ins.AddSkill((range) => owner.photonView.RPC("Ult_Excute",RpcTarget.All, ins.SelectedRange.position));
            }
            else
            {
                if (ins != null)
                    ins.Cancle();
                owner.AttackController.enabled = true;
                casting = false;
            }
        }
    }

    public override void Ult_Logic(PlayerController player, object pos)
    {
        casting = false;
        owner.AttackController.enabled = true;
        StartCoroutine(WizardUltRoutine(player,(Vector3) pos));
    }
    public override void Ult_Excute(object pos)
    {
        owner.PlayerAnim.SetTrigger("Ult");
        owner.AttackController.AttackPoint.WeaponAnim("Magic");
        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
        }
        float ultFirstDelay = Utils.CalculateDelay(ultFirstDelayMin, ultFirstDelayMax, owner.AttackController.AtkSpeed);
        float ultAfterDelay = Utils.CalculateDelay(ultAfterDelayMin, ultAfterDelayMax, owner.AttackController.AtkSpeed);
        owner.PlayerAnim.speed = 1 / ultFirstDelay;
        skillCoroutine = StartCoroutine(SkillRoutine(ultFirstDelay, ultAfterDelay, () => Ult_Logic(owner, ( Vector3 )pos)));
    }

    IEnumerator WizardUltRoutine(PlayerController player, Vector3 pos)
    {

        yield return new WaitForSeconds(waitTime);
        for (int i = 0; i < attackCount; i++)
        {
            WizardUlt ins = Instantiate(ult, (Vector3)pos + new Vector3(0, 8), Quaternion.Euler(0, 0, -90f)).GetComponent<WizardUlt>();
            ins.damagableMask = player.AttackController.DamageableMask;

            ins.dmg = player.FactorCalculate(skill3Data.skEffect1_Data.dmgFactor);
            ins.kbTime = skill3Data.skEffect1_Data.knockbackTime;
            ins.invTime = skill3Data.skEffect1_Data.unbeatableTime;
            StartCoroutine(ins.Active(( Vector3 )pos, range.x, range.y, player, i % 2 == 0));
            yield return new WaitForSeconds(1f);
        }
    }
    protected override void Start()
    {
        base.Start();
    }
    public override void GetSkillId()
    {
        skill1id = 1002010;
        skill2id = 1002011;
        skill3id = 1002012;
    }


    #endregion
}
