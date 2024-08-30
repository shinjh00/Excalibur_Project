using System;

/// <summary>
/// 던전 세팅 필드 데이터
/// </summary>
[Serializable]
public struct DungeonTableData : ICsvReadable
{
    public int id;
    public bool isTeamFight;
    public int teamEA;
    public int excaliburType;
    public int wallResource;
    public int voidResource;
    public float monAtkReinforceRate;
    public float monDefReinforceRate;
    public float monHpReinforceRate;
    public int BGM;
    public int monsterSpawnBundle;
    public int chestSpawnBundle;
    public int trapSpawnBundle;
    public int innerWallSpawnCount;
    public int roomCountMin;
    public int roomCountMax;
    public int endRoomRange;
    public int lateRoomRange;
    public int beginRoomRange;
    public int puzzleRoomCount;

    public void CsvRead(string[] elements)
    {
        int index = 0;

        id = int.Parse(elements[index++]);
        isTeamFight = bool.Parse(elements[index++]);
        teamEA = int.Parse(elements[index++]);
        excaliburType = int.Parse(elements[index++]);
        wallResource = int.Parse(elements[index++]);
        voidResource = int.Parse(elements[index++]);
        monAtkReinforceRate = float.Parse(elements[index++]);
        monDefReinforceRate = float.Parse(elements[index++]);
        monHpReinforceRate = float.Parse(elements[index++]);
        BGM = int.Parse(elements[index++]);
        monsterSpawnBundle = int.Parse(elements[index++]);
        chestSpawnBundle = int.Parse(elements[index++]);
        trapSpawnBundle = int.Parse(elements[index++]);
        innerWallSpawnCount = int.Parse(elements[index++]);
        roomCountMin = int.Parse(elements[index++]);
        roomCountMax = int.Parse(elements[index++]);
        endRoomRange = int.Parse(elements[index++]);
        lateRoomRange = int.Parse(elements[index++]);
        beginRoomRange = int.Parse(elements[index++]);
        puzzleRoomCount = int.Parse(elements[index++]);
    }
}

/// <summary>
/// 방 정보 데이터 테이블
/// <br/> id, curRoomType(열거형), mobSpawnPer(int), roomSizeMinX, roomSizeMinY, roomSizeMaxX, roomSizeMaxY, tileResource
/// </summary>
[Serializable]
public struct RoomData : ICsvReadable 
{
    public int id;
    public Define.RoomType curRoomType;
    public int mobSpawnPer;
    public int roomSizeMinX;
    public int roomSizeMinY;
    public int roomSizeMaxX;
    public int roomSizeMaxY;
    public int tileResource;

    public void CsvRead(string[] elements)
    {
        int index = 0;

        id = int.Parse(elements[index++]);
        curRoomType = (Define.RoomType)int.Parse(elements[index++]);
        mobSpawnPer = int.Parse(elements[index++]);
        roomSizeMinX = int.Parse(elements[index++]);
        roomSizeMinY = int.Parse(elements[index++]);
        roomSizeMaxX = int.Parse(elements[index++]);
        roomSizeMaxY = int.Parse(elements[index++]);
        tileResource = int.Parse(elements[index++]);
    }
}

/// <summary>
/// 박스 번들 데이터
/// <br/> id, explanation, chestSpawnerCount, chestSpawnIDs(배열)
/// </summary>
[Serializable]
public struct BoxBundleData : ICsvReadable
{
    public int id;                  // id
    public string explanation;      // 설명
    public int chestSpawnerCount;     // 몹 스폰 테이블 ID의 수
    public int[] chestSpawnIDs;       // 몹 스폰 테이블 ID들

    public void CsvRead(string[] elements)
    {
        int index = 0;

        id = int.Parse(elements[index++]);
        explanation = elements[index++];
        chestSpawnerCount = int.Parse(elements[index++]);
        chestSpawnIDs = new int[chestSpawnerCount];

        for (int i = 0; i < chestSpawnIDs.Length; i++)
        {
            chestSpawnIDs[i] = int.Parse(elements[index++]);
        }
    }
}

/// <summary>
/// 함정 번들 데이터
/// <br/> id, explanation, chestSpawnerCount, chestSpawnIDs(배열)
/// </summary>
[Serializable]
public struct TrapBundleData : ICsvReadable
{
    public int id;                      // id
    public string explanation;          // 설명
    public int trapSpawnerCount;        // 함정 스폰 테이블 ID의 수
    public int[] trapSpawnIDs;          // 함정 스폰 테이블 ID들

    public void CsvRead(string[] elements)
    {
        int index = 0;

        id = int.Parse(elements[index++]);
        explanation = elements[index++];
        trapSpawnerCount = int.Parse(elements[index++]);
        trapSpawnIDs = new int[trapSpawnerCount];

        for (int i = 0; i < trapSpawnerCount; i++)
        {
            trapSpawnIDs[i] = int.Parse(elements[index++]);
        }
    }
}