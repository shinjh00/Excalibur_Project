using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 개발자: 이예린 / 가방 슬롯의 드롭 가능 여부 시각화 관련 클래스 (슬롯 아이템 부모 오브젝트인 슬롯에 할당)
/// </summary>
public class Availability : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected Slot slot;
    protected bool canDrop = true;

    #region Unity Events
    protected virtual void Start()
    {
        slot = transform.GetComponentInChildren<Slot>();
    }
    #endregion

    #region Availability
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (Inventory.Instance.DraggingSlot == null 
            || Inventory.Instance.DraggingSlot.Type.Equals(SlotType.Quick))
        {
            return;
        }

        if (!slot.Type.Equals(SlotType.Bag))
        {
            return;
        }

        Inventory.Instance.PointOnSlot = slot;

        if (Inventory.Instance.CheckItemDropable(slot.Row, slot.Col, Inventory.Instance.DraggingSlot))   // 드롭이 가능할 경우
        {
            canDrop = false;
            Inventory.Instance.ShowAvailability(true, slot, Color.green);
        }
        else                                                                                            // 드롭이 불가능할 경우
        {
            if (!Inventory.Instance.DraggingSlot.Type.Equals(SlotType.Equipment) && slot.Item != null)
            {
                // 상점에서 구매 시도 중일 경우
                if (Inventory.Instance.DraggingSlot.Type.Equals(SlotType.Store))
                {
                    Inventory.Instance.ShowAvailability(false, slot, Color.red);
                    return;
                }

                // 만약 인벤토리 가방으로 같은 크기의 아이템 위치 서로 바꾸기 시도일 경우
                if (Inventory.Instance.DraggingSlot.Item.Width == slot.Item.Width
                    && Inventory.Instance.DraggingSlot.Item.Height == slot.Item.Height)
                {
                    canDrop = false;
                    if (slot.canDrag)
                    {
                        Inventory.Instance.ShowAvailability(true, slot, Color.green);
                    }
                    else
                    {
                        Inventory.Instance.ShowAvailability(false, slot, Color.red);
                    }
                    return;
                }
            }
            else if (Inventory.Instance.DraggingSlot.Type.Equals(SlotType.Equipment) && slot.Item != null)
            {
                if (!slot.Item.ItemType.Equals(ItemBaseType.Wearable))  // 드롭하고자 하는 위치에 있는 아이템이 장비 아이템이 아닐 경우
                {
                    Inventory.Instance.ShowAvailability(false, slot, Color.red);
                    return;
                }
                // 만약 장비 슬롯에서 가방 슬롯으로 같은 종류의 아이템 위치 서로 바꾸기 시도일 경우
                if (Inventory.Instance.DraggingSlot.WearableItem.WearablesType.Equals(slot.WearableItem.WearablesType))
                {
                    canDrop = false;
                    Inventory.Instance.ShowAvailability(true, slot, Color.green);
                    return;
                }
            }

            Inventory.Instance.ShowAvailability(false, slot, Color.red);
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (Inventory.Instance.DraggingSlot == null
            || Inventory.Instance.DraggingSlot.Type.Equals(SlotType.Quick))
        {
            return;
        }

        if (!slot.Type.Equals(SlotType.Bag))
        {
            return;
        }

        if (!canDrop)
        {
            canDrop = true;
            Inventory.Instance.ShowAvailability(true, slot, Color.white);
        }
        else
        {
            Inventory.Instance.ShowAvailability(false, slot, Color.white);
        }

    }
    #endregion
}
