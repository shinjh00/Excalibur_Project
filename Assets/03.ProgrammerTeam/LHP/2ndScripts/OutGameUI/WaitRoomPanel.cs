using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
public class WaitRoomPanel : MonoBehaviourPun
{
    [SerializeField] public Button leaveButton;
    [SerializeField] public Button roomOptionButton;
    [SerializeField] Button readyButton;
    [SerializeField] LobbyPanel lobbyPanel;
    [SerializeField] PlayerEntry playerEntryPrefab;
    [SerializeField] RectTransform entryPos;
    [SerializeField] Transform spawnPos;
    [SerializeField] GameObject roomArea;
    [SerializeField] ReadyArea readyArea;

    public List<PlayerEntry> playerList;

    public List<PlayerController> playerInstances;
    Photon.Realtime.Room currentRoom;
    [SerializeField] TMP_Text roomName;
    [SerializeField] TMP_Text modeText;
    [SerializeField] GameObject teamArea;   
    bool isMaster;
    [SerializeField] Chat chat;

    [SerializeField] MaintenanceController maintenance;

    private void Awake()
    {
        playerList = new List<PlayerEntry>();
        playerInstances = new List<PlayerController>();
        leaveButton.onClick.AddListener(LeaveRoom);
        roomOptionButton.onClick.AddListener(RoomOptionChange);

    }
    private void OnEnable()
    {
        RoomOptionUpdate(true);
    }
    
    public void ChangeTeam()
    {
        foreach(var player in playerList)
        {

        }
    }
    public async void RoomOptionUpdate(bool create)
    {
        
        currentRoom = PhotonNetwork.CurrentRoom;
        PhotonNetwork.EnableCloseConnection = true; //밴 가능하게
        //방 이름 표기
        roomName.text = currentRoom.CustomProperties[DefinePropertyKey.ROOMNAME] as string;
        if (PhotonNetwork.CurrentRoom.GetProperty<bool>(DefinePropertyKey.TEAMGAME))
        {
            modeText.text = "Mode : Team";
            teamArea.SetActive(true);
            if(PhotonNetwork.CurrentRoom.GetProperty<int>(DefinePropertyKey.ROOMID) != 1400001)
            {
                PhotonNetwork.CurrentRoom.SetProperty<int>(DefinePropertyKey.ROOMID, 1400001);
            }


        }
        else
        {
            modeText.text = "Mode : Solo";
            teamArea.SetActive(false);
            EntryToTeam();
            if (PhotonNetwork.CurrentRoom.GetProperty<int>(DefinePropertyKey.ROOMID) != 1400000)
            {
                PhotonNetwork.CurrentRoom.SetProperty<int>(DefinePropertyKey.ROOMID, 1400000);
            }
        }

        if (create)
        {
            ClearRoomData();
            SetEntry();
        }

        roomOptionButton.interactable = PhotonNetwork.IsMasterClient;

        lobbyPanel.CreateRoomPanel(false);

        bool success = await WaitForValidPhotonViewID(5000); // 5초 대기
        if (success)
        {
            if(readyArea != null && readyArea.photonView.ViewID !=0)
            {
                Debug.Log("roomUpdate ReadyCnt");
                readyArea.photonView.RPC("ReadyCnt", RpcTarget.All);
            }

        }
        else
        {
            Debug.Log("5초동안 유효 포톤뷰 id를 가진 플레이어를 못가져왓음");
        }

    }
    void SetEntry()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            PlayerSet(player);
        }
    }
    void EntryToTeam()
    {
        foreach(var p in playerList)
        {
            p.Player.RemoveProperty(DefinePropertyKey.RED);
            p.Player.RemoveProperty(DefinePropertyKey.BLUE);
            p.EntryImage.color = Color.white;
        }
    }
    async Task<bool> WaitForValidPhotonViewID(int timeout)
    {
        // 타임아웃 작업 생성
        var timeoutTask = Task.Delay(timeout);
        var tcs = new TaskCompletionSource<bool>();

        // 매 프레임 체크 작업 생성
        async void CheckCondition()
        {
            while (readyArea.photonView == null || readyArea.photonView.ViewID < 1)
            {
                await Task.Yield();
            }
            tcs.TrySetResult(true);
        }

        // 체크 작업 시작
        CheckCondition();

        // 타임아웃 작업과 매 프레임 체크 작업 중 하나가 완료될 때까지 대기
        if (await Task.WhenAny(tcs.Task, timeoutTask) == tcs.Task)
        {
            // 유효한 PhotonView ID
            return true;
        }

        // 타임아웃 발생
        return false;
    }
    public void RoomOptionChange()
    {
        lobbyPanel.CreateRoomPanel(true);
    }
    private void OnDisable()
    {
        ClearRoomData();
    }
    void ClearRoomData()
    {
        playerList.Clear();
        playerInstances.Clear();
        for (int i = 0; i < entryPos.childCount; i++)
        {
            Destroy(entryPos.GetChild(i).gameObject);
        }
    }
    void PlayerSet(Player player)
    {
        PlayerEntry entry = Instantiate(playerEntryPrefab, entryPos);
        entry.SetPlayer(player);

        playerList.Add(entry);
        Debug.Log("PlayerSetting");
    }
    public void RoomPropertiesUpdate(PhotonHashtable changedProps)
    {
        if (changedProps.ContainsKey(DefinePropertyKey.GAMEON))     //게임 시작시 바뀌는 프로퍼티는 업데이트 안함
        {
            return;
        }
        foreach (var key in changedProps.Keys)
        {

            Debug.Log("Changed property key: " + key.ToString());
        }
        RoomOptionUpdate(false);

    }
    public void PlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps)
    {
        foreach (PlayerEntry player in playerList)
        {

            if (player.Player.ActorNumber == targetPlayer.ActorNumber)
            {
                //해당 플레이어 업데이트
                player.UpdateProperty(changedProps);
                break;
            }
        }
        roomOptionButton.interactable = PhotonNetwork.IsMasterClient;
    }
    public async void PlayerJoinRoom()
    {
        Debug.Log("joinRoom");
        PlayerController ins = PhotonNetwork.Instantiate("6.Prefab/Character/Knight", Vector2.zero, Quaternion.identity).GetComponent<PlayerController>();
        PhotonNetwork.LocalPlayer.RemoveProperty(DefinePropertyKey.RED);
        PhotonNetwork.LocalPlayer.RemoveProperty(DefinePropertyKey.BLUE);
        PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL1, CommonSkill.Dash);
        PhotonNetwork.LocalPlayer.SetProperty<CommonSkill>(DefinePropertyKey.COMMONSKILL2, CommonSkill.Heal);
        await PhotonNetwork.LocalPlayer.SetPropertyAsync<ClassType>(DefinePropertyKey.CHARACTERCLASS, ClassType.Knight);
        await Task.Run(() =>
        {
            while (ins == null)
            {
                Task.Delay(100);
            }
        });
     //   ins.photonView.RPC("RPC_EntryConnect", RpcTarget.AllBuffered, ins.photonView.OwnerActorNr);
        GameManager.Ins.m_Player = ins;
        maintenance.CharacterChange();

        if (Inventory.Instance != null && Inventory.Instance.Player != null)
        {
            Inventory.Instance.DownLoadInventoryData();
        }
    }

    public void PlayerEnterRoom(Player targetPlayer)
    {
        PlayerEntry playerEntry = Instantiate(playerEntryPrefab, entryPos);
        playerEntry.SetPlayer(targetPlayer);
        playerList.Add(playerEntry);
        GameManager.Ins.Init();
    }
    public void PlayerLeftRoom(Player targetPlayer)
    {
        //플레이어 리스트에서 플레이어 제거
        RemovePlayer(targetPlayer, playerList);
        chat.LeftPlayer(targetPlayer);
        if(readyArea != null && readyArea.photonView.ViewID !=0)
        {
            Debug.Log("Left ReadyCnt");
            readyArea.photonView.RPC("ReadyCnt", RpcTarget.All);
        }


    }
    void RemovePlayer(Player otherPlayer, List<PlayerEntry> playerList)
    {
        //플레이어 리스트를 돌면서 
        for (int i = 0; i < playerList.Count; i++)
        {
            //액터넘버가 같을 경우 
            if (playerList[i].Player.ActorNumber == otherPlayer.ActorNumber)
            {

                Destroy(playerList[i].gameObject);
                playerList.RemoveAt(i);
                return;
            }
        }
    }
    public void MasterClientSwitched(Player targetPlayer)
    {
        Debug.Log($"Master Switched => {targetPlayer}");
       // currentRoom.SetProperty(DefinePropertyKey.HOST, targetPlayer.NickName);
    }
    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            for(int i = 0; i < playerList.Count;i++)
            {
                if (playerList[i].Player.IsLocal)
                {
                  PhotonNetwork.Destroy(playerList[i].controller.gameObject);
                }
            }
            PhotonNetwork.LeaveRoom();

            roomArea.gameObject.SetActive(false);
        }
    }

}


