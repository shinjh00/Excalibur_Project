using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 개발자: 이예린/ 상점 슬롯 구현
/// </summary>
public class StoreSlot : Slot
{
    public StorePage storePage;

    #region Unity Event
    protected override IEnumerator Start()
    {
        StartCoroutine(base.Start());
        yield return null;
        type = SlotType.Store;
        bagRect = Inventory.Instance.Store.gameObject.transform as RectTransform;
    }
    #endregion

    #region Item Drag & Drop
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

        SlotRect.localScale = Inventory.Instance.DefaultScale;
        Rect.localScale = new Vector2(Item.Width, Item.Height);
        storePage.SlotActive(Row, Col, Item, true);

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
                    case SlotType.Storage:
                        Inventory.Instance.StoragePage.TryDropItem(this);
                        break;
                }
            }
        }
        if (Item != null)
        {
            SlotRect.localScale = new Vector2(Item.Width * Inventory.Instance.DefaultScale.x, Item.Height * Inventory.Instance.DefaultScale.y);
            if (Inventory.Instance.PointOnSlot.Type.Equals(SlotType.Bag))
            {
                Inventory.Instance.ShowAvailability(false, Inventory.Instance.PointOnSlot, Color.white);
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
            storePage.SlotActive(Row, Col, Item, false);
        }
        else
        {
            storePage.Items.Remove(this);
        }

        transform.localPosition = DefaultPos;
        Rect.localScale = Vector2.one;
        image.raycastTarget = true;
        Inventory.Instance.DraggingSlot = null;

        /*Debug.Log($"Item Count: {Inventory.Instance.Items.Count}");
        foreach (var item in Inventory.Instance.Items)
        {
            if (item.Key.Type == SlotType.Bag)
            {
                Debug.Log($"Slot[{item.Key.Row}, {item.Key.Col}] - {item.Value.Name}");
            }
            else
            {
                Debug.Log($"Slot[{(item.Key as EquipmentSlot).EquipmentSlotType}] - {item.Value.Name}");
            }
        }*/
    }
    #endregion
}