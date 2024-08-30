using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebuggingManager : MonoBehaviourPunCallbacks
{
    private static DebuggingManager ins;
    public static DebuggingManager Ins
    {
        get { return ins; }
        set { ins = value; }
    }

    [SerializeField] string debugRoomName = "DebugRoom 0001";
    [SerializeField] TMP_Text text;
    [SerializeField] GameObject loadImg;
    [SerializeField] TrapManager trapManager;
    [SerializeField] ItemSpawnManager itemSpawnManager;
    [SerializeField] PuzzleManager puzzleManager;


    [SerializeField] string[] firstMonsters = { "MonsterSpanwer/SlimeSpawner", "MonsterSpanwer/FirstSpawner" };
    [SerializeField] string[] lateMonsters = { "MonsterSpanwer/SkeletonSpawner", "MonsterSpanwer/SecondSpawner" };
    [SerializeField] string[] endMonsters = { "MonsterSpanwer/OrcSpawner", "MonsterSpanwer/ThirdSpawner" };
    [SerializeField] Camera _camera;
    [SerializeField] public Vector3 MousePos;





    private void Start()
    {
        ins = this;
        _camera = Camera.main;
        PhotonNetwork.LocalPlayer.NickName = $"TestPlayer : {UnityEngine.Random.Range(0, 1000)}";
        PhotonNetwork.ConnectUsingSettings();
        PhotonPeer.RegisterType(typeof(Vector2), (byte)'V', SerializeVector2, DeserializeVector2);


    }
    private void Update()
    {
        MousePos = new Vector3(_camera.ScreenToWorldPoint(Input.mousePosition).x, _camera.ScreenToWorldPoint(Input.mousePosition).y);
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
    public override void OnConnectedToMaster()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;
        roomOptions.IsVisible = false;
        TypedLobby typedLobby = new TypedLobby("DebugLobby", LobbyType.Default);

        PhotonNetwork.JoinOrCreateRoom(debugRoomName, roomOptions, typedLobby);
    }

    public override void OnJoinedRoom()
    {
       // if (PhotonNetwork.IsMasterClient)
            //StartCoroutine(DelayCoroutine());
    }

    IEnumerator DelayCoroutine()
    {
        text.text = "Room Create Complete Press Space Key";
        yield return new WaitUntil(() => (Input.GetKeyDown(KeyCode.Space)));

        SecondMapGen.Ins.StartGenerateMap();

        yield return GameStartDelay();
    }

    IEnumerator GameStartDelay()
    {

        text.text = "Map Create Success Wait other player";
        yield return new WaitForSeconds(1f);
        yield return new WaitUntil(AllPlayersReady);

        text.text = "All Player Ready Press Space Key";
        yield return new WaitUntil(() => (Input.GetKeyDown(KeyCode.Space)));
        text.text = "NOW LOADING";
        List<Vector2> posList = new List<Vector2>();
        posList = SecondMapGen.Ins.GetRoomPos(Define.RoomType.PlayerSpawnRoom, PhotonNetwork.PlayerList.Length);
        Vector2[] arr = posList.ToArray();


        Destroy(loadImg);
        photonView.RPC("GameStart", RpcTarget.All, arr);


        itemSpawnManager.ItemSpawnStart();
        trapManager.TrapInit();
        puzzleManager.PuzzleInit();
        MonsterSpawnStart();

    }
    bool AllPlayersReady()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!player.GetProperty<bool>(DefinePropertyKey.READY))
            {
                return false;
            }
        }
        return true;
    }
    [PunRPC]
    public void GameStart(Vector2[] posArr)
    {
        Destroy(loadImg);
        //PhotonNetwork.Instantiate("3", posArr[PhotonNetwork.LocalPlayer.ActorNumber-1], Quaternion.identity);
        //PhotonNetwork.Instantiate("Wizard", Vector3.zero, Quaternion.identity);
        PlayerController player = PhotonNetwork.Instantiate("Soldier4", posArr[PhotonNetwork.LocalPlayer.ActorNumber-1], Quaternion.identity).GetComponent<PlayerController>();


        if (photonView.IsMine)
        {
            Inventory.Instance.Player = player;
        }
    }

    /// <summary>
    /// 몬스터스폰을 시작하는 메서드
    /// 리소스의 이름을 배열에 넣고 
    /// SpawnerInit으로 시작
    /// </summary>
    void MonsterSpawnStart()
    {

        SpawnerInit(15, firstMonsters, Define.RoomType.BeginningRoom);
        SpawnerInit(15, lateMonsters, Define.RoomType.LateRoom);
        SpawnerInit(15, endMonsters, Define.RoomType.EndRoom);
    }
    /// <summary>
    /// 스포너를 생성하는 메서드
    /// </summary>
    /// <param name="max">0부터 max까지 확률</param>
    /// <param name="monstersName">생성할 스포너 배열,max배열에 할당되지 않은 몬스터는 생성되지 않음</param>
    /// <param name="roomType">생성할 룸타입</param>
    void SpawnerInit(int max, string[] monstersName, Define.RoomType roomType)
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
           // if (init != null)
           //     init.InitSpawn();

        }
    }

}
