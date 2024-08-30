using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// <br/>플레이어가 직업과 공용 스킬을 선택할 때 이용할 컨트롤러  
/// </summary>
public class PlayerSelectController : MonoBehaviour
{
    [SerializeField] PlayerSelectUI playerSelectUI;
    [SerializeField] CommonSkillSelectUI skillSelectUI;

    private void Awake()
    {
        SetClass();
    }

    /// <summary>
    /// 클래스 선택 창에 대한 메소드 지정 및 활성화
    /// </summary>
    public void SetClass()
    {
        playerSelectUI.gameObject.SetActive(true);
        playerSelectUI.AddCloseEvent(SetCommonSkill);
    }

    /// <summary>
    /// 공용스킬 선택 창 활성화
    /// </summary>
    public void SetCommonSkill()
    {
        skillSelectUI.gameObject.SetActive(true);
    }
}
