using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class Room : MonoBehaviour
{
    [SerializeField] RoomData curRoomData;

    
    public int roomGen;
    public float width;
    public float height;
    [Header("UP,DOWN,LEFT,RIGHT")]
    public Room [] nextRooms = new Room [4];
    [Range(0f, 100f)]public float weight;
    SpriteRenderer sr;

    public int hallMin;
    public int hallMax;
    public List<float> rootDistance = new List<float>();
    public float resultRootDistance;
    Define.RoomType roomType;

    public Define.RoomType RoomType { get { return roomType; } 
        set 
        {
            roomType = value;
            curRoomData = FindData(roomType);
        }
    }

    public RoomData CurRoomData { get { return curRoomData; } }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    public void AddRootDistance(Room parentRoom, Room curRoom)
    {
        rootDistance.Add(Vector2.Distance(parentRoom.transform.position, curRoom.transform.position));
        if (parentRoom != null)
        {
            foreach(float rootDis in parentRoom.rootDistance)
            {
                rootDistance.Add(rootDis);
            }
        }
        foreach(float rootDis in rootDistance)
        {
            resultRootDistance += rootDis;
        }
    }
    /// <summary>
    /// 해당 룸 가로,세로 변수에 할당
    /// </summary>
    public void SetScale()
    {
        width = transform.localScale.x;
        height = transform.localScale.y;
    }/// <summary>
    /// 자연스러움을 위해서 가로,세로 더 작은사이즈를 찾아서 룸들을 연결할거임
    /// </summary>
    /// <returns></returns>
    public int CheckMinScale() { return (int)Mathf.Min(width, height); }
    /// <summary>
    /// 룸 컬러 변경
    /// </summary>
    /// <param name="color"></param>
    public void SetColor(Color color )
    {
        
        SpriteRenderer[] childSR = GetComponentsInChildren<SpriteRenderer>();
        foreach(  SpriteRenderer child in childSR)
        {
            child.color = color;
        }
    }
    private void OnDrawGizmosSelected()
    {
        foreach ( Room room in nextRooms )
        {
            if ( room == null ) continue;
            Gizmos.DrawLine(transform.position, room.transform.position);
        }
    }

    public Vector2 GetRoomHallWay()
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        int hallwayLayer = LayerMask.NameToLayer("HallWay");

        foreach (Transform child in allChildren)
        {
            if (child.gameObject.layer == hallwayLayer)
            {
                return child.position;
            }
        }
        return Vector2.zero;
        
    }

    /// <summary>
    /// 룸 타입을 이용하여 해당 룸에 대한 정보를 Csv에서 읽어오는 메소드
    /// </summary>
    /// <param name="targetRoom"></param>
    /// <returns></returns>
    public static RoomData FindData(Define.RoomType targetRoom)
    {
        int curId = 0;
        RoomData targetData;

        switch (targetRoom)
        {
            case Define.RoomType.PlayerSpawnRoom:
                curId = 1401000;
                break;
            case Define.RoomType.ExcaliburRoom:
                curId = 1401001;
                break;
            case Define.RoomType.BeginningRoom:
                curId = 1401002;
                break;
            case Define.RoomType.LateRoom:
                curId = 1401003;
                break;
            case Define.RoomType.EndRoom:
                curId = 1401004;
                break;
            case Define.RoomType.PuzzleRoom:                                               
                curId = 1401005;
                break;

        }

        if (curId != 0)
        {
            targetData = CsvParser.Instance.RoomDataDic[curId];
        }
        else
        {
            targetData = CsvParser.Instance.RoomDataDic[1401000];
        }

        return targetData;
    }
}