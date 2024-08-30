using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 신지혜 / 설정창 메뉴 탭
/// </summary>
public class Tab : MonoBehaviour
{
    [SerializeField] Image tabBtn_normal;
    [SerializeField] Image tabBtn_pressed;

    TabController tabController;

    public Image TabBtn_normal { get { return tabBtn_normal; } }
    public Image TabBtn_pressed { get { return tabBtn_pressed; } }

    private void Start()
    {
        tabController = GetComponentInParent<TabController>();
    }

    public void SwitchTab()
    {
        tabController.SwitchTab(this);
    }
}
