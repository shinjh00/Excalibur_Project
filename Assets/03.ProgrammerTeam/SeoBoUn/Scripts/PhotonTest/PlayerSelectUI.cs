using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 서보운
/// <br/>직업 선택 UI
/// </summary>
public class PlayerSelectUI : MonoBehaviour
{
    [SerializeField] Button warriorButton;
    [SerializeField] Button knightButton;
    [SerializeField] Button magicianButton;
    [SerializeField] Button archerButton;

    Action CloseEvent;

    private void Awake()
    {
        warriorButton.onClick.AddListener(SelectWarrior);
        knightButton.onClick.AddListener(SelectKnight);
        magicianButton.onClick.AddListener(SelectMagician);
        archerButton.onClick.AddListener(SelectArcher);
    }

    public void AddCloseEvent(Action CloseEventMethod)
    {
        CloseEvent += CloseEventMethod;
    }

    /// <summary>
    /// 로컬 플레이어의 클래스 타입을 설정하는 메소드들
    /// </summary>
    private void SelectWarrior()
    {
        PhotonNetwork.LocalPlayer.SetProperty<ClassType>(DefinePropertyKey.CHARACTERCLASS, ClassType.Warrior);
        this.gameObject.SetActive(false);
    }

    private void SelectKnight()
    {
        PhotonNetwork.LocalPlayer.SetProperty<ClassType>(DefinePropertyKey.CHARACTERCLASS, ClassType.Knight);
        this.gameObject.SetActive(false);
    }

    private void SelectMagician()
    {
        PhotonNetwork.LocalPlayer.SetProperty<ClassType>(DefinePropertyKey.CHARACTERCLASS, ClassType.Wizard);
        this.gameObject.SetActive(false);
    }

    private void SelectArcher()
    {
        PhotonNetwork.LocalPlayer.SetProperty<ClassType>(DefinePropertyKey.CHARACTERCLASS, ClassType.Archer);
        this.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        CloseEvent?.Invoke();
    }
}
