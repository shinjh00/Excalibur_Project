using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필 / 아이템을 스폰시켜주는 기능을 맵에 적용시키는 클래스
/// </summary>
public class ItemSpawnManager : MonoBehaviour
{
    [SerializeField] SpawnItemList spawnItemList;
    [SerializeField] LayerMask wallTrap;
    Collider2D[] cols = new Collider2D[3];

    private void Start()
    {
        wallTrap = LayerMask.GetMask("Wall") | LayerMask.GetMask("Trap") | LayerMask.GetMask("Interact");
    }
    /// <summary>
    /// 아이템 스폰을 시작하는 메서드
    /// </summary>
    public void ItemSpawnStart()
    {
        // 1. 번들 가져오기(ex 1306000)
        BoxBundleData curBundleData = CsvParser.Instance.BoxSpawnBundleDic[SecondMapGen.Ins.CurDunegon.chestSpawnBundle];

        // 2. 모든 번들 데이터를 검사하며, 해당 번들에 있는 방에, 스폰 확률에 따라 스폰 시키기
        for (int i = 0; i < curBundleData.chestSpawnerCount; i++)
        {
            // 2-1. 스폰 번들에 대한 ID 저장.
            BoxSpawnData targetData = CsvParser.Instance.BoxSpawnDic[curBundleData.chestSpawnIDs[i]];
            Debug.Log($"{targetData.roomType}, {targetData.id}");
            // 2-2. 상자 스폰 세팅 필드에 따른 스폰 시작
            List<Vector2> posList = new List<Vector2>();
            posList = SecondMapGen.Ins.GetRoomPosAll(targetData.roomType);

            for (int p = 0; p < posList.Count; p++)
            {
                int randProb = Random.Range(0, 101);

                if (randProb > targetData.chestSpawnProb)
                {   // 상자 스폰 확률 검사
                    continue;
                }

                Vector2 targetPos; // 실제 스폰 위치
                int isCol = 0;
                while (true)
                {   // 박스 스폰 위치를 설정. 만약 중복되는 위치가 없다면 break.
                    targetPos = posList[p] + Random.insideUnitCircle * 4f;
                    isCol = Physics2D.OverlapBoxNonAlloc(targetPos, new Vector2(1.5f, 1.5f), 0, cols, wallTrap);

                    if (isCol == 0)
                        break;
                }

                BoxSpawner instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Spanwer/Item/BoxSpawner", targetPos, Quaternion.identity).GetComponent<BoxSpawner>();
                instance.SetBoxSpawnData(targetData);
            } 
        }

        // 기존
        /*ItemSpawn(Define.RoomType.BeginningRoom, beginItemDropPer);
        ItemSpawn(Define.RoomType.LateRoom, lateItemDropPer);
        ItemSpawn(Define.RoomType.EndRoom, endItemDropPer);
        ItemSpawn(Define.RoomType.PuzzleRoom, 1); */
    }


    /// <summary>
    /// 룸타입을 가져와서 해당 방에 퍼센테이지에 따른 스폰을 실행시키는 메서드
    /// </summary>
    /// <param name="roomType"></param>
    /// <param name="percentage"></param>
    /*
    void ItemSpawn(Define.RoomType roomType, float percentage)
    {
        List<Vector2> posList = new List<Vector2>();
        posList = SecondMapGen.Ins.GetRoomPosAll(roomType);
        foreach (Vector2 p in posList)
        {
            if (Random.value < percentage)
            {
                if (roomType != Define.RoomType.PuzzleRoom)
                {
                    do
                    {
                        Vector2 pos;
                        Collider2D[] cols = new Collider2D[3];
                        WeightedRandom<BoxRank> wr = new WeightedRandom<BoxRank>();
                        BoxRank result;
                        BoxSpawner box = PhotonNetwork.InstantiateRoomObject("ItemBox/BoxSpawner", p, Quaternion.identity).GetComponent<BoxSpawner>();
                        int isCol = 0;
                        while (true)
                        {
                            pos = p + Random.insideUnitCircle * 4f;
                            isCol = Physics2D.OverlapBoxNonAlloc(pos, new Vector2(1.5f, 1.5f), 0, cols, wallTrap);

                            if (isCol == 0)
                                break;
                        }
                        switch (roomType)
                        {
                            case Define.RoomType.BeginningRoom:
                                box.SetBoxRank(BoxRank.Low);
                                box.BoxSpawn(pos);
                                break;
                            case Define.RoomType.LateRoom:
                                wr.Add(BoxRank.Low, 1);
                                wr.Add(BoxRank.Middle, 100);
                                result = wr.GetItem();
                                box.SetBoxRank(result);
                                box.BoxSpawn(pos);
                                break;
                            case Define.RoomType.EndRoom:
                                wr.Add(BoxRank.Middle, 100);
                                wr.Add(BoxRank.High, 50);
                                result = wr.GetItem();
                                box.SetBoxRank(result);
                                box.BoxSpawn(pos);
                                break;

                            default: break;
                        }

                    }
                    while (Random.value < percentage);
                }
                else
                {
                    BoxSpawner box = PhotonNetwork.InstantiateRoomObject("ItemBox/BoxSpawner", p, Quaternion.identity).GetComponent<BoxSpawner>();
                    box.SetBoxRank(BoxRank.Legend);
                    box.BoxSpawn(p);
                    break;
                }

            }
        }

    }
    */
}
