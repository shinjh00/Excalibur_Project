using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class Debugging2nd : MonoBehaviourPunCallbacks
{
    private static Debugging2nd ins;
    public static Debugging2nd Ins
    {
        get { return ins; }
    }

    [Tooltip("메인 로비의 로딩text를 할당할것")]
    [SerializeField] TMP_Text text;
    [SerializeField] GameObject loadImg;
    //   [SerializeField] MonsterGridManager monsterGridManager;
    [SerializeField] TrapManager trapManager;
    [SerializeField] ItemSpawnManager itemSpawnManager;
    [SerializeField] MaintenanceController mainTenanceController;
    [SerializeField] PuzzleManager puzzleManager;


    [SerializeField] Camera _camera;
    [SerializeField] public Vector3 MousePos;
    List<PlayerController> players = new List<PlayerController>();

    [Tooltip("체크할 시 중앙에서 스폰")]
    [SerializeField] bool isSpawnCenter;

    [Tooltip("인게임캔버스에 미니맵 할당")]
    public GameObject miniMapUi;
    [Tooltip("인게임캔버스에 챗ui 할당")]
    public GameObject chatUi;



    private void Awake()
    {
        trapManager = Resources.Load<TrapManager>("6.Prefab/Manager/TrapManager");

    }
    public void InitSetting()
    {
        _camera = Camera.main;
    }
    private void Start()
    {
        ins = this;
        _camera = Camera.main;
        PhotonPeer.RegisterType(typeof(Vector2), (byte)'V', SerializeVector2, DeserializeVector2);
    }

    private void Update()
    {
        MousePos = new Vector3(_camera.ScreenToWorldPoint(Input.mousePosition).x, _camera.ScreenToWorldPoint(Input.mousePosition).y);
    }

    public void GameStart()
    {

        loadImg.SetActive(true);
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(DelayCoroutine());

    }

    IEnumerator DelayCoroutine()
    {
        //   yield return new WaitUntil(() => (Input.GetKeyDown(KeyCode.Space)));

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            PhotonNetwork.PlayerList[i].SetProperty(DefinePropertyKey.OUTGAME, false);
        }
        SecondMapGen.Ins.StartGenerateMap();

        yield return GameStartDelay();
    }
    public void SelectCharacter()
    {
        ShowSelectImage();
    }
    IEnumerator GameStartDelay()
    {

        text.text = "Map Create Success Wait other player";
        yield return new WaitUntil(() => AllPlayersGetProperty(DefinePropertyKey.READY));
        text.text = "Map Generate Complete Wait other player Selecting";
        yield return new WaitForSeconds(1f);
        //       yield return new WaitUntil(AllPlayersSelect);
        //       PhotonNetwork.CurrentRoom.SetProperty<bool>(DefinePropertyKey.TEAMGAME, false);
        //       text.text = "All Player Ready Press Space Key";
        //       yield return new WaitUntil(() => (Input.GetKeyDown(KeyCode.Space)));
        text.text = "NOW LOADING";
        List<Vector2> posList = new List<Vector2>();
        posList = SecondMapGen.Ins.GetRoomPos(Define.RoomType.PlayerSpawnRoom, PhotonNetwork.PlayerList.Length);
        Vector2[] arr = posList.ToArray();

        PortalSpawn(arr);
        photonView.RPC("GameStart", RpcTarget.All, arr);
        PhotonNetwork.InstantiateRoomObject("6.Prefab/Item/Excalibur", Vector3.zero, Quaternion.identity);
        itemSpawnManager.ItemSpawnStart();
        trapManager.TrapInit();
        puzzleManager.PuzzleInit();
        MonsterSpawnStart();
    }
    public bool AllPlayersGetProperty(string key)
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!player.GetProperty<bool>(key))
            {
                return false;
            }
        }
        return true;
    }
    /// <summary>
    /// 추가 개발용
    /// </summary>
    /// <returns></returns>
    bool AllPlayersSelect()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!player.GetProperty<bool>(DefinePropertyKey.SELECTECOMPLETE))
            {
                return false;
            }
        }
        return true;
    }

    [PunRPC]
    private void ShowSelectImage()
    {
        mainTenanceController.gameObject.SetActive(true);
    }

    [PunRPC]
    public void GameStart(Vector2[] posArr)
    {
        Destroy(loadImg);
        //PhotonNetwork.Instantiate("3", posArr[PhotonNetwork.LocalPlayer.ActorNumber-1], Quaternion.identity);
        //PhotonNetwork.Instantiate("Wizard", Vector3.zero, Quaternion.identity);
        //PlayerController player = PhotonNetwork.Instantiate("Soldier4", posArr[PhotonNetwork.LocalPlayer.ActorNumber - 1], Quaternion.identity).GetComponent<PlayerController>();
        int playerIndex = 0;
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer)
            {
                playerIndex = i;
                break;
            }
        }

        GameManager.Ins.ExitPointController.Sync_ExitPoint();
        Debug.Log(playerIndex);

        Vector2 spawnPosition = posArr[playerIndex];
        Vector3 pos = isSpawnCenter ? Vector3.zero : (Vector3)spawnPosition;
        switch (PhotonNetwork.LocalPlayer.GetProperty<ClassType>(DefinePropertyKey.CHARACTERCLASS))
        {
            case ClassType.Warrior:
                Debug.Log("ab");
                players.Add(PhotonNetwork.Instantiate("6.Prefab/Character/Warrior", pos, Quaternion.identity).GetComponent<PlayerController>());
                break;
            case ClassType.Knight:
                players.Add(PhotonNetwork.Instantiate("6.Prefab/Character/Knight", pos, Quaternion.identity).GetComponent<PlayerController>());
                break;
            case ClassType.Wizard:
                players.Add(PhotonNetwork.Instantiate("6.Prefab/Character/Wizard", pos, Quaternion.identity).GetComponent<PlayerController>());
                break;
            case ClassType.Archer:
                players.Add(PhotonNetwork.Instantiate("6.Prefab/Character/Archer", pos, Quaternion.identity).GetComponent<PlayerController>());
                break;
            default:
                players.Add(PhotonNetwork.Instantiate("Player/Archer", pos, Quaternion.identity).GetComponent<PlayerController>());
                break;
        }

        GameManager.Ins.ExitPointController.Sync_ExitPoint();
        PhotonNetwork.LocalPlayer.SetProperty<bool>(DefinePropertyKey.LOAD, true);
        StartCoroutine(GameManager.Ins.InitRoutine());
    }

    /// <summary>
    /// 몬스터스폰을 시작하는 메서드
    /// 리소스의 이름을 배열에 넣고 
    /// SpawnerInit으로 시작
    /// </summary>
    void MonsterSpawnStart()
    {
        List<int> beginList = new List<int>();
        List<int> lateList = new List<int>();
        List<int> endList = new List<int>();

        // 1. 번들 전부 받아오기
        MonsterSpawnBundle curBundleTable = CsvParser.Instance.MonsterSpawnBunddleDic[SecondMapGen.Ins.CurDunegon.monsterSpawnBundle];

        // 2. 모든 번들 데이터를 검사하며 리스트에 해당 아이디 넣어주기
        for(int i = 0; i < curBundleTable.mobSpawnerCount; i++)
        {   // 2-1 넣어주기 위해서 해당 ID의 룸 타입이 어떻게 되는지 먼저 체크
            Define.RoomType targetRoom = CsvParser.Instance.MonsterSpawnDic[curBundleTable.mobSpawnIDs[i]].roomType;

            // 2-2 체크하였다면 초반, 중반, 후반 방 리스트에 해당 ID를 추가
            switch (targetRoom)
            {
                case Define.RoomType.BeginningRoom:
                    beginList.Add(curBundleTable.mobSpawnIDs[i]);
                    break;
                case Define.RoomType.LateRoom:
                    lateList.Add(curBundleTable.mobSpawnIDs[i]);
                    break;
                case Define.RoomType.EndRoom:
                    endList.Add(curBundleTable.mobSpawnIDs[i]);
                    break;
            }
        }

        SpawnerInit(beginList, Define.RoomType.BeginningRoom);
        SpawnerInit(lateList, Define.RoomType.LateRoom);
        SpawnerInit(endList, Define.RoomType.EndRoom);
    }

    /// <summary>
    /// 몬스터 스포너를 생성하는 메소드
    /// </summary>
    /// <param name="spawnerIDList">스포너의 ID가 들어있는 리스트</param>
    /// <param name="roomType">생성할 방 위치</param>
    void SpawnerInit(List<int> spawnerIDList, Define.RoomType roomType)
    {
        List<Vector2> posList = new List<Vector2>();
        posList = SecondMapGen.Ins.GetRoomPosAll(roomType);

        for (int i = 0; i < posList.Count; i++)
        {
            // 1. 룸 타입에서 확률 찾기
            int spawnProb = Room.FindData(roomType).mobSpawnPer;

            if (UnityEngine.Random.Range(0, 101) > spawnProb)
            {   // 스폰 확률이 되지 않는다면 실행하지 않음
                return;
            }

            // 2. 스포너 ID찾기
            if(spawnerIDList.Count == 0)
            {
                continue;
            }
            int targetID = spawnerIDList[UnityEngine.Random.Range(0, spawnerIDList.Count)];

            // 3. 스폰하기
            MonsterSpanwer instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Spanwer/Monster/MonsterSpanwer", posList[i], Quaternion.identity).GetComponent<MonsterSpanwer>();
            instance.SetSpawn(targetID);
            if (ObjectGrouping.Instance != null)
            {
                instance.transform.parent = ObjectGrouping.Instance.MonsterSpawnerGroup;
            }
        }

    }

    private void PortalSpawn(Vector2[] posArr)
    {
        for(int i = 0; i < posArr.Length; i++)
        {
            GameManager.Ins.ExitPointController.SpawnExitPoint(posArr[i]);
        }
    }

    private static byte[] SerializeVector2(object customObject)
    {
        Vector2 vector = (Vector2)customObject;
        byte[] bytes = new byte[8];
        BitConverter.GetBytes(vector.x).CopyTo(bytes, 0);
        BitConverter.GetBytes(vector.y).CopyTo(bytes, 4);
        return bytes;
    }

    private static object DeserializeVector2(byte[] data)
    {
        Vector2 vector = new Vector2();
        vector.x = BitConverter.ToSingle(data, 0);
        vector.y = BitConverter.ToSingle(data, 4);
        return vector;
    }
}
