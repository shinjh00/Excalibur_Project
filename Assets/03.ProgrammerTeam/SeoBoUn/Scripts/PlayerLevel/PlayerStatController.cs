using JetBrains.Annotations;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 개발자 : 서보운
/// <br/>플레이어 스탯 컨트롤러
/// </summary>
public class PlayerStatController : BaseController
{
    [Tooltip("디버깅용 플레이어 레벨(에디터 할당 x)")]
    [SerializeField] int playerLevel;
    [Tooltip("디버깅용 플레이어 레벨업 포인트(에디터 할당 x)")]
    [SerializeField] int levelUpPoint;
    [Tooltip("디버깅용 플레이어 스킬업 포인트(에디터 할당 x)")]
    [SerializeField] int skillPoint;

    [Tooltip("디버깅용 현재 경험치(에디터 할당 x)")]
    [SerializeField] int curExp;
    [Tooltip("디버깅용 다음 경험치(에디터 할당 x)")]
    [SerializeField] int nextExp;
    [Tooltip("디버깅용 idValue / 기사 1041001, 전사 1042001...(에디터 할당 x)")]
    [SerializeField] int idValue;

    [Tooltip("디버깅용 스탯 레벨 / 공격력, 방어력, 공격속도, 최대체력 (에디터 할당 x)")]
    [SerializeField] int[] statLevel;
    [Tooltip("디버깅용 해금 스킬 / Q, E, R (에디터 할당 x)")]
    [SerializeField] bool[] unLockSkills;

    // 포인트 레벨업 시 오를 스탯들
    LevelData levelPerData;
    // 현재 레벨에 맞는 기본 데이터
    LevelData curData;
    // 최종 데이터
    LevelData totalData = new LevelData();

    public LevelData CurData { get { return curData; } }

    public float equipMaxHpRate;           // 장비로 올라가는 최대 체력 비율
    public float equipDefenseRate;         // 장비로 올라가는 방어력 비율
    public float equipAtkDamageRate;       // 장비로 올라가는 어택 데미지 비율
    public float equipAtkSpeedRate;        // 장비로 올라가는 어택 스피드 비율
    public float equipMoveSpeedRate;       // 장비로 올라가는 이동 속도 비율

    Dictionary<StatLevel, List<float>> buffRates;   // 퍼센트 버프값
    Dictionary<StatLevel, List<int>> buffAmounts;   // 고정수치 버프값

    [Tooltip("플레이어 스탯 UI(에디터 할당 필요)")]
    [SerializeField] PlayerStatUI playerStatUI;
    [Tooltip("플레이어 스킬 선택 UI(에디터 할당 필요)")]
    [SerializeField] PlayerSkillSelectUI playerSkillSelectUI;
    [Tooltip("플레이어 경험치바 UI(에디터 할당 x)")]
    [SerializeField] PlayerExpbarUI playerExpbarUI;

    public bool[] UnLockSkills { get { return unLockSkills; } }
    public int[] StatLevels { get { return statLevel; } set { statLevel = value; } }

    // 레벨이 변경되었을 때 반응할 액션(UI 변동)
    public Action<LevelData, int> OnChangeLevel;
    // 특정 스킬이 해금되었을 때 반응할 액션(버튼이 더 이상 눌리지 못하도록)
    public Action<SkillButton, int> OnChangeSkillUnLock;

    public int PlayerLevel { get { return playerLevel; } }
    public PlayerStatUI PlayerStatUI { get { return playerStatUI; } }

    protected override void GetStat()
    {
        statLevel = new int[(int)StatLevel.Size];
        unLockSkills = new bool[(int)SkillButton.Size];
        if (!photonView.IsMine)
        {
            this.enabled = false;
            if (playerExpbarUI != null)
            {
                playerExpbarUI.gameObject.SetActive(false);
            }
            return;
        }

        buffRates = new Dictionary<StatLevel, List<float>>();
        buffAmounts = new Dictionary<StatLevel, List<int>>();

        buffRates.Add(StatLevel.Atk, new List<float>());
        buffRates.Add(StatLevel.AtkSpd, new List<float>());
        buffRates.Add(StatLevel.Def, new List<float>());
        buffRates.Add(StatLevel.MoveSpeed, new List<float>());
        buffRates.Add(StatLevel.MaxHp, new List<float>());
        
        buffAmounts.Add(StatLevel.Atk, new List<int>());
        buffAmounts.Add(StatLevel.AtkSpd, new List<int>());
        buffAmounts.Add(StatLevel.Def, new List<int>());
        buffAmounts.Add(StatLevel.MoveSpeed, new List<int>());
        buffAmounts.Add(StatLevel.MaxHp, new List<int>());

        playerExpbarUI = FindObjectOfType<PlayerExpbarUI>();

        levelUpPoint = -1;
        skillPoint = 0;
        playerSkillSelectUI.OnChangePoint(skillPoint);
        switch (owner.PlayerClassData.classType)
        {
            case ClassType.Knight:
                idValue = LevelData.KNIGHTID;
                break;
            case ClassType.Warrior:
                idValue = LevelData.WARRIORID;
                 break;
            case ClassType.Wizard:
                idValue = LevelData.WIZARDID;
                break;
            case ClassType.Archer:
                idValue = LevelData.ARCHERID;
                break;
            case ClassType.Excalibur:
                idValue = LevelData.EXCALIBURID;
                break;
        }
        playerLevel = 0;

        OnChangeLevel += playerStatUI.ChangeUI;
        OnChangeSkillUnLock += playerSkillSelectUI.OnChangeUnLock;
        playerStatUI.StatUpEvent += StatPointUp;
        playerSkillSelectUI.UnLockSkillEvent += UnLockSkill;
        if (playerExpbarUI != null)
        {
            playerExpbarUI.ChangeMaxExp += SetNextExp;
        }

        LevelUp();

        levelPerData = CsvParser.Instance.LevelDic[idValue];
        if(playerExpbarUI!=null)
        playerExpbarUI.Init(curData.requiredExp);
    }

    /// <summary>
    /// exp를 얻을 메소드
    /// </summary>
    /// <param name="exp"></param>

    [PunRPC]
    public void GetExp(int exp)
    {
        if(!photonView.IsMine)
        {
            return;
        }
        StartCoroutine(playerExpbarUI.SetExp(curExp, curExp + exp));
        owner.HealthController.FloatingMessage($"+{exp}Exp",MessageType.Exp);
        curExp += exp;

        while(curExp >= nextExp)
        {
            int fillValue = curExp - nextExp;

            curExp = fillValue;
            owner.HealthController.FloatingMessage($"Level Up", MessageType.Level,1);
            LevelUp();
        }
    }

    /// <summary>
    /// 개발자 : 서보운
    /// <br/> 레벨업 메소드
    /// </summary>    
    public void LevelUp()
    {
        playerLevel++;
        levelUpPoint++;

        playerStatUI.ChangeLevel(playerLevel);

        if (playerLevel == 2 || playerLevel == 4 || playerLevel == 7)
        {   // 2, 4, 7레벨에 스킬을 찍을 수 있도록 포인트 설정
            skillPoint++;
            playerSkillSelectUI.OnChangePoint(skillPoint);
        }

        ApplyStat();
    }

    // 디버깅용 메소드(경험치 10 획득)
    [ContextMenu("GetExp")]
    public void GetExp()
    {
        curExp += 10;

        if(curExp >= nextExp)
        {
            int fillValue = curExp - nextExp;

            curExp = fillValue;
            LevelUp();
        }
    }

    /// <summary>
    /// 스탯업 포인트용
    /// </summary>
    public void StatPointUp()
    {
        PlayerStat targetStat = playerStatUI.PointUp.Invoke();
        playerStatUI.PointUp = null;

        if (levelUpPoint > 0)
        {
            switch (targetStat)
            {
                case PlayerStat.Hp:
                    statLevel[(int)StatLevel.MaxHp]++;
                    break;
                case PlayerStat.AtkDamage:
                    statLevel[(int)StatLevel.Atk]++;
                    break;
                case PlayerStat.AtkSpeed:
                    statLevel[(int)StatLevel.AtkSpd]++;
                    break;
                case PlayerStat.Defense:
                    statLevel[(int)StatLevel.Def]++;
                    break;
            }
            levelUpPoint--;
            ApplyStat();
        }
        else
        {
            Debug.Log("포인트 부족");
        }
    }

    /// <summary>
    /// 스킬업 포인트용
    /// </summary>
    public void UnLockSkill()
    {
        SkillButton targetSkill = playerSkillSelectUI.PointUp.Invoke();
        playerSkillSelectUI.PointUp = null;

        if(skillPoint > 0)
        {
            unLockSkills[(int)targetSkill] = true;
            skillPoint--;

            OnChangeSkillUnLock(targetSkill, skillPoint);
        }
    }

    /// <summary>
    /// 변경된 레벨, 스탯 포인트에 따른 실제 스탯 적용메소드
    /// </summary>
    public void ApplyStat()
    {   
        if(playerLevel <= 0)
        {
            return;
        }
        curData = CsvParser.Instance.LevelDic[idValue + playerLevel];
        // (기본 스탯 + 레벨업 포인트로 인한 스탯) * (장비 / 포션에 대한 증감 비율치) -> 최종 스탯
        // 최종 스탯에 버프, 디버프 비율 적용

        // 레벨업당 오를 능력치 * 선택한 스탯 레벨
        curData.atk += levelPerData.atk * statLevel[(int)StatLevel.Atk];
        curData.atkSpeed += levelPerData.atkSpeed * statLevel[(int)StatLevel.AtkSpd];
        curData.maxHp += levelPerData.maxHp * statLevel[(int)StatLevel.MaxHp];
        curData.def += levelPerData.def * statLevel[(int)StatLevel.Def];

        // 장비 계산식 할당 필요
        curData.atk *= (1f + (equipAtkDamageRate * 0.001f));
        curData.atkSpeed *= (1f + (equipAtkSpeedRate * 0.001f));
        curData.def *= (1f + (equipDefenseRate * 0.001f));
        curData.maxHp *= (1f + (equipMaxHpRate * 0.001f));
        curData.moveSpeed *= (1f + (equipMoveSpeedRate * 0.001f));

        ApplyBuffStat();
    }

    /// <summary>
    /// 개발자: 이예린 / 장비 아이템 장착에 따른 캐릭터 스텟 변화
    /// </summary>
    /// <param name="stats">변화시킬 스탯 리스트</param>
    /// <param name="statValues">스탯 변화량 리스트</param>
    /// <param name="add">스탯을 더하는지 여부</param>
    public void ApplyEquipStat(List<IncrementalStat> stats, List<float> statValues, bool add)
    {
        if (add)
        {
            for (int i = 0; i < stats.Count; i++)
            {
                if (stats[i].Equals(IncrementalStat.Offensive))
                {
                    equipAtkDamageRate += statValues[i];
                }
                else if (stats[i].Equals(IncrementalStat.Defense))
                {
                    equipDefenseRate += statValues[i];
                }
                else if (stats[i].Equals(IncrementalStat.AttackSpeed))
                {
                    equipAtkSpeedRate += statValues[i];
                }
                else if (stats[i].Equals(IncrementalStat.HP))
                {
                    equipMaxHpRate += statValues[i];
                }
                else if (stats[i].Equals(IncrementalStat.MoveSpeed))
                {
                    equipMoveSpeedRate += statValues[i];
                }
            }
        }
        else
        {
            for (int i = 0; i < stats.Count; i++)
            {
                if (stats[i].Equals(IncrementalStat.Offensive))
                {
                    equipAtkDamageRate -= statValues[i];
                }
                else if (stats[i].Equals(IncrementalStat.Defense))
                {
                    equipDefenseRate -= statValues[i];
                }
                else if (stats[i].Equals(IncrementalStat.AttackSpeed))
                {
                    equipAtkSpeedRate -= statValues[i];
                }
                else if (stats[i].Equals(IncrementalStat.HP))
                {
                    equipMaxHpRate -= statValues[i];
                }
                else if (stats[i].Equals(IncrementalStat.MoveSpeed))
                {
                    equipMoveSpeedRate -= statValues[i];
                }
            }
        }

        ApplyStat();
    }

    /// <summary>
    /// 플레이어에게 버프를 추가하기 위한 메소드(일정 퍼센트값)
    /// <br/> AddBuffStat(StatLevel.Atk, 1.5f) -> 공격력 50% 증가 버프
    /// <br/> AddBuffStat(StatLevel.MaxHp, 0.5f) -> 최대 체력 50% 감소 버프
    /// </summary>
    /// <param name="targetStat"></param>
    /// <param name="rate"></param>
    public void AddBuffStat(StatLevel targetStat, float rate)
    {
        if(!photonView.IsMine)
        {
            return;
        }

        buffRates[targetStat].Add(rate);
        ApplyStat();
    }

    /// <summary>
    /// 특정 버프를 삭제하기 위한 메소드(비율값)
    /// <br/> RemoveBuffStat(StatLevel.MaxHp, 0.2f) -> 최대 체력 0.2배 디버프 지우기
    /// </summary>
    /// <param name="targetStat"></param>
    /// <param name="rate"></param>
    public void RemoveBuffStat(StatLevel targetStat, float rate)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        buffRates[targetStat].Remove(rate);
        ApplyStat();
    }

    /// <summary>
    /// 특정 버프를 추가하기 위한 메소드(고정 수치값)
    /// </summary>
    /// <param name="targetStat"></param>
    /// <param name="amout"></param>
    public void AddBuffStat(StatLevel targetStat, int amount)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        buffAmounts[targetStat].Add(amount);
        ApplyStat();
    }
    public bool ContainBuff(StatLevel targetStat, float rate)
    {
        if (buffRates[targetStat].Contains(rate))
        {
            return true;
        }
        return false;
    }
   /* public void ContainBuff(StatLevel targetStat, int amount)
    {
        if (buffRates[targetStat].Contains(amount))
        {

        }
    }*/
    /// <summary>
    /// 특정 버프를 삭제하기 위한 메소드(고정 수치값)
    /// </summary>
    /// <param name="targetStat"></param>
    /// <param name="amount"></param>
    public void RemoveBuffStat(StatLevel targetStat, int amount)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        buffAmounts[targetStat].Remove(amount);
        ApplyBuffStat();
    }

    private void ApplyBuffStat()
    {
        // 버프는 합연산 먼저 수행 후 디버프는 개수당 곱연산으로 계산
        // ex) 1.7배, 1.5배, 0.3배, 0.5배 버프가 동시에 걸려있다면
        // 1 + (0.7 + 0.5) => 2.2 * 0.3 * 0.5 로 계산할 것.
        
        for(int i = 0; i < buffRates.Count; i++)
        {
            // 1. 비율 계산하기
            float plusTotalRate = 1f;
            float minusTotalRate = 1f;

            for(int j = 0; j < buffRates[(StatLevel)i].Count; j++)
            {
                if (buffRates[(StatLevel)i][j] >= 1f)
                {   // 버프
                    plusTotalRate += (buffRates[(StatLevel)i][j] - 1f);
                }
                else
                {   // 디버프
                    minusTotalRate *= (buffRates[(StatLevel)i][j]);
                }
            }
            float totalRate = plusTotalRate * minusTotalRate;

            switch (i)
            {
                case (int)StatLevel.Atk:
                    totalData.atk = curData.atk * totalRate;
                    break;
                case (int)StatLevel.AtkSpd:
                    totalData.atkSpeed = curData.atkSpeed * totalRate;
                    owner.PlayerAnim.speed = totalData.atkSpeed;
                    break;
                case (int)StatLevel.MoveSpeed:
                    totalData.moveSpeed = curData.moveSpeed * totalRate;
                    break;
                case (int)StatLevel.Def:
                    totalData.def = curData.def * totalRate;
                    break;
                case (int)StatLevel.MaxHp:
                    totalData.maxHp = curData.maxHp * totalRate;
                    break;
            }
        }

        // 최종 스탯에 고정 수치값 적용
        for(int i = 0; i < buffAmounts.Count; i++)
        {
            // 1. 고정 수치값 계산하기
            int totalAmount = 0;

            for(int j = 0; j < buffAmounts[(StatLevel)i].Count; j++)
            {
                totalAmount += (buffAmounts[(StatLevel)i][j]);
            }

            switch (i)
            {   // 2. 고정 수치 스탯에 따른 최종 스탯 적용(합연산)
                case (int)StatLevel.Atk:
                    totalData.atk = totalData.atk + totalAmount;
                    break;
                case (int)StatLevel.AtkSpd:
                    totalData.atkSpeed = totalData.atkSpeed + totalAmount;
                    owner.PlayerAnim.speed = totalData.atkSpeed;
                    break;
                case (int)StatLevel.MoveSpeed:
                    totalData.moveSpeed = totalData.moveSpeed + totalAmount;
                    break;
                case (int)StatLevel.Def:
                    totalData.def = totalData.def + totalAmount;
                    break;
                case (int)StatLevel.MaxHp:
                    totalData.maxHp = totalData.maxHp + totalAmount;
                    break;
            }
        }

        owner.AttackController.AtkDamage = totalData.atk;
        owner.AttackController.AtkSpeed = totalData.atkSpeed;
        owner.HealthController.MaxHp = totalData.maxHp;
        if (owner.HealthController.hpBarUI != null)
        {
            owner.HealthController.hpBarUI.ChangeMaxHp(totalData.maxHp);
        }
        owner.HealthController.Defense = totalData.def;
        owner.MoveController.MoveSpeed = totalData.moveSpeed;
        nextExp = curData.requiredExp;

        OnChangeLevel?.Invoke(totalData, levelUpPoint);
    }

    public void SetNextExp()
    {
        playerExpbarUI.SetNextExp(curData.requiredExp);
    }

    /// <summary>
    /// 엑스칼리버 캐릭터 변신 시 레벨 변경 메소드
    /// </summary>
    /// <param name="targetLevel"></param>
    public void ChangeLevel(int targetLevel)
    {
        idValue = LevelData.EXCALIBURID;

        for(int i = 1; i < targetLevel; i++)
        {
            LevelUp();
        }
    }

    /// <summary>
    /// 엑스칼리버 캐릭터 변신 시 장비 아이템 효과 전송 메소드
    /// <br/>(Hp, Dmg, AtkSpd, Def, MoveSpd)
    /// </summary>
    public void ChangeExcaliburStat(float[] equip)
    {
        equipMaxHpRate = equip[0];
        equipAtkDamageRate = equip[1];
        equipAtkSpeedRate = equip[2];
        equipDefenseRate = equip[3];
        equipMoveSpeedRate = equip[4];
    }

    /// <summary>
    /// 해금할 스킬
    /// <br/> Q, E, R
    /// </summary>
    public enum SkillButton { Q, E, R, Size }
}

/// <summary>
/// 스탯 레벨
/// <br/> Atk, Def, AtkSpd, MaxHp, MoveSpeed
/// </summary>
public enum StatLevel { Atk, Def, AtkSpd, MaxHp, MoveSpeed, Size }