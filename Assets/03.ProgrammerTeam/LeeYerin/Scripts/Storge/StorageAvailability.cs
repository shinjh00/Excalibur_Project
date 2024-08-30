using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

/// <summary>
/// 개발자: 이예린 / 창고 슬롯의 드롭 가능 여부 시각화 관련 클래스 (슬롯 아이템 부모 오브젝트인 슬롯에 할당)
/// </summary>
public class StorageAvailability : Availability
{
    [SerializeField] StoragePage storagePage;
    StorageSlot storageSlot;

    #region Unity Events
    protected override void Start()
    {
        base.Start();
        storageSlot = slot as StorageSlot;
        storagePage = storageSlot.storagePage;
    }
    #endregion

    #region Availability
    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (!Inventory.Instance.StoragePage.isOpend)
        {
            return;
        }

        if (Inventory.Instance.DraggingSlot == null
            || Inventory.Instance.DraggingSlot.Type.Equals(SlotType.Quick))
        {
            return;
        }

        Inventory.Instance.PointOnSlot = storageSlot;

        if (storagePage == null)
        {
            return;
        }

        if (storagePage.CheckItemDropable(storageSlot.Row, storageSlot.Col, Inventory.Instance.DraggingSlot))
        {
            canDrop = false;
            storagePage.ShowAvailability(true, storageSlot, Color.green);
        }
        else
        {
            if (!Inventory.Instance.DraggingSlot.Type.Equals(SlotType.Equipment) && storageSlot.Item != null)
            {
                // 상점에서 구매 시도일 경우
                if (Inventory.Instance.DraggingSlot.Type.Equals(SlotType.Store))
                {
                    storagePage.ShowAvailability(false, slot, Color.red);
                    return;
                }

                // 만약 인벤토리 아이템 박스로 같은 크기의 아이템 위치 서로 바꾸기 시도일 경우
                if (Inventory.Instance.DraggingSlot.Item.Width == storageSlot.Item.Width
                    && Inventory.Instance.DraggingSlot.Item.Height == storageSlot.Item.Height)
                {
                    canDrop = false;
                    storagePage.ShowAvailability(true, storageSlot, Color.green);
                    return;
                }
            }
            else if (Inventory.Instance.DraggingSlot.Type.Equals(SlotType.Equipment) && storageSlot.Item != null)
            {
                if (!storageSlot.Item.ItemType.Equals(ItemBaseType.Wearable))  // 드롭하고자 하는 위치에 있는 아이템이 장비 아이템이 아닐 경우
                {
                    storagePage.ShowAvailability(false, storageSlot, Color.red);
                    return;
                }

                // 만약 장비 슬롯에서 가방 슬롯으로 같은 종류의 아이템 위치 서로 바꾸기 시도일 경우
                if (Inventory.Instance.DraggingSlot.WearableItem.WearablesType.Equals(storageSlot.WearableItem.WearablesType))
                {
                    canDrop = false;
                    storagePage.ShowAvailability(true, storageSlot, Color.green);
                    return;
                }
            }

            storagePage.ShowAvailability(false, storageSlot, Color.red);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (!Inventory.Instance.StoragePage.isOpend)
        {
            return;
        }

        if (Inventory.Instance.DraggingSlot == null
            || Inventory.Instance.DraggingSlot.Type.Equals(SlotType.Quick))
        {
            return;
        }

        if (!canDrop)
        {
            canDrop = true;
            storagePage.ShowAvailability(true, storageSlot, Color.white);
        }
        else
        {
            storagePage.ShowAvailability(false, storageSlot, Color.white);
        }
    }
    #endregion
}
