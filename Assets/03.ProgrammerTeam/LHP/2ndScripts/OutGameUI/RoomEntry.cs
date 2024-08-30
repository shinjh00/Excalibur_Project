using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using PH = ExitGames.Client.Photon.Hashtable;

public class RoomEntry : MonoBehaviour
{
    [SerializeField] TMP_Text roomName;
    [SerializeField] TMP_Text currentPlayer;
    [SerializeField] TMP_Text roomHostName;
    [SerializeField] Button joinRoomButton;

    [SerializeField] Sprite isTeamMode;
    [SerializeField] Sprite isSoloMode;
    [SerializeField] Image gameModeImage;
    [SerializeField] Image isPasswordRoom;
    

    RoomInfo roomInfo;
    string roomPassword;

    LobbyPanel lobby;

    ConfirmPasswordRoomPanel pp;

    public LobbyPanel Lobby { get { return lobby; } set { lobby  = value; } }

    public void SetRoomInfo(RoomInfo info)
    {
        joinRoomButton.onClick.RemoveAllListeners();
        this.roomInfo = info;
        roomName.text = roomInfo.CustomProperties[DefinePropertyKey.ROOMNAME] as string;
        currentPlayer.text = $"{roomInfo.PlayerCount} / {roomInfo.MaxPlayers}";
        joinRoomButton.interactable = roomInfo.PlayerCount < roomInfo.MaxPlayers;
        joinRoomButton.onClick.AddListener(OnJoinRoomButtonClicked);

        // 비밀번호 확인 로직을 위해 커스텀 프로퍼티에서 비밀번호를 가져옴
        if (roomInfo.CustomProperties.ContainsKey(DefinePropertyKey.PASSWORD))
        {
            roomPassword = roomInfo.CustomProperties[DefinePropertyKey.PASSWORD] as string;
            isPasswordRoom.enabled = true;
            if(roomPassword == "")
            {
                roomPassword = string.Empty;
                isPasswordRoom.enabled = false;
            }
        }
        else
        {
            roomPassword = string.Empty;
            isPasswordRoom.enabled = false; 
        }
        if (roomInfo.CustomProperties.ContainsKey(DefinePropertyKey.HOST))
        {
            roomHostName.text = roomInfo.CustomProperties[DefinePropertyKey.HOST] as string;
        }
        else
        {
            roomHostName.text = "NULL";
        }
        if (roomInfo.CustomProperties.TryGetValue(DefinePropertyKey.TEAMGAME, out var teamGameObj))
        {
            bool isTeam = Convert.ToBoolean(teamGameObj);
            gameModeImage.sprite = isTeam ? isTeamMode : isSoloMode; // Assuming you have a default or other mode
        }
    }

    void OnJoinRoomButtonClicked()
    {
        if (!string.IsNullOrEmpty(roomPassword))
        {
            // 비밀번호가 설정된 방이면 비밀번호 입력 패널을 활성화
            lobby.RoomPasswordPanel.gameObject.SetActive(true);
            pp = lobby.RoomPasswordPanel;
            Lobby.PanelController.ButtonBlock(true);
            pp.Init(this);


        }
        else
        {
            // 비밀번호가 없는 방이면 바로 방에 입장
            JoinRoom();
            Debug.Log("Password isn't it Join Room");
        }
    }


    public void OnConfirmPassword()
    {
        if (pp.passwordInputField.text == roomPassword)
        {
            // 비밀번호가 일치하면 방에 입장
            JoinRoom();
            Debug.Log("Password Pass Join Room");
            pp.gameObject.SetActive(false);

            RemoveConfirmPasswordListener(OnConfirmPassword);
        }
        else
        {
            // 비밀번호가 일치하지 않으면 경고 메시지를 출력하거나 다시 입력 받음
            Debug.Log("비밀번호가 일치하지 않습니다.");
        }
    }

    void JoinRoom()
    {
        Debug.Log("JoinRoom");
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.JoinRoom(roomInfo.Name);
    }
    public void RemoveConfirmPasswordListener(UnityAction listener)
    {
        if (pp != null && pp.confirmButton != null)
        {
            pp.confirmButton.onClick.RemoveListener(listener);
            Lobby.PanelController.ButtonBlock(false);
        }
    }
}
