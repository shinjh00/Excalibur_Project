using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Store_WarehouseImage : MonoBehaviour
{
    [Tooltip("뒤로가기 버튼(에디터 할당 필요)")]
    [SerializeField] Button backButton;

    // GameObject mainPanelBlockImage;

    private void Awake()
    {
        backButton.onClick.AddListener(BackButton);

        // mainPanelBlockImage = GetComponentInParent<MainPanel>().MainPanelBlockImage;
    }

    private void BackButton()
    {
        gameObject.SetActive(false);
        // mainPanelBlockImage.SetActive(false);
    }
}
