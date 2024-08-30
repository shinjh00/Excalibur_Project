using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 서보운
/// <br/> 전용 스킬에 대한 정보를 표현할 UI
/// </summary>
public class SkillButtons : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image skillDetailImage;

    [Tooltip("전용 스킬 대표 이미지 컴포넌트 할당 필요")]
    [SerializeField] Image detailImage;
    [Tooltip("전용 스킬 대표 설명 컴포넌트 할당 필요")]
    [SerializeField] TMP_Text detailName;
    [Tooltip("전용 스킬  자세한 설명1 컴포넌트 할당 필요")]
    [SerializeField] TMP_Text detailText1;
    [Tooltip("전용 스킬  자세한 설명2 컴포넌트 할당 필요")]
    [SerializeField] TMP_Text detailText2;

    [Tooltip("전용 스킬 이미지 할당")]
    [SerializeField] Sprite[] detail_Images;
    [Tooltip("전용 스킬 이름 할당")]
    [SerializeField] string[] detail_Names;
    [Tooltip("전용 스킬 설명 1")]
    [SerializeField] string[] detail_Text1s;
    [Tooltip("전용 스킬 설명 2")]
    [SerializeField] string[] detail_Text2s;

    [SerializeField] Sprite detail_Image;
    [SerializeField] string detail_Name;
    [SerializeField] string detail_Text1;
    [SerializeField] string detail_Text2;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(SetMainChar);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetDetail();

        detailImage.sprite = detail_Image;
        detailName.text = detail_Name;
        detailText1.text = detail_Text1;
        detailText2.text = detail_Text2;

        skillDetailImage.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        skillDetailImage.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        skillDetailImage.gameObject.SetActive(false);
    }

    private void SetDetail()
    {
        ClassType curClass = PhotonNetwork.LocalPlayer.GetProperty<ClassType>(DefinePropertyKey.CHARACTERCLASS);

        detail_Image = detail_Images[(int)curClass];
        detail_Name = detail_Names[(int)curClass];
        detail_Text1 = detail_Text1s[(int)curClass];
        detail_Text2 = detail_Text2s[(int)curClass];
    }

    private void SetMainChar()
    {
        //selectCharImage.sprite = detail_Image;
        //selectChatName.text = detail_Name;
    }
}
