using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PlayerStatController;

/// <summary>
/// 개발자: 이예린/ 상점 구매 UI 관련 클래스
/// </summary>
public class BuyUI : MonoBehaviour
{
    [SerializeField] Button buyButton;
    [SerializeField] Button noBuyButton;
    [SerializeField] TMP_Text question;
    [SerializeField] TMP_Text currentCahText;
    [SerializeField] TMP_Text itemPriceText;
    [SerializeField] TMP_Text afterCashText;
    [SerializeField] GameObject ui;

    public Button BuyButton => buyButton;
    public Button NoBuyButton => noBuyButton;
    public GameObject UI => ui;

    public Action<ItemData> curItem;
   

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => (Inventory.Instance != null && Inventory.Instance.Store != null 
        && Inventory.Instance.Store.WearablePage != null && Inventory.Instance.Store.ConsumablePage != null 
        && Inventory.Instance.Store.CashPage != null));

        Inventory.Instance.Store.WearablePage.buyPopUpUI = this;
        Inventory.Instance.Store.ConsumablePage.buyPopUpUI = this;
        Inventory.Instance.Store.CashPage.buyPopUpUI = this;

        curItem += Inventory.Instance.StorePage.TargetItem;




    }
    public void SetBuyUI()
    {
        Slot dragSlot = Inventory.Instance.DraggingSlot;
        curItem?.Invoke(dragSlot.Item);
        switch (dragSlot.Item.ItemType)
        {
            case ItemBaseType.Consumable:
                itemPriceText.text = $"아이템 가격 : {dragSlot.ConsumableItem.BuyPrice}";
                break;
            case ItemBaseType.Wearable:
                itemPriceText.text = $"아이템 가격 : {dragSlot.WearableItem.BuyPrice}";
                break;
        }

        switch (Inventory.Instance.StorePage.PageType)
        {
            case ItemBaseType.Cash:
                question.text = "아이템을 재구매하시겠습니까?";
                break;
            default:
                question.text = "아이템을 구매하시겠습니까?";
                break;
        }

        buyButton.onClick.AddListener(Inventory.Instance.StorePage.BuyItemSuccess);
        noBuyButton.onClick.AddListener(Inventory.Instance.StorePage.BuyItemFail);
    }
}