using System;
using System.Collections.Generic;

/// <summary>
/// 개발자 : 서보운
/// 임시 플레이어 스탯 데이터 구조체
/// </summary>
[Serializable]
public struct PlayerStatData : ICsvReadable
{
    public int id;
    public ClassType classType;
    public float damage;
    public float attackSpeed;
    public float hp;
    public float defence;
    public float moveSpeed;

    public void CsvRead(string[] elements)
    {
        id = int.Parse(elements[0]);
        classType = (ClassType)int.Parse(elements[1]);
        damage = float.Parse(elements[2]);
        attackSpeed = float.Parse(elements[3]);
        hp = int.Parse(elements[4]);
        defence = float.Parse(elements[5]);
        moveSpeed = float.Parse(elements[6]);
    }
}

/// <summary>
/// 박스 테이블 구조체
/// </summary>
[Serializable]
public struct BoxData : ICsvReadable, IFireBaseReadable
{
    public int id;                  // 상자 ID
    public string name;             // 상자 이름
    public BoxType boxType;         // 상자 유형
    public BoxRank boxRank;         // 상자 종류
    public int weight;            // 스폰 가중치
    public int dropTableID;         // 드랍 테이블ID
    public BoxDropTable dropTable;  // 실제 드랍 테이블
    public int openAnim;            // 열리는 애니메이션
    public int closeAnim;           // 닫히는 애니메이션
    public int openSound;           // 열리는 소리
    public int closeSound;          // 닫히는 소리
    public List<int> boxItemsID;    // 박스안의 아이템(테이블 x)

    public void CsvRead(string[] elements)
    {
        int index = 0;

        id = int.Parse(elements[index++]);
        name = elements[index++];
        boxType = (BoxType)int.Parse(elements[index++]);
        boxRank = (BoxRank)int.Parse(elements[index++]);
        weight = int.Parse(elements[index++]);
        dropTableID = int.Parse(elements[index++]);
        openAnim = int.Parse(elements[index++]);
        closeAnim = int.Parse(elements[index++]);
        openSound = int.Parse(elements[index++]);
        closeSound = int.Parse(elements[index++]);

        boxItemsID = new List<int>();
    }

    public FirebaseReadData FireBaseString()
    {
        return new FirebaseReadData("BoxData", id, PlayDataID.Box);
    }

    public void SetTable()
    {
        dropTable = CsvParser.Instance.BoxDropTableDic[dropTableID];
    }
}

/// <summary>
/// 박스의 아이템 스폰율 구조체
/// </summary>
[Serializable]
public struct BoxDropTable : ICsvReadable
{
    public int id;
    public float weaponWeight;
    public float armorWeight;

    public float[] equipmentWeights;    // 4가지(일반, 희귀, 유물, 전설)
    public float[] potionWeights;       // 3가지(일반, 희귀, 유물)
    public float[] foodWeights;         // 2가지(일반, 희귀)
    public float[] buffPotionWeight;    // 1가지(일반)
    public float[] cashbackWeights;     // 4가지(일반, 희귀, 유물, 전설)

    public int repeatCount;             // 반복 횟수
    public int maxEquipmentCount;       // 장비 최대 수
    public int maxPotionCount;          // 포션 최대 수
    public int maxFoodCount;            // 식량 최대 수
    public int maxBuffPotionCount;      // 버프포션 최대 수
    public int maxCashbackCount;        // 환금 최대 수

    public bool isConfirm;              // 확정지급 여부
    public int confirmCount;            // 확정 지급 아이템의 수

    public List<ConfirmStruct> confirmStructs;

    public void CsvRead(string[] elements)
    {
        confirmStructs = new List<ConfirmStruct>();

        equipmentWeights = new float[(int)Rank.Size];
        potionWeights = new float[(int)Rank.Size - 1];
        foodWeights = new float[(int)Rank.Size - 2];
        buffPotionWeight = new float[(int)Rank.Size - 3];
        cashbackWeights = new float[(int)Rank.Size];

        int index = 0;

        id = int.Parse(elements[index++]);
        weaponWeight = float.Parse(elements[index++]);
        armorWeight = 1f - weaponWeight;

        for (int i = 0; i < equipmentWeights.Length; i++)
        {
            equipmentWeights[i] = float.Parse(elements[index++]);
        }

        for (int i = 0; i < potionWeights.Length; i++)
        {
            potionWeights[i] = float.Parse(elements[index++]);
        }

        for (int i = 0; i < foodWeights.Length; i++)
        {
            foodWeights[i] = float.Parse(elements[index++]);
        }

        for (int i = 0; i < buffPotionWeight.Length; i++)
        {
            buffPotionWeight[i] = float.Parse(elements[index++]);
        }

        for (int i = 0; i < cashbackWeights.Length; i++)
        {
            cashbackWeights[i] = float.Parse(elements[index++]);
        }

        repeatCount = int.Parse(elements[index++]);
        maxEquipmentCount = int.Parse(elements[index++]);
        maxPotionCount = int.Parse(elements[index++]);
        maxFoodCount = int.Parse(elements[index++]);
        maxBuffPotionCount = int.Parse(elements[index++]);
        maxCashbackCount = int.Parse(elements[index++]);

        isConfirm = bool.Parse(elements[index++]);

        if (!isConfirm)
            return;

        confirmCount = int.Parse(elements[index++]);

        for (int i = 0; i < confirmCount; i++)
        {
            confirmStructs.Add(new ConfirmStruct((SpawnItemType)int.Parse(elements[index++]), (Rank)int.Parse(elements[index++]), int.Parse(elements[index++])));
        }
    }
}

/// <summary>
/// 박스 스폰 데이터 테이블
/// <br/> id, roomType, chestSpawnProb, chestTypeCount, chestIDs(배열)
/// </summary>
[Serializable]
public struct BoxSpawnData : ICsvReadable, IFireBaseReadable
{
    public int id;                      // ID
    public Define.RoomType roomType;    // 방 종류
    public int chestSpawnProb;          // 스폰 가중치
    public int chestTypeCount;          // 상자 종류의 개수
    public int[] chestIDs;              // 스폰 상자들

    public void CsvRead(string[] elements)
    {
        int index = 0;

        id = int.Parse(elements[index++]);
        roomType = (Define.RoomType)(int.Parse(elements[index++]));
        chestSpawnProb = int.Parse(elements[index++]);
        chestTypeCount = int.Parse(elements[index++]);
        chestIDs = new int[chestTypeCount];

        for (int i = 0; i < chestTypeCount; i++)
        {
            chestIDs[i] = int.Parse(elements[index++]);
        }
    }

    public FirebaseReadData FireBaseString()
    {
        return new FirebaseReadData("BoxSpawnData", id, PlayDataID.None);
    }

    /// <summary>
    /// 통합 스폰 가중치 반환 메소드
    /// </summary>
    /// <returns></returns>
    public int ReturnTotalWeight()
    {
        int totalWeight = 0;
        for(int i = 0; i < chestTypeCount; i++)
        {
            totalWeight += CsvParser.Instance.BoxDic[chestIDs[i]].weight;
        }

        return totalWeight;
    }
}

/// <summary>
/// CSV를 읽어올 때 사용해야할 인터페이스
/// </summary>
public interface ICsvReadable
{
    public void CsvRead(string[] elements);
}

public interface IFireBaseReadable
{
    public FirebaseReadData FireBaseString();
}

/// <summary>
/// 아이템 랭크(노말, 희귀, 유물, 전설)
/// </summary>
public enum Rank
{
    Normal,
    Rare,
    Artifact,
    Legendary,
    Size,
    None
}

/// <summary>
/// 박스 타입(보물상자, 몬스터상자, 플레이어상자)
/// </summary>
public enum BoxType
{
    Treasure,
    Monster,
    Player
}

/// <summary>
/// 박스 랭크(일반, 중급, 상급, 전설)
/// </summary>
public enum BoxRank
{
    Low = 1201000,
    Middle,
    High,
    Legend,
    Size
}

/// <summary>
/// 파이어 베이스 데이터를 읽기 위한 구조체(name, id, PlayDataID)
/// </summary>
[Serializable]
public struct FirebaseReadData
{
    public string name;     // 참조할 이름
    public int id;          // 저장될 ID
    public PlayDataID firebaseID;  // 파이어 베이스에 올라갈 때 참조할 번호

    public FirebaseReadData(string name, int id, PlayDataID firebaseID)
    {
        this.name = name;
        this.id = id;
        this.firebaseID = firebaseID;
    }
}

/// <summary>
/// 확정 지급품 목록
/// </summary>
[Serializable]
public struct ConfirmStruct
{
    public SpawnItemType type;  // 확정 지급품 카테고리
    public Rank rank;           // 확정 지급 랭크
    public int count;           // 확정 지급 수

    public ConfirmStruct(SpawnItemType type, Rank rank, int count)
    {
        this.type = type;
        this.rank = rank;
        this.count = count;
    }
}

/// <summary>
/// 개발자 : 서보운
/// 레벨별 데이터 및 성장률
/// <br/> 0레벨에는 스탯에 대한 성장치
/// </summary>
[Serializable]
public struct LevelData : ICsvReadable
{
    public int id;              // ID
    public int level;           // 레벨
    public float atk;           // 데미지
    public float def;           // 방어력
    public float atkSpeed;      // 공격속도(스킬에 영향)
    public float maxHp;         // 최대 체력
    public float moveSpeed;     // 이동 속도
    public int requiredExp;     // 다음 레벨로 가기위한 요구 경험치

    public const int KNIGHTID = 104101;
    public const int WARRIORID = 104201;
    public const int WIZARDID = 104301;
    public const int ARCHERID = 104401;
    public const int EXCALIBURID = 104501;

    public void CsvRead(string[] elements)
    {
        id = int.Parse(elements[0]);
        level = int.Parse(elements[1]);
        atk = float.Parse(elements[2]);
        def = float.Parse(elements[3]);
        atkSpeed = float.Parse(elements[4]);
        maxHp = float.Parse(elements[5]);
        moveSpeed = float.Parse(elements[6]);
        requiredExp = int.Parse(elements[7]);
    }
}

/// <summary>
/// 개발자 : 서보운
/// <br/> 리소스용 데이터 테이블
/// </summary>
[Serializable]
public struct ResourceData : ICsvReadable
{
    public int id;
    public ResourceID curResourceID;
    public string name;
    public string route;
    public void CsvRead(string[] elements)
    {
        id = int.Parse(elements[0]);
        curResourceID = (ResourceID)int.Parse(elements[1]);
        name = elements[2];
        route = elements[3];
    }
}

/// <summary>
/// 개발자 : 서보운
/// <br/> 리소스 아이디를 표현할 열거형
/// <br/> Script, Effect, Sprite, Animation, Tile, Sound, Prefab, Font, UI
/// </summary>
public enum ResourceID
{
    Script, // 0
    Effect, // 1
    Sprite, // 2
    Animation, // 3
    Tile, // 4
    Sound, // 5
    Prefab, // 6
    Font, // 7
    UI // 8
}
