using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 서보운
/// <br/>계정 생성용 UI 스크립트
/// </summary>
public class SignUpPanel : MonoBehaviour
{
    [SerializeField] PanelController panelController;

    [SerializeField] TMP_InputField loginIDInputField;
    [SerializeField] TMP_InputField passInputField;
    [SerializeField] TMP_InputField confirmInputField;

    [SerializeField] Button cancelButton;
    [SerializeField] Button signUpButton;

    bool isCheck;

    private void Awake()
    {
        signUpButton.onClick.AddListener(SignUp);
        cancelButton.onClick.AddListener(Cancel);

        isCheck = false;
    }

    /// <summary>
    /// 회원 가입용 메소드
    /// </summary>
    public void SignUp()
    {
        SetInteractable(false);

        string loginID = loginIDInputField.text;
        string password = passInputField.text;
        string confirm = confirmInputField.text;

        if (password != confirm)
        {   // 일치하지 않으면
            panelController.ShowInfo("Password doesn't matched.");
            SetInteractable(true);
            return;
        }

        if (password.Length < 8 || password.Length > 16)
        {   // 너무 짧거나(8자 미만), 너무 길 때(16자 초과)
            panelController.ShowInfo("Password Too Short or Too Long.");
            SetInteractable(true);
            return;
        }

        Regex regexPass = new Regex(@"^(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{9,}$", RegexOptions.IgnorePatternWhitespace);
        isCheck = regexPass.IsMatch(password);

        if (!isCheck)
        {   // 영문자, 숫자, 특수문자중 하나라도 포함하지 않았다면
            panelController.ShowInfo("Password must contain one of numbers, alphabetic characters, or special characters.");
            SetInteractable(true);
            return;
        }

        if (FirebaseManager.Instance.LoginIDs.loginIDs.Contains(loginID))
        {   // 만약 로그인 ID가 중복된다면
            panelController.ShowInfo("This ID already exists");
            SetInteractable(true);
            return;
        }

        // 새로운 데이터 생성
        PlayerDataTable newPlayer = new PlayerDataTable();

        newPlayer.SetNewPlayer(loginID, password);

        FirebaseManager.Instance.UpLoadData.UpLoadStructData(newPlayer);
        FirebaseManager.Instance.LoginIDs.loginIDs.Add(newPlayer.LoginID);
        FirebaseManager.Instance.LoginIDs.passwords.Add(newPlayer.password);
        FirebaseManager.Instance.LoginIDs.IDs.Add(newPlayer.ID.ToString());
        FirebaseManager.Instance.UpLoadLoginList();

        FirebaseManager.Instance.Link.PlayerData.Add(newPlayer.ID, newPlayer);
        panelController.ShowInfo("ID Create Success");
        panelController.InfoPanel.AddCloseListener(PanelController.Panel.Login);
        FirebaseManager.Instance.UpLoadData.FirstUpLoadData(newPlayer.ID);
        SetInteractable(true);

        gameObject.SetActive(false);
    }

    public void Cancel()
    {
        panelController.SetActivePanel(PanelController.Panel.Login);
    }

    private void SetInteractable(bool interactable)
    {
        loginIDInputField.interactable = interactable;
        passInputField.interactable = interactable;
        confirmInputField.interactable = interactable;
        cancelButton.interactable = interactable;
        signUpButton.interactable = interactable;
    }
}
