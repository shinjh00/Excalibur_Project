using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using PH = ExitGames.Client.Photon.Hashtable;

public class PlayerEntry : MonoBehaviour
{
    [SerializeField] TMP_Text playerName;
    [SerializeField] Image charImage;
    [SerializeField] Image entryImage;

    [SerializeField] Sprite knight;
    [SerializeField] Sprite warrior;
    [SerializeField] Sprite wizard;
    [SerializeField] Sprite archer;
    public bool ReadyState { get; private set; }
    public Player Player { get { return player; } }
    public Image EntryImage { get { return entryImage; } set { entryImage = value; } }

    public TMP_Text readyText;
    Player player;
    

    public PlayerController controller;
    public PlayerController Controller { get; private set; }
    public int playerNum;
    public void SetPlayer(Player player)
    {
        this.player = player;
        playerName.text = player.NickName; //플레이어 닉네임 출력
        playerNum = player.ActorNumber;

        if (PhotonNetwork.CurrentRoom.GetProperty<bool>(DefinePropertyKey.TEAMGAME))
        {
            bool isRed = player.GetProperty<bool>(DefinePropertyKey.RED);
            EntryImage.color = isRed ? Color.red : Color.cyan;
        }

        
        if (!player.GetProperty<bool>(DefinePropertyKey.OUTGAME))
        {
            entryImage.color = Color.gray;
        }
        charImage.sprite = CharacterImageUpdate(player.GetProperty<ClassType>(DefinePropertyKey.CHARACTERCLASS));

    }
    public void SetPlayerController(PlayerController newController)
    {
        Controller = newController;
    }

    public void UpdateProperty(PH property)
    {
        if (property.ContainsKey(DefinePropertyKey.RED))
        {
            bool isRed = (bool)property[DefinePropertyKey.RED];
            EntryImage.color = isRed ? Color.red : Color.cyan;
        }

        // Check and update the OUTGAME property
        if (property.ContainsKey(DefinePropertyKey.OUTGAME))
        {
            bool isOutGame = (bool)property[DefinePropertyKey.OUTGAME];
            entryImage.color = isOutGame ? Color.white : Color.gray;
        }
        if (property.ContainsKey(DefinePropertyKey.LOBBYREADY))
        {
            bool ready = (bool)property[DefinePropertyKey.LOBBYREADY];
            readyText.text = ready ? "R" : "";
        }

    }
    Sprite CharacterImageUpdate(ClassType classType)
    {
        Sprite image = classType
            switch
        {
            ClassType.Knight => knight,
            ClassType.Warrior => warrior,
            ClassType.Wizard => wizard,
            ClassType.Archer => archer,
            _ => knight
        };


        return image;
    }

}
