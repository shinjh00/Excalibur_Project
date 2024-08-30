using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 개발자: 이예린 / 아이템 박스 슬롯의 드롭 가능 여부 시각화 관련 클래스 (슬롯 아이템 부모 오브젝트인 슬롯에 할당)
/// </summary>
public class ItemBoxAvailability : Availability
{
    ItemBoxSlot itemBoxSlot;
    [SerializeField] ItemBox box;

    #region Unity Events
    protected override void Start()
    {
        base.Start();
        itemBoxSlot = slot as ItemBoxSlot;
        box = itemBoxSlot.box;
    }
    #endregion

    #region Availability
    public override void OnPointerEnter(PointerEventData eventData)
    {

        if (Inventory.Instance.DraggingSlot == null
            || Inventory.Instance.DraggingSlot.Type.Equals(SlotType.Quick))
        {
            return;
        }

        Inventory.Instance.PointOnSlot = itemBoxSlot;

        if (box == null)
        {
            return;
        }

        if (box.CheckItemDropable(itemBoxSlot.Row, itemBoxSlot.Col, Inventory.Instance.DraggingSlot))
        {
            canDrop = false;
            box.ShowAvailability(true, itemBoxSlot, Color.green);
        }
        else
        {
            if (!Inventory.Instance.DraggingSlot.Type.Equals(SlotType.Equipment) && itemBoxSlot.Item != null)
            {
                // 만약 인벤토리 아이템 박스로 같은 크기의 아이템 위치 서로 바꾸기 시도일 경우
                if (Inventory.Instance.DraggingSlot.Item.Width == itemBoxSlot.Item.Width
                    && Inventory.Instance.DraggingSlot.Item.Height == itemBoxSlot.Item.Height)
                {
                    canDrop = false;
                    box.ShowAvailability(true, itemBoxSlot, Color.green);
                    return;
                }
            }
            else if (Inventory.Instance.DraggingSlot.Type.Equals(SlotType.Equipment) && itemBoxSlot.Item != null)
            {
                if (!itemBoxSlot.Item.ItemType.Equals(ItemBaseType.Wearable))  // 드롭하고자 하는 위치에 있는 아이템이 장비 아이템이 아닐 경우
                {
                    box.ShowAvailability(false, itemBoxSlot, Color.red);
                    return;
                }

                // 만약 장비 슬롯에서 가방 슬롯으로 같은 종류의 아이템 위치 서로 바꾸기 시도일 경우
                if (Inventory.Instance.DraggingSlot.WearableItem.WearablesType.Equals(itemBoxSlot.WearableItem.WearablesType))
                {
                    canDrop = false;
                    box.ShowAvailability(true, itemBoxSlot, Color.green);
                    return;
                }
            }

            box.ShowAvailability(false, itemBoxSlot, Color.red);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (Inventory.Instance.DraggingSlot == null
            || Inventory.Instance.DraggingSlot.Type.Equals(SlotType.Quick))
        {
            return;
        }

        if (!canDrop)
        {
            canDrop = true;
            box.ShowAvailability(true, itemBoxSlot, Color.white);
        }
        else
        {
            box.ShowAvailability(false, itemBoxSlot, Color.white);
        }
    }
    #endregion
}