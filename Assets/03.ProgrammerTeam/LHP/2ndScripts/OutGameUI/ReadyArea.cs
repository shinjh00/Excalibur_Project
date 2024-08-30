using Photon.Pun;
using Photon.Realtime;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReadyArea : MonoBehaviourPun
{
    [SerializeField] LayerMask playerLayer;
    [SerializeField] TMP_Text readyCountText;
    [SerializeField] TMP_Text startCountText;
    [SerializeField] TMP_Text readyText;
    [SerializeField] Button readyButton;
    [SerializeField] Collider2D wall;
    [SerializeField] Collider2D onStartTeamWall;
    [SerializeField] float startTimeValue;
    [SerializeField] WaitRoomPanel roomPanel;
    [SerializeField] GameObject lobby;

    [SerializeField] PlayerController m_player;
    [SerializeField] Transform redArea;
    [SerializeField] Transform blueArea;

    public bool start;
    int readyCount = 0;



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (playerLayer.Contain(collision.gameObject.layer) && AllPlayerOutGame())
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player.photonView.IsMine)
            {
                m_player = player;
                readyButton.interactable = true;
            }
        }
    }
    bool AllPlayerOutGame()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!player.GetProperty<bool>(DefinePropertyKey.OUTGAME))
            {
                return false;
            }
        }

        return true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (playerLayer.Contain(collision.gameObject.layer) && AllPlayerOutGame())
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player.photonView.IsMine && player.photonView.Owner.GetProperty<bool>(DefinePropertyKey.OUTGAME))
            {
                if (readyButton != null)
                {
                    readyButton.interactable = false;
                    ReadyOff();
                }


            }
        }
    }
    async void ReadyOff()
    {
        if (gameObject != null && photonView != null && photonView.ViewID != 0)
        {
                await PhotonNetwork.LocalPlayer.SetPropertyAsync(DefinePropertyKey.LOBBYREADY, false);
                readyText.text = "READY";
                Debug.Log("ExitReadyCounter");
            if(gameObject != null)
                photonView.RPC("ReadyCnt", RpcTarget.All);
        }

    }
    public async void ReadyOn() //버튼으로 호출
    {
        if (AllPlayerOutGame())
        {

            try
            {
                await PhotonNetwork.LocalPlayer.SetPropertyAsync(DefinePropertyKey.LOBBYREADY, true);
                readyText.text = "READIED";
                Debug.Log("ReadyOn");
                photonView.RPC("ReadyCnt", RpcTarget.All);
            }
            catch
            {

            }
        }

    }

    [PunRPC]
    void ReadyCnt()
    {
        Debug.Log("ReadyCount Up");
        readyCount = 0;
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].GetProperty<bool>(DefinePropertyKey.LOBBYREADY))
            {
                Debug.Log($"{PhotonNetwork.PlayerList[i].ActorNumber} lobbyReady");
                readyCount++;
            }
        }

        readyCountText.text = $"READY {readyCount}/{PhotonNetwork.PlayerList.Length}";
        if (PhotonNetwork.IsMasterClient)
        {
            CheckReady();
        }
    }
    void CheckReady()
    {

        if (readyCount == PhotonNetwork.PlayerList.Length)
        {
         //  if (readyCount % 4 == 0)
          //  {
                    photonView.RPC("StartGameCount", RpcTarget.All);
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    PhotonNetwork.CurrentRoom.SetProperty(DefinePropertyKey.GAMEON, true);

         //   }
        }
        else
        {
            return;
        }
    }
    [PunRPC]
    void StartGameCount()
    {



        if (!start)
        {
            start = true;
            if (PhotonNetwork.CurrentRoom.GetProperty<bool>(DefinePropertyKey.TEAMGAME))
            {
                onStartTeamWall.enabled = true;
                if (PhotonNetwork.LocalPlayer.GetProperty<bool>(DefinePropertyKey.RED))
                {
                    m_player.transform.position = redArea.position;
                }
                else if (PhotonNetwork.LocalPlayer.GetProperty<bool>(DefinePropertyKey.BLUE))
                {
                    m_player.transform.position = blueArea.position;
                }
            }
            else
            {
                m_player.transform.position = transform.position;
            }


            readyButton.gameObject.SetActive(false);
            roomPanel.leaveButton.interactable = false;
            wall.enabled = true;
            StartCoroutine(Utils.Timer(startTimeValue, startCountText, StartGame));
            roomPanel.roomOptionButton.interactable = false;
        }


    }
    async void StartGame()
    {
       // PhotonNetwork.Destroy(GameManager.Ins.m_Player.gameObject);
        await PhotonNetwork.LocalPlayer.SetPropertyAsync(DefinePropertyKey.LOBBYREADY, false);
        await PhotonNetwork.LocalPlayer.SetPropertyAsync(DefinePropertyKey.VICTORY, false);
        await PhotonNetwork.LocalPlayer.SetPropertyAsync(DefinePropertyKey.LOADCOMPLETE, false);


        /*  foreach (var p in roomPanel.playerInstances)
          {
              if (p != null && p.photonView.IsMine)
              {
                  PhotonNetwork.Destroy(p.gameObject);
              }
          }*/
        if (PhotonNetwork.IsMasterClient)
        {
            //  PhotonNetwork.DestroyAll();
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                await PhotonNetwork.PlayerList[i].SetPropertyAsync(DefinePropertyKey.OUTGAME, false);
            }

        }
        Debug.Log("Load InGame");
        PhotonNetwork.LoadLevel("InGame");

    }
}
