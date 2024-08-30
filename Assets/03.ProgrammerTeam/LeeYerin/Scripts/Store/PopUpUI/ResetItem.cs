using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PlayerStatController;

/// <summary>
/// 개발자: 이예린 / 상점 아이템 리셋 관련 기능 구현 클래스
/// </summary>
public class ResetItem : MonoBehaviour
{
    [SerializeField] Button resetButton;
    [SerializeField] Button noResetButton;
    [SerializeField] TMP_Text costText; 
    [SerializeField] GameObject ui;
    [SerializeField] float cost;
    [SerializeField] int resetCount = 0;

    private void Start()
    {
        resetButton.onClick.AddListener(ResetItemSuccess);
        noResetButton.onClick.AddListener(ResetItemFail);
    }

    #region Reset items
    public void TryResetItem()
    {
        if (!Inventory.Instance.Store.canReset)
        {
            // TODO... 소모 금액 텍스트 
            ui.SetActive(true);
        }
    }
    public void ResetItemSuccess()
    {
        if (Inventory.Instance.Store.wearableSpwner == null || Inventory.Instance.Store.consumableSpwner == null)
        {
            Debug.Log("할당 안 된 상점 페이지 아이템 스폰너 존재");
            return;
        }

        if (!Inventory.Instance.Store.canReset)
        {
            StopCoroutine(Inventory.Instance.Store.resetTimer);
            Inventory.Instance.Store.timeCount = 0;
        }

        if (Inventory.Instance.DraggingSlot != null && Inventory.Instance.DraggingSlot.Type.Equals(SlotType.Store))
        {
            Inventory.Instance.DraggingSlot.SlotBehaviorOnClose();
        }

        Inventory.Instance.Store.StartItemResetTimer();

        Inventory.Instance.Store.wearableSpwner.SpawnItemInStore();
        Inventory.Instance.Store.wearableSpwner.SetItem();
        Inventory.Instance.Store.consumableSpwner.SpawnItemInStore();
        Inventory.Instance.Store.consumableSpwner.SetItem();
        Inventory.Instance.Store.cashSpwner.SpawnItemInStore();
        Inventory.Instance.Store.cashSpwner.SetItem();

        resetCount++;

        ui.SetActive(false);
    }

    public void ResetItemFail()
    {
        ui.SetActive(false);
    }
    #endregion
}