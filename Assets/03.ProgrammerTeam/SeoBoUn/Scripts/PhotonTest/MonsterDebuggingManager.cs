using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class MonsterDebuggingManager : MonoBehaviourPunCallbacks
{
    [SerializeField] string debugRoomName = "DebugRoom 000001";
    [SerializeField] MonsterSpanwer[] spawner;

    private void Start()
    {
        PhotonNetwork.LocalPlayer.NickName = $"TestPlayer : {Random.Range(0, 1000)}";
        PhotonNetwork.ConnectUsingSettings();
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
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        /*
        foreach(MonsterSpanwer spawn in spawner)
        {
            spawn.InitSpawn();
        }
        */
        // yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.U));

        PhotonNetwork.Instantiate("Prefabs/TestSoldier", Vector3.zero, Quaternion.identity);
    }
}