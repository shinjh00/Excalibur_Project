using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
/// <summary>
/// 개발자 : 이형필 / 관전기능 활성화를 위한 클래스
/// </summary>
public class DeadToWatch : MonoBehaviour
{

    [Tooltip("InitImage를 할당")]
    [SerializeField] GameObject deadPanel;
    [Tooltip("OnSpacePanel을 할당")]
    [SerializeField] GameObject onDeadPlayerPanel;
    [Tooltip("OnSpacePanel-Down-PlayerName을 할당")]
    public TMP_Text onDeadPlayerPanelText;
    [Tooltip("관전버튼을 할당")]
    public Button onCamCanvasOpenButton;
    [Tooltip("로비로 나가기버튼을 할당")]
    public Button toLobbyButton;
    [Tooltip("로비로 나가기버튼을 할당")]
    public Button toLobbyButton2;


    private void Awake()
    {
        onCamCanvasOpenButton.onClick.AddListener(OpenCamCanvas);
        toLobbyButton.onClick.AddListener(LeaveGame);
        //씬 로비씬으로 넘기고
    }
    /// <summary>
    /// 스페이스를 눌렀을때 다른 플레이어로 관전 화면 넘어감
    /// </summary>
    /// <param name="value"></param>
    public void OnSpace(InputValue value)
    {
        if (onDeadPlayerPanel.activeSelf)
        {
            GameManager.Ins.ChangeCameraPlayers();
        }

    }
    async void LeaveGame()
    {
        if (PhotonNetwork.CurrentRoom.GetProperty<bool>(DefinePropertyKey.GAMEON))
        {

            // 방 나간 후에 계속 대기하여 방이 완전히 나갔는지 확인
            await Task.Run(() =>
            {
                while (PhotonNetwork.CurrentRoom != null)
                {
                    Task.Delay(100); // 비동기적으로 대기
                }
            }).ContinueWith(task =>
            {
                Debug.Log($"Client : {PhotonNetwork.NetworkClientState}");
                PhotonNetwork.LoadLevel("OutGame");
            });
            return;
        }
        else
        {
            if (PhotonNetwork.CurrentRoom.GetProperty<bool>(DefinePropertyKey.LOAD))
            {
                PhotonNetwork.DestroyAll(false);
                PhotonNetwork.CurrentRoom.SetProperty<bool>(DefinePropertyKey.LOAD, false);
                await PhotonNetwork.LocalPlayer.SetPropertyAsync<bool>(DefinePropertyKey.GAMEOVER, false);
                await PhotonNetwork.LocalPlayer.SetPropertyAsync<bool>(DefinePropertyKey.OUTGAME, true);

                PhotonNetwork.LoadLevel("OutGame");
            }

            else
            {
                toLobbyButton.interactable = false;
                toLobbyButton2.interactable = false;
                 StartCoroutine(WaitMaster());
            }
        }
    }
    IEnumerator WaitMaster()
    {
        toLobbyButton.interactable = false;
        toLobbyButton2.interactable = false;
        onDeadPlayerPanelText.text = "Wait For RoomMaster";
        yield return new WaitUntil(() => PhotonNetwork.PlayerList.Any(player => player.GetProperty<bool>(DefinePropertyKey.LOADCOMPLETE)));
        PhotonNetwork.LocalPlayer.SetProperty<bool>(DefinePropertyKey.GAMEOVER, false);
        PhotonNetwork.LocalPlayer.SetProperty<bool>(DefinePropertyKey.OUTGAME, true);
        PhotonNetwork.LoadLevel("OutGame");

    }
    void OpenCamCanvas()
    {
        SecondMapGen.Ins.FowOpen();
        onDeadPlayerPanel.SetActive(true);
        toLobbyButton2.gameObject.SetActive(true);
        toLobbyButton2.onClick.AddListener(LeaveGame);
        deadPanel.SetActive(false);
        GameManager.Ins.m_Player.visionController.FovVision(false);
        foreach (PlayerController player in GameManager.Ins.playerList)
        {
            player.visionController.gameObject.SetActive(true);
            player.visionController.CircleVision(true);
        }
    }
}
