using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPanel : MonoBehaviour
{
    [SerializeField] CreateRoomPanel createRoomPanel;
    [SerializeField] PanelController panelController;
    [SerializeField] ConfirmPasswordRoomPanel roomPasswordPanel;
    [SerializeField] Button createButton;
    [SerializeField] Button leaveButton;


    [SerializeField] RectTransform roomContent;
    [SerializeField] RoomEntry roomEntryPrefab;
    Dictionary<string, RoomEntry> roomDictionary;


    public ConfirmPasswordRoomPanel RoomPasswordPanel { get { return roomPasswordPanel; } set {  roomPasswordPanel = value; } }
    public PanelController PanelController { get {  return panelController; } set {  panelController = value; } }


    private void Awake()
    {
        roomDictionary = new Dictionary<string, RoomEntry>();
        createButton.onClick.AddListener(() =>CreateRoomPanel(true)); //방 생성 버튼에 해당 이벤트 연결
        leaveButton.onClick.AddListener(LeaveLobby); //로비 나가기 버튼에 로비 나가기 이벤트 연결
    }
    private void OnDisable()
    {
        for (int i = 0; i < roomContent.childCount; i++)
            Destroy(roomContent.GetChild(i).gameObject);
        roomDictionary.Clear();
    }
    public void UpdateRoomList(List<RoomInfo> roomList)
    {
     /*   if(roomDictionary != null)
        {
            return;
        }


        roomDictionary = new Dictionary<string, RoomEntry>();*/
        foreach (RoomInfo room in roomList)
        {
            
            if (roomDictionary.ContainsKey(room.Name))
            {
                if (room.RemovedFromList || room.IsOpen == false || room.IsVisible == false)
                {
                    RoomEntry roomEntry = roomDictionary[room.Name];
                    Debug.Log($"No Longer in RoomList : {roomEntry}");
                    if (roomEntry != null)
                        Destroy(roomEntry.gameObject);
                    roomDictionary.Remove(room.Name);
                }
                else
                    roomDictionary[room.Name].SetRoomInfo(room);
            }
            else
            {
                if(room.PlayerCount == 0)
                {
                    return;
                }
                RoomEntry entry = Instantiate(roomEntryPrefab, roomContent);
                entry.Lobby = this;
                entry.SetRoomInfo(room);
                roomDictionary.Add(room.Name, entry);
            }
        }
    }

    public void CreateRoomPanel(bool b)
    {
        createRoomPanel.gameObject.SetActive(b);
        panelController.ButtonBlock(b);
    }
    public void LeaveLobby()
    {
        PhotonNetwork.LeaveLobby();
    }

    public void ConfirmPassword()
    {
        roomPasswordPanel.gameObject.SetActive(true);
    }
}
