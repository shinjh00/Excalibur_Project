using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 이형필 / 방 리스트를 가지고 있는 세대
/// </summary>
public class Generation
{/// <summary>
/// 이전 세대 룸 리스트
/// </summary>
    [SerializeField] public List<Room> PreRooms = new List<Room>();
    UnDuplicateRanPick<Room> udrpRoom = new UnDuplicateRanPick<Room>();
    /// <summary>
    /// 특정 방 추가
    /// </summary>
    /// <param name="room">방</param>
    public void AddRoom(Room room)
    {
        PreRooms.Add(room);
        Debug.Log($"add {room.name} {room.RoomType}");
    }
    /// <summary>
    /// 방 리스트를 추가
    /// </summary>
    /// <param name="rooms"></param>
    public void AddRoom(List<Room> roomList)
    {
        PreRooms.AddRange(roomList);
    }
    /// <summary>
    /// 새로운 방 추가
    /// </summary>
    /// <param name="generation"> 해당 세대 </param>
    /// <param name="wayWidth">복도의 짧은쪽</param>
    /// <param name="wayHeight">복도의 긴쪽</param>
    /// <returns></returns>
    public List<Room> CreateNewRooms(int generation, int end, int late, int begin)
    {
        //  Debug.Log(generation);
        List<Room> newRooms = new List<Room>();
        List<BuildInfo> infos = new List<BuildInfo>();
        UnDuplicateRanPick<EDir> udrpEdir = new UnDuplicateRanPick<EDir>();
        WeightedRandom<BuildInfo> wrpInfo = new WeightedRandom<BuildInfo>();
        Room randomRoom;
        EDir eDir;
        udrpRoom.SetItem(PreRooms);
        //  Debug.Log($"udrp count : {udrpRoom.itemList.Count}");
        while (!udrpRoom.IsEmpty())
        {


            randomRoom = udrpRoom.GetItem();

            udrpEdir.SetItem(GetEmptyDirList(randomRoom));
            while (!udrpEdir.IsEmpty())
            {

                eDir = udrpEdir.GetItem();
                infos.Clear();
                int minScale = randomRoom.CheckMinScale(); //복도 크기 조정
                minScale /= 2;
                int width = Random.Range(10, minScale + 3);          // 복도 크기 랜덤으로 결정.. DB 영향 받을 수 있음 (DB CHECK)
                int height = Random.Range(10, minScale + 8);
                infos.AddRange(RoomChecker.GetCanBuildInfoList(randomRoom, eDir, width, height, generation, end, late, begin));            // 방을 만들수 있는지 확인하고 infos리스트에 추가
                if (infos.Count == 0)
                {
                    continue;
                }


                wrpInfo.Clear();
                string keys = "";
                for (int i = 0; i < infos.Count; i++)
                {
                    wrpInfo.Add(infos[i], infos[i].roomPrefab.weight);

                    string keyName = infos[i].roomPrefab.name;
                    keys += $"{keyName}({infos[i].roomPrefab.weight}) ";
                }


                BuildInfo ranInfo = wrpInfo.GetItem();          //가중치 따라서 랜덤으로 가져옴
                Room newRoom = GameObject.Instantiate(ranInfo.roomPrefab, ranInfo.pos, Quaternion.identity);
                if (ObjectGrouping.Instance != null)
                {
                    newRoom.transform.parent = ObjectGrouping.Instance.RoomsGroup;
                }
                if (ranInfo.roomPrefab.RoomType == Define.RoomType.PuzzleRoom)
                {
                    newRoom.name = "PuzzleRoom";
                }
                GameManager.Ins.totalRoomCount++;
                RoomChecker.LinkRoom(randomRoom, newRoom, eDir);
                newRooms.Add(newRoom);
                newRoom.AddRootDistance(randomRoom, newRoom);

            }
        }
        return newRooms;

    }


    /// <summary>
    /// //가져온 룸에서 비어있는 방향 확인하고 비어있는 방향을 리스트로 반환
    /// </summary>
    /// <param name="room"></param>
    /// <returns></returns>
    List<EDir> GetEmptyDirList(Room room)
    {
        List<EDir> list = new List<EDir>();
        for (int i = 0; i < Direction.eDir.Length; i++)
        {
            if (room.nextRooms[(int)Direction.eDir[i]] == null)
            {
                list.Add(Direction.eDir[i]);
            }
        }
        return list;
    }
    /// <summary>
    /// 해당 세대 모든 룸 색 변경
    /// </summary>
    /// <param name="color"></param>
    public void SetAllColor(Color color)
    {
        foreach (Room room in PreRooms)
        {
            room.SetColor(color);
        }
    }
    /// <summary>
    /// 해당 세대 모든 룸 타입 변경
    /// </summary>
    /// <param name="roomType"></param>
    public void SetAllRoomType(Define.RoomType roomType)
    {
        foreach (Room room in PreRooms)
        {
            room.RoomType = roomType;
        }
    }
    /// <summary>
    /// 해당 세대의 룸리스트에서 count만큼 랜덤으로 룸 타입 변경 x,y크기만큼 검사하고 겹치는 룸타입이 있다면 다른 위치를 찾음
    /// </summary>
    /// <param name="roomType"></param>
    /// <param name="count"></param>
    public int SetRoomType(Define.RoomType roomType, int x, int y, int count)
    {
        int complete = 0;
        Room rRoom;
        udrpRoom.SetItem(PreRooms); int inf = 0;
        for (int i = 0; i < count; i++)
        {
            if (!udrpRoom.IsEmpty())
            {
                bool isSpawnRoom = false;
                while (!isSpawnRoom)
                {
                    if (inf < 30)
                    {
                        rRoom = udrpRoom.GetItem();
                        if (rRoom == null)
                            break;

                        if (count <= 0)
                        {
                            Debug.LogError("Count is less than or equal to zero");
                            return complete;
                        }
                        Collider2D[] cols = Physics2D.OverlapBoxAll(rRoom.transform.position, new Vector2(x, y), 0, LayerMask.GetMask("Room"));
                        Collider2D selfCollider = rRoom.GetComponent<Collider2D>();

                        bool hasSameRoomType = false;
                        foreach (Collider2D col in cols)
                        {
                            if (col == selfCollider)
                                continue;
                            Room r = col.GetComponent<Room>();
                            if (r.RoomType == roomType)
                            {
                                hasSameRoomType = true;
                                inf++;

                                break;
                            }
                        }

                        if (!hasSameRoomType)
                        {
                            rRoom.RoomType = roomType;
                            rRoom.name = "PlayerSpawnRoom";
                            isSpawnRoom = true;
                            complete++;

                        }

                    }
                    else
                    {
                        return complete;
                    }

                }

            }
        }
        return complete;
    }



}
