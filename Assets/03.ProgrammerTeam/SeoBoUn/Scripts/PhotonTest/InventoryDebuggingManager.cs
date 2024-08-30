using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryDebuggingManager : MonoBehaviourPunCallbacks
{
    [SerializeField] string debugRoomName = "DebugRoom 02101";
    [SerializeField] BaseMonster monster;
    [SerializeField] BoxSpawner spawner;

    private void Start()
    {
        PhotonNetwork.LocalPlayer.NickName = $"TestPlayer : {Random.Range(0, 1000)}";
        PhotonNetwork.ConnectUsingSettings();
         Screen.SetResolution(800, 450, false);
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
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        //spawner.SetBoxRank(BoxRank.Low);
        //spawner.BoxSpawn(Random.insideUnitCircle * 4f);

        photonView.RPC("Spawn", RpcTarget.All);

        PhotonNetwork.InstantiateRoomObject("6.Prefab/Monster/Monster_Slime", Vector3.zero, Quaternion.identity);
        PhotonNetwork.InstantiateRoomObject("6.Prefab/Monster/Monster_Slime", Vector3.zero, Quaternion.identity);
    }

    [PunRPC]
    private void Spawn()
    {
        PlayerController player;
        if (PhotonNetwork.IsMasterClient)
        {
            player = PhotonNetwork.Instantiate("6.Prefab/Character/ExcaliburChar", Vector3.zero, Quaternion.identity).GetComponent<PlayerController>();
            PhotonNetwork.LocalPlayer.SetProperty(DefinePropertyKey.RED, true);
            PhotonNetwork.LocalPlayer.SetProperty(DefinePropertyKey.BLUE, false);
        }
        else
        {
            player = PhotonNetwork.Instantiate("6.Prefab/Character/Wizard", Vector3.zero, Quaternion.identity).GetComponent<PlayerController>();
            PhotonNetwork.LocalPlayer.SetProperty(DefinePropertyKey.BLUE, true);
            PhotonNetwork.LocalPlayer.SetProperty(DefinePropertyKey.RED, false);
        }

        // Inventory.Instance.Player = player;
        //spawner.SetBoxRank(BoxRank.Low);
        //spawner.BoxSpawn(Random.insideUnitCircle * 4f);
        //spawner.BoxSpawn(Random.insideUnitCircle * 4f);
        //spawner.BoxSpawn(Random.insideUnitCircle * 4f);
    }
}