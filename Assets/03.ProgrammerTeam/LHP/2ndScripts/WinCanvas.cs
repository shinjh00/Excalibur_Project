using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class WinCanvas : MonoBehaviour
{
    [SerializeField] Button toLobbyButton;
    [SerializeField] TMP_Text text;
    private void OnEnable()
    {
        toLobbyButton.onClick.AddListener(LeaveGame);
    }
    private void OnDisable()
    {
        toLobbyButton.onClick.RemoveListener(LeaveGame);
    }

    async void LeaveGame()
    {

        if (PhotonNetwork.CurrentRoom.GetProperty<bool>(DefinePropertyKey.GAMEON))
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LoadLevel("OutGame");
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
                StartCoroutine(WaitMaster());
            }
        }




    }
    IEnumerator WaitMaster()
    {
        toLobbyButton.interactable = false;
        text.text = "Wait For Master";
        yield return new WaitUntil(() => PhotonNetwork.PlayerList.Any(player => player.GetProperty<bool>(DefinePropertyKey.LOADCOMPLETE)));
        PhotonNetwork.LocalPlayer.SetProperty<bool>(DefinePropertyKey.GAMEOVER, false);
        PhotonNetwork.LocalPlayer.SetProperty<bool>(DefinePropertyKey.OUTGAME, true);
        PhotonNetwork.LoadLevel("OutGame");

    }
}
