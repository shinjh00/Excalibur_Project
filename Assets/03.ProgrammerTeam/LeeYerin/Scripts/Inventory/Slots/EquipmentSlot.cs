using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 개발자: 이예린 / 장비 슬롯 구현
/// </summary>
public class EquipmentSlot : Slot
{
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] WearablesType slotType;

    public WearablesType EquipmentSlotType { get { return slotType; } set { slotType = value; } }

    #region Unity Event
    protected override IEnumerator Start()
    {
        StartCoroutine(base.Start());
        yield return null;
    }
    #endregion

    #region Drag and Drop
    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (Item == null)
        {
            return;
        }

        Inventory.Instance.DraggingSlot = this;
        SoundManager.instance.PlaySFX(1650075, Inventory.Instance.Player.audioSource);  // 인벤토리 아이템 옮기는 사운드 실행

        DefaultPos = transform.localPosition;
        SlotRect.SetAsLastSibling();  // local transform list에서 해당 오브젝트 순위를 맨 밑으로 보냄(제일 앞에 보이게 된다)
        bagRect.SetAsLastSibling();
        
        image.raycastTarget = false;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        Slot pointOnSlot = Inventory.Instance.PointOnSlot;

        if (Item == null)
        {
            return;
        }

        if (!Inventory.Instance.TryDropItem(eventData.position, this))
        {
            if (pointOnSlot != null)
            {
                switch (pointOnSlot.Type)
                {
                    case SlotType.ItemBox:
                        if ((pointOnSlot as ItemBoxSlot).box == Inventory.Instance.ItemBox5X5)
                        {
                            Inventory.Instance.ItemBox5X5.TryDropItem(eventData.position, this);
                        }
                        else
                        {
                            Inventory.Instance.ItemBox6X6.TryDropItem(eventData.position, this);
                        }
                        break;
                    case SlotType.Storage:
                        Inventory.Instance.StoragePage.TryDropItem(this);
                        break;
                    case SlotType.Store:
                        Debug.Log("상점 슬롯 들어옴");
                        Inventory.Instance.StorePage.TryDropItem(this);
                        break;
                }
            }
        }
        if (Item != null)
        {
            if (Inventory.Instance.PointOnSlot.Type.Equals(SlotType.Bag))
            {
                Inventory.Instance.ShowAvailability(false, Inventory.Instance.PointOnSlot, Color.white);
            }
            else if (Inventory.Instance.PointOnSlot.Type.Equals(SlotType.ItemBox))
            {
                ItemBox box = (Inventory.Instance.PointOnSlot as ItemBoxSlot).box;
                box.ShowAvailability(false, Inventory.Instance.PointOnSlot, Color.white);
            }
            else if (Inventory.Instance.PointOnSlot.Type.Equals(SlotType.Storage))
            {
                StoragePage storagePage = (Inventory.Instance.PointOnSlot as StorageSlot).storagePage;
                storagePage.ShowAvailability(false, Inventory.Instance.PointOnSlot, Color.white);
            }
            else if (Inventory.Instance.PointOnSlot.Type.Equals(SlotType.Store))
            {
                StorePage storePage = (Inventory.Instance.PointOnSlot as StoreSlot).storePage;
                storePage.ShowAvailability(false, Inventory.Instance.PointOnSlot, Color.white);
            }
        }
        else
        {
            Inventory.Instance.Items.Remove(this);
        }

        transform.localPosition = DefaultPos;
        image.raycastTarget = true;
        Inventory.Instance.DraggingSlot = null;
    }
    #endregion

    #region Wear Equipment
    /// <summary>
    /// 장비 슬롯에 아이템 장착 여부에 따른 효과 플레이어한테 적용하는 함수
    /// </summary>
    /// <param name="add">효과 더하는지 여부</param>
    public void EquipmentEffects(bool add)
    {
        if (Item == null)
        {
            return;
        }

        Inventory.Instance.Player.StatController.ApplyEquipStat(WearableItem.Stats, WearableItem.StatValues, add);
    }
    #endregion
}
