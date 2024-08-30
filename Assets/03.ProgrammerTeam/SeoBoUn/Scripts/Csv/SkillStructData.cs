using System;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 개발자 : 서보운
/// <br/> 스킬 데이터 테이블
/// </summary>
[Serializable]
public struct SkillInfoData : ICsvReadable
{
    public int id;
    public string name;         // 이름
    public int skUser;          // 스킬 사용가능 직군
    public int buyPrice;        // 스킬 가격
    public int skEffect1;       // 스킬효과1(ID)
    public SkillEffectData skEffect1_Data;
    public int skEffect2;       // 스킬효과2(ID)
    public SkillEffectData skEffect2_Data;
    public int skEffect3;       // 스킬효과3(ID)
    public SkillEffectData skEffect3_Data;
    public float coolTime;      // 스킬 쿨타임
    public int icon;            // 스킬 아이콘
    public int description;     // 스킬 설명 텍스트
    public int flavorText;      // 스킬 플레이버 텍스트

    public void CsvRead(string[] elements)
    {
        int index = 0;

        id = int.Parse(elements[index++]);
        name = elements[index++];
        skUser = int.Parse(elements[index++]);
        buyPrice = int.Parse(elements[index++]);
        skEffect1 = int.Parse(elements[index++]);
        skEffect2 = int.Parse(elements[index++]);
        skEffect3 = int.Parse(elements[index++]);
        coolTime = float.Parse(elements[index++]);
        icon = int.Parse(elements[index++]);
        description = int.Parse(elements[index++]);
        flavorText = int.Parse(elements[index++]);
    }

    public void ReadEffectData()
    {
        if(skEffect1 != -9999)
        {
            skEffect1_Data = CsvParser.Instance.SkillEffectDic[skEffect1];
        }
        if (skEffect2 != -9999)
        {
            skEffect2_Data = CsvParser.Instance.SkillEffectDic[skEffect2];
        }
        if(skEffect3 != -9999)
        {
            skEffect3_Data = CsvParser.Instance.SkillEffectDic[skEffect3];
        }
    }
}

/// <summary>
/// 개발자 : 서보운
/// <br/> 스킬 효과 데이터 테이블
/// </summary>
[Serializable]
public struct SkillEffectData : ICsvReadable
{
    public int id;
    public string name;             // 스킬 이름
    public int target;              // 스킬 적용 대상(1 : 적군, 2 : 아군, 4 : 본인)
    public int type;                // 스킬 유형
    public float dmgFactor;         // 데미지 계수
    public float defFactor;         // 방어력 계수
    public float hpFactor;          // 체력 계수
    public int defuff;              // 적용 상태이상
    public float minForeDelay;      // 최소 선딜레이
    public float maxForeDelay;      // 최대 선딜레이
    public float minAftDelay;       // 최소 후딜레이
    public float maxAftDelay;       // 최대 후딜레이 
    public float reuseTime;         // 재사용 가능 시간
    public float skillRange;        // 스킬 사정거리
    public float skillSpd;          // 스킬 속도
    public float susTime;           // 스킬지속시간
    public float knockbackDis;      // 넉백 거리
    public float knockbackTime;     // 넉백 시간
    public float unbeatableTime;    // 무적 시간
    public int effect;              // 이펙트
    public int beforeSound;         // 선딜 사운드
    public int mainSound;           // 메인 사운드
    public int afterSound;          // 후딜 사운드
    public int animation1;          // 애니메이션1
    public int animation2;          // 애니메이션2
    public int animation3;          // 애니메이션3

    public void CsvRead(string[] elements)
    {
        int index = 0;

        id = int.Parse(elements[index++]);
        name = elements[index++];
        target = int.Parse(elements[index++]);
        type = int.Parse(elements[index++]);
        dmgFactor = float.Parse(elements[index++]);
        defFactor = float.Parse(elements[index++]);
        hpFactor = float.Parse(elements[index++]);
        defuff = int.Parse(elements[index++]);
        minForeDelay = float.Parse(elements[index++]);
        maxForeDelay = float.Parse(elements[index++]);
        minAftDelay = float.Parse(elements[index++]);
        maxAftDelay = float.Parse(elements[index++]);
        reuseTime = float.Parse(elements[index++]);
        skillRange = float.Parse(elements[index++]);
        skillSpd = float.Parse(elements[index++]);
        susTime = float.Parse(elements[index++]);
        knockbackDis = float.Parse(elements[index++]);
        knockbackTime = float.Parse(elements[index++]);
        unbeatableTime = float.Parse(elements[index++]);
        effect = int.Parse(elements[index++]);
        beforeSound = int.Parse(elements[index++]);
        mainSound = int.Parse(elements[index++]);
        afterSound = int.Parse(elements[index++]);
        animation1 = int.Parse(elements[index++]);
        animation2 = int.Parse(elements[index++]);
        animation3 = int.Parse(elements[index++]);
    }
}