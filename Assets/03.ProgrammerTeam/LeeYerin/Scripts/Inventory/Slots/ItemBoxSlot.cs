using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 개발자: 이예린/ 아이템 박스 슬롯 구현
/// </summary>
public class ItemBoxSlot : Slot
{
    [SerializeField] public ItemBox box;

    #region Unity Event
    protected override IEnumerator Start()
    {
        StartCoroutine(base.Start());
        yield return null;
        type = SlotType.ItemBox;
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

        SlotRect.localScale = Inventory.Instance.DefaultScale;
        Rect.localScale = new Vector2(Item.Width, Item.Height);
        Inventory.Instance.ItemBox5X5.SlotActive(Row, Col, Item, true);

        image.raycastTarget = false;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (Item == null)
        {
            return;
        }

        if (!Inventory.Instance.TryDropItem(eventData.position, this))
        {
            box.TryDropItem(eventData.position, this);
        }
        if (Item != null)
        {
            SlotRect.localScale = new Vector2(Item.Width * Inventory.Instance.DefaultScale.x, Item.Height * Inventory.Instance.DefaultScale.y);
            if (Inventory.Instance.PointOnSlot != null)
            {
                if (Inventory.Instance.PointOnSlot.Type.Equals(SlotType.Bag))
                {
                    Inventory.Instance.ShowAvailability(false, Inventory.Instance.PointOnSlot, Color.white);
                }
                else if (Inventory.Instance.PointOnSlot.Type.Equals(SlotType.ItemBox))
                {
                    box.ShowAvailability(false, Inventory.Instance.PointOnSlot, Color.white);
                }
            }
            box.SlotActive(Row, Col, Item, false);
        }

        transform.localPosition = DefaultPos;
        Rect.localScale = new Vector2(1f, 1f);
        image.raycastTarget = true;
        Inventory.Instance.DraggingSlot = null;
    }
    #endregion
}
