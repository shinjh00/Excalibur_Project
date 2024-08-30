using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using PH = ExitGames.Client.Photon.Hashtable;
/// <summary>
/// 개발자 : 이형필 // 게임 전반 진행을 위한 클래스
/// </summary>
public class GameManager : MonoBehaviourPunCallbacks
{
    public int totalRoomCount = 0;
    public List<PlayerController> playerList = new List<PlayerController>();
    public Dictionary<Player, PlayerController> playerDic = new Dictionary<Player, PlayerController>();
    public PlayerController m_Player;
    // 엑스칼리버 유저를 캐싱(다른 유저들이 위치를 알아야 함)
    public PlayerController excaliburPlayer;
    [Tooltip("LHP - Prefabs - DeadCanvas 할당")]
    public DeadToWatch deadToWatch;
    public WinCanvas winCanvas;
    public bool buttonDown = false;

    [SerializeField] MessageFloater msgFloat;
    [SerializeField] Image stateCanvas;

    [SerializeField] InventoryUI invenUI;
    [SerializeField] SkillUI skillUI;
    [SerializeField] SettingUI settingUI;
    [SerializeField] Minimap minimap;
    [SerializeField] Chat chat;
    [SerializeField] GameObject inGameCanvas;
    [Tooltip("탈출 포인트를 관리할 컨트롤러(에디터 할당 필요)")]
    [SerializeField] ExitPointController exitPointController;

    [SerializeField] public MagneticField magneticField;

    public ExitPointController ExitPointController { get { return exitPointController; } }
    public Minimap MiniMap { get { return minimap; } }

    public Action ChangeExcalibur;
    public Action updates;

    public bool isTeamFight;

    [SerializeField] public Vector3 MousePos;
    private static GameManager ins;
    public static GameManager Ins
    {
        get { return ins; }
        set { ins = value; }
    }
    [SerializeField] Camera _camera;
    public int curCameraIndex = 0;
    public bool onChat = false;

    private void Awake()
    {
        ins = this;
        _camera = Camera.main;
    }
    private void Update()
    {
        Photon.Realtime.Room curRoom = PhotonNetwork.CurrentRoom;
        if (curRoom != null && isTeamFight != curRoom.GetProperty<bool>(DefinePropertyKey.TEAMGAME))
        {
            Debug.Log("Change TEAM MODE");
            isTeamFight = curRoom.GetProperty<bool>(DefinePropertyKey.TEAMGAME);
        }


        MousePos = new Vector3(_camera.ScreenToWorldPoint(Input.mousePosition).x, _camera.ScreenToWorldPoint(Input.mousePosition).y);
    
    }

    public void FindExcaliburExit()
    {
        exitPointController = FindObjectOfType<ExitPointController>();
    }
    /// <summary>
    /// 플레이어 리스트에서 해당 뷰ID에 해당하는 플레이어 반환
    /// </summary>
    /// <param name="photonViewID"></param>
    /// <returns></returns>
    public PlayerController FindPlayer(int photonViewID)
    {
        foreach (PlayerController player in playerList)
        {
            if (player.photonView.ViewID == photonViewID)
            {
                return player;
            }
        }

        Message($"Can't Find ID : {photonViewID} Player");
        return null;
    }


    /// <summary>
    /// Player의 ActorNumber로 PlayerController를 찾는 메서드
    /// </summary>
    /// <param name="actorNumber"></param>
    /// <returns></returns>
    public PlayerController FindPlayerByActorNumber(int actorNumber)
    {
        foreach (PlayerController player in playerList)
        {
            if (player.photonView.OwnerActorNr == actorNumber)
            {
                return player;
            }
        }

        Message($"Can't Find ActorNumber : {actorNumber} Player");
        return null;
    }
    /// <summary>
    /// 모든 플레이어가 캐릭터로드가 끝났는지 확인하는 루틴
    /// </summary>
    /// <returns></returns>
    public IEnumerator InitRoutine()
    {
        Message("YOU LOAD COMPLETE");
        yield return new WaitUntil(() => Debugging3rd.Ins.AllPlayersGetProperty(DefinePropertyKey.LOAD));

        photonView.RPC("InitRPC", RpcTarget.All);

        PhotonNetwork.LocalPlayer.SetProperty(DefinePropertyKey.LOAD, false);
    }
    /// <summary>
    /// 각 플레이어의 정보를 받아오는 메서드
    /// </summary>
    public void Init()
    {
        playerList.Clear();
        playerDic.Clear();

        List<PlayerController> players = new List<PlayerController>(FindObjectsOfType<PlayerController>());

     //   Debug.Log($"Check     {players.Count} = {PhotonNetwork.PlayerList.Length}");

        foreach (PlayerController player in players)
        {
            Debug.Log($"Try : {player}");
            if (player == null || player.photonView.Owner == null)
            {
              //  Debug.Log($"Continue : player : {player} , playerPV : {player.photonView.Owner}");
                continue;
            }

            // 이미 리스트에 추가된 플레이어인지 확인
            if (!playerList.Contains(player))
            {
                playerList.Add(player);
                playerDic[player.photonView.Owner] = player;
         //       Debug.Log($"Add KEY {player} : VALUE : {playerDic[player.photonView.Owner]}");
            }

            if (player.photonView.IsMine)
            {
                m_Player = player;
            }

            if (player.PlayerClassData.classType == ClassType.Excalibur)
            {
                // 엑스칼리버 유저는 위치를 공유해주어야 함
                excaliburPlayer = player;
                m_Player = excaliburPlayer;
            }
        }

        if (Debugging2nd.Ins != null && !PhotonNetwork.LocalPlayer.GetProperty<bool>(DefinePropertyKey.OUTGAME))
        {
            Debugging2nd.Ins.miniMapUi.SetActive(true);
            Debugging2nd.Ins.chatUi.SetActive(true);
        }
        Inventory.Instance.Player = m_Player;

        Inventory.Instance.Player.SkillController.skillUI = skillUI;

        SetInventoryUI();
        StartCoroutine(StartDownLoadInven());
    }

    IEnumerator StartDownLoadInven()
    {
        yield return new WaitUntil(() => (Inventory.Instance != null && Inventory.Instance.Player != null));

        // Inventory.Instance.DownLoadInventoryData();
    }

    /// <summary>
    /// 사망 시 나올 메서드
    /// </summary>
    public void DieEvent()
    {
        if (!deadToWatch.gameObject.activeSelf)
        {
            Message("YOU DIE");
            deadToWatch.gameObject.SetActive(true);
            InGameCanvasActive(false);
            m_Player.SkillController.enabled = false;
            GameOverAndCheck(PhotonNetwork.LocalPlayer);
        }
    }

    async void GameOverAndCheck(Player player)
    {
        await player.SetPropertyAsync<bool>(DefinePropertyKey.GAMEOVER, true);
        CheckOtherPlayerOver();
    }
    async void GroggyTeamOver(List<Player> players)
    {
        for (int i = 0; i < players.Count; i++)
        {
            await players[i].SetPropertyAsync<bool>(DefinePropertyKey.GAMEOVER, true);
        }
        CheckOtherPlayerOver();
    }
    public void CheckOtherPlayerGroggy(Player player)
    {
        Debug.Log("Check Other Player Groggy");
        bool allGroggy = true;
        List<Player> allies = new List<Player>();
        allies = player.AllyPlayers();
        for (int i = 0; i < allies.Count; i++)
        {
            Debug.Log(allies[i]);
            PlayerController playerController = allies[i].GetPlayerController();
            if (!playerController.StateController.CurState.Contain(PlayerState.Groggy))
            {
                allGroggy = false;
                Debug.Log($"allies[i] alive");
                break;
            }
        }
        if (allGroggy)
        {
            allies.Add(player);
            GroggyTeamOver(allies);


        }
    }
   async void CheckOtherPlayerOver()
    {
        List<Player> livePlayers = new List<Player>();
        int playerCount = PhotonNetwork.PlayerList.Length;
        Debug.Log($"playerCount :{playerCount}");
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.GetProperty<bool>(DefinePropertyKey.GAMEOVER))
            {
                playerCount--;
            }
            else
            {
                livePlayers.Add(player);
            }
        }
        if (!PhotonNetwork.CurrentRoom.GetProperty<bool>(DefinePropertyKey.TEAMGAME))
        {
            if (playerCount <= 1)
            {
                if (livePlayers.Count > 0)
                    livePlayers[0].SetProperty<bool>(DefinePropertyKey.VICTORY, true);
                else
                    Message("Alive Player is NULL");
                GameOver();
            }
        }
        else
        {
            if (livePlayers.Count > 0)
            {
                if (livePlayers.ArePlayersOnSameTeam())
                {
                    foreach (var p in livePlayers)
                    {
                       await p.SetPropertyAsync<bool>(DefinePropertyKey.VICTORY, true);
                        Debug.Log($"{p} Victory");
                    }
                    GameOver();
                }
            }
            else
            {
                Debug.Log("live Player Count is 0");
                GameOver();
            }
        }
    }
    /// <summary>
    /// 관전 시 사용할 카메라 이동 메서드
    /// </summary>
    public void ChangeCameraPlayers()
    {
        if (!deadToWatch.gameObject.activeSelf)
            return;
        if (playerList[curCameraIndex].camMoveEvent != null)
        {
            playerList[curCameraIndex].camMoveEvent -= InDeadCameraMove;
        }

        // 비활성화된 플레이어를 필터링
        playerList = playerList.Where(player => player != null && player.photonView.Owner != null && player.photonView.Owner.IsInactive == false).ToList();

        if (playerList.Count == 0)
        {
            Debug.LogWarning("관전할 활성 플레이어가 없습니다.");
            return;
        }

        int originalIndex = curCameraIndex;
        do
        {
            // 다음 플레이어로 인덱스 증가
            curCameraIndex = (curCameraIndex + 1) % playerList.Count;

            // 만약 이전과 같은 인덱스면 루프를 돌면서 유효한 플레이어를 찾지 못한 경우
            if (curCameraIndex == originalIndex)
            {
                Message("Can't find other Player");
                return;
            }
        } while (playerList[curCameraIndex] == null || playerList[curCameraIndex].photonView.Owner == null || playerList[curCameraIndex].photonView.Owner.IsInactive);


        deadToWatch.onDeadPlayerPanelText.text = playerList[curCameraIndex].PlayerClassData.classType.ToString();

        Debug.Log($"Cam :  {curCameraIndex}");



        playerList[curCameraIndex].camMoveEvent += InDeadCameraMove;
        if (playerList[curCameraIndex].targetCamOther)
        {
            InDeadCameraMove(playerList[curCameraIndex].targetPos);
        }
        else
        {
            InDeadCameraMove(playerList[curCameraIndex].transform);
        }
    }

    public void InDeadCameraMove(Vector2 Pos)
    {
        if (Pos == Vector2.zero)
        {
            InDeadCameraMove(playerList[curCameraIndex].transform);
            return;
        }

        Debug.Log("In Manager Vector2 Cam Move");
        m_Player.CameraMove(Pos);
    }
    public void InDeadCameraMove(Transform target)
    {
        Debug.Log($"DeadCam T : {target}");
        m_Player.CameraMove(target);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer.GetProperty<bool>(DefinePropertyKey.OUTGAME))
        {
            return;
        }

        Debug.Log($"Player {otherPlayer.NickName} left the room.");

        //나간 플레이어와 관련된 처리를 수행 (추후 관련기능 추가 가능성)
        PlayerController playerController = playerList.FirstOrDefault(player => player.photonView.Owner == otherPlayer);
        if (playerController != null)
        {
            playerList.Remove(playerController);
            playerDic.Remove(otherPlayer);
        }
        ChangeCameraPlayers();
        CheckOtherPlayerOver();

    }

    public void Message(string msg)
    {
        msgFloat.ShowMessage(msg);
    }

    public async void GameOver()
    {
        Debug.Log("ManagerOver Active");
        PhotonNetwork.CurrentRoom.IsVisible = true;
        PhotonNetwork.CurrentRoom.IsOpen = true;
        PhotonNetwork.CurrentRoom.SetProperty<bool>(DefinePropertyKey.GAMEON, false);
        await Task.Run(() =>
        {
            while (PhotonNetwork.CurrentRoom.GetProperty<bool>(DefinePropertyKey.GAMEON))
            {
                Thread.Sleep(100);

            }
        });
        photonView.RPC("GameClosing", RpcTarget.All);
        //    Debugging2nd.Ins.miniMapUi.SetActive(false);
        //    Debugging2nd.Ins.chatUi.SetActive(false);
    }

    /// <summary>
    /// 개발자 : 서보운
    /// <br/> 추가 메소드. 로비에서 넘어갈 때 인벤토리가 유저를 찾지 못함.
    /// 추가로 찾아주는 작업을 진행해야 함.
    /// </summary>
    public void SetInventoryUI()
    {
        if (Inventory.Instance.Player.UIController.inventoryUI == null)
        {
            Inventory.Instance.Player.UIController.inventoryUI = invenUI;
            Inventory.Instance.Player.UIController.settingUI = settingUI;
            invenUI.InventoryButton.onClick.AddListener(Inventory.Instance.Player.UIController.OpenInventoryUI);
            settingUI.SettingButton.onClick.AddListener(Inventory.Instance.Player.UIController.OpenSettingUI);
        }
    }
    public void ChatActivate(bool b)
    {
        chat.gameObject.SetActive(b);
    }


    /// <summary>
    /// 엑스칼리버를 주웠을 때 호출할 메소드
    /// </summary>
    public void GetExcalibur()
    {
        if (excaliburPlayer != null)
        {
            // 엑스칼리버를 주웠다는 이벤트 실행(탈출 포인트 활성화, 화살표 설정).
            ChangeExcalibur?.Invoke();

            // 수정 후
            m_Player.UIController.TargetingPoing();

            msgFloat.ShowMessageExcalibur();
        }
    }

    /// <summary>
    /// 엑스칼리버 유저가 사망했을 때 실행할 메소드
    /// </summary>
    public void ExcaliburDie()
    {
        exitPointController.DisableExitPoint();
    }

    [PunRPC]
    public void GameClosing()
    {
        Debug.Log("ClosingRPC Active");
        GameManager.Ins.totalRoomCount = 0;
        bool victory = PhotonNetwork.LocalPlayer.GetProperty<bool>(DefinePropertyKey.VICTORY);          //승리 체크 돼있으면 승리 아니면 패배

        if (victory)
        {
            Victory();
        }
        else
        {
            Defeat();
        }


    }

    void Victory()
    {
        Debug.Log("Victory Active");
        InGameCanvasActive(false);
        winCanvas.gameObject.SetActive(true);
        FirebaseManager.Instance.UpLoadInventoryData();
        FirebaseManager.Instance.curGold += 5000;
        FirebaseManager.Instance.UploadCurGold();
    }

    void Defeat()
    {
        Debug.Log("Defeat Active");
        m_Player.Die();
        DieEvent();

        Inventory.Instance.Items.Clear();
        Inventory.Instance.QuickItems.Clear();
        //Inventory.Instance.FreshSlots();
        FirebaseManager.Instance.UpLoadInventoryData();
        if (m_Player.StatController.PlayerLevel >= 7)
        {
            FirebaseManager.Instance.curGold += 600;
            FirebaseManager.Instance.UploadCurGold();
        }

    }

    public IEnumerator CanvasColorChange(float time, Color color)
    {
        float t = 0;
        
        while(t < time)
        {
            t += Time.deltaTime;
            stateCanvas.color = Color.Lerp(color, Color.clear, t);
            yield return null;
        }
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, PH changedProps)
    {
        CustomKeyStoring(targetPlayer, changedProps);
        // Init();                                                                                                   TODO .. 플레이어 캐릭터 바뀌면 이거 해줘야함 

    }
    public void InGameCanvasActive(bool active)
    {
        inGameCanvas.gameObject.SetActive(active);
    }
    [PunRPC]
    public void InitRPC()
    {
        Init();
    }

    /// <summary>
    /// 개발자 : 이형필 / 커스텀프로퍼티 동기화를 위해 tcs에 저장된 모든 키값에서 해당 키 검색하고 셋업 끝나면 result를 true로 바꿔줌
    /// </summary>
    /// <param name="targetPlayer"></param>
    /// <param name="changedProps"></param>
    void CustomKeyStoring(Player targetPlayer, PH changedProps)
    {
        var keys = PropertyTaskCompletionSourceStore.GetAllKeys(targetPlayer).ToList();


        foreach (var key in keys)
        {
            if (changedProps.ContainsKey(key))
            {
                var tcs = PropertyTaskCompletionSourceStore.Retrieve(targetPlayer, key);

                if (tcs != null)
                {
                    Debug.Log($"Property {key} updated");
                    tcs.TrySetResult(true);
                    PropertyTaskCompletionSourceStore.Remove(targetPlayer, key);
                }
            }
        }
    }

}

