using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 신지혜 / 팝업창 제어 스크립트
/// </summary>
public class PopUpUI : BaseUI
{
    GameObject instance;

    [Tooltip("InGameCanvas 내에 있는 PopUpCanvas 할당")]
    [SerializeField] Canvas popUpCanvas;

    [Tooltip("PopUpCanvas 하위에 있는 PopUpBlocker 할당")]
    [SerializeField] Image popUpBlocker;

    /// <summary>
    /// 팝업창 열기
    /// </summary>
    /// <param name="targetUI"> 열게 될 팝업창 </param>
    /// <returns></returns>
    public GameObject ShowPopUpUI(GameObject targetUI)
    {
        if (instance != null)
        {
            Destroy(instance);
        }
        else
        {
            instance = Instantiate(targetUI);
            popUpBlocker.gameObject.SetActive(true);
        }

        //Time.timeScale = 0f;

        return instance;
    }

    /// <summary>
    /// 팝업창 닫기
    /// </summary>
    public void ClosePopUpUI()
    {
        if (instance == null)
        {
            return;
        }
        else
        {
            Destroy(instance);
            popUpBlocker.gameObject.SetActive(false);
        }

        //Time.timeScale = 1f;
    }
}
