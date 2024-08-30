
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;
/// <summary>
/// 개발자 : 이형필 / 맵을 만드는 클래스
/// </summary>
public class SecondMapGen : MonoBehaviourPun
{
    private static SecondMapGen ins;
    public static SecondMapGen Ins
    {
        get { return ins; }
        set { ins = value; }
    }
    [SerializeField] Grid tileMap;

    [SerializeField] ShadowCaster2DCreator[] sc;
    [SerializeField] Camera miniImgCam;


    [Header("Room")]
    public Room roomPrefab;

    [Space(10f)]

    [Header("Wall")]
    [SerializeField] GameObject wallPrefab;


    [SerializeField] int obstacleRoomCountMin = 0;
    [SerializeField] int obstacleRoomCountMax = 8;


    [Space(10)]

    [Header("Room Count")]
    [Range(30, 100)][SerializeField] int minCount;
    [Range(100, 200)][SerializeField] int maxCount;

    [Space(10)]

    [Header("Room Properties")]
    [SerializeField] int branchAreaCount;
    [Range(5, 3)]
    [SerializeField] int CA_strength;
    [Range(1, 10)][SerializeField] int CA_Count;


    [SerializeField] int endRoomRange;
    [SerializeField] int lateRoomRange;
    [SerializeField] int beginRoomRange;

    [SerializeField] public int puzzleRoomCurCount = 0;             //총 퍼즐룸 카운트
    [SerializeField] public int puzzleRoomMaxCount = 2;




    [Space(10)]
    [field: SerializeField] List<Generation> generations = new List<Generation>();

    [SerializeField] public List<Room> rooms = new List<Room>();
    [SerializeField] List<GameObject> hallwaysList = new List<GameObject>();

    [SerializeField] float waitTime;

    int[,] map;
    int minX = int.MaxValue, minY = int.MaxValue;
    int maxX = int.MinValue, maxY = int.MinValue;

    const int ExcaliburRoomID = 4000;
    const int PuzzleRoomID = 5000;
    const int hallWayId = 2000;
    const int cellularId = 3000;


    [SerializeField] GameObject prefab;



    DungeonTableData curDungeon;
    int curRoomID;
    int excaliburType;

    public DungeonTableData CurDunegon { get { return curDungeon; } }

    private void Awake()
    {
        ins = this;
        SaveValue();
        if (fowTile is Tile tile)
        {
            tile.color = Color.black;
        }
        puzzleRoomMaxCount = UnityEngine.Random.Range(1, puzzleRoomMaxCount + 1);
        puzzleRoomCurCount = 0;
        // StartCoroutine(CreateRandomMap());
    }
    public void FowOpen()
    {
        fowTilemap.gameObject.SetActive(false);
    }
    public void StartGenerateMap()
    {
        StartCoroutine(SetDungeonInfo());
        StartCoroutine(CreateRandomMap());

    }
    IEnumerator SetDungeonInfo()
    {
        yield return new WaitUntil(() => CsvParser.Instance != null);
        curRoomID = PhotonNetwork.CurrentRoom.GetProperty<int>(DefinePropertyKey.ROOMID);       //게임이 시작되면 마스터가 설정 된 룸아이디를 가져옴

        if (curRoomID == 0)
        {
            curRoomID = 1400000;
        }
        curDungeon = CsvParser.Instance.DungeonTableData[curRoomID];
        // GameManager.Ins.isTeamFight = curDungeon.isTeamFight;
        excaliburType = curDungeon.excaliburType;


    }
    #region initValue
    int initGen;
    float initWeight;
    float initWidth;
    float initheight;
    int initMin;
    int initMax;
    Vector3 initScale;
    void SaveValue()
    {
        initGen = roomPrefab.roomGen;
        initWeight = roomPrefab.weight;
        initWidth = roomPrefab.width;
        initheight = roomPrefab.height;
        initScale = roomPrefab.transform.localScale;
        initMin = roomPrefab.hallMin;
        initMax = roomPrefab.hallMax;
    }
    private void OnApplicationQuit()
    {
        Room roomComponent = roomPrefab.GetComponentInChildren<Room>();

        roomPrefab.roomGen = initGen;
        roomPrefab.weight = initWeight;
        roomPrefab.width = initWidth;
        roomPrefab.height = initheight;
        roomPrefab.transform.localScale = initScale;
        roomPrefab.hallMin = initMin;
        roomPrefab.hallMax = initMax;
        roomComponent.weight = initWeight;
        roomComponent.roomGen = initGen;
        roomComponent.width = initWidth;
        roomComponent.height = initheight;
        roomComponent.transform.localScale = initScale;
        roomComponent.hallMin = initMin;
        roomComponent.hallMax = initMax;

        if (fowTile is Tile tile)
        {
            tile.color = Color.black;
        }
    }
    #endregion
    int index = 0;
    /// <summary>
    /// 랜덤맵 생성시작
    /// </summary>
    IEnumerator CreateRandomMap()
    {
        int targetCount = UnityEngine.Random.Range(minCount, maxCount + 1);
        Debug.Log($"target is {targetCount}");
        CreateStartRoom();

        List<Room> newRooms = new List<Room>();

        UnDuplicateRanPick<Room> udrpRooms = new UnDuplicateRanPick<Room>();

        Room removeRoom;

        int removeCount;
        bool isFull = false;
        int loop = 0;

        while (!isFull)
        {
            yield return new WaitForSeconds(waitTime);

            newRooms.Clear();

            for (int i = index; i >= 0; i--)
            {
                // Debug.Log(index);
                newRooms.AddRange(generations[i].CreateNewRooms(i, endRoomRange, lateRoomRange, beginRoomRange));           //n세대 방생성하면서 newRooms 리스트에 넣기


                if (newRooms.Count != 0)                //새로운 방 생성 못하면 이전세대에서 방 생성
                {
                    // Debug.Log("hasNotNewRoom");
                    break;
                }
            }
            if (GameManager.Ins.totalRoomCount >= targetCount)
            {
                removeCount = GameManager.Ins.totalRoomCount - targetCount;
                //토탈카운트 넘겨서 생성되면 그만큼 리무브카운트 할당

            }
            else
            {
                removeCount = UnityEngine.Random.Range(0, newRooms.Count);          // 아니라면 랜덤으로 생성된 방 -1만큼
            }
            yield return new WaitForSeconds(waitTime);

            udrpRooms.SetItem(newRooms);                //중복 안되게 newRooms중에서 픽
            if (index > branchAreaCount)
            {
                for (int i = 0; i < removeCount; i++)
                {
                    removeRoom = udrpRooms.GetItem();

                    for (int j = 0; j < removeRoom.nextRooms.Length; j++)
                    {
                        if (removeRoom.nextRooms[j] != null)
                            removeRoom.nextRooms[j].
                                nextRooms[(int)Direction.GetReverseDir(Direction.eDir[j])] = null;
                    }
                    newRooms.Remove(removeRoom);
                    Destroy(removeRoom.gameObject);
                    GameManager.Ins.totalRoomCount--;
                }
            }
            rooms.AddRange(newRooms);
            foreach (Room room in newRooms)
            {
                RoomChecker.CheckAndLinkRoom(room);

            }

            generations.Add(new Generation());
            generations[++index].AddRoom(newRooms);         // 새로운 방을 다음 세대에 룸으로 제공

            //전체 맵으로 배열화
            foreach (var room in rooms)
            {
                Vector3 pos = room.transform.position;
                Vector3 scale = room.transform.localScale;

                minX = Mathf.Min(minX, Mathf.FloorToInt(pos.x - scale.x));
                minY = Mathf.Min(minY, Mathf.FloorToInt(pos.y - scale.y));
                maxX = Mathf.Max(maxX, Mathf.CeilToInt(pos.x + scale.x));
                maxY = Mathf.Max(maxY, Mathf.CeilToInt(pos.y + scale.y));
            }

            // 배열 크기 계산 및 초기화
            int width = maxX - minX;
            int height = maxY - minY;
            map = new int[height, width];

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++) map[i, j] = -1;

            // 배열에 GameObject 저장
            for (int i = 0; i < rooms.Count; i++)
            {
                Vector3 pos = rooms[i].transform.position;
                Vector3 scale = rooms[i].transform.localScale;

                // GameObject의 크기와 위치를 고려하여 배열에 저장

                for (int x = (int)-scale.x / 2; x < scale.x / 2; x++)
                {
                    for (int y = (int)-scale.y / 2; y < scale.y / 2; y++)
                    {
                        int mapX = Mathf.FloorToInt(pos.x - minX + x);
                        int mapY = Mathf.FloorToInt(pos.y - minY + y);
                        if (rooms[i].CurRoomData.curRoomType == Define.RoomType.ExcaliburRoom)
                        {
                            map[mapY, mapX] = ExcaliburRoomID;

                        }
                        else if (rooms[i].CurRoomData.curRoomType == Define.RoomType.PuzzleRoom)
                        {
                            map[mapY, mapX] = PuzzleRoomID;
                        }
                        else
                        {
                            map[mapY, mapX] = i;
                        }

                    }
                }

            }


            if (index - 1 >= 0)
                generations[index - 1].SetAllColor(Color.white);
            generations[index].SetAllColor(Color.red);
            yield return new WaitForSeconds(waitTime);
            isFull = targetCount <= GameManager.Ins.totalRoomCount;

            if (loop++ > 100)
            {
                throw new System.Exception("Inf loop");
            }
        }

        HallWayArr();
        for (int i = 0; i < CA_Count; i++)
        {
            CellularAutomata(CA_strength);
        }

        int curIndex = index;
        int complete = 0;
        int roomCnt = 0;
        int inf = 0;
        do
        {
            Debug.Log($"curIndex : {curIndex}");
            inf++;
            roomCnt += generations[curIndex].SetRoomType(Define.RoomType.PlayerSpawnRoom, 10, 10, 4 - complete);
            curIndex--;
            complete = roomCnt;




        }
        while (roomCnt < 4 && inf < 30);



        foreach (Room r in rooms) //방 안보이게
        {
            r.SetColor(new Color(0, 0, 0, 0));
        }

        if (isFull)
        {
            MapArrNormalization();

            StartChangeArray();
            AutoTiling();
            CenterArrPosMove();
            Debug.Log($"Begin : {CountRoomType(Define.RoomType.BeginningRoom)}");
            Debug.Log($"Late : {CountRoomType(Define.RoomType.LateRoom)}");
            Debug.Log($"End : {CountRoomType(Define.RoomType.EndRoom)}");
            Debug.Log(map.GetLength(0));
            Debug.Log(map.GetLength(1));
        }
    }

    public int CountRoomType(Define.RoomType RoomType)
    {
        int count = 0;
        foreach (Room r in rooms)
        {
            if (r.CurRoomData.curRoomType == RoomType)
                count++;
        }
        return count;
    }
    public void SetRoomTypeInRoomList(Define.RoomType preRoomType, Define.RoomType change2RoomType, int x, int y, int count)
    {
        UnDuplicateRanPick<Room> udrpRoom = new UnDuplicateRanPick<Room>();
        List<Room> roomList = new List<Room>();
        Room rRoom;
        foreach (Room r in rooms)
        {
            if (r.CurRoomData.curRoomType == preRoomType)
            {
                roomList.Add(r);
            }
        }
        udrpRoom.SetItem(roomList);
        for (int i = 0; i < count; i++)
        {
            if (!udrpRoom.IsEmpty())
            {
                bool isSpawnRoom = false;
                while (!isSpawnRoom)
                {
                    rRoom = udrpRoom.GetItem();
                    if (rRoom == null)
                        break;

                    if (count <= 0)
                    {
                        Debug.LogError("Count is less than or equal to zero");
                        return;
                    }
                    Collider2D[] cols = Physics2D.OverlapBoxAll(rRoom.transform.position, new Vector2(x, y), 0, LayerMask.GetMask("Room"));
                    Collider2D selfCollider = rRoom.GetComponent<Collider2D>();

                    bool hasSameRoomType = false;
                    foreach (Collider2D col in cols)
                    {
                        if (col == selfCollider)
                            continue;
                        Room r = col.GetComponent<Room>();
                        if (r.CurRoomData.curRoomType == change2RoomType)
                        {
                            hasSameRoomType = true;
                            break;
                        }
                    }

                    if (!hasSameRoomType)
                    {
                        rRoom.RoomType = change2RoomType;
                        isSpawnRoom = true;
                    }
                }
            }
        }
    }
    /// <summary>
    /// 배열을 로그로 띄우는 메서드
    /// </summary>
    void DebugArr()
    {
        string mapString = "";
        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                mapString += map[y, x].ToString().PadLeft(3) + " ";
            }
            mapString += "\n";
        }
    }
    /// <summary>
    /// 0세대 엑스칼리버 방 고정생성
    /// </summary>
    void CreateStartRoom()
    {
        generations.Add(new Generation());
        Room startRoom = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity);
        startRoom.transform.localScale = new Vector3(12, 12, 1);           //엑스칼리버 방은 12,12 고정임      
        startRoom.SetScale();
        startRoom.RoomType = Define.RoomType.ExcaliburRoom;
        startRoom.roomGen = 1;
        GameManager.Ins.totalRoomCount++;
        rooms.Add(startRoom);

        generations.Add(new Generation());              // 세대 생성
        generations[++index].AddRoom(startRoom);        // 다음세대에 엑스칼리버 방을 부모로 제공

    }



    /// <summary>
    /// 복도 배열화
    /// </summary>
    public void HallWayArr()
    {
        foreach (GameObject go in hallwaysList)
        {
            if (go != null)
            {
                Vector3 position = go.transform.position;
                Vector3 scale = go.transform.lossyScale;
                for (int x = (int)-scale.x / 2; x < (scale.x / 2) + 2; x++)
                {
                    for (int y = (int)-scale.y / 2; y < (scale.y / 2) + 2; y++)
                    {
                        int mapX = Mathf.FloorToInt(position.x - minX + x);
                        int mapY = Mathf.FloorToInt(position.y - minY + y);
                        map[mapY, mapX] = hallWayId;
                    }
                }
            }
        }
    }/// <summary>
     ///  타일을 룸 오브젝트 위치로 동기화 ( 엑스칼리버 방이 기준)
     /// </summary>
    void CenterArrPosMove()
    {
        Vector2Int startArr = GetRoomArrStart(1);                //첫번째방 찾기
        Vector2Int startArrSize = GetRoomArrSize(1);
        Vector3 pos = tileMap.WorldToCell(new Vector3(startArr.x + (startArrSize.x / 2), startArr.y + (startArrSize.y / 2), 0));
        photonView.RPC("SetCenterPos", RpcTarget.All, pos, maxX, maxY);
    }
    /// <summary>
    /// 룸의 배열 사이즈 리턴
    /// </summary>
    /// <param name="roomNum"></param>
    /// <param name="grid"></param>
    /// <returns></returns>
    Vector2Int GetRoomArrSize(int roomNum)
    {
        int yCount = 0;
        int xCount = 0;

        Vector2Int startArr = GetRoomArrStart(roomNum);
        if (startArr.x != -1 && startArr.y != -1)
        {
            for (int x = startArr.x; x < maxX - minX && map[startArr.y, x] == roomNum || map[startArr.y, x] == (int)Define.GridType.HallWay; x++)
            {
                xCount++;
            }
            for (int y = startArr.y; y < maxY - minY && map[y, startArr.x] == roomNum || map[y, startArr.x] == (int)Define.GridType.HallWay; y++)
            {
                yCount++;
            }
        }

        return new Vector2Int(xCount, yCount);
    }
    /// <summary>
    /// 방배열의 좌하단 배열 리턴
    /// </summary>
    /// <param name="roomNum"></param>
    /// <returns></returns>
    Vector2Int GetRoomArrStart(int roomNum)
    {
        int initX = -1;
        int initY = -1;

        bool foundZero = false;
        for (int x = 0; x < maxX - minX; x++)
        {
            for (int y = 0; y < maxY - minY; y++)
            {
                if (map[y, x] == roomNum)
                {
                    initX = x;
                    initY = y;
                    foundZero = true;
                    break;
                }
            }
            if (foundZero) break;
        }
        return new Vector2Int(initX, initY);
    }
    int mapXSize;
    int mapYSize;
    /// <summary>
    /// 생성된 룸에서 중심을 맞출 수 있도록 진행하는 RPC 메소드
    /// </summary>
    /// <param name="pos">중심 위치</param>

    [PunRPC]
    public void SetCenterPos(Vector3 pos, int mX, int mY)
    {
        maxX = mX; maxY = mY;
        GameObject ins = Instantiate(prefab, pos, Quaternion.identity);
        tileMap.transform.SetParent(ins.transform);
        ins.transform.position = Vector3.zero;
         mapXSize = (int)Mathf.Abs(tileMap.transform.position.x) + Mathf.Abs(maxX);
         mapYSize = (int)Mathf.Abs(tileMap.transform.position.y) + Mathf.Abs(maxY);

   //     Vector3 centerPos = tileMap.transform.position + new Vector3(mapXSize * 0.5f, mapYSize * 0.5f, -1);


    }


    /// <summary>
    /// 오토타일과 같은 방식으로 주위타일 검사해서 n칸 이상이 벽이 아닌 공간일 경우 복도배열로 변환
    /// </summary>
    /// <param name="n"></param>
    void CellularAutomata(int n)
    {
        for (int x = 0; x < maxX - minX; x++)
        {
            for (int y = 0; y < maxY - minY; y++)
            {
                #region IDcondition
                if (map[y, x] == hallWayId) continue;
                if (map[y, x] == ExcaliburRoomID) continue;
                if (map[y, x] == PuzzleRoomID) continue;
                if ((map[y, x] != -1 && rooms[map[y, x]].gameObject.activeSelf)) continue;
                #endregion

                int nonWallCount = 0; // 벽이 아닌 공간의 수를 카운트

                // 나를 포함한 주위 9칸 검사
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    for (int offsetY = -1; offsetY <= 1; offsetY++)
                    {
                        int checkX = x + offsetX;
                        int checkY = y + offsetY;

                        // 배열 범위를 벗어나면 벽으로 간주
                        #region IDcondition
                        if (checkX < 0 || checkX >= maxX - minX || checkY < 0 || checkY >= maxY - minY)
                        {
                            continue;
                        }
                        else if (map[checkY, checkX] == -1) continue;
                        else if (map[checkY, checkX] == cellularId) continue;
                        else if (map[checkY, checkX] == ExcaliburRoomID) continue;
                        else if (map[checkY, checkX] == PuzzleRoomID) continue;
                        else if (map[checkY, checkX] == hallWayId || rooms[map[checkY, checkX]].gameObject.activeSelf) // 벽이 아닌 공간 확인
                        {
                            nonWallCount++;
                        }
                        #endregion
                    }
                }
                if (nonWallCount >= n)
                {
                    map[y, x] = cellularId;
                }
            }
        }
        for (int x = 0; x < maxX - minX; x++)                               //셀룰러 아이디를 복도아이디로
        {
            for (int y = 0; y < maxY - minY; y++)
            {
                if (map[y, x] == cellularId) map[y, x] = hallWayId;
            }
        }
    }/// <summary>
     /// a에서 b로 복도 생성
     /// </summary>
     /// <param name="a">기존 방</param>
     /// <param name="b">생성 방</param>
     /// <param name="a2b">복도 생성 방향</param>
    public void CreatePath(Room a, Room b, EDir a2b)
    {
        GameObject door;
        Vector2 pos;

        if (a2b == EDir.UP || a2b == EDir.DOWN)
        {
            pos = (Vector2)a.transform.position +
                  new Vector2(0, Direction.yDir[(int)a2b] * ((a.height + b.hallMax) * 0.5f));
            door = Instantiate(wallPrefab, pos, Quaternion.identity);
            door.transform.localScale = new Vector3(b.hallMin, b.hallMax + 2, 0);

        }
        else
        {
            pos = (Vector2)a.transform.position +
                  new Vector2(Direction.xDir[(int)a2b] * ((b.hallMax + a.width) * 0.5f), 0);
            door = Instantiate(wallPrefab, pos, Quaternion.identity);
            door.transform.localScale = new Vector3(b.hallMax + 2, b.hallMin, 0);
        }

        door.transform.SetParent(b.transform);
        hallwaysList.Add(door);                 //복도리스트에 추가
    }
    #region AUTO TILING

    [Header("Tilemaps")]
    [SerializeField] Tilemap floorTilemap;
    [SerializeField] Tilemap wallTilemap;
    [SerializeField] Tilemap hallWayTilemap;
    [SerializeField] Tilemap colliderTilemap;
    [SerializeField] Tilemap fowTilemap;

    [Header("Tiles")]
    [SerializeField] Tile wall_Top_Left;
    [SerializeField] Tile wall_Top_Right;
    [SerializeField] Tile wall_Top_Center;
    [SerializeField] Tile wall_Bottom_Left;
    [SerializeField] Tile wall_Bottom_Right;
    [SerializeField] Tile wall_Bottom;
    [SerializeField] Tile wall_Top;
    [SerializeField] Tile wall_Right;
    [SerializeField] Tile wall_Left;

    [SerializeField] Tile floor;
    [SerializeField] Tile noneTile;
    [SerializeField] Tile fowTile;
    [SerializeField] Tile wall_j;
    [SerializeField] Tile wall_k;
    [SerializeField] Tile wall_cross;
    [SerializeField] Tile tile_Center;

    [Header("Random Tiles")]
    [SerializeField] Tile floor_Random_0;
    [SerializeField] Tile floor_Random_1;
    [SerializeField] Tile floor_Random_2;
    [SerializeField] Tile floor_Random_3;

    #region bitMask
    const int TopLeftMask = (1 << 4) | (1 << 5) | (1 << 7) | (1 << 8);
    const int TopCenterMask = (1 << 3) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7) | (1 << 8);
    const int TopRightMask = (1 << 3) | (1 << 4) | (1 << 6) | (1 << 7);

    const int BottomRightMask = (1 << 0) | (1 << 1) | (1 << 3) | (1 << 4);
    const int BottomCenterMask = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 5);
    const int bottomLeftMask = (1 << 1) | (1 << 2) | (1 << 4) | (1 << 5);

    const int floorMask = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7) | (1 << 8);
    const int wallLeftMask = (1 << 1) | (1 << 2) | (1 << 4) | (1 << 5) | (1 << 7) | (1 << 8);
    const int wallRightMask = (1 << 0) | (1 << 1) | (1 << 3) | (1 << 4) | (1 << 6) | (1 << 7);

    const int rightWallUp2DownUp = (1 << 0) | (1 << 1) | (1 << 3) | (1 << 4) | (1 << 6) | (1 << 7) | (1 << 8);
    const int rightWallUp2DownCenter = (1 << 0) | (1 << 1) | (1 << 3) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7) | (1 << 8);
    const int rightWallUp2DownDown = (1 << 0) | (1 << 3) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7) | (1 << 8);

    const int rightWallDown2UpDown = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 6) | (1 << 7);
    const int rightWallDown2UpCenter = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7);
    const int rightWallDown2UpUp = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 5) | (1 << 6);

    const int leftWallUp2DownUp = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 5) | (1 << 8);
    const int leftWallUp2DownCenter = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 5) | (1 << 7) | (1 << 8);
    const int leftWallUp2DownDown = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 4) | (1 << 5) | (1 << 7) | (1 << 8);

    const int leftWallDown2UpDown = (1 << 2) | (1 << 3) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7) | (1 << 8);
    const int leftWallDown2UpCenter = (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7) | (1 << 8);
    const int leftWallDown2UpUp = (1 << 1) | (1 << 2) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7) | (1 << 8);

    const int connectUpRight = (1 << 3) | (1 << 4) | (1 << 6) | (1 << 7) | (1 << 8);
    const int connectUpLeft = (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7) | (1 << 8);
    const int connectDownRight = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4);
    const int connectDownLeft = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 4) | (1 << 5);

    const int connectUpRight2 = (1 << 0) | (1 << 3) | (1 << 4) | (1 << 6) | (1 << 7) | (1 << 8);
    const int connectUpLeft2 = (1 << 2) | (1 << 4) | (1 << 5) | (1 << 7) | (1 << 8);
    const int connectDownRight2 = (1 << 0) | (1 << 1) | (1 << 3) | (1 << 4) | (1 << 6);
    const int connectDownLeft2 = (1 << 1) | (1 << 2) | (1 << 4) | (1 << 5) | (1 << 8);

    const int connectUpRight3 = (1 << 0) | (1 << 3) | (1 << 4) | (1 << 6) | (1 << 7);
    const int connectUpLeft3 = (1 << 2) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7) | (1 << 8);
    const int connectDownRight3 = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 6);
    const int connectDownLeft3 = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 4) | (1 << 5) | (1 << 8);

    const int connectUp = (1 << 0) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7) | (1 << 8);
    const int connectDown = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 8);
    const int connectLeft = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7) | (1 << 8);
    const int connectRight = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 6) | (1 << 7) | (1 << 8);

    const int otherConnect1 = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 7) | (1 << 8);
    const int otherConnect2 = (1 << 0) | (1 << 1) | (1 << 4) | (1 << 5) | (1 << 7) | (1 << 8);
    const int otherConnect3 = (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7);
    const int otherConnect4 = (1 << 0) | (1 << 1) | (1 << 3) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7);
    const int otherConnect5 = (1 << 1) | (1 << 2) | (1 << 3) | ((1 << 4) | (1 << 5)) | (1 << 7) | (1 << 8);
    const int otherConnect6 = (1 << 0) | (1 << 2) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7) | (1 << 8);
    const int otherConnect7 = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 6) | (1 << 8);
    const int otherConnect8 = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 8);
    const int otherConnect9 = (1 << 0) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 6) | (1 << 7) | (1 << 8);
    const int otherConnect10 = (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 8);
    const int otherConnect11 = (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 5) | (1 << 8);
    const int otherConnect12 = (1 << 0) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7);
    const int otherConnect13 = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 5) | (1 << 7);
    const int otherConnect14 = (1 << 1) | (1 << 8) | (1 << 3) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7);
    const int otherConnect15 = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7);
    const int otherConnect16 = (1 << 0) | (1 << 1) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7) | (1 << 8);
    const int otherConnect17 = (1 << 0) | (1 << 1) | (1 << 3) | (1 << 4) | (1 << 7) | (1 << 8);
    const int otherConnect18 = (1 << 0) | (1 << 1) | (1 << 3) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 8);
    const int otherConnect19 = (1 << 0) | (1 << 1) | (1 << 3) | (1 << 4) | (1 << 5) | (1 << 7) | (1 << 8);
    const int otherConnect20 = (1 << 0) | (1 << 1) | (1 << 3) | (1 << 4) | (1 << 6) | (1 << 7);



    const int allNoneMask = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) | (1 << 5) | (1 << 6) | (1 << 7) | (1 << 8);

    const int TopResult1 = (1 << 3) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7);
    const int TopResult3 = (1 << 3) | (1 << 4) | (1 << 5) | (1 << 7) | (1 << 8);
    const int TopResult2 = (1 << 3) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 8);

    const int LeftResult1 = (1 << 1) | (1 << 4) | (1 << 5) | (1 << 7) | (1 << 8);
    const int LeftResult2 = (1 << 1) | (1 << 2) | (1 << 4) | (1 << 7) | (1 << 8);
    const int LeftResult3 = (1 << 1) | (1 << 2) | (1 << 4) | (1 << 5) | (1 << 7);

    const int BottomResult1 = (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 5);
    const int BottomResult2 = (1 << 0) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 5);
    const int BottomResult3 = (1 << 0) | (1 << 1) | (1 << 3) | (1 << 4) | (1 << 5);


    const int RightResult1 = (1 << 1) | (1 << 3) | (1 << 4) | (1 << 6) | (1 << 7);
    const int RightResult2 = (1 << 0) | (1 << 1) | (1 << 4) | (1 << 6) | (1 << 7);
    const int RightResult3 = (1 << 0) | (1 << 1) | (1 << 3) | (1 << 4) | (1 << 7);
    #endregion
    /// <summary>
    /// 맵 배열 그리드타입 지정,정규화
    /// </summary>
    void MapArrNormalization()
    {
        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                if (map[i, j] < 0 || (map[i, j] != hallWayId && map[i, j] != PuzzleRoomID && map[i, j] != ExcaliburRoomID && !rooms[map[i, j]].gameObject.activeSelf))
                {
                    map[i, j] = (int)Define.GridType.None;

                }
                else if (map[i, j] == ExcaliburRoomID)
                {
                    map[i, j] = (int)Define.GridType.ExcaliburRoom;
                }
                else if (map[i, j] == PuzzleRoomID)
                {
                    map[i, j] = (int)Define.GridType.PuzzleRoom;
                }
                else if (map[i, j] == hallWayId) map[i, j] = (int)Define.GridType.HallWay;         //복도 타일이라면 배열에 복도그리드 할당
                else if (map[i, j] < obstacleRoomCountMax && map[i, j] > obstacleRoomCountMin + 1)          //방번호 min~ max 까지 확률로 장애물 생성
                {
                    if (map[i, j] == (int)Define.GridType.PuzzleRoom)
                        continue;

                    map[i, j] = (int)Define.GridType.MainRoom;

                    if (UnityEngine.Random.value < 0.03)
                    {
                        CreateObs(j, i);
                    }
                }

                else map[i, j] = (int)Define.GridType.MainRoom;                             //그 외는 룸 타일
            }
        }
        Debug.Log("normalize Clear");
      /*  for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                if (map[i, j] == (int)Define.GridType.Obs)
                {
                    map[i, j] = (int)Define.GridType.None;
                }
            }
        }*/
      
    }
    /// <summary>
    /// 엄폐물 크기를 랜덤으로 설정하고 지정한 위치에 랜덤으로 생성
    /// </summary>
    /// <param name="j"></param>
    /// <param name="i"></param>
    void CreateObs(int j, int i)
    {
        Vector2Int r_p = new Vector2Int(j, i); // 시작 위치 설정

        for (int y = -4; y <= 4; y++)
        {
            for (int x = -4; x <= 4; x++)
            {
                int checkY = r_p.y + y;
                int checkX = r_p.x + x;
                if (map[checkY, checkX] == (int)Define.GridType.None)
                {
                    return;
                }
            }
        }

        map[r_p.y, r_p.x] = (int)Define.GridType.None;

        int count = UnityEngine.Random.Range(1, 6);

        for (int k = 0; k < count; k++)
        {
            
            EDir dir = (EDir)UnityEngine.Random.Range(0, 4);


            switch (dir)
            {
                case EDir.UP:
                    r_p.y++;

                    break;
                case EDir.DOWN:
                    r_p.y--;
                    break;
                case EDir.LEFT:
                    r_p.x--;
                    break;
                case EDir.RIGHT:
                    r_p.x++;
                    break;
            }
            map[r_p.y, r_p.x] = (int)Define.GridType.None;

        }


    }
    void CompleteGeneration()
    {
        miniImgCam.orthographicSize = 250f; // (mapXSize * mapYSize) * 0.003f;

        foreach (ShadowCaster2DCreator s in sc)
        {
            s.Create();
        }
        GameManager.Ins.FindExcaliburExit();
        PhotonNetwork.LocalPlayer.SetProperty(DefinePropertyKey.READY, true);
        Debug.Log("Player Ready");
    }
    /// <summary>
    /// 배열을 돌며 타일을 까는 작업
    /// </summary>
    void AutoTiling()
    {

        // 바닥/벽 처리
        for (int i = map.GetLength(0) - 1; i >= 0; i--)
        {
            for (int j = map.GetLength(1) - 1; j >= 0; j--)
            {

                switch (map[i, j])
                {
                    case (int)Define.GridType.None:
                        PlaceTile(j, i, 3);
                        break;
                    case (int)Define.GridType.ExcaliburRoom:
                    case (int)Define.GridType.PuzzleRoom:
                    case (int)Define.GridType.MainRoom:
                        PlaceTile(j, i, 1);
                        break;
                    case (int)Define.GridType.UpperRoom:
                        PlaceTile(j, i, 1);
                        break;

                    case (int)Define.GridType.HallWay:
                        PlaceTile(j, i, 2); break;

                }


            }
        }
        CompleteGeneration();
    }

    /// <summary>
    /// 2차원 배열을 1차원으로 변경 후 RPC호출
    /// </summary>
    public void StartChangeArray()
    {
        StartCoroutine(SendChunksCoroutine());
    }

    private IEnumerator SendChunksCoroutine()
    {
        int rows = map.GetLength(0);
        int cols = map.GetLength(1);
        int[] oneDimensionMap = new int[rows * cols];

        // 2차원 배열을 1차원 배열로 변환
        int index = 0;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                oneDimensionMap[index++] = map[i, j];
            }
        }
        Debug.Log("2arr => 1arr Clear");
        // 청크 크기 정의 및 총 청크 수 계산
        int chunkSize = 100;
        int totalChunks = (oneDimensionMap.Length + chunkSize - 1) / chunkSize;

        // 각 청크를 전송
        for (int i = 0; i < totalChunks; i++)
        {
            int currentChunkSize = Mathf.Min(chunkSize, oneDimensionMap.Length - i * chunkSize);
            int[] chunk = new int[currentChunkSize];
            Array.Copy(oneDimensionMap, i * chunkSize, chunk, 0, currentChunkSize);

            // RPC 호출로 청크 전송
            photonView.RPC("ReceiveChunk", RpcTarget.Others, chunk, i, totalChunks, rows, cols);

            yield return new WaitForEndOfFrame();  
        }
    }


    private int[] receivedArray;
    private int totalChunks;
    private int receivedChunksCount;
    private int receivedRows;
    private int receivedCols;

    [PunRPC]
    public void ReceiveChunk(int[] chunk, int chunkIndex, int expectedChunks, int rows, int cols)
    {
        // 초기화
        if (receivedArray == null)
        {
            receivedArray = new int[rows * cols];
            totalChunks = expectedChunks;
            receivedChunksCount = 0;
            receivedRows = rows;
            receivedCols = cols;
        }

        // 청크를 올바른 위치에 삽입
        int startIndex = chunkIndex * chunk.Length;
        Array.Copy(chunk, 0, receivedArray, startIndex, chunk.Length);

        receivedChunksCount++;

        // 모든 청크가 수신되었는지 확인
        if (receivedChunksCount >= totalChunks)
        {
            // 전체 배열을 사용하여 작업 수행
            AutoTilingOthers(receivedArray, receivedRows, receivedCols);

            // 상태 초기화
            receivedArray = null;
            receivedChunksCount = 0;
            totalChunks = 0;
        }
    }


    IEnumerator SendPartWithDelay(int[] part, string methodName, float delay)
    {
        yield return new WaitForSeconds(delay);
        photonView.RPC(methodName, RpcTarget.Others, part);
    }

    /// <summary>
    /// 마스터를 제외한 나머지 클라이언트에서 실행할 타일링 메소드
    /// </summary>
    /// <param name="mapInfo">1차원 배열</param>
    /// <param name="rows">2차원 배열로 만들 행</param>
    /// <param name="cols">2차원 배열로 만들 열</param>
    void AutoTilingOthers(int[] mapInfo, int rows, int cols)
    {
        // 1차원 배열을 2차원 배열로 변경
        map = new int[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                map[i, j] = mapInfo[(i * cols) + j];
            }
        }

        // 바닥/벽 처리
        for (int i = map.GetLength(0) - 1; i >= 0; i--)
        {
            for (int j = map.GetLength(1) - 1; j >= 0; j--)
            {

                switch (map[i, j])
                {
                    case (int)Define.GridType.None:
                        PlaceTile(j, i, 3);
                        break;
                    case (int)Define.GridType.ExcaliburRoom:
                    case (int)Define.GridType.PuzzleRoom:
                    case (int)Define.GridType.MainRoom:
                        PlaceTile(j, i, 1);
                        break;
                    case (int)Define.GridType.UpperRoom:
                        PlaceTile(j, i, 1);
                        break;

                    case (int)Define.GridType.HallWay:
                        PlaceTile(j, i, 2); break;
                }


            }
        }
        CompleteGeneration();
    }

    /// <summary>
    /// 해당 좌표에 비트마스크연산이 된 적합한 타일을 깐다
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="tileType"></param>
    void PlaceTile(int x, int y, int tileType)
    {
        Tile tile = null;
        Vector3Int tilePos = new Vector3Int(x, y, 0);

        switch (tileType)
        {
            case 3:
                int pattern = PatternCalNone(x, y);
                if (Matches(pattern, allNoneMask))
                {
                    tile = tile_Center;
                    colliderTilemap.SetTile(tilePos, tile);
                }
                else
                {
                    tile = noneTile;
                    colliderTilemap.SetTile(tilePos, tile);
                }

                break;
            case 1:
            case 2:
                tile = DetermineWall(x, y);
                if (!Matches(PatternCal(x, y), floorMask))
                {
                    wallTilemap.SetTile(tilePos, tile);
                    if (tile == null)
                    {
                        wallTilemap.SetTile(tilePos, tile_Center);
                    }

                }
                else
                {
                    floorTilemap.SetTile(tilePos, tile);
                }

                break;
            case 4:
                {

                    break;
                }



        }

        fowTilemap.SetTile(tilePos, fowTile);
    }

    /// <summary>
    /// 해당 좌표가 어떤 패턴에 해당하는지 계산
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    Tile DetermineWall(int x, int y)
    {
        int pattern = PatternCal(x, y);
        if (Matches(pattern, floorMask)) return GetRandomFloorTile();
        if (Matches(pattern, TopLeftMask)) return wall_Top_Left;
        if (Matches(pattern, TopRightMask)) return wall_Top_Right;
        if (Matches(pattern, TopCenterMask)) return wall_Top_Center;
        if (Matches(pattern, wallLeftMask)) return wall_Left;
        if (Matches(pattern, wallRightMask)) return wall_Right;
        if (Matches(pattern, bottomLeftMask)) return wall_Bottom_Left;
        if (Matches(pattern, BottomCenterMask)) return wall_Bottom;
        if (Matches(pattern, BottomRightMask)) return wall_Bottom;

        if (Matches(pattern, rightWallUp2DownUp)) return wall_Right;
        if (Matches(pattern, rightWallUp2DownCenter)) return wall_Bottom_Left;
        if (Matches(pattern, rightWallUp2DownDown)) return wall_Top_Center;

        if (Matches(pattern, rightWallDown2UpDown)) return wall_Right;
        if (Matches(pattern, rightWallDown2UpCenter)) return wall_Top_Left;
        if (Matches(pattern, rightWallDown2UpUp)) return wall_Bottom;

        if (Matches(pattern, leftWallUp2DownUp)) return wall_Bottom;
        if (Matches(pattern, leftWallUp2DownCenter)) return wall_Top_Right;
        if (Matches(pattern, leftWallUp2DownDown)) return wall_Left;

        if (Matches(pattern, leftWallDown2UpUp)) return wall_Left;
        if (Matches(pattern, leftWallDown2UpCenter)) return wall_Bottom_Right;
        if (Matches(pattern, leftWallDown2UpDown)) return wall_Top_Center;

        if (Matches(pattern, connectUpRight)) return wall_Top_Right;
        if (Matches(pattern, connectUpLeft)) return wall_Top_Left;
        if (Matches(pattern, connectDownRight)) return wall_Bottom_Right;
        if (Matches(pattern, connectDownLeft)) return wall_Bottom_Left;

        if (Matches(pattern, connectUpRight2)) return wall_Top_Right;
        if (Matches(pattern, connectUpLeft2)) return wall_Top_Left;
        if (Matches(pattern, connectDownRight2)) return wall_Bottom_Right;
        if (Matches(pattern, connectDownLeft2)) return wall_Bottom_Left;

        if (Matches(pattern, connectUpRight3)) return wall_Top_Right;
        if (Matches(pattern, connectUpLeft3)) return wall_Top_Left;
        if (Matches(pattern, connectDownRight3)) return wall_Bottom_Right;
        if (Matches(pattern, connectDownLeft3)) return wall_Bottom_Left;

        if (Matches(pattern, connectUp)) return wall_Top_Center;
        if (Matches(pattern, connectDown)) return wall_Bottom;
        if (Matches(pattern, connectRight)) return wall_Right;
        if (Matches(pattern, connectLeft)) return wall_Left;

        if (Matches(pattern, otherConnect1)) return wall_j;
        if (Matches(pattern, otherConnect2)) return wall_k;
        if (Matches(pattern, otherConnect3)) return wall_cross;
        if (Matches(pattern, otherConnect4)) return wall_Left;
        if (Matches(pattern, otherConnect5)) return wall_Right;
        if (Matches(pattern, otherConnect6)) return wall_Top_Left;
        if (Matches(pattern, otherConnect7)) return wall_Bottom_Right;
        if (Matches(pattern, otherConnect8)) return wall_Bottom_Left;
        if (Matches(pattern, otherConnect9)) return wall_Top_Right;
        if (Matches(pattern, otherConnect10)) return wall_Bottom_Left;
        if (Matches(pattern, otherConnect11)) return wall_Bottom_Left;
        if (Matches(pattern, otherConnect12)) return wall_Top_Right;
        if (Matches(pattern, otherConnect13)) return wall_Top_Center;
        if (Matches(pattern, otherConnect14)) return wall_Bottom;
        if (Matches(pattern, otherConnect15)) return wall_Bottom_Left;
        if (Matches(pattern, otherConnect16)) return wall_j;
        if (Matches(pattern, otherConnect17)) return wall_Top_Right;
        if (Matches(pattern, otherConnect18)) return wall_Bottom_Right;
        if (Matches(pattern, otherConnect19)) return wall_Top_Left;
        if (Matches(pattern, otherConnect20)) return wall_Bottom_Right;

        if (Matches(pattern, TopResult1)) return wall_Top_Right;
        if (Matches(pattern, TopResult3)) return wall_Top_Left;

        if (Matches(pattern, LeftResult1)) return wall_Top_Left;
        if (Matches(pattern, LeftResult3)) return wall_Bottom_Left;

        if (Matches(pattern, RightResult1)) return wall_Top_Right;
        if (Matches(pattern, RightResult3)) return wall_Bottom_Right;

        if (Matches(pattern, BottomResult1)) return wall_Bottom_Left;
        if (Matches(pattern, BottomResult3)) return wall_Bottom_Right;
        // 기본값
        return null;
    }
    /// <summary>
    /// 바닥 타일 랜덤으로 깔기
    /// </summary>
    /// <returns></returns>
    Tile GetRandomFloorTile()
    {
        int r = UnityEngine.Random.Range(0, 200);
        switch (r)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
                return floor_Random_0;
            case 11:
                return floor_Random_1;
            case 12:
                return floor_Random_2;
            case 13:
                return floor_Random_3;
            default:
                return floor;
        }
    }
    int[] surrX = { -1, 0, 1, -1, 0, 1, -1, 0, 1 };             //해당 좌표 기준으로 주위 x,y좌표를 따올 배열
    int[] surrY = { 1, 1, 1, 0, 0, 0, -1, -1, -1 };

    /// <summary>
    /// 상하좌우 대각에 해당하는 위치에 타일이 있는지 계산
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    int PatternCal(int x, int y)
    {
        int pattern = 0;
        int bitIdx = 0;

        for (int i = 0; i < 9; i++)  //본인 포함 9좌표
        {
            int checkX = x + surrX[i];
            int checkY = y + surrY[i];

            // 맵 배열 위에 있을때만 로직진행
            if (checkX >= 0 && checkX < map.GetLength(1) && checkY >= 0 && checkY < map.GetLength(0))
            {

                if (map[checkY, checkX] != (int)Define.GridType.None)
                {
                    pattern |= (1 << bitIdx);       //None이 아니라면(0) 비트연산
                }
            }
            bitIdx++;
        }
        return pattern;             //비트마스킹을 완료한 해당 좌표 패턴을 반환
    }
    /// <summary>
    /// None타일 기준으로 상하좌우 대각에 해당하는 위치에 플로어그리드가 있는지 계산
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    int PatternCalNone(int x, int y)
    {
        int pattern = 0;
        int bitIdx = 0;

        for (int i = 0; i < 9; i++)  //본인 포함 9좌표
        {
            int checkX = x + surrX[i];
            int checkY = y + surrY[i];

            // 맵 배열 위에 있을때만 로직진행
            if (checkX >= 0 && checkX < map.GetLength(1) && checkY >= 0 && checkY < map.GetLength(0))
            {

                if (map[checkY, checkX] == (int)Define.GridType.MainRoom)
                {
                    pattern |= (1 << bitIdx);       //None이 아니라면(0) 비트연산
                }
            }
            bitIdx++;
        }
        return pattern;             //비트마스킹을 완료한 해당 좌표 패턴을 반환
    }
    /// <summary>
    /// 마스크가 패턴과 같다면 true반환
    /// </summary>
    /// <param name="pattern">해당 좌표 패턴</param>
    /// <param name="mask">검사 할 마스크</param>
    /// <returns></returns>
    bool Matches(int pattern, int mask)
    {
        return pattern == mask;
    }

    #endregion




    /// <summary>
    /// 방 타입에 해당하는 방 중에서 랜덤으로 방을 선정하고 위치를 반환
    /// </summary>
    /// <param name="roomType"></param>
    /// <returns></returns>
    public List<Vector2> GetRoomPos(Define.RoomType roomType, int count)
    {
        UnDuplicateRanPick<Room> udrpRoom = new UnDuplicateRanPick<Room>();
        List<Room> roomList = new List<Room>();
        List<Vector2> confirmList = new List<Vector2>();

        foreach (Room room in rooms)
        {
            if (room.CurRoomData.curRoomType == roomType)
            {
                roomList.Add(room);
            }
        }
        udrpRoom.SetItem(roomList);
        for (int i = 0; i < count; i++)
        {
            if (roomList.Count != 0)
            {

                Room r = udrpRoom.GetItem();
                if (r != null)
                    confirmList.Add(r.transform.position);
            }
        }
        return confirmList;
    }
    /// <summary>
    /// 방 타입에 해당하는 방들을 모두 가져와서 리스트로 위치를 반환
    /// </summary>
    /// <param name="roomType"></param>
    /// <returns></returns>
    public List<Vector2> GetRoomPosAll(Define.RoomType roomType)
    {
        UnDuplicateRanPick<Room> udrpRoom = new UnDuplicateRanPick<Room>();
        List<Room> roomList = new List<Room>();
        List<Vector2> confirmList = new List<Vector2>();

        foreach (Room room in rooms)
        {
            if (room.CurRoomData.curRoomType == roomType)
            {
                roomList.Add(room);
            }
        }
        udrpRoom.SetItem(roomList);
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList.Count != 0)
            {
                Room r = udrpRoom.GetItem();
                confirmList.Add(r.transform.position);
            }
        }
        return confirmList;
    }
}
