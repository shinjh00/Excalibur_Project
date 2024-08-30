using Photon.Pun;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 개발자 : 신지혜 / 플레이어 UI 관련 컨트롤러
/// </summary>
public class PlayerUIController : BaseController
{
    public InventoryUI inventoryUI;
    public SettingUI settingUI;
    [SerializeField] PlayerStatUI playerStatUI;
    [SerializeField] PlayerSkillSelectUI playerSkillSelectUI;
    [Tooltip("엑스칼리버 탈출구 표시를 위한 화살표 오브젝트(에디터 할당 필요)")]
    [SerializeField] ExcaliburArrowPointing arrowPointing;

    [SerializeField] PhotonView playerPv;

    bool canOpenInventory;

    public bool CanOpenInventory { get { return canOpenInventory; } set { canOpenInventory = value; } }

    private IEnumerator Start()
    {
        /*if (!owner.StateController.CurState.Contain(PlayerState.Lobby))
        {
            yield break;
        }*/
        yield return new WaitUntil(() => (owner != null) && (owner.isSetting));

        playerPv = GetComponent<PhotonView>();
        canOpenInventory = true;

        if (!playerPv.IsMine)
        {
            this.enabled = false;
            Destroy(playerStatUI.gameObject);
            Destroy(playerSkillSelectUI.gameObject);
        }
        else
        {
            if (Inventory.Instance.Player == null)
            {
                Inventory.Instance.Player = owner;
            }
        }
    }

    /// <summary>
    /// 인벤토리 열고 닫기
    /// </summary>
    public void OpenInventoryUI()
    {
        if (canOpenInventory)
        {
            inventoryUI.OpenInventory();
            if (playerStatUI != null && playerSkillSelectUI != null)
            {
                playerStatUI.gameObject.SetActive(!playerStatUI.gameObject.activeSelf);
                playerSkillSelectUI.gameObject.SetActive(!playerSkillSelectUI.gameObject.activeSelf);
                if (Inventory.Instance.DraggingSlot != null)
                {
                    Inventory.Instance.DraggingSlot.SlotBehaviorOnClose();
                }
            }
        }
    }

    /// <summary>
    /// 설정창 열고 닫기
    /// </summary>
    public void OpenSettingUI()
    {
        settingUI.OpenSetting();
    }

    /// <summary>
    /// 개발자 : 서보운
    /// <br/> 엑스칼리버를 획득하였을 때 화살표 UI의 방향 설정
    /// </summary>
    [PunRPC]
    public void TargetingPoing()
    {
        arrowPointing.TargetingArrow(owner.PlayerClassData.classType);
    }

}
