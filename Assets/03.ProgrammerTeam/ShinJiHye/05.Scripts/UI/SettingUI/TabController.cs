using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 신지혜 / 설정창 메뉴 탭 컨트롤러
/// </summary>
public class TabController : MonoBehaviour
{
    [SerializeField] Tab[] tabs;
    [SerializeField] GameObject[] panels;

    private void OnEnable()
    {
        SwitchTab(tabs[0]);
    }

    private void Start()
    {
        SwitchTab(tabs[0]);
    }

    /// <summary>
    /// 탭 바꾸는 함수
    /// </summary>
    /// <param name="targetTab"></param>
    public void SwitchTab(Tab targetTab)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            bool isActiveTab = targetTab == tabs[i];  // tabs[i]가 눌려진 버튼인지 판단
            tabs[i].TabBtn_pressed.gameObject.SetActive(isActiveTab);  // pressed 버튼 : true면 켜고 false면 끄기
            tabs[i].TabBtn_normal.gameObject.SetActive(!isActiveTab);  // normal 버튼 : false면 켜고 true면 끄기
            panels[i].gameObject.SetActive(isActiveTab);  // panel : true면 켜고 false면 끄기
        }
    }
}
