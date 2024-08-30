using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 신지혜
/// </summary>
public class SkillUI : BaseUI
{
    [Tooltip("스킬 버튼 이미지(에디터 할당 필요)")]
    [SerializeField] Image[] images;
    [Tooltip("스킬 쿨타임 텍스트(에디터 할당 필요)")]
    [SerializeField] TextMeshProUGUI[] skillCoolTimeText;
    [SerializeField] Color coolTimeColor;
    [SerializeField] Color normalColor;

    private void Start()
    {
        coolTimeColor = new Color(0.6f, 0.6f, 0.6f, 0.6f);
        normalColor = new Color(1f, 1f, 1f, 1f);
    }

    /// <summary>
    /// 스킬 쿨타임을 변경시키기 위한 메소드
    /// </summary>
    /// <param name="curType"></param>
    /// <param name="curCoolTime"></param>
    public void SkillCoolTime_UI(SkillType curType, int curCoolTime)
    {   // 나중에 SkillController에 SkillCoolTime 코루틴과 연계 필요함.
        if(curCoolTime > 0)
        {
            images[(int)curType].color = coolTimeColor;
            skillCoolTimeText[(int)curType].text = $"{curCoolTime}";
        }
        else
        {
            images[(int)curType].color = normalColor;
            skillCoolTimeText[(int)curType].text = $"";
        }
    }

    /// <summary>
    /// 스킬의 타입
    /// <br/> 공용1, 2 / 전용1, 2, 3
    /// </summary>
    public enum SkillType
    {
        Common1,
        Common2,
        General1,
        General2,
        General3
    }
}
