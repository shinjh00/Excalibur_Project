using Delaunay;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class MapGenPrac : MonoBehaviour
{
    static MapGenPrac instance;
    public static MapGenPrac Instance {  get { return instance; } }

    [Header("Variables")]
    [SerializeField] GameObject mapObject;
    [SerializeField] GameObject gridPrefab;
    [SerializeField] int startRoomCnt;
    [SerializeField] int selectRoomCnt;

    [Header("SmallRoomSize")]
    [SerializeField] int smallMinSizeX;
    [SerializeField] int smallMaxSizeX;
    [SerializeField] int smallMinSizeY;
    [SerializeField] int smallMaxSizeY;
    [Header("BigRoomSize")]
    [SerializeField] int bigMinSizeX;
    [SerializeField] int bigMaxSizeX;
    [SerializeField] int bigMinSizeY;
    [SerializeField] int bigMaxSizeY;

    [Header("offset")]
    [SerializeField] int overlapOffset;

    int minX = int.MaxValue,minY = int.MaxValue;
    int maxX = int.MinValue,maxY = int.MinValue;
    int [,] map;

    [SerializeField] List<GameObject> rooms = new List<GameObject>();

    Dictionary<int,Vector2> selectedRooms = new Dictionary<int,Vector2>();
   HashSet<Delaunay.Vertex> points = new HashSet<Delaunay.Vertex>();
    [field : SerializeField] List<Delaunay.Edge> hallwayEdges;


    int hallwayId = 200;
    private void Awake()
    {
        if(instance == null )
        {
            GameObject go = GameObject.Find("MapGenerator");
            if(go == null )
            {
                go = new GameObject { name = "MapGenerator" };
                go.AddComponent<MapGenPrac>();
            }
            DontDestroyOnLoad(go);
            instance = go.GetComponent<MapGenPrac>();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private void Start()
    {
        StartCoroutine(MapGenRoutine());
    }
    IEnumerator MapGenRoutine()
    {

        for(int i = 0;i<startRoomCnt;i++ )                      // # 1 : 시작하면 방 프리팹 랜덤한  원모양 랜덤으로 생성 후 리스트에 넣기
        {
            rooms.Add(Instantiate(gridPrefab, GetRandomPointInCircle(50), Quaternion.identity));
            gridPrefab.name = $"room {i}";
            if ( i > selectRoomCnt )
                rooms [i].transform.localScale = new Vector3(Random.Range(smallMinSizeX, smallMaxSizeX),Random.Range(smallMinSizeY,smallMaxSizeY),0);
            else
                rooms [i].transform.localScale = new Vector3(Random.Range(bigMinSizeX, bigMaxSizeX), Random.Range(bigMinSizeY, bigMaxSizeY), 0);
        }
        yield return null;

        for(int i = 0;i<startRoomCnt; i++ )                                         // # 2 : 물리엔진 켜서 방을 흩뿌림
        {
            rooms [i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            rooms [i].GetComponent<Rigidbody2D>().gravityScale = 0f;
        }
        yield return new WaitForSeconds(5);


        FindMapRooms(selectRoomCnt);
        GenerateMapArr();
        MainRoomFraming();
        ConnectRoom();
        GenerateHallWays(hallwayEdges);
    }

    public Vector3 GetRandomPointInCircle( int rad )
    {
        Vector2 RandomPos = Random.insideUnitSphere * rad;

        return new Vector2(Mathf.Round(RandomPos.x),Mathf.Round(RandomPos.y));  
    }

    void FindMapRooms(int roomCnt )         // # 3 : 방 포지션을 정수로 바꾸고 비율이 맞는 방은 딕셔너리와 정점해쉬셋에 추가하는 함수
    {
        Dictionary<int,float> tmpRooms = new Dictionary<int,float>();       // # 4 : 임시로 방을 선택할 딕셔너리 초기화
        for(int i = 0; i < rooms.Count; i++)
        {
            rooms [i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;     // # 5 : 모든 방 물리엔진 끄기
            rooms [i].GetComponent<BoxCollider2D>().isTrigger = true;
            rooms [i].transform.position = new Vector3(Mathf.Round(rooms [i].transform.position.x), Mathf.Round(rooms [i].transform.position.y), 1);

            Vector3 scale = rooms [i].transform.localScale;
            float size = scale.x * scale.y;
            float ratio = scale.x / scale.y;

            if ( ratio > 2f || ratio < 0.5f )                   // # 6 : 비율 안맞으면 건너뛰고 맞는 방은 임시딕셔너리에 추가 (인덱스,사이즈)
                continue;

            tmpRooms.Add(i, size);
        }
        // # 7 : 방의 크기에 따라 내림차순으로 정렬
        var sortedRooms = tmpRooms.OrderByDescending(room => room.Value).ToList();
        foreach ( var room in rooms )
        {
            room.SetActive(false);              // # 8 : 일단 전부 게임오브젝트 비활성화
        }

        int count = 0;
        selectedRooms = new Dictionary<int, Vector2> ();
        foreach( var room in sortedRooms )
        {
            
            if ( count >= roomCnt )
                break;
            GameObject selectedRoom = rooms [room.Key];                 // # 9 : 부적합한 방을 제외하고 선택, 활성화하고 정점해쉬셋에 추가,딕셔너리에 추가(인덱스,포지션)
            SpriteRenderer sr = selectedRoom.GetComponent<SpriteRenderer>();
            if(sr != null )
                sr.color = Color.red;
            selectedRoom.SetActive(true);
            points.Add(new Delaunay.Vertex(( int )selectedRoom.transform.position.x, ( int )selectedRoom.transform.position.y));
            selectedRooms.Add(room.Key,new Vector2((int)selectedRoom.transform.position.x,( int )selectedRoom.transform.position.y) );

            count++;
        }
    }
    void GenerateMapArr()  
    {
        // # 10 : 배열 크기 결정을 위한 최소,최대 좌표 초기화

        foreach(var room in rooms )                     // # 11 : 최대/최소 좌표 탐색
        {
            Vector3 pos = room.transform.position;
            Vector3 scale = room.transform.localScale;

            minX = Mathf.Min(minX, Mathf.FloorToInt(pos.x - scale.x));
            minY = Mathf.Min(minY,Mathf.FloorToInt(pos.y - scale.y));
            maxX = Mathf.Max(maxX, Mathf.CeilToInt(pos.x + scale.x));
            maxY = Mathf.Max(maxY,Mathf.CeilToInt(pos.y + scale.y));               
        }
        // # 12 : 배열 너비와 높이 계산
        int width = maxX - minX ;
        int height = maxY - minY ;
                                            
        map = new int [height, width];
        Debug.Log("map arr Created");// # 13 : 배열 최대 크기만큼 -1로 초기화
        for ( int i = 0; i < height; i++ )
            for ( int j = 0; j < width; j++ ) map [i, j] = -1;     

        for(int i = 0; i < rooms.Count; i++ )               // # 14 : 배열에서 방에 해당하는 부분을 방 인덱스로 설정
        {
            Vector3 pos = rooms [i].transform.position; 
            Vector3 scale = rooms [i].transform.localScale;

            for ( int x = ( int )-scale.x / 2; x < scale.x / 2; x++ )
            {
                for ( int y = ( int )-scale.y / 2; y < scale.y / 2; y++ )
                {
                    int mapX = Mathf.FloorToInt(pos.x - minX + x);
                    int mapY = Mathf.FloorToInt(pos.y - minY + y);
                    map [mapY, mapX] = i;
                    
                }
            }
             
        }
    }
    void MainRoomFraming()
    {
        foreach(var room in selectedRooms )
        {
            int selectedId = room.Key;                      //# 15 :  방 크기와 위치 재설정
       //  rooms [selectedId].transform.position = room.Value - new Vector2(minX, minY) + new Vector2(0, 0.5f);
       //  rooms [selectedId].transform.localScale = rooms[selectedId].transform.localScale + new Vector3(0, 1, 0);

       //  rooms [selectedId].GetComponent<SpriteRenderer>().color = Color.clear;           //# 16 :  방 색을 투명, 순위 4로
         rooms [selectedId].GetComponent<SpriteRenderer>().sortingOrder = 4;

            int minIx = int.MaxValue; int maxIx = int.MinValue;
            int minIy = int.MaxValue; int maxIy = int.MinValue;

            for ( int y = 0; y < map.GetLength(0); y++ )
            {
                for ( int x = 0; x < map.GetLength(1); x++ )
                {
                    if ( map [y, x] == selectedId )                  //# 17 :  맵 배열에서 인덱스에 해당하는 부분이 오버플로 하는지 안하는지 확인?
                    {
                        minIx = Mathf.Min(minIx, x);
                        minIy = Mathf.Min(minIy, y);
                        maxIx = Mathf.Max(maxIx, x);
                        maxIy = Mathf.Max(maxIy, y);
                    }

                }
            }
            //# 18 : 찾은 경계를 따라 끝부분을 0으로 설정
            for ( int y = minIy; y <= maxIy; y++ )
            {
                for ( int x = minIx; x <= maxIx; x++ )
                {
                    //# 19 : 가장자리인지 확인하고, 가장자리라면 -1으로 설정
                    if ( x == minIx || x == maxIx || y == minIy || y == maxIy )
                    {
                        map [y, x] = -1;
                    }
                }
            }
        }
        
    }
    void ConnectRoom()
    {
        var triangles = DelaunayTriangulation.Triangulate(points);       //# 20 : 방의 정점해쉬들로 들로네 삼각분할

        var graph = new HashSet<Delaunay.Edge>();          
        foreach ( var trg in triangles )
            graph.UnionWith(trg.edges);                                 // # 21 : 간선화

        hallwayEdges = Kruskal.MinimumSpanningTree(graph);              // # 22 : MST로 간선들 정리하고 이걸로 복도를 만들 리스트에 넣기


    }
    public LineRenderer lineRendererPrefab; // 라인 렌더러 프리팹 설정
    void GenerateHallWays(IEnumerable<Delaunay.Edge> isTree )              // # 23 : 정리한 간선리스트들(isTree)로 복도 만들기
    {
        Vector2Int size1 = new Vector2Int(2, 2);
        Vector2Int size2 = new Vector2Int(2, 2);

        foreach ( Delaunay.Edge edge in isTree )
        {
            
            // 시작점과 끝점 사이의 복도 생성
            
               Vertex start = edge.a;
               Vertex end = edge.b;
              // DrawCorridor(start, end);

               size1 = new Vector2Int(( int )rooms [map [start.y - minY, start.x - minX]].transform.localScale.x, 
                                      ( int )rooms [map [start.y - minY, start.x - minX]].transform.localScale.y);
               size2 = new Vector2Int(( int )rooms [map [end.y - minY, end.x - minX]].transform.localScale.x, 
                                      ( int )rooms [map [end.y - minY, end.x - minX]].transform.localScale.y);

            CreateCorridor(start, end, size1, size2);



        }

    }
    void DrawCorridor( Delaunay.Vertex start, Delaunay.Vertex end )
    {
        // 라인 렌더러 인스턴스 생성
        LineRenderer lineRenderer = Instantiate(lineRendererPrefab);

        // 시작점과 끝점 설정
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, new Vector3(start.x,start.y));
        lineRenderer.SetPosition(1, new Vector3(end.x,end.y));
    }
    private void CreateCorridor( Vertex start, Vertex end, Vector2Int startSize, Vector2Int endSize )
    {

        // overlapOffset : 수평, 수직 복도를 그리는데 있어 두 방의 간격이 얼마나 어긋난가 판정하는 지표.
        // Mathf.Abs(start.x - end.x) => 그 축에 대한 간선거리(대각선 아님), ( startSize.x + endSize.x ) / 2f => 해당 방 중심부터 방 끝지점까지 거리
        // 해당 방 중심부터 방 끝지점까지 거리에서 해당 축 간선거리를 빼면 그 사이 거리(겹치는 부분)가 나옴
        // 오프셋을 빼줬는데도 겹치는 부분이 있다면 true 반환 아니면 false

        bool isHorizontalOverlap = Mathf.Abs(start.x - end.x) < ( ( startSize.x + endSize.x ) / 2f - overlapOffset );
        bool isVerticalOverlap = Mathf.Abs(start.y - end.y) < ( ( startSize.y + endSize.y ) / 2f - overlapOffset );
        if ( isVerticalOverlap )
        {
            int startY = Mathf.Min(start.y + startSize.y / 2, end.y + endSize.y / 2) + Mathf.Max(start.y - startSize.y / 2, end.y - endSize.y / 2);
            startY /= 2;
            for ( int x = Mathf.Min(start.x + startSize.x / 2, end.x + endSize.x / 2); x <= Mathf.Max(start.x - startSize.x / 2, end.x - endSize.x / 2); x++ )
            {
                AddHallwayWidth(x, startY);
            }
        }
        else if ( isHorizontalOverlap )
        {
            int startX = Mathf.Min(start.x + startSize.x / 2, end.x + endSize.x / 2) + Mathf.Max(start.x - startSize.x / 2, end.x - endSize.x / 2);
            startX /= 2;
            for ( int y = ( int )Mathf.Min(start.y + startSize.y / 2, end.y + endSize.y / 2); y <= ( int )Mathf.Max(start.y - startSize.y / 2, end.y - endSize.y / 2); y++ )
            {
                AddHallwayWidth(startX, y);
            }
        }
        else
        {   // 수직, 수평복도로 이어지지 않은 방들 잇기('ㄴ', 'ㄱ'자)
            int mapCenterX = map.GetLength(0) / 2;
            int mapCenterY = map.GetLength(1) / 2;
            int midX = ( start.x + end.x ) / 2;
            int midY = ( start.y + end.y ) / 2;
            // 맵의 중심에 비하여 이어야 할 길의 중심점이 어느 사분면에 위치한지 파악하여
            // 1, 3 가로 이동 후 세로 이동, 2, 4 세로 이동 후 가로 이동
            int quadrant = DetermineQuadrant(midX - mapCenterX - minX, midY - mapCenterY - minY);
            if ( quadrant == 1 || quadrant == 3 )
            {
                CreateStraightHall(start.x, start.y, end.x, start.y);
                CreateStraightHall(end.x, start.y, end.x, end.y);
            }
            else
            {
                CreateStraightHall(start.x, start.y, start.x, end.y);
                CreateStraightHall(start.x, end.y, end.x, end.y);
            }
        }
    }

    private void CreateStraightHall( int startX, int startY, int endX, int endY )
    {

        for ( int x = Mathf.Min(startX, endX); x <= Mathf.Max(startX, endX); x++ )
        {
            for ( int y = Mathf.Min(startY, endY); y <= Mathf.Max(startY, endY); y++ )
            {
                AddHallwayWidth(x, y);
            }
        }
    }
    private int DetermineQuadrant( int x, int y )
    {
        if ( x >= 0 && y >= 0 ) return 1;
        if ( x < 0 && y >= 0 ) return 2;
        if ( x < 0 && y < 0 ) return 3;
        if ( x >= 0 && y < 0 ) return 4;
        return 0; // 이 경우는 발생하지 않음
    }
    private void AddHallwayWidth( int x, int y )
    {
        for ( int i = -1; i <= 1; i++ )
        {
            for ( int j = -1; j <= 1; j++ )
            {
                var px = x + i; var py = y + j;
                if ( px < minX || py < minY || py >= maxY || px >= maxX ) continue;
                if ( map [py - minY, px - minX] == hallwayId ) continue;


                if ( map [py - minY, px - minX] == -1 || !rooms [map [py - minY, px - minX]].activeSelf )
                {

                    map [py - minY, px - minX] = hallwayId;
                    
                     GameObject grid = Instantiate(gridPrefab, new Vector3(px + 0.5f, py + 0.5f, 0), Quaternion.identity);
                    grid.GetComponent<SpriteRenderer>().color = Color.black;
                }
            }
        }
    }
    private void PrintMapToLog()
    {
        string logOutput = "Map Array:\n";
        for ( int i = 0; i < map.GetLength(0); i++ )
        {
            for ( int j = 0; j < map.GetLength(1); j++ )
            {
                if ( map [i, j] == -1 )
                {
                    logOutput += "- 1 ";
                }
                else
                {
                    logOutput += $"+{( map [i, j] + 1 ).ToString().PadLeft(2)} ";
                }
            }
            logOutput += "\n";
        }
        Debug.Log(logOutput);
    }




}

