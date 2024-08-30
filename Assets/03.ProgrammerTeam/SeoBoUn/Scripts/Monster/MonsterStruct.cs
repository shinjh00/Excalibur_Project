using System;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 몬스터 스탯 구조체
/// </summary>
[Serializable]
public struct MonsterData : ICsvReadable, IFireBaseReadable
{
    public const int MONSTERID = 1200000;

    [Tooltip("몬스터의 id")] public int id;
    [Tooltip("몬스터의 이름")] public string name;
    [Tooltip("탐지 거리")] public float detectionRange;
    [Tooltip("탐지 시간")] public float detectionTime;
    [Tooltip("횃불 탐지 거리")] public float torchDetectionRange;
    [Tooltip("체력")] public float hp;
    [Tooltip("데미지")] public float damage;
    [Tooltip("어택 사거리")] public float attackRange;
    [Tooltip("어택 스피드")] public float attackSpeed;
    [Tooltip("이동 속도")] public int moveSpeed;
    [Tooltip("경험치")] public int exp;
    [Tooltip("몬스터 상자")] public int monsterBoxID;

    public MonsterType monsterType;

    public int animIdleID;
    public int animMoveID;
    public int animAtkID;
    public int animHittedID;
    public int animDeathID;

    public int soundAtkID;
    public int soundHittedID;
    public int soundDeathID;
    public int soundChaseID;

    public void CsvRead(string[] elements)
    {
        int index = 0;

        id = int.Parse(elements[index++]);
        name = elements[index++];
        detectionRange = float.Parse(elements[index++]);
        detectionTime = float.Parse(elements[index++]);
        torchDetectionRange = float.Parse(elements[index++]);
        hp = int.Parse(elements[index++]);
        damage = int.Parse(elements[index++]);
        attackRange = float.Parse(elements[index++]);
        attackSpeed = float.Parse(elements[index++]);
        moveSpeed = int.Parse(elements[index++]);
        exp = int.Parse(elements[index++]);
        monsterBoxID = int.Parse(elements[index++]);

        animIdleID = int.Parse(elements[index++]);
        animMoveID = int.Parse(elements[index++]);
        animAtkID = int.Parse(elements[index++]);
        animHittedID = int.Parse(elements[index++]);
        animDeathID = int.Parse(elements[index++]);

        soundAtkID = int.Parse(elements[index++]);
        soundHittedID = int.Parse(elements[index++]);
        soundDeathID = int.Parse(elements[index++]);
        soundChaseID = int.Parse(elements[index++]);
    }

    public MonsterType ReturnType()
    {
        return (MonsterType)id;
    }

    public FirebaseReadData FireBaseString()
    {
        return new FirebaseReadData("MonsterData", id, PlayDataID.Monster);
    }
}

/// <summary>
/// 몹 스폰 정보
/// <br/> id, roomType, minMobNum, maxMobNum, mobTypeCount, mobSpawns(몹 스폰에 대한 정보 구조체 배열)
/// </summary>
[Serializable]
public struct MonsterSpawnData : ICsvReadable, IFireBaseReadable
{
    public const int ID = 1300000;

    public int id;                      // ID
    public Define.RoomType roomType;    // 방 정보
    public int minMobNum;               // 최소 스폰 수
    public int maxMobNum;               // 최대 스폰 수
    public int mobTypeCount;            // 스포너가 소환할 몹의 종류 수
    public MobSpawn[] mobSpawns;        // 몹 스폰에 대한 정보(몹ID, 최소 수, 최대 수)

    public void CsvRead(string[] elements)
    {
        int index = 0;

        id = int.Parse(elements[index++]);
        roomType = (Define.RoomType)int.Parse(elements[index++]);
        minMobNum = int.Parse(elements[index++]);
        maxMobNum = int.Parse(elements[index++]);
        mobTypeCount = int.Parse(elements[index++]);

        mobSpawns = new MobSpawn[mobTypeCount];

        for(int i = 0; i < mobTypeCount; i++)
        {
            mobSpawns[i].mobID = int.Parse(elements[index++]);
            mobSpawns[i].mobMinCount = int.Parse(elements[index++]);
            mobSpawns[i].mobMaxCount = int.Parse(elements[index++]);
        }
    }

    public FirebaseReadData FireBaseString()
    {
        return new FirebaseReadData("MonsterSpawnData", ID, PlayDataID.Monster);
    }

    // 몹 스폰에 대한 정보를 담을 구조체(id, 최소 수, 최대 수)
    public struct MobSpawn
    {
        public int mobID;
        public int mobMinCount;
        public int mobMaxCount;
    }
}

/// <summary>
/// 몬스터 데이터 번들
/// <br/> id, explanation, mobSpanwerCount, mobSpawnIDs(배열)
/// </summary>
[Serializable]
public struct MonsterSpawnBundle : ICsvReadable
{
    public int id;                  // id
    public string explanation;      // 설명
    public int mobSpawnerCount;     // 몹 스폰 테이블 ID의 수
    public int[] mobSpawnIDs;       // 몹 스폰 테이블 ID들

    public void CsvRead(string[] elements)
    {
        int index = 0;

        id = int.Parse(elements[index++]);
        explanation = elements[index++];
        mobSpawnerCount = int.Parse(elements[index++]);
        mobSpawnIDs = new int[mobSpawnerCount];

        for(int i = 0; i < mobSpawnIDs.Length; i++)
        {
            mobSpawnIDs[i] = int.Parse(elements[index++]);
        }
    }
}
public enum MonsterAttackType
{
    Melee,
    Range
}


[Serializable]
public enum MonsterType
{
    Slime = 1200000,              
    Orc,
    Skeleton,
    ArmorOrc,
    ArmorSkeleton,
    Elite_Orc,
    LowMimic,
    MiddleMimic
}