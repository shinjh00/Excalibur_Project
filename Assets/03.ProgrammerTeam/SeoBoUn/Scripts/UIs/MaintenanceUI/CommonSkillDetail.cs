using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 서보운
/// <br/> 공용스킬 정보 표시용 UI
/// </summary>
public class CommonSkillDetail : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image commonDetailImage;

    [Tooltip("공용 스킬 대표 이미지 컴포넌트 할당 필요")]
    [SerializeField] Image detailImage;
    [Tooltip("공용 스킬 대표 설명 컴포넌트 할당 필요")]
    [SerializeField] TMP_Text detailName;
    [Tooltip("공용 스킬  자세한 설명1 컴포넌트 할당 필요")]
    [SerializeField] TMP_Text detailText1;
    [Tooltip("공용 스킬  자세한 설명2 컴포넌트 할당 필요")]
    [SerializeField] TMP_Text detailText2;

    [Tooltip("공용 스킬 이미지 할당")]
    [SerializeField] Sprite detail_Image;
    [Tooltip("공용 스킬 이름 할당")]
    [SerializeField] string detail_Name;
    [Tooltip("공용 스킬 설명 1")]
    [SerializeField] string detail_Text1;
    [Tooltip("공용 스킬 설명 2")]
    [SerializeField] string detail_Text2;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(SetMainChar);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        detailImage.sprite = detail_Image;
        detailName.text = detail_Name;
        detailText1.text = detail_Text1;
        detailText2.text = detail_Text2;

        commonDetailImage.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        commonDetailImage.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        commonDetailImage.gameObject.SetActive(false);
    }

    private void SetMainChar()
    {
        //selectCharImage.sprite = detail_Image;
        //selectChatName.text = detail_Name;
    }
}
