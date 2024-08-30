using Photon.Pun;
using UnityEngine;

public class TeamModeArea : MonoBehaviour
{
    [SerializeField] LayerMask playerLayer;
    [SerializeField] bool red;
    [SerializeField] bool blue;
    [SerializeField] ReadyArea area;
    private void Start()
    {
        if (red && blue)
        {
            blue = false;
        }
        if (!red && !blue)
        {
            red = true;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (area.start)
            return;
        if (playerLayer.Contain(collision.gameObject.layer))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player.photonView.IsMine)
            {
                if (red)
                {
                    PhotonNetwork.LocalPlayer.SetProperty(DefinePropertyKey.BLUE, false);
                    PhotonNetwork.LocalPlayer.SetProperty(DefinePropertyKey.RED, true);
                }

                else if (blue)
                {
                    PhotonNetwork.LocalPlayer.SetProperty(DefinePropertyKey.RED, false);
                    PhotonNetwork.LocalPlayer.SetProperty(DefinePropertyKey.BLUE, true);


                }

            }
        }
    }


  /*  private void OnTriggerExit2D(Collider2D collision)
    {
        if (!PhotonNetwork.LocalPlayer.GetProperty<bool>(DefinePropertyKey.OUTGAME))
        {
            Debug.Log($"{PhotonNetwork.LocalPlayer} :is NotOutGame");
            return;
        }
        if (playerLayer.Contain(collision.gameObject.layer))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player.photonView.IsMine)
            {
                if (red)
                {
                    PhotonNetwork.LocalPlayer.SetProperty(DefinePropertyKey.RED, false);
                }

                else if (blue)
                {
                    PhotonNetwork.LocalPlayer.SetProperty(DefinePropertyKey.BLUE, false);

                }
            }
        }
    }*/
}
