using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using PH = ExitGames.Client.Photon.Hashtable;

public class Debugging3rd : MonoBehaviourPunCallbacks
{
    private static Debugging3rd ins;
    public static Debugging3rd Ins
    {
        get { return ins; }
    }

    [SerializeField] string debugRoomName = "DebugRoom 0001";
    [Tooltip("메인 로비의 로딩text를 할당할것")]
    [SerializeField] TMP_Text text;
    [SerializeField] GameObject loadImg;
    //   [SerializeField] MonsterGridManager monsterGridManager;
    [SerializeField] TrapManager trapManager;
    [SerializeField] ItemSpawnManager itemSpawnManager;
    [SerializeField] MaintenanceController mainTenanceController;
    [SerializeField] PuzzleManager puzzleManager;
    [Tooltip("인게임캔버스에 미니맵 할당")]
    public GameObject miniMapUi;
    [Tooltip("인게임캔버스에 챗ui 할당")]
    public GameObject chatUi;

    List<PlayerController> players = new List<PlayerController>();

    [Tooltip("체크할 시 중앙에서 스폰 및 디버그")]
    public bool _DEBUG;
    public bool TEAM;

    private void Awake()
    {
        ins = this;
        PhotonNetwork.SendRate = 5;
        PhotonNetwork.SerializationRate = 10;

        PhotonPeer.RegisterType(typeof(Vector2), (byte)'V', SerializeVector2, DeserializeVector2);
        trapManager = Resources.Load<TrapManager>("6.Prefab/Manager/TrapManager");
       /* if (_DEBUG)
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.LocalPlayer.NickName = $"TestPlayer : {UnityEngine.Random.Range(0, 1000)}";
            StartCoroutine(DelayCoroutine());
        }*/

        if (PhotonNetwork.IsMasterClient )
        {
            Debug.Log("딜레이 시작");
            StartCoroutine(DelayCoroutine());
        }

    }



    public override void OnConnectedToMaster()
    {
        if (_DEBUG)
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 4;
            roomOptions.IsVisible = false;
            TypedLobby typedLobby = new TypedLobby("DebugLobby", LobbyType.Default);
            PH roomMode = new PH();
            roomMode[DefinePropertyKey.TEAMGAME] = TEAM;
            roomOptions.CustomRoomProperties = roomMode;

            PhotonNetwork.JoinOrCreateRoom(debugRoomName, roomOptions, typedLobby);
        }

    }

    IEnumerator DelayCoroutine()
    {
        text.text = "WAIT FOR FIND MAP GENERATOR";
        yield return new WaitUntil(() => SecondMapGen.Ins != null);
        SecondMapGen.Ins.StartGenerateMap();
        text.text = "WAIT FOR FIND GAMEMANAGER";
        yield return new WaitUntil(() => GameManager.Ins != null);
        text.text = "FIND COMPLETE";
        yield return GameStartDelay();
    }

    IEnumerator GameStartDelay()
    {
        text.text = "FIND COMPLETE..";
        text.text = "Map Create Success Wait other player";
        yield return new WaitUntil(() => AllPlayersGetProperty(DefinePropertyKey.READY));

        if (_DEBUG)
        {
            photonView.RPC("ShowSelectImage", RpcTarget.All);
            text.text = "WAIT FOR Other player Selecting";
            yield return new WaitUntil(AllPlayersSelect);
        }

        text.text = "NOW LOADING";
        List<Vector2> posList = new List<Vector2>();
        posList = SecondMapGen.Ins.GetRoomPos(Define.RoomType.PlayerSpawnRoom, PhotonNetwork.PlayerList.Length);
        Vector2[] arr = posList.ToArray();

        //GameManager.Ins.ExitPointController.SpawnExitPoint(new Vector3(5f, 5f, 0));
        //GameManager.Ins.ExitPointController.SpawnExitPoint(new Vector3(-5f, -5f, 0));
        Destroy(loadImg);
        PortalSpawn(arr);
        photonView.RPC("GameStart", RpcTarget.All, arr);

        PhotonNetwork.CurrentRoom.SetProperty<double>(DefinePropertyKey.STARTTIME, PhotonNetwork.Time);
        Excalibur excalibur = PhotonNetwork.InstantiateRoomObject("6.Prefab/Item/Excalibur", Vector3.zero, Quaternion.identity).GetComponent<Excalibur>();
        excalibur.SetExcaliburData(SecondMapGen.Ins.CurDunegon.excaliburType);

        itemSpawnManager.ItemSpawnStart();
        trapManager.TrapInit();
        puzzleManager.PuzzleInit();
        MonsterSpawnStart();
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.CurrentRoom.SetProperty<bool>(DefinePropertyKey.LOAD, true);
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].photonView.IsMine)
            {
                Inventory.Instance.Player = players[i];
            }
        }



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
    /// <summary>
    /// 디버그용 캐릭 셀렉 RPC
    /// </summary>
    [PunRPC]
    private void ShowSelectImage()
    {
        mainTenanceController.gameObject.SetActive(true);
        PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1, CommonSkill.Dash);
        PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2, CommonSkill.Heal);
        mainTenanceController.enabled = true;
        SoundManager.instance.PlayBGM(1650048);                 // 이거 뭐임?
    }

    [PunRPC]
    public void GameStart(Vector2[] posArr)
    {

         SoundManager.instance.PlayBGM(1650048);
        Destroy(loadImg);
        //PhotonNetwork.Instantiate("3", posArr[PhotonNetwork.LocalPlayer.ActorNumber-1], Quaternion.identity);
        //PhotonNetwork.Instantiate("Wizard", Vector3.zero, Quaternion.identity);
        //PlayerController player = PhotonNetwork.Instantiate("Soldier4", posArr[PhotonNetwork.LocalPlayer.ActorNumber - 1], Quaternion.identity).GetComponent<PlayerController>();
        int index = 0;
        if (PhotonNetwork.CurrentRoom.GetProperty<bool>(DefinePropertyKey.TEAMGAME))
        {
            index = PhotonNetwork.LocalPlayer.GetProperty<bool>(DefinePropertyKey.RED) ?  0 : 1;
        }
        else
        {
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer)
                {
                    index = i;
                }
            }
        }


        Vector3 pos = _DEBUG ? Vector3.zero : posArr[index];

        Debug.Log("GameStart");
        switch (PhotonNetwork.LocalPlayer.GetProperty<ClassType>(DefinePropertyKey.CHARACTERCLASS))
        {
            case ClassType.Warrior:
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
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    /// <summary>
    /// 몬스터스폰을 시작하는 메서드
    /// 리소스의 이름을 배열에 넣고 
    /// SpawnerInit으로 시작
    /// </summary>
    void MonsterSpawnStart()
    {
        Debug.Log("MonsterSpawn");
        List<int> beginList = new List<int>();
        List<int> lateList = new List<int>();
        List<int> endList = new List<int>();

        // 1. 번들 전부 받아오기
        MonsterSpawnBundle curBundleTable = CsvParser.Instance.MonsterSpawnBunddleDic[SecondMapGen.Ins.CurDunegon.monsterSpawnBundle];

        // 2. 모든 번들 데이터를 검사하며 리스트에 해당 아이디 넣어주기
        for (int i = 0; i < curBundleTable.mobSpawnerCount; i++)
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

        //StartCoroutine(SpawnerInit(15, lateMonsters, Define.RoomType.LateRoom));
        //StartCoroutine(SpawnerInit(15, endMonsters, Define.RoomType.EndRoom));
        //StartCoroutine(SpawnerInit(15, firstMonsters, Define.RoomType.BeginningRoom));
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
    /*
    /// <summary>
    /// 스포너를 생성하는 메서드
    /// </summary>
    /// <param name="max">0부터 max까지 확률</param>
    /// <param name="monstersName">생성할 스포너 배열,max배열에 할당되지 않은 몬스터는 생성되지 않음</param>
    /// <param name="roomType">생성할 룸타입</param>
    IEnumerator SpawnerInit(int max, string[] monstersName, Define.RoomType roomType)
    {
        List<Vector2> posList = new List<Vector2>();
        posList = SecondMapGen.Ins.GetRoomPosAll(roomType);
        foreach (Vector2 p in posList)
        {
            int r = UnityEngine.Random.Range(0, max + 1);
            InitSpawner init = null;
            if (r < monstersName.Length)
            {
                init = PhotonNetwork.InstantiateRoomObject(monstersName[r], p, Quaternion.identity).GetComponent<InitSpawner>();
            }

            if (init != null)
            {
                yield return new WaitUntil(() => (init.IsSetting));
                init.InitSpawn();
            }
        }
    }
    */

    private void PortalSpawn(Vector2[] posArr)
    {
        // GameManager.Ins.ExitPointController.SpawnExitPoint(new Vector3(5f, 5f, 0));
        // GameManager.Ins.ExitPointController.SpawnExitPoint(new Vector3(-3f, -3f, 0));
        
        for (int i = 0; i < posArr.Length; i++)
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
     public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Disconnected due to: {cause}");
        // 원인 분석
    }
}
