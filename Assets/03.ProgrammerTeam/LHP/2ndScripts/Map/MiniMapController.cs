using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 개발자 : 이형필 / 게임을 시작하면 미니맵을 찾아 참조하고 활용하기 위한 클래스
/// </summary>
public class MiniMapController : BaseController
{
    [SerializeField] Icons icons;
    [SerializeField] Minimap minimap;
    [SerializeField] FowClear fow;

    private void Start()
    {
        if (PhotonNetwork.LocalPlayer.GetProperty<bool>(DefinePropertyKey.OUTGAME))
        {
            return;
        }
        FindMinimap();
    }
    public void FindMinimap()
    {
        Debug.Log("Find minimap");
        if (photonView.IsMine)
        {
            icons.myIcon.SetActive(true);
            minimap = GameManager.Ins.MiniMap;
            minimap.minimap.enabled = true;
            minimap.camImage.enabled = true;
            minimap?.gameObject.SetActive(false);
        }
        else
        {
            Destroy(fow);
            Destroy(this);
            icons.myIcon.SetActive(false);
            //    icons.enemyIcon.SetActive(true);
        }
    }

    void OnMinimap(InputValue value)
    {
        if (GameManager.Ins.onChat)
        {
            return;
        }
        if(minimap != null)
        {
            minimap.gameObject.SetActive(!minimap.gameObject.activeSelf);
        }
    }


}
