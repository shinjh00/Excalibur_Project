using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 개발자 : 이형필 / 방향을 전역적으로 보기쉽게 활용하기 위한 구조체
/// </summary>
public enum EDir { UP,DOWN,LEFT,RIGHT}
public enum GolemEDir { UP, RIGHT, DOWN, LEFT }
/// <summary>
/// 개발자 : 이형필 / Edir을 활용하기 위한 글래스
/// </summary>
public static class Direction
{
    public static EDir [] eDir = { EDir.UP, EDir.DOWN, EDir.LEFT, EDir.RIGHT };
    
    public static int [] xDir = { 0, 0, -1, 1 };
    public static int [] yDir = { 1, -1, 0, 0 };
    /// <summary>
    /// 개발자 : 이형필 / 방향의 반대방향을 반환하는 메서드
    /// </summary>
    public static EDir GetReverseDir( EDir eDir )
    {
        switch ( eDir )
        {
            case EDir.UP: return EDir.DOWN;
            case EDir.DOWN: return EDir.UP;
            case EDir.LEFT: return EDir.RIGHT;
            default: return EDir.LEFT;
        }
    }

    public static GolemEDir GetFrontDirection(GolemEDir currentDirection)
    {
        return currentDirection;
    }
    public static GolemEDir GetRightDirection(GolemEDir currentDirection)
    {
        return (GolemEDir)(((int)currentDirection + 1) % 4);
    }
    public static GolemEDir GetBackDirection(GolemEDir currentDirection)
    {
        return (GolemEDir)(((int)currentDirection + 2) % 4);
    }

    public static GolemEDir GetLeftDirection( GolemEDir currentDirection )
    {
        return (GolemEDir)(((int)currentDirection + 3) % 4);
    }


}
/// <summary>
/// 현재 방과 부모방, 그 사이 복도가 설계되어있는 정보구조
/// </summary>
public struct BuildInfo
{
    public int ID { get; private set; }
    public Room roomPrefab;
    public Vector2 pos;
    private static int nextId = 0;
    /// <summary>
    /// 빌드인포 사용구조
    /// </summary>
    /// <param name="roomPrefab">생성 방</param>
    /// <param name="parent">부모 방</param>
    /// <param name="eDir">부모가 생성방으로 향하고 있는 방향</param>
    /// <param name="min">복도 크기 작은값</param>
    /// <param name="max">복도 크기 큰값</param>
    public BuildInfo( Room roomPrefab, Room parent, EDir eDir,int min,int max)
    {
        ID = nextId++;                  //구조체 가져올때 중복방지용으로 빌드인포 짤때마다 전역적으로 id++
        this.roomPrefab = roomPrefab;
        this.roomPrefab.hallMin = min;
        this.roomPrefab.hallMax = max;
        pos = Vector2.zero;
        pos = GetPos(parent, eDir);
    }
    /// <summary>
    /// 부모 방 좌표
    /// </summary>
    /// <param name="parent">부모 방</param>
    /// <param name="eDir"> 방향 </param>
    /// <returns></returns>
    Vector2 GetPos(Room parent, EDir eDir)
    {
        
        if( eDir == EDir.UP ||eDir == EDir.DOWN )
        {
            return new Vector2
                (parent.transform.position.x,
                 parent.transform.position.y + ( ( parent.transform.localScale.y + roomPrefab.height ) * 0.5f + roomPrefab.hallMax ) * Direction.yDir [( int )eDir]);
        }
        else
        {
            return new Vector2
            (parent.transform.position.x + ( ( parent.transform.localScale.x + roomPrefab.width ) * 0.5f + roomPrefab.hallMax ) * Direction.xDir [( int )eDir],
            parent.transform.position.y);
        }
        

    }
}

public class RoomChecker
{
    /// <summary>
    /// 인접 확인하고 인접해있으면 true 아니면 false
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool IsNextPosition(Room a,Room b )             
    {
        float a_x = ( float )( Mathf.Round(a.transform.position.x * 10f) ) * 0.1f;
        float a_y = ( float )( Mathf.Round(a.transform.position.y * 10f) ) * 0.1f;
        float b_x = ( float )( Mathf.Round(b.transform.position.x * 10f) ) * 0.1f;
        float b_y = ( float )( Mathf.Round(b.transform.position.y * 10f) ) * 0.1f;

        if(a_x == b_x )                 //y축 확인
        {
            float dist  = (float)Mathf.Abs(a_y - b_y);          //둘 사이 거리 
            float value = ( a.height + b.height ) * 0.5f + b.hallMin;       //둘 사이의 거리와 복도 거리 합한 수치

            return dist == value;                           //같다면 true 반환
        }
        else if (a_y == b_y )           //x축 확인(위와 같은 로직)
        {
            float dist =(float)Mathf.Abs(a_x- b_x);
            float value = ( a.width + b.width ) * 0.5f +b.hallMax;

            return dist == value;
        }
        return false;
    }
    /// <summary>
    ///  // 본인크기+복도크기 만큼 충돌체가 없으면 true반환
    /// </summary>
    /// <param name="room"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static bool CheckRoomCollision( Room room, Vector2 pos )
    {
        Collider2D [] colls = new Collider2D [10];
        int collisionCount = Physics2D.OverlapBoxNonAlloc(             
            pos,
            new Vector2(room.width + room.hallMin,
                        room.height + room.hallMax),
            0,
            colls,
            LayerMask.GetMask("Room")
            
        );
        return collisionCount == 0;
    }
    /// <summary>
    /// 생성될 방 위치에 방을 생성 할 수 있는지 확인하고 된다면 빌드인포 리스트 반환
    /// </summary>
    /// <param name="parentRoom">부모 방</param>
    /// <param name="eDir">방향 </param>
    /// <param name="width">복도 작은 값</param>
    /// <param name="height">복도 큰 값</param>
    /// <returns></returns>
    public static List<BuildInfo> GetCanBuildInfoList(Room parentRoom, EDir eDir,int width,int height,int gen,int endRoomMin,
                                                                                                              int lateRoomMin,
                                                                                                              int beginRoomMin)
    {
        int pzCount = 0;                                    //세대 안에서 생길 퍼즐룸 카운트
        List<BuildInfo> list = new List<BuildInfo>();
        Vector2 pos;
      //  Debug.Log($"getCanBuild Logic {gen}gen");
        List<Room> tempList = new List<Room>();
        Room r = SecondMapGen.Ins.roomPrefab;
            r.roomGen = gen+1;
            int weight = Random.Range(1, 51);
            r.weight = weight;
            int w = 1;
            int h = 1;
            if (r.roomGen > endRoomMin && r.roomGen <= lateRoomMin)
            {

                r.RoomType = Define.RoomType.EndRoom;
                w = Random.Range(r.CurRoomData.roomSizeMinX, r.CurRoomData.roomSizeMaxX);                               // DB 영향 받을 수 있음 (DB CHECK)
                h = Random.Range(r.CurRoomData.roomSizeMinY, r.CurRoomData.roomSizeMaxY);

            }
            else if (r.roomGen > lateRoomMin && r.roomGen <= beginRoomMin)
            {

                if (Random.value < 0.4f && SecondMapGen.Ins.puzzleRoomCurCount < SecondMapGen.Ins.puzzleRoomMaxCount && pzCount < SecondMapGen.Ins.puzzleRoomMaxCount)
                {
                    pzCount++;
                r.RoomType = Define.RoomType.PuzzleRoom;
                w = Random.Range(r.CurRoomData.roomSizeMinX, r.CurRoomData.roomSizeMaxX);                               // DB 영향 받을 수 있음 (DB CHECK)
                h = Random.Range(r.CurRoomData.roomSizeMinY, r.CurRoomData.roomSizeMaxY);
                    r.weight = 100;
                }
                else
                {
                    r.RoomType = Define.RoomType.LateRoom;
                w = Random.Range(r.CurRoomData.roomSizeMinX, r.CurRoomData.roomSizeMaxX);                               // DB 영향 받을 수 있음 (DB CHECK)
                h = Random.Range(r.CurRoomData.roomSizeMinY, r.CurRoomData.roomSizeMaxY);
            }



            }
            else if (r.roomGen > beginRoomMin)
            {
                

                    r.RoomType = Define.RoomType.BeginningRoom;
            w = Random.Range(r.CurRoomData.roomSizeMinX, r.CurRoomData.roomSizeMaxX);                               // DB 영향 받을 수 있음 (DB CHECK)
            h = Random.Range(r.CurRoomData.roomSizeMinY, r.CurRoomData.roomSizeMaxY);
        }
       
            r.transform.localScale = new Vector3(w, h, 0);
            r.SetScale();
            tempList.Add(r);

        foreach(Room room in tempList)
        {
            
            
            if (room.weight <= 0f)
            {
                Debug.Log("weight 0");
                continue;
            }
                
            switch ( eDir )                 
            {

                case EDir.LEFT:             //왼쪽이나 오른쪽에 방을 생성하려면
                    case EDir.RIGHT:
                    {
                                                //부모 방의 x위치 + (방너비+ 생성할 방 크기)/2 + 복도 높이(긴쪽) * 방향(왼쪽,오른쪽) , 부모 방의 y위치   => 새롭게 설지될 방의 포지션
                        pos = new Vector2(parentRoom.transform.position.x + ( ( parentRoom.width + room.width ) * 0.5f + height) * Direction.xDir [( int )eDir],
                                          parentRoom.transform.position.y);
                        
                        break;
                    }
                default:
                    {
                        //부모방의 x위치 , 부모 방의 y위치 + (방너비+ 생성할 방 크기)/2 + 복도 높이(긴쪽) * 방향(위,아래)   => 새롭게 설지될 방의 포지션
                        pos = new Vector2(parentRoom.transform.position.x,
                                          parentRoom.transform.position.y + ( ( parentRoom.height + room.height ) * 0.5f + height ) * Direction.yDir [( int )eDir]);
                       
                        break;
                    }
            }
            if(room.RoomType != Define.RoomType.EndRoom)
            {
                if (CheckRoomCollision(room, pos)) //새롭게 생성될 방의 위치에 충돌체확인
                {

                    list.Add(new BuildInfo(room, parentRoom, eDir, width, height));                 //없으면 리스트에 추가 (생성방,부모방,방향)
                    if (room.RoomType == Define.RoomType.PuzzleRoom)
                    {
                        Debug.Log($"{room.RoomType} Created success");
                        SecondMapGen.Ins.puzzleRoomCurCount++;
                    }
                }
            }
            else
            {
                list.Add(new BuildInfo(room, parentRoom, eDir, width, height));// END룸 4개 강제 생성

            }


        }
        return list;
        
    }

    /// <summary>
    /// a방에서 b방으로 복도 연결
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="a2b"></param>
    public static void LinkRoom(Room a, Room b, EDir a2b)
    {
        a.nextRooms [(int)a2b] = b;                        
        b.nextRooms [( int )Direction.GetReverseDir(a2b)] = a;

        SecondMapGen.Ins.CreatePath(a, b, a2b);         
    }
    /// <summary>
    /// 생성 방들 중 인접해는지 확인하고 인접해있으면 복도 연결
    /// </summary>
    /// <param name="room"></param>
    public static void CheckAndLinkRoom(Room room)              
    {
        Vector2 pos;
        foreach(EDir edir in Direction.eDir )
        {
            if ( room.nextRooms [( int )edir] != null )
                continue;

            if ( edir == EDir.UP || edir == EDir.DOWN )
                pos = ( Vector2 )room.transform.position +
                    new Vector2(0, Direction.yDir [( int )edir] *
                    room.height * 0.5f + room.hallMax*0.5f);
            else
            {
                pos = ( Vector2 )room.transform.position +
                    new Vector2(Direction.xDir [( int )edir] *
                    room.width * 0.5f + room.hallMin*0.5f,0f);
            }

            RaycastHit2D hit = Physics2D.Raycast(pos, new Vector2(Direction.xDir [( int )edir],
                                                                    Direction.yDir [( int )edir]), 2f);
            if(!hit)
                continue;

            Room tempRoom = hit.transform.GetComponent<Room>();
            if ( tempRoom == null )
                continue;
            if ( IsNextPosition(room, tempRoom) )
            {
                LinkRoom(room, tempRoom, edir);
            }
                
        }
    }
    }
