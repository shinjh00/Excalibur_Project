using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 신지혜 / 설정 창 IU
/// </summary>
public class SettingUI : PopUpUI
{
    [Tooltip("Resources > 8.UI > InGame에 있는 Setting 프리팹 할당")]
    [SerializeField] GameObject settingPrefab;

    [Tooltip("SettingButton 할당")]
    [SerializeField] Button settingButton;
    [Tooltip("SettingButton_normal 할당")]
    [SerializeField] GameObject settingButton_normal;
    [Tooltip("SettingButton_pressed 할당")]
    [SerializeField] GameObject settingButton_pressed;

    Setting setting;
    RectTransform settingRect;
    bool isOpen = false;

    public Button SettingButton { get { return settingButton; } }

    protected override void Awake()
    {
        // Bind() 사용 안함
        settingButton_normal.SetActive(!isOpen);
        settingButton_pressed.SetActive(isOpen);
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => (Inventory.Instance != null && Inventory.Instance.Player != null));
        Inventory.Instance.Player.UIController.settingUI = this;
        settingButton.onClick.AddListener(OpenSetting);
    }

    /// <summary>
    /// 설정 창 열기
    /// </summary>
    public void OpenSetting()
    {
        settingButton_normal.SetActive(isOpen);
        settingButton_pressed.SetActive(!isOpen);

        if (setting == null && isOpen == false)
        {
            // setting 생성
            GameObject _setting = ShowPopUpUI(settingPrefab);
            _setting.transform.parent = transform;
            setting = _setting.GetComponent<Setting>();
            settingRect = setting.GetComponent<RectTransform>();
            settingRect.anchoredPosition = Vector3.zero;

            // 생성할 때 텍스트, 텍스트 효과 적용
            setting.SetTextSetting();

            Inventory.Instance.Player.StateController.StateChange(PlayerState.Maintain, 0, 0, true, false);

        }
        else
        {
            ClosePopUpUI();
            Inventory.Instance.Player.StateController.StateChange(PlayerState.Maintain, 0, 0, false, false);
        }

        isOpen = !isOpen;
    }
    
}
