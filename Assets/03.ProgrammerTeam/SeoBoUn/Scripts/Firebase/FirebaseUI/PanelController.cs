using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PH = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// 개발자 : 서보운
/// <br/> 인증용 화면 컨트롤러
/// </summary>
public class PanelController : MonoBehaviourPunCallbacks
{
    [SerializeField] LoginPanel loginPanel;
    [SerializeField] InfoPanel infoPanel;
    [SerializeField] SignUpPanel signUpPanel;
    [SerializeField] LobbyPanel lobbyPanel;
    [SerializeField] WaitRoomPanel roomPanel;
    [SerializeField] MainPanel mainPanel;
    [SerializeField] GameObject roomArea;
    [SerializeField] GameObject blockImage;
    [SerializeField] GameObject inGameCanvas;

    [SerializeField] AudioClip roomBGM;
    [SerializeField] AudioClip lobbyBGM;
    [SerializeField] AudioClip loginBGM;
   // new AudioSource audio;
    ClientState state;

    [SerializeField] PlayerController dummyPlayer;

    public InfoPanel InfoPanel { get { return infoPanel; } set { infoPanel = value; } }

    bool prevIsRoom;
    private void Start()
    {

        if (PhotonNetwork.CurrentRoom == null)
        {
            Debug.Log("CurrentRoom is Null");
            StartCoroutine(SetOutGame());
            if (!CsvParser.Instance.initComplete)
                CsvParser.Instance.Init();
            SetActivePanel(Panel.Login);
        }
        else
        {
            Debug.Log("CurrentRoom is not Null");
            StartCoroutine(ReconnectToPreviousRoom());
        }


    }

    IEnumerator ReconnectToPreviousRoom()
    {
        // 네트워크 연결이 완료될 때까지 대기
        yield return new WaitUntil(() => PhotonNetwork.IsConnected);
        Debug.Log("CurrentRoom is not Null => IsConnected");
        // 이전에 참여했던 방으로 다시 접속을 시도
        string previousRoomName = PhotonNetwork.CurrentRoom.Name; // 또는 저장된 방 이름 사용
        Debug.Log($"CurrentRoom is not Null => Name is {previousRoomName}");
        if (PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.LeaveRoom();  // 현재 방에서 나가기
        }
        PhotonNetwork.JoinLobby();

        yield return new WaitUntil(() => PhotonNetwork.NetworkClientState == ClientState.JoinedLobby);
        Debug.Log($"CurrentRoom is not Null => JoinedLobby");
        // 방 재접속 시도
        PhotonNetwork.JoinRoom(previousRoomName);
        Debug.Log("Trying to rejoin the room: " + previousRoomName);
    }
    IEnumerator SetOutGame()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnected);

        PhotonNetwork.LocalPlayer.SetProperty(DefinePropertyKey.OUTGAME, true);
    }
    public void SetActivePanel(Panel panel)
    {
        loginPanel.gameObject.SetActive(panel == Panel.Login);
        infoPanel.gameObject.SetActive(panel == Panel.Info);
        signUpPanel.gameObject.SetActive(panel == Panel.SignUp);
        lobbyPanel.gameObject?.SetActive(panel == Panel.Lobby);
        roomPanel.gameObject?.SetActive(panel == Panel.WaitRoom);
        mainPanel.gameObject?.SetActive(panel == Panel.Main);
    }
    public void ButtonBlock(bool b)
    {
        blockImage.SetActive(b);
    }
    public void ShowInfo(string message)
    {
        infoPanel.ShowInfo(message);
    }

    public enum Panel
    {
        Login,
        Info,
        SignUp,
        Lobby, WaitRoom,
        Main
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log($"Client state : {state}");
        }
        ClientState curState = PhotonNetwork.NetworkClientState;
        if (state == curState)
        {
            return;
        }
        state = curState;
        Debug.Log($"Client state : {state}");

    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected To Master Success");
        inGameCanvas.SetActive(false);
        PhotonNetwork.LocalPlayer.SetProperty(DefinePropertyKey.READY, false);

        if (prevIsRoom)
        {
            PhotonNetwork.JoinLobby();
        }
        else
        {
            SetActivePanel(Panel.Main);

        }

    }
    public void CreateDummyPlayer()
    {
        PlayerController player = Instantiate(dummyPlayer, Vector2.zero, Quaternion.identity).GetComponent<PlayerController>(); //더미 플레이어 생성
        player.AllControllerStop();
        GameManager.Ins.m_Player = player;
        Inventory.Instance.Player = player;                                                                                     // 인벤 플레아어 연결
        player.InputSystemActive(false);
    }
    public override void OnJoinedRoom()
    {
        SetActivePanel(Panel.WaitRoom);
        GameManager.Ins.ChatActivate(true);
        roomArea.gameObject.SetActive(true);
        Debug.Log("Join Room Success");
        inGameCanvas.SetActive(true);
        ButtonBlock(false);
        roomPanel.PlayerJoinRoom();
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Create Room Success");

    }
    public override void OnLeftRoom()
    {
        prevIsRoom = true;
        Debug.Log("Left Room");
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Join Lobby");

        if (PhotonNetwork.CurrentRoom == null)
        {
            GameManager.Ins.ChatActivate(false);
            SetActivePanel(Panel.Lobby);
        }
        else
        {
            SetActivePanel(Panel.WaitRoom);
        }

    }
    public override void OnLeftLobby()
    {
        SetActivePanel(Panel.Main);

        Debug.Log("LeftLobby");
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (PhotonNetwork.NetworkClientState != ClientState.JoinedLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        Debug.Log("JoinRoom Failed Try to JoinLobby");
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log($"Create Room Failed With Error : {returnCode} , :{message}");
    }

    public override void OnConnected()
    {
        Debug.Log($"Connect : {PhotonNetwork.LocalPlayer.NickName}");

    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("updateList");
        lobbyPanel.UpdateRoomList(roomList);
        if (PhotonNetwork.InRoom)
        {
            roomPanel.RoomOptionUpdate(false);
        }

    }
    public override void OnRoomPropertiesUpdate(PH propertiesThatChanged)
    {
        roomPanel.RoomPropertiesUpdate(propertiesThatChanged);
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, PH changedProps)
    {
        roomPanel.PlayerPropertiesUpdate(targetPlayer, changedProps);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"{newPlayer} on Enter the room");

        roomPanel.PlayerEnterRoom(newPlayer);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("OnPlayerLeftRoom");
        roomPanel.PlayerLeftRoom(otherPlayer);
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        roomPanel.MasterClientSwitched(newMasterClient);
        Debug.Log("OnMasterChange");
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (cause == DisconnectCause.DisconnectByServerLogic)
            return;
        SetActivePanel(Panel.Login);
        prevIsRoom = false;

        if (PhotonNetwork.CurrentRoom != null)
        {
            if (PhotonNetwork.CurrentRoom.GetProperty<bool>(DefinePropertyKey.GAMEON))
            {
                Inventory.Instance.Items.Clear();
                Inventory.Instance.QuickItems.Clear();
                Inventory.Instance.FreshSlots();
                FirebaseManager.Instance.UpLoadInventoryData();
            }
        }

        Debug.Log("OnDisconnected");


        if (PhotonNetwork.CurrentRoom != null)
        {
            if (PhotonNetwork.CurrentRoom.GetProperty<bool>(DefinePropertyKey.GAMEON))
            {

            }
        }
    }

  /*  public void PlayAudio(Panel panel)
    {
        AudioClip clip = panel
        switch
        {
            Panel.Login => loginBGM,
            Panel.Lobby => lobbyBGM,
            Panel.WaitRoom => roomBGM,
            _ => null
        };
        if (clip != null)
        {
            if (audio.clip == clip)
                return;
            audio.clip = clip;
            audio.Play();
        }
        else
        {
            audio.Stop();
        }
    }*/



}
