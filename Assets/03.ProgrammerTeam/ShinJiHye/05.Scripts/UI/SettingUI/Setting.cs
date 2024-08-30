using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 신지혜
/// 설정(Setting)창을 열면 생성되는 프리팹
/// 설정창 내에 있는 오브젝트와 컴포넌트 전달 & 텍스트 적용
/// </summary>
public class Setting : MonoBehaviour
{
    [SerializeField] ApplyTextData applyTextData;
    
    [Header("Title 할당")]
    [SerializeField] TextMeshProUGUI titleText;

    [Header("Tabs의 Text 할당")]
    [SerializeField] TextMeshProUGUI[] tabsText;

    [Header("Panels > General의 Text 할당")]
    [SerializeField] TextMeshProUGUI[] generalPanelsText;
    [Header("Panels > Sound의 Text 할당")]
    [SerializeField] TextMeshProUGUI[] soundPanelsText;
    [Header("Panels > KeySetting의 Text 할당")]
    [SerializeField] TextMeshProUGUI[] keysettingPanelsText;

    private void Awake()
    {
        applyTextData = GetComponent<ApplyTextData>();
    }

    private void Start()
    {
        SetTextSetting();
    }

    /// <summary>
    /// 설정창 텍스트 테이블 적용
    /// </summary>
    public void SetTextSetting()
    {
        SetTextSettingTitle();
        SetTextSettingTabs();
        SetTextGeneralPanel();
        SetTextSoundPanel();
        SetTextKeySettingPanel();
    }

    private void SetTextSettingTitle()
    {
        applyTextData.ApplyText(titleText, 1520006);
    }

    private void SetTextSettingTabs()
    {
        // Tabs
        applyTextData.ApplyText(tabsText[0], 1520021);
        applyTextData.ApplyText(tabsText[1], 1520021);

        applyTextData.ApplyText(tabsText[2], 1520022);
        applyTextData.ApplyText(tabsText[3], 1520022);

        applyTextData.ApplyText(tabsText[4], 1520024);
        applyTextData.ApplyText(tabsText[5], 1520024);
    }

    private void SetTextGeneralPanel()
    {
        // General Panel
        applyTextData.ApplyText(generalPanelsText[0], 1520025);
        applyTextData.ApplyText(generalPanelsText[1], 1520026);
    }

    private void SetTextSoundPanel()
    {
        // Sound Panel
        applyTextData.ApplyText(soundPanelsText[0], 1520028);
        applyTextData.ApplyText(soundPanelsText[1], 1520029);
        applyTextData.ApplyText(soundPanelsText[2], 1520030);
    }

    private void SetTextKeySettingPanel()
    {
        // KeySetting Panel
        int firstID = 1520033;
        for (int i = 0; i < keysettingPanelsText.Length; i++)
        {
            applyTextData.ApplyText(keysettingPanelsText[i], firstID);
            firstID++;
        }
    }
}
