using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using PH = ExitGames.Client.Photon.Hashtable;

public class CreateRoomPanel : MonoBehaviour
{
    [SerializeField] Button createRoomButton;
    [SerializeField] Button createCancleButton;
    [SerializeField] Button checkPasswordRoom;

    [SerializeField] Button modeButtonIndividual;
    [SerializeField] Button modeButtonTeam;


    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_InputField roomPasswordInputField;
    [SerializeField] TMP_Text roomPanelText;

    [SerializeField] Image passwordRoomCheckImage;
    [SerializeField] PanelController panelController;
    [SerializeField] WaitRoomPanel roomPanel;
    bool passwordChecker = false;
    PH roomMode = new PH();

    void Awake()
    {
        createCancleButton.onClick.AddListener(CreateRoomCancel);
        createRoomButton.onClick.AddListener(CreateRoomConfirm);
        checkPasswordRoom.onClick.AddListener(PasswordRoomChecker);
        modeButtonIndividual.onClick.AddListener(() =>ModeChange(false));
        modeButtonTeam.onClick.AddListener(()=>ModeChange(true));
        roomNameInputField.characterLimit = 10;


        ModeChange(false);
    }
    private void OnEnable()
    {
        if (PhotonNetwork.InLobby)
        {
            roomPanelText.text = "ROOM CREATE";
        }
        else if (PhotonNetwork.InRoom)
        {
            roomPanelText.text = "ROOM OPTION";
        }
        else
        {
            Debug.Log($"State : {PhotonNetwork.NetworkClientState}");
        }

    }
    void CreateRoomConfirm()
    {
        if(PhotonNetwork.InLobby)
        {
            string roomName = roomNameInputField.text;
            string roomPassword = roomPasswordInputField.text;
            if (roomName == "")
                roomName = Random.Range(1000, 10000).ToString();

            RoomOptions roomOptions = new RoomOptions { MaxPlayers = 4 };

            PH customProperties = new PH();
            customProperties[DefinePropertyKey.ROOMNAME] = roomName;
            if (passwordChecker)
            {
                customProperties[DefinePropertyKey.PASSWORD] = roomPassword;
            }

            customProperties[DefinePropertyKey.TEAMGAME] = roomMode[DefinePropertyKey.TEAMGAME];
            if (PhotonNetwork.LocalPlayer != null)
            {
                customProperties[DefinePropertyKey.HOST] = PhotonNetwork.LocalPlayer.NickName;
            }
            roomOptions.CustomRoomProperties = customProperties;
            roomOptions.EmptyRoomTtl = 7;
            roomOptions.CustomRoomPropertiesForLobby = new string[] {DefinePropertyKey.ROOMNAME, DefinePropertyKey.PASSWORD, DefinePropertyKey.HOST,DefinePropertyKey.TEAMGAME };
            PhotonNetwork.CreateRoom(roomName, roomOptions);

            CreateRoomCancel();
        }
        else if (PhotonNetwork.InRoom)
        {
            StartCoroutine(UpdateRoomProperties());
        }


    }
    IEnumerator UpdateRoomProperties()
    {
        string roomName = roomNameInputField.text;
        string roomPassword = roomPasswordInputField.text;

        PH customProperties = new PH();
        customProperties[DefinePropertyKey.ROOMNAME] = roomName;
        customProperties[DefinePropertyKey.PASSWORD] = roomPassword;
        if(!passwordChecker)
            customProperties[DefinePropertyKey.PASSWORD] = "";
        customProperties[DefinePropertyKey.TEAMGAME] = roomMode[DefinePropertyKey.TEAMGAME];
        if (PhotonNetwork.LocalPlayer != null)
        {
            customProperties[DefinePropertyKey.HOST] = PhotonNetwork.LocalPlayer.NickName;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);

        Debug.Log($"try : {customProperties} cur : { PhotonNetwork.CurrentRoom.CustomProperties}" );
        yield return new WaitUntil(() => AreCustomPropertiesEqual(customProperties, PhotonNetwork.CurrentRoom.CustomProperties));
        Debug.Log($"Complete");

        CreateRoomCancel();
        roomPanel.RoomOptionUpdate(false);
    }
    private bool AreCustomPropertiesEqual(PH expected, PH current)
    {
        if (expected.Count != current.Count)
            return false;

        foreach (DictionaryEntry entry in expected)
        {
            if (!current.ContainsKey(entry.Key) || !current[entry.Key].Equals(entry.Value))
                return false;
        }

        return true;
    }
    void CreateRoomCancel()
    {
        gameObject.SetActive(false);
        panelController.ButtonBlock(false);
    }

    void PasswordRoomChecker()
    {
        passwordChecker = !passwordChecker;
        passwordRoomCheckImage.enabled = passwordChecker;
        roomPasswordInputField.interactable = passwordChecker;
    }
    void ModeChange(bool team)
    {
        roomMode[DefinePropertyKey.TEAMGAME] = team;
        modeButtonIndividual.interactable = team;
        modeButtonTeam.interactable = !team;

    }
}
