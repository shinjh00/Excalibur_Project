using Firebase.Database;
using Firebase.Extensions;
using Photon.Pun;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 서보운
/// <br/>로그인 패널 스크립트
/// </summary>
public class LoginPanel : MonoBehaviour
{
    [SerializeField] PanelController panelController;

    [SerializeField] TMP_InputField IDInputField;
    [SerializeField] TMP_InputField passInputField;

    [SerializeField] Button signUpButton;
    [SerializeField] Button loginButton;

    bool isCheck = false;

    private void Awake()
    {

        signUpButton.onClick.AddListener(SignUp);
        loginButton.onClick.AddListener(Login);
    }

    public void SignUp()
    {   // 회원 가입 버튼이 눌렸을 때 실행될 메소드
        panelController.SetActivePanel(PanelController.Panel.SignUp);
    }

    public void Login()
    {   // 이메일과 비밀번호를 가지고 로그인 진행
        SetInteractable(false);

        string ID = IDInputField.text;
        string password = passInputField.text;
        
        if(!FirebaseManager.Instance.LoginIDs.loginIDs.Contains(ID))
        {   // ID 리스트에 없다면 계정 미존재
            panelController.ShowInfo("Login Fail");
            SetInteractable(true);
        }

        int index = FirebaseManager.Instance.LoginIDs.loginIDs.IndexOf(ID);
        string accountID = FirebaseManager.Instance.LoginIDs.IDs[index];
        string accountPassword = FirebaseManager.Instance.LoginIDs.passwords[index];

        if (accountPassword == password)
        {
            isCheck = true;
        }
        else
        {
            isCheck = false;
        }

        if (!isCheck)
        {
            panelController.ShowInfo("Login Fail");
            SetInteractable(true);
        }

        panelController.ShowInfo("Login Success");
        SetInteractable(true);

        FirebaseManager.Instance.curPlayer = accountID;
        FirebaseManager.Instance.curClass = PhotonNetwork.LocalPlayer.GetProperty<ClassType>(DefinePropertyKey.CHARACTERCLASS);
        
        FirebaseManager.Instance.Link.LoginUser(accountID);
        FirebaseManager.Instance.DownLoadStore_StorageData();
        GetData();

        PhotonNetwork.ConnectUsingSettings();
        gameObject.SetActive(false);
        return;
    }

    private void SetInteractable(bool interactable)
    {
        IDInputField.interactable = interactable;
        passInputField.interactable = interactable;
        signUpButton.interactable = interactable;
        loginButton.interactable = interactable;
    }

    private void GetData()
    {
        FirebaseManager.DB
            .GetReference("PlayerDataTable")
            .Child(FirebaseManager.Instance.curPlayer)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    return;
                }
                else if (task.IsFaulted)
                {
                    return;
                }

                DataSnapshot snapShot = task.Result;

                if (snapShot.Exists)
                {
                    IDictionary DataTable = (IDictionary)snapShot.Value;
                    FirebaseManager.Instance.curGold = Convert.ToInt32(DataTable["havingGold"]);
                    PhotonNetwork.LocalPlayer.NickName = $"{(string)DataTable["nickname"]}";
                    Debug.Log($"LoginName : {PhotonNetwork.LocalPlayer.NickName}");
                }
            });
    }
}
