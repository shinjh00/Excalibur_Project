using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 서보운
/// <br/> 캐릭터 선택 버튼에 할당
/// </summary>
public class CharButtons : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image charDetailImage;

    [Tooltip("캐릭터 대표 이미지 컴포넌트 할당 필요")]
    [SerializeField] Image detailImage;
    [Tooltip("캐릭터 대표 설명 컴포넌트 할당 필요")]
    [SerializeField] TMP_Text detailName;
    [Tooltip("캐릭터 자세한 설명1 컴포넌트 할당 필요")]
    [SerializeField] TMP_Text detailText1;
    [Tooltip("캐릭터 자세한 설명2 컴포넌트 할당 필요")]
    [SerializeField] TMP_Text detailText2;

    [Tooltip("선택한 캐릭터 이미지(에디터 할당 필요)")]
    [SerializeField] Image selectCharImage;
    [Tooltip("선택한 캐릭터 이름(에디터 할당 필요)")]
    [SerializeField] TMP_Text selectChatName;

    [Tooltip("캐릭터 대표 이미지 할당")]
    [SerializeField] Sprite detail_Image;
    [Tooltip("캐릭터 이름 할당")]
    [SerializeField] string detail_Name;
    [Tooltip("캐릭터 설명 1")]
    [SerializeField] string detail_Text1;
    [Tooltip("캐릭터 설명 2")]
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

        charDetailImage.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        charDetailImage.gameObject.SetActive(false);
    }

    private void SetMainChar()
    {
        selectCharImage.sprite = detail_Image;
        selectChatName.text = detail_Name;
    }
}
