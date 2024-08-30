using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 개발자: 이예린 / 상점 슬롯의 드롭 가능 여부 시각화 관련 클래스 (슬롯 아이템 부모 오브젝트인 슬롯에 할당)
/// </summary>
public class StoreAvailability : Availability
{
    [SerializeField] StorePage storePage;
    StoreSlot storeSlot;

    #region Unity Events
    protected override void Start()
    {
        base.Start();
        storeSlot = slot as StoreSlot;
        storePage = storeSlot.storePage;
    }
    #endregion

    #region Availability
    public override void OnPointerEnter(PointerEventData eventData)
    {
        /*if (!Inventory.Instance.StoragePage.isOpend)
        {
            return;
        }*/

        if (Inventory.Instance.DraggingSlot == null
            || Inventory.Instance.DraggingSlot.Type.Equals(SlotType.Quick))
        {
            return;
        }

        Inventory.Instance.PointOnSlot = storeSlot;

        if (storePage == null)
        {
            return;
        }

        if (!storePage.PageType.Equals(ItemBaseType.Cash))
        {
            storePage.ShowAvailability(false, storeSlot, Color.red);
            return;
        }

        // 팔 수 없는 아이템일 경우
        if (Inventory.Instance.DraggingSlot.Item.SellPrice == -9999)
        {
            storePage.ShowAvailability(false, storeSlot, Color.red);
            return;
        }

        if (storePage.CheckItemDropable(storeSlot.Row, storeSlot.Col, Inventory.Instance.DraggingSlot))
        {
            // 상점 슬롯은 상점 안에서 재배치 불가능
            if (Inventory.Instance.DraggingSlot.Type.Equals(SlotType.Store))
            {
                storePage.ShowAvailability(false, storeSlot, Color.red);
                return;
            }

            canDrop = false;
            storePage.ShowAvailability(true, storeSlot, Color.green);
        }
        else
        {
            storePage.ShowAvailability(false, storeSlot, Color.red);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        /*if (!Inventory.Instance.StoragePage.isOpend)
        {
            return;
        }*/

        if (Inventory.Instance.DraggingSlot == null
            || Inventory.Instance.DraggingSlot.Type.Equals(SlotType.Quick))
        {
            return;
        }

        if (!canDrop)
        {
            canDrop = true;
            storePage.ShowAvailability(true, storeSlot, Color.white);
        }
        else
        {
            storePage.ShowAvailability(false, storeSlot, Color.white);
        }
    }
    #endregion
}
