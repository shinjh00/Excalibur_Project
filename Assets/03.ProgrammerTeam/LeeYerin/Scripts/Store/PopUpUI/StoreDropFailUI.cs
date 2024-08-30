using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreDropFailUI : MonoBehaviour
{
    public TMP_Text dropFailText;
    [SerializeField] Button closePopUpButton;
    [SerializeField] GameObject ui;

    public GameObject UI => ui;
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => (Inventory.Instance != null && Inventory.Instance.Store != null));

        Inventory.Instance.Store.DropFailPopUpUI = this;
        closePopUpButton.onClick.AddListener(Inventory.Instance.Store.PopUpClose);
    }
}