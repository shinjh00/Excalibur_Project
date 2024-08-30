using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 서보운
/// <br/> 준비 완료 버튼
/// </summary>
public class SelectionCompleteButton : MonoBehaviour
{
    [SerializeField] MaintenanceController controller;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Ready);
    }

    private void Ready()
    {
        if (PhotonNetwork.LocalPlayer.GetProperty<bool>(DefinePropertyKey.OUTGAME))
        {
            controller.change?.Invoke();
        }

        PhotonNetwork.LocalPlayer.SetProperty<bool>(DefinePropertyKey.SELECTECOMPLETE, true);
        controller.gameObject.SetActive(false);
    }

}
