using Firebase.Database;
using Firebase.Extensions;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 서보운
/// <br/> 메인 패널
/// </summary>
public class MainPanel : MonoBehaviour
{
    [SerializeField] PanelController panelController;

    [Tooltip("게임 찾기 버튼(에디터 할당 필요)")]
    [SerializeField] Button findGameButton;
    [Tooltip("도움말 버튼(에디터 할당 필요)")]
    [SerializeField] Button informationButton;
    [Tooltip("상점 / 창고 버튼(에디터 할당 필요)")]
    [SerializeField] Button store_WarehouseButton;
    [Tooltip("설정 버튼(에디터 할당 필요)")]
    [SerializeField] Button settingButton;
    [Tooltip("프로필 버튼(에디터 할당 필요)")]
    [SerializeField] Button profileButton;
    [Tooltip("게임 종료 버튼(에디터 할당 필요)")]
    [SerializeField] Button gameEndButton;

    [Tooltip("프로필 이미지(에디터 할당 필요)")]
    [SerializeField] ProfileImage profileImage;
    [Tooltip("도움말 이미지(에디터 할당 필요)")]
    [SerializeField] MainInformation information;
    [Tooltip("설정창 이미지(에디터 할당 필요)")]
    [SerializeField] GameObject settingImage;
    [Tooltip("Setting Image > Setting (에디터 할당 필요)")]
    [SerializeField] Setting setting;

    [Tooltip("8.UI > OutGame > OutGameInventory 프리팹 (에디터 할당 필요)")]
    [SerializeField] Inventory outGameInventoryPrefab;

    [Tooltip("MainPanelBlockImage (에디터 할당 필요)")]
    [SerializeField] GameObject mainPanelBlockImage;

    public GameObject MainPanelBlockImage { get { return mainPanelBlockImage; } }

    private void Awake()
    {
        profileButton.onClick.AddListener(OpenProfile);
        store_WarehouseButton.onClick.AddListener(OpenStore);
        settingButton.onClick.AddListener(OpenSetting);
        findGameButton.onClick.AddListener(JoinLobby);
        gameEndButton.onClick.AddListener(LeaveGame);
        informationButton.onClick.AddListener(OpenInformation);
    }

    private void OnEnable()
    {
        FirebaseManager.DB
            .GetReference("PlayerDataTable")
            .Child(FirebaseManager.Instance.curPlayer)
            .Child("settingData")
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if(task.IsCanceled)
                {
                    return;
                }
                else if(task.IsFaulted)
                {
                    return;
                }

                DataSnapshot snapshot = task.Result;

                if(snapshot.Exists)
                {
                    SoundManager.instance.saveUserData = JsonUtility.FromJson<SettingSaveData>(snapshot.GetRawJsonValue());
                
                    // TODO.. 데이터 넣기 및 UI 반영하는 메소드 짜기
                }
            });
        panelController.CreateDummyPlayer();
        StartCoroutine(StartDownLoadInven());
    }
    IEnumerator StartDownLoadInven()
    {
        yield return new WaitUntil(() => (Inventory.Instance != null && Inventory.Instance.Player != null));

        // Inventory.Instance.DownLoadInventoryData();
    }
    private void OnDisable()
    {
        Destroy(GameManager.Ins.m_Player.gameObject);
    }
    private void OpenProfile()
    {
        profileImage.gameObject.SetActive(true);
        profileImage.SetProfile();
        mainPanelBlockImage.SetActive(true);
    }

    private void OpenStore()
    {
        //Instantiate(outGameInventoryPrefab, store_warehouseImage.transform);
    }

    private void OpenSetting()
    {
        settingImage.gameObject.SetActive(true);
        mainPanelBlockImage.SetActive(true);
        setting.SetTextSetting();
    }

    private void OpenInformation()
    {
        information.gameObject.SetActive(true);
        mainPanelBlockImage.SetActive(true);
    }

    private void JoinLobby()
    {
        profileImage.gameObject.SetActive(false);
        PhotonNetwork.JoinLobby();
        panelController.SetActivePanel(PanelController.Panel.Lobby);
    }

    private void LeaveGame()
    {
        PhotonNetwork.Disconnect();
    }
}
