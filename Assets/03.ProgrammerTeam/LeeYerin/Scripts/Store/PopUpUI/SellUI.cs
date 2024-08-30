using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자: 이예린/ 상점 판매 UI 관련 클래스
/// </summary>
public class SellUI : MonoBehaviour
{
    [SerializeField] Button sellButton;
    [SerializeField] Button noSellButton;
    [SerializeField] TMP_Text currentCahText;
    [SerializeField] TMP_Text itemPriceText;
    [SerializeField] TMP_Text afterCashText;
    [SerializeField] GameObject ui;

    public GameObject UI => ui;

    public Action<ItemData> curItem;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => (Inventory.Instance != null && Inventory.Instance.Store != null
        && Inventory.Instance.Store.CashPage != null));

        Inventory.Instance.Store.CashPage.sellPopUpUI = this;
        sellButton.onClick.AddListener(Inventory.Instance.Store.CashPage.SellItemSuccess);
        noSellButton.onClick.AddListener(Inventory.Instance.Store.CashPage.SellItemFail);

        curItem += Inventory.Instance.StorePage.TargetItem;
        curItem += ActionDebug;
    }

    public void SetSellUI()
    {
        Debug.Log("Sell UI");
        curItem?.Invoke(Inventory.Instance.StorePage.ItemDropSlot.Item);
        itemPriceText.text = $"아이템 가격 : {Inventory.Instance.StorePage.ItemDropSlot.Item.SellPrice}";
    }

    public void ActionDebug(ItemData itemData)
    {
        Debug.Log("액션 실행");
        Debug.Log($"{itemData.Name}");
    }
}
