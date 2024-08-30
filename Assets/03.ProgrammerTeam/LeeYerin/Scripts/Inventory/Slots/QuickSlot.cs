using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 개발자: 이예린 / 퀵슬롯의 슬롯 구현
/// </summary>
public class QuickSlot : Slot
{
    [SerializeField] int quickNum;

    public int QuickNum => quickNum;

    #region Unity Event
    protected override IEnumerator Start()
    {
        StartCoroutine(base.Start());
        yield return null;
        type = SlotType.Quick;
    }
    #endregion

    #region Drag and Drop
    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (!canDrag)
        {
            return;
        }

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

    public override void OnDrag(PointerEventData eventData)
    {
        if (!canDrag)
        {
            return;
        }

        base.OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (!canDrag)
        {
            return;
        }

        if (Item == null)
        {
            return;
        }

        Inventory.Instance.TryDropQuick(eventData.position, this);

        transform.localPosition = DefaultPos;
        image.raycastTarget = true;
        Inventory.Instance.DraggingSlot = null;
    }
    #endregion

    #region Use Item
    /// <summary>
    /// 인벤토리 퀵슬롯 안에서 소비 아이템 사용
    /// </summary>
    /// <param name="i">퀵슬롯 인덱스</param>
    public override void UseItem()
    {
        if (!Type.Equals(SlotType.Quick))
        {
            return;
        }

        if (!canDrag)
        {
            return;
        }

        bool checkUse = false;
        bool isFoodItem = false;

        switch (ConsumableItem.ConsumablesType)
        {
            case ConsumablesType.HpPotion:
                checkUse = UseHpPotion();
                break;
            case ConsumablesType.Food:
                checkUse = UseFood(this);
                isFoodItem = true;
                break;
            case ConsumablesType.Buffpotion:
                checkUse = UseBuffPotion();
                break;
        }

        if (!checkUse)
        {
            return;
        }

        if (isFoodItem)
        {
            canDrag = false;
            Inventory.Instance.QuickItems[this].canDrag = false;
            return;
        }

        AfterUseItem();
    }

    public override void AfterUseItem()
    {
        onUsedItem?.Invoke();
        if (!canDrag)
        {
            canDrag = true;
            Inventory.Instance.QuickItems[this].canDrag = true;
        }
        Inventory.Instance.QuickItems[this].Item = null;
        Inventory.Instance.InGameQuickSlots[quickNum].Item = null;
        Inventory.Instance.Items.Remove(Inventory.Instance.QuickItems[this]);
        Item = null;
        Inventory.Instance.QuickItems.Remove(this);
    }
    #endregion
}
