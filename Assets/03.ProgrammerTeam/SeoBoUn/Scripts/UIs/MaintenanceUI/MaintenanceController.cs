using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 서보운
/// <br/> 정비창 컨트롤러
/// </summary>
public class MaintenanceController : MonoBehaviour
{
    [Header("CharSelecting Component")]
    [Tooltip("캐릭터 선택 버튼(최상단 / 에디터 할당 필요)")]
    [SerializeField] Button charSelectButton;
    [Tooltip("캐릭터 선택 이미지(버튼이 들어있는 이미지 / 에디터 할당 필요)")]
    [SerializeField] Image charListImage;
    [Tooltip("각각의 캐릭터 버튼(에디터 할당 필요)")]
    [SerializeField] List<Button> charButtons;
    [Tooltip("캐릭터 디테일한 설명 이미지(에디터 할당 필요)")]
    [SerializeField] Image charDetail;
    [Tooltip("선택한 캐릭터 이미지(에디터 할당 필요)")]
    [SerializeField] Image selectCharImage;
    [Tooltip("선택한 캐릭터 이름(에디터 할당 필요)")]
    [SerializeField] TMP_Text selectChatName;

    [Space(10)]
    [Header("CommonSkill Selecting Component")]
    [Tooltip("공용 스킬 선택 버튼(에디터 할당 필요)")]
    [SerializeField] List<Button> commonSkillButton;
    [Tooltip("공용 스킬 선택 이미지(버튼이 들어있는 이미지 / 에디터 할당 필요)")]
    [SerializeField] Image commonSkillListImage_First;
    [Tooltip("공용 스킬 선택 이미지(버튼이 들어있는 이미지 / 에디터 할당 필요)")]
    [SerializeField] Image commonSkillListImage_Second;


    public Action change;
    private void Awake()
    {

        charSelectButton.onClick.AddListener(ShowCharSelectImage);

        for (int i = 0; i < charButtons.Count; i++)
        {
            charButtons[i].onClick.AddListener(CloseCharSelectImage);
        }

        charButtons[0].onClick.AddListener(SelectWarrior);
        charButtons[1].onClick.AddListener(SelectKnight);
        charButtons[2].onClick.AddListener(SelectMagician);
        charButtons[3].onClick.AddListener(SelectArcher);

        commonSkillButton[0].onClick.AddListener(ShowFirstCommonSkillImage);
        commonSkillButton[1].onClick.AddListener(ShowSecondCommonSkillImage);

    }
    private void OnEnable()
    {
        if (PhotonNetwork.LocalPlayer.GetProperty<bool>(DefinePropertyKey.OUTGAME))
        {
            change += () => CharacterChange();
        }
    }
    private void OnDisable()
    {
        if (PhotonNetwork.LocalPlayer.GetProperty<bool>(DefinePropertyKey.OUTGAME))
        {
            change -= () => CharacterChange();
        }
    }
    #region CharSelecting
    private void ShowCharSelectImage()
    {   // 캐릭터 선택창 열기
        charListImage.gameObject.SetActive(true);
    }

    private void CloseCharSelectImage()
    {   // 캐릭터 선택창 닫기
        charListImage.gameObject.SetActive(false);
        charDetail.gameObject.SetActive(false);
    }

    /// <summary>
    /// 각각의 캐릭터 선택 버튼(포톤 프로퍼티로 할당)
    /// </summary>
    private void SelectWarrior()
    {
        PhotonNetwork.LocalPlayer.SetProperty<ClassType>(DefinePropertyKey.CHARACTERCLASS, ClassType.Warrior);
    }

    private void SelectKnight()
    {
        PhotonNetwork.LocalPlayer.SetProperty<ClassType>(DefinePropertyKey.CHARACTERCLASS, ClassType.Knight);
    }

    private void SelectMagician()
    {

        PhotonNetwork.LocalPlayer.SetProperty<ClassType>(DefinePropertyKey.CHARACTERCLASS, ClassType.Wizard);
    }

    private void SelectArcher()
    {
        PhotonNetwork.LocalPlayer.SetProperty<ClassType>(DefinePropertyKey.CHARACTERCLASS, ClassType.Archer);
    }
    #endregion


    private void ShowFirstCommonSkillImage()
    {
        if (commonSkillListImage_Second.gameObject.activeSelf)
        {
            commonSkillListImage_Second.gameObject.SetActive(false);
        }
        commonSkillListImage_First.gameObject.SetActive(true);
    }
    private void ShowSecondCommonSkillImage()
    {
        if (commonSkillListImage_First.gameObject.activeSelf)
        {
            commonSkillListImage_First.gameObject.SetActive(false);
        }
        commonSkillListImage_Second.gameObject.SetActive(true);
    }

    public async void CharacterChange()
    {
        Vector2 playerPos = GameManager.Ins.m_Player.transform.position;
        if(Inventory.Instance.isInventoryOpen)
        GameManager.Ins.m_Player.UIController.OpenInventoryUI();
        PhotonNetwork.Destroy(GameManager.Ins.m_Player.gameObject);
   /*     await Task.Run(() =>
        {
            while (GameManager.Ins.m_Player != null)
            {
                Thread.Sleep(100);
            }
        });*/

        switch (PhotonNetwork.LocalPlayer.GetProperty<ClassType>(DefinePropertyKey.CHARACTERCLASS))
        {
            case ClassType.Warrior:
                GameManager.Ins.m_Player = PhotonNetwork.Instantiate("6.Prefab/Character/Warrior", playerPos, Quaternion.identity).GetComponent<PlayerController>();
                break;
            case ClassType.Knight:
                GameManager.Ins.m_Player = PhotonNetwork.Instantiate("6.Prefab/Character/Knight", playerPos, Quaternion.identity).GetComponent<PlayerController>();
                break;
            case ClassType.Wizard:
                GameManager.Ins.m_Player = PhotonNetwork.Instantiate("6.Prefab/Character/Wizard", playerPos, Quaternion.identity).GetComponent<PlayerController>();
                break;
            case ClassType.Archer:
                GameManager.Ins.m_Player = PhotonNetwork.Instantiate("6.Prefab/Character/Archer", playerPos, Quaternion.identity).GetComponent<PlayerController>();
                break;
            default:
                GameManager.Ins.m_Player = PhotonNetwork.Instantiate("6.Prefab/Character/Knight", playerPos, Quaternion.identity).GetComponent<PlayerController>();
                break;
        }

        await Task.Run(() =>
        {
            while (GameManager.Ins.m_Player == null)
            {
                Task.Delay(100);
            }
        });
        
        GameManager.Ins.m_Player.photonView.RPC("RPC_EntryConnect", RpcTarget.AllBuffered, GameManager.Ins.m_Player.photonView.OwnerActorNr);

        Inventory.Instance.Player = GameManager.Ins.m_Player;
        GameManager.Ins.SetInventoryUI();
        GameManager.Ins.photonView.RPC("InitRPC", RpcTarget.All);

        FirebaseManager.Instance.curClass = PhotonNetwork.LocalPlayer.GetProperty<ClassType>(DefinePropertyKey.CHARACTERCLASS);
        //Inventory.Instance.FreshSlots();
        if (FirebaseManager.Instance.curClass != ClassType.Excalibur)
        {
            Inventory.Instance.DownLoadInventoryData();
        }
    }
}
