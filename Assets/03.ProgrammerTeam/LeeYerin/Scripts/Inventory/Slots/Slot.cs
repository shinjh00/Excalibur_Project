using Photon.Realtime;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 개발자: 이예린 / 슬롯 관련 클래스
/// </summary>
public class Slot : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [Header("Component")]
    [SerializeField] protected SlotType type;
    [SerializeField] protected Image image;
    [SerializeField] RectTransform rect;
    [SerializeField] RectTransform slotRect;
    [SerializeField] public RectTransform bagRect;
    [SerializeField] Image slotImage;

    [SerializeField] int row;
    [SerializeField] int col;

    protected Vector3 DefaultPos; // 슬롯의 원래 위치
    [SerializeField] ItemData item;
    [SerializeField] ConsumablesData consumableItem;
    [SerializeField] WearableData wearableItem;
    [SerializeField] public bool canDrag = true;

    protected Action<int, float> UseHpPotionAction;
    protected Action<Slot, int, float> UseFoodAction;
    protected Action<int, float, IncrementalStat> UseBuffPotionAction;
    public UnityEvent onUsedItem;

    #region Property
    public SlotType Type {get{ return type; } set { type = value; } }
    public RectTransform Rect => rect;
    public RectTransform SlotRect => slotRect;
    public Image SlotImage => slotImage;
    public int Row { get { return row; } set { row = value; } }
    public int Col { get { return col; } set { col = value; } }
    public ConsumablesData ConsumableItem => consumableItem;
    public WearableData WearableItem => wearableItem;

    /// <summary>
    /// get: Slot 속 Item 리턴 | set: item이 null일 경우 slot 비워지게, null일 아닐 경우 item의 이미지로 채워지게 설정
    /// </summary>
    public ItemData Item
    {
        get { return item; }
        set
        {
            item = value;

            if (item != null)
            {
                if (Item.ItemType.Equals(ItemBaseType.Consumable))
                {
                    consumableItem = Item as ConsumablesData;
                }
                else if (Item.ItemType.Equals(ItemBaseType.Wearable))
                {
                    wearableItem = Item as WearableData;
                }
                if (image != null)
                {
                    image.sprite = item.ItemImage;
                    image.color = new Color(1, 1, 1, 1);
                }
            }
            else
            {
                consumableItem = null;
                wearableItem = null;
                image.sprite = null;
                image.color = new Color(1, 1, 1, 0);
            }
        }
    }
    #endregion

    #region Unity Events
    protected virtual IEnumerator Start()
    {
        yield return new WaitUntil(() => (Inventory.Instance != null && Inventory.Instance.Player != null));

        if (type.Equals(SlotType.Bag) || type.Equals(SlotType.Quick))
        {
            SetSlotEvent();
        }

        slotRect = (RectTransform)transform.parent;
    }
    #endregion

    #region Item Drag & Drop
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null)
        {
            return;
        }

        if (!canDrag)
        {
            return;
        }

        Inventory.Instance.DraggingSlot = this;
        SoundManager.instance.PlaySFX(1650075, Inventory.Instance.Player.audioSource);  // 인벤토리 아이템 옮기는 사운드 실행

        DefaultPos = transform.localPosition;
        slotRect.SetAsLastSibling();  // local transform list에서 해당 오브젝트 순위를 맨 밑으로 보냄(제일 앞에 보이게 된다)
        bagRect.SetAsLastSibling();

        slotRect.localScale = Inventory.Instance.DefaultScale;
        rect.localScale = new Vector2(item.Width, item.Height);
        Inventory.Instance.SlotActive(row, col, item, true);

        image.raycastTarget = false;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (item == null)
        {
            return;
        }

        if (!canDrag)
        {
            return;
        }

        Vector3 mousePos = Camera.main.WorldToScreenPoint(eventData.position);
        transform.position = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, DefaultPos.z));
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        Slot pointOnSlot = Inventory.Instance.PointOnSlot;

        if (item == null)
        {
            return;
        }

        if (!canDrag)
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
                        Inventory.Instance.StorePage.TryDropItem(this);
                        break;
                }
            }
        }
        if (item != null)
        {
            slotRect.localScale = new Vector2(item.Width * Inventory.Instance.DefaultScale.x, item.Height * Inventory.Instance.DefaultScale.y);
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
            Inventory.Instance.SlotActive(row, col, item, false);
        }
        else
        {
            Inventory.Instance.Items.Remove(this);
        }

        transform.localPosition = DefaultPos;
        rect.localScale = Vector2.one;
        image.raycastTarget = true;
        Inventory.Instance.DraggingSlot = null;

        Debug.Log($"Item Count: {Inventory.Instance.Items.Count}");
    }
    #endregion

    #region Use consumable Item
    /// <summary>
    /// 인벤토리 가방 안에서 소비 아이템 사용
    /// </summary>
    public virtual void UseItem()
    {
        if (!type.Equals(SlotType.Bag))
        {
            return;
        }

        if (!canDrag)
        {
            return;
        }

        bool checkUse = false;
        bool isFoodItem = false;

        switch (consumableItem.ConsumablesType)
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

        if (!checkUse)  // 아이템 사용에 실패했을 경우
        {
            return;
        }

        Debug.Log("CanUse");

        if (isFoodItem)
        {
            canDrag = false;
            for (int i = 0; i < Inventory.Instance.QuickSlots.Length; i++)
            {
                // 아이템이 들어있는 퀵슬롯이 아닐 경우
                if (!Inventory.Instance.QuickItems.ContainsKey(Inventory.Instance.QuickSlots[i]))
                {
                    continue;
                }

                // 아이템이 들어있는 퀵슬롯일 경우 valueSlot에 할당
                Slot valueSlot = Inventory.Instance.QuickItems[Inventory.Instance.QuickSlots[i]];
                if (valueSlot.Row.Equals(row) && valueSlot.Col.Equals(col))     // 해당 슬롯의 아이템이 퀵슬롯에 있을 경우
                {
                    Inventory.Instance.QuickSlots[i].canDrag = false;
                    break;
                }
            }
            return;
        }

        SoundManager.instance.PlaySFX(consumableItem.UseSoundID, Inventory.Instance.Player.audioSource);  // 아이템 사용 사운드 실행
        AfterUseItem();
    }

    /// <summary>
    /// 아이템 사용 성공 시 이루어져야 하는 작업 구현 메소드
    /// </summary>
    public virtual void AfterUseItem()
    {
        for (int i = 0; i < Inventory.Instance.QuickSlots.Length; i++)
        {
            // 아이템이 들어있는 퀵슬롯이 아닐 경우
            if (!Inventory.Instance.QuickItems.ContainsKey(Inventory.Instance.QuickSlots[i]))
            {
                continue;
            }

            // 아이템이 들어있는 퀵슬롯일 경우 valueSlot에 할당
            Slot valueSlot = Inventory.Instance.QuickItems[Inventory.Instance.QuickSlots[i]];
            if (valueSlot.Row.Equals(row) && valueSlot.Col.Equals(col))     // 해당 슬롯의 아이템이 퀵슬롯에 있을 경우
            {
                Inventory.Instance.QuickSlots[i].Item = null;
                Inventory.Instance.InGameQuickSlots[i].Item = null;
                Inventory.Instance.QuickItems.Remove(Inventory.Instance.QuickSlots[i]);
                if (!Inventory.Instance.QuickSlots[i].canDrag)
                {
                    Inventory.Instance.QuickSlots[i].canDrag = true;
                }
                break;
            }
        }

        ItemData tempItem = item;
        Inventory.Instance.Items.Remove(this);
        onUsedItem?.Invoke();
        Item = null;
        if (!slotRect.localScale.Equals(Vector2.one))
        {
            Inventory.Instance.SlotActive(row, col, tempItem, true);
            slotRect.localScale = Vector2.one;
        }

        if (!canDrag)
        {
            canDrag = true;
        }
    }

    #region Hp Potion
    /// <summary>
    /// 회복 포션 아이템 사용하는 함수
    /// </summary>
    protected bool UseHpPotion()
    {
        if (UseHpPotionAction == null)
        {
            Debug.Log("Null");
            return false;
        }
        if (!Inventory.Instance.canUseHpPotion)
        {
            Debug.Log("Hp Potion items are already in use");
            return false;
        }
        UseHpPotionAction?.Invoke(consumableItem.Duration, consumableItem.HealPerSec);
        return true;
    }
    #endregion

    #region Food
    /// <summary>
    /// 식량 아이템 사용하는 함수
    /// </summary>
    protected bool UseFood(Slot slot)
    {
        if (UseFoodAction == null)
        {
            Debug.Log("Null");
            return false;
        }

        if (!Inventory.Instance.canUseFood)
        {
            Debug.Log("Food items are already in use");
            return false;
        }
        UseFoodAction?.Invoke(slot, consumableItem.Casting, consumableItem.Heal);
        return true;
    }
    #endregion

    #region Buff Potion
    /// <summary>
    /// 버프 포션 아이템 사용하는 함수
    /// </summary>
    protected bool UseBuffPotion()
    {
        if (UseBuffPotionAction == null)
        {
            Debug.Log("Null");
            return false;
        }

        if (!Inventory.Instance.canUseBuffPotion)
        {
            Debug.Log("Buff Potion items are already in use");
            return false;
        }
        UseBuffPotionAction?.Invoke(consumableItem.Duration, consumableItem.StatUp, consumableItem.Stat);
        return true;
    }
    #endregion

    #endregion

    #region When the inventory is closed

    /// <summary>
    /// 슬롯 드래그 중 인벤토리가 닫혔을 시 슬롯 원래 자리로 돌려주는 작업 해주는 메소드
    /// </summary>
    public void SlotBehaviorOnClose()
    {
        if (!(type.Equals(SlotType.Equipment) || type.Equals(SlotType.Quick)))
        {
            slotRect.localScale = new Vector2(item.Width * Inventory.Instance.DefaultScale.x, item.Height * Inventory.Instance.DefaultScale.y);
        }

        if (type.Equals(SlotType.Bag))
        {
            Inventory.Instance.SlotActive(row, col, item, false);
        }
        else if (type.Equals(SlotType.ItemBox))
        {
            (this as ItemBoxSlot).box.SlotActive(row, col, item, false);
        }
        else if (type.Equals(SlotType.Storage))
        {
            Inventory.Instance.StoragePage.SlotActive(row, col, item, false);
        }
        else if (type.Equals(SlotType.Store))
        {
            Inventory.Instance.StorePage.SlotActive(row, col, item, false);
        }

        switch (Inventory.Instance.PointOnSlot.Type)
        {
            case SlotType.Bag:
                Inventory.Instance.ShowAvailability(false, Inventory.Instance.PointOnSlot, Color.white);
                break;
            case SlotType.ItemBox:
                (Inventory.Instance.PointOnSlot as ItemBoxSlot).box.ShowAvailability(false, Inventory.Instance.PointOnSlot, Color.white);
                break;
            case SlotType.Store:
                Inventory.Instance.StorePage.ShowAvailability(false, Inventory.Instance.PointOnSlot, Color.white);
                break;
            case SlotType.Storage:
                Inventory.Instance.StoragePage.ShowAvailability(false, Inventory.Instance.PointOnSlot, Color.white);
                break;
        }

        transform.localPosition = DefaultPos;
        rect.localScale = Vector2.one;
        image.raycastTarget = true;
        Inventory.Instance.DraggingSlot = null;

        /*if (DefaultPos != null && transform.localPosition != DefaultPos)
        {
            
        }*/
    }
    #endregion

    #region When player get Excalibur
    /// <summary>
    /// 슬롯 아이템 사용 관련 이벤트 액션에 추가하는 함수
    /// </summary>
    public void SetSlotEvent()
    {
        UseHpPotionAction += Inventory.Instance.Player.UseHpPotion;
        UseFoodAction += Inventory.Instance.Player.UseFood;
        UseBuffPotionAction += Inventory.Instance.Player.UseBuffPotion;
    }
    #endregion
}

public enum SlotType
{
    Bag,
    Quick,
    Equipment,
    ItemBox,
    Storage,
    Store
}