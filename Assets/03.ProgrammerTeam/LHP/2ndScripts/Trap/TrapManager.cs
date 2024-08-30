using Photon.Pun;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필 / 함정을 세팅하고 만들고 정의하는 클래스
/// </summary>
public class TrapManager : MonoBehaviour
{
    [Header("Trap")]
    [SerializeField] float trapSelectivity;
    [SerializeField] LayerMask interLayer;
    [SerializeField] LayerMask trapLayer;
    [SerializeField] LayerMask wallLayer;
    //   [SerializeField] List<TrapConfig> trapList;
    private void Start()
    {
        interLayer = LayerMask.GetMask("Interact");
        trapLayer = LayerMask.GetMask("Trap");
        wallLayer = LayerMask.GetMask("Wall");
    }
    public void TrapInit()
    {
        List<int> beginList = new List<int>();
        List<int> lateList = new List<int>();
        List<int> endList = new List<int>();
        // 1. 번들 전부 받아오기
        TrapBundleData curBundleTable = CsvParser.Instance.TrapSpawnBundleDic[SecondMapGen.Ins.CurDunegon.trapSpawnBundle];
        // 2. 모든 번들 데이터를 검사하며 리스트에 해당 아이디 넣어주기
        for (int i = 0; i < curBundleTable.trapSpawnerCount; i++)
        {   // 2-1 넣어주기 위해서 해당 ID의 룸 타입이 어떻게 되는지 먼저 체크
            Define.RoomType targetRoom = CsvParser.Instance.TrapSpawnDic[curBundleTable.trapSpawnIDs[i]].roomType;
            // 2-2 체크하였다면 초반, 중반, 후반 방 리스트에 해당 ID를 추가
            switch (targetRoom)
            {
                case Define.RoomType.BeginningRoom:
                    beginList.Add(curBundleTable.trapSpawnIDs[i]);
                    break;
                case Define.RoomType.LateRoom:
                    lateList.Add(curBundleTable.trapSpawnIDs[i]);
                    break;
                case Define.RoomType.EndRoom:
                    endList.Add(curBundleTable.trapSpawnIDs[i]);
                    break;
            }
        }
        SetTrap(beginList, Define.RoomType.BeginningRoom);
        SetTrap(lateList, Define.RoomType.LateRoom);
        SetTrap(endList, Define.RoomType.EndRoom);
    }
    async void SetTrap(List<int> trapSpawnID, Define.RoomType roomType)
    {
        List<Vector2> posList = new List<Vector2>();
        posList = SecondMapGen.Ins.GetRoomPosAll(roomType);                 //룸 타입에 해당하는 방 위치 리스트
        UnDuplicateRanPick<Vector2> udrpPos = new UnDuplicateRanPick<Vector2>();
        udrpPos.SetItem(posList);
        for (int j = 0; j < trapSpawnID.Count; j++)                  // 트랩 스폰id 수 만큼
        {
            TrapSpawnStruct trapSpawnstruct = CsvParser.Instance.TrapSpawnDic[trapSpawnID[j]];              //for문 돌아갈 트랩 스폰 id 캐싱
            for (int k = 0; k < trapSpawnstruct.trapCount; k++)                                             //트랩 스폰 id에 해당하는 트랩 갯수 만큼
            {
                TrapSpawnStruct.trapInfo info = trapSpawnstruct.trapInfos[k];                               //트랩 스폰 id에 들어있는 for문 돌릴 트랩 캐싱
                int createCount = UnityEngine.Random.Range(info.trapMin, info.trapMax + 1);                 // 해당 트랩 랜덤으로 갯수 지정
                for (int l = 0; l < createCount; l++)                                                //갯수만큼 실행
                {
                    if (UnityEngine.Random.Range(0, 101) <= info.trapProb)                                      //그 트랩에 해당하는 확률에 들어가면
                    {
                        if (udrpPos.itemList.Count < 1)
                        {
                            udrpPos.SetItem(posList);
                        }
                        Vector2 pos = udrpPos.GetItem();
                        await CreateTrap(pos, info);                                     //중복되지 않는 룸 위치를 랜덤으로 가져와서 트랩id에 해당하는 오브젝트 생성
                                                                                         //          Debug.Log($"Create : {info.trapId} Count : {createCount} Room : {roomType}");
                    }
                    else
                    {
                        l--;                  // 확률에 안맞으면 다시 위치를 돌림
                    }
                }
            }
        }
    }
    /// <summary>
    /// 트랩 ID를 가져와서 해당 pos에 생성시키는 메서드
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="trapId"></param>
    async Task CreateTrap(Vector2 pos, TrapSpawnStruct.trapInfo info)
    {
        Vector2 r = Vector2.zero;
        HashSet<Vector2> usedPositions = new HashSet<Vector2>();  // 이미 사용된 위치 저장
        int inf = 0;
        bool positionFound = false;
        while (!positionFound && inf < 30)
        {
            Collider2D[] cols = new Collider2D[2];
            r = pos + UnityEngine.Random.insideUnitCircle * trapSelectivity;
            int count = Physics2D.OverlapCircleNonAlloc(r, 2f, cols, trapLayer | wallLayer | interLayer);
            inf++;
            if (count < 1 && !usedPositions.Contains(r))  // 위치가 유효하고 중복되지 않는 경우
            {
                positionFound = true;
                usedPositions.Add(r);
            }
        }
        if (positionFound)
        {
            if (info.trapId == 1202000)
            {
                SpikeTrapCreate(r, info);
                return;
            }
            GameObject ins = CreatePhotonObject.Create(info.trapId, r);
            Trap trap = null;
            while (ins == null)
            {
                await Task.Yield();  // 비동기적으로 대기
            }
            trap = ins.GetComponent<Trap>();
            trap.TrapId = info.trapId;
            if (ObjectGrouping.Instance != null && ins != null)
            {
                ins.transform.parent = ObjectGrouping.Instance.TrapGroup;
            }
        }
        else
        {
            Debug.Log("자리를 못찾음");
        }
    }
    void SpikeTrapCreate(Vector2 pos, TrapSpawnStruct.trapInfo info)
    {
        Collider2D[] colliders;
        Vector2 changedPos = pos;
        int r = UnityEngine.Random.Range(4, 7 + 1);                                 //테이블화 시킬 가능성이.. 있따
        for (int i = 1; i < r; i++)
        {
            int inf = 0;
            do
            {
                inf++;
                EDir dir = (EDir)UnityEngine.Random.Range(0, 5);
                switch (dir)
                {
                    case EDir.UP:
                        changedPos += new Vector2(0, 1);
                        break;
                    case EDir.DOWN:
                        changedPos += new Vector2(0, -1);
                        break;
                    case EDir.LEFT:
                        changedPos += new Vector2(1, 0);
                        break;
                    case EDir.RIGHT:
                        changedPos += new Vector2(-1, 0);
                        break;
                }
                colliders = Physics2D.OverlapCircleAll(changedPos, 0.1f, trapLayer | wallLayer);
            }
            while (colliders.Length != 0 && inf < 8);
            GameObject ins = CreatePhotonObject.Create(info.trapId, changedPos);
            Trap trap = ins.GetComponent<Trap>();
            trap.TrapId = info.trapId;
        }
    }
    /* [Serializable]
     public class TrapConfig
     {
         public Trap trap;
         public int minCount;
         public int maxCount;
         public Define.RoomType roomType;
         [Range(0, 100)] public float spawnProbability;
     }*/
}