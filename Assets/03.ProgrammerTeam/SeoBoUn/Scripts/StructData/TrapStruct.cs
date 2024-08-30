using System;

/// <summary>
/// 함정 스폰 세팅 필드
/// </summary>
[Serializable]
public struct TrapSpawnStruct : ICsvReadable
{
    public int id;                      // ID
    public Define.RoomType roomType;    // 방 타입
    public int trapCount;               // 트랩의 종류 수
    public trapInfo[] trapInfos;        // 트랩 정보(id, 최소 수, 최대 수, 설치될 확률)

    public void CsvRead(string[] elements)
    {
        int index = 0;

        id = int.Parse(elements[index++]);
        roomType = (Define.RoomType)int.Parse(elements[index++]);
        trapCount = int.Parse(elements[index++]);
        trapInfos = new trapInfo[trapCount];

        for(int i = 0; i < trapCount; i++)
        {
            trapInfos[i].trapId = int.Parse(elements[index++]);
            trapInfos[i].trapMin = int.Parse(elements[index++]);
            trapInfos[i].trapMax = int.Parse(elements[index++]);
            trapInfos[i].trapProb = int.Parse(elements[index++]);
        }
    }

    /// <summary>
    /// 해당 방에 깔릴 트랩에 대한 정보
    /// <br/> id, min, max
    /// </summary>
    public struct trapInfo
    {
        public int trapId;
        public int trapMin;
        public int trapMax;
        public int trapProb;
    }
}

/// <summary>
/// 함정 세팅 필드
/// <br/> id, name, range, coolTime, perDamage, knockBackDis, debuff, debuffTime....
/// </summary>
[Serializable]
public struct TrapStruct : ICsvReadable
{
    public int id;                  // id
    public string name;             // 함정 이름
    public int range;               // 함정 발동 범위
    public int coolTime;            // 쿨타임
    public float perDamage;         // 퍼센트 데미지
    public int knockBackDis;        // 넉백 거리
    public int debuff;              // 디버프 유형
    public int debuffTime;          // 디버프 시간
    public bool isSeeThrought;      // 시야 확보 여부
    public int trapImage;           // 트랩 이미지ID
    public int trapEffect;          // 트랩 이펙트ID
    public int trapEffectAnim;      // 트랩 이펙트 애니메이션ID
    public int trapActiveEffect;    // 트랩 발동 이펙트ID
    public int trapActiveAnim;      // 트랩 발동 애니메이션ID
    public int trapSound;           // 트랩 사운트ID

    public void CsvRead(string[] elements)
    {
        int index = 0;

        id = int.Parse(elements[index++]);
        name = elements[index++];
        range = int.Parse(elements[index++]);
        coolTime = int.Parse(elements[index++]);
        perDamage = float.Parse(elements[index++]);
        knockBackDis = int.Parse(elements[index++]);
        debuff = int.Parse(elements[index++]);
        debuffTime = int.Parse(elements[index++]);
        isSeeThrought = bool.Parse(elements[index++]);
        trapImage = int.Parse(elements[index++]);
        trapEffect = int.Parse(elements[index++]);
        trapEffectAnim = int.Parse(elements[index++]);
        trapActiveEffect = int.Parse(elements[index++]);
        trapActiveAnim = int.Parse(elements[index++]);
        trapSound = int.Parse(elements[index++]);
    }
}
