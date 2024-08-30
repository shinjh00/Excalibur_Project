using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 서보운
/// <br/>정보 표시용 창 스크립트
/// </summary>
public class InfoPanel : MonoBehaviour
{
    [SerializeField] TMP_Text infoText;
    [SerializeField] Button closeButton;
    [SerializeField] PanelController panelController;

    UnityAction closeAction;

    public Button CloseButton { get { return closeButton; } set { closeButton = value; } }
    private void Awake()
    {
        closeAction = Close;
        closeButton.onClick.AddListener(Close);
    }

    public void ShowInfo(string message)
    {
        gameObject.SetActive(true);
        infoText.text = message;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
    public void RemoveCloseListener()
    {
        closeButton.onClick.RemoveListener(closeAction);
    }

    public void AddCloseListener(PanelController.Panel targetPanel)
    {
        UnityAction closeListener = null;
        closeListener = () =>
        {
            panelController.SetActivePanel(targetPanel);
            panelController.InfoPanel.CloseButton.onClick.RemoveListener(closeListener);
        };
        panelController.InfoPanel.CloseButton.onClick.AddListener(closeListener);
    }
}
