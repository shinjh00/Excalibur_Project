using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 개발자: 이예린/ 상점 페이지 관련 내용 구현 클래스 (상점 슬롯 관련 기능 구현)
/// </summary>
public class StorePage : MonoBehaviour
{
    [SerializeField] Transform store;
    [SerializeField] ItemBaseType pageType;
    [SerializeField] int row = 12;
    [SerializeField] int col = 8;
    [SerializeField] List<ItemData> spawnItems = new List<ItemData>();
    Dictionary<StoreSlot, ItemData> items = new Dictionary<StoreSlot, ItemData>();

    [Header("Buy")]
    public BuyUI buyPopUpUI;

    [Header("SellPage")]
    public SellUI sellPopUpUI;

    StoreSlot[,] slots;

    [SerializeField] StoreSlot itemDropSlot;
    [SerializeField] public Slot itemDragSlot;
    [SerializeField] ItemData targetItem;

    public ItemBaseType PageType => pageType;
    public int Row => row;
    public int Col => col;
    public StoreSlot[,] Slots => slots;
    public List<ItemData> SpwanItems => spawnItems;
    public Dictionary<StoreSlot, ItemData> Items => items;
    public StoreSlot ItemDropSlot => itemDropSlot;
    public ItemData TargetItem_ { get { return targetItem; } set { targetItem = value; Debug.Log(value); } }

    #region Unity Event
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => (Inventory.Instance.Store != null));

        StoreSlot[] tempSlots = store.GetComponentsInChildren<StoreSlot>();

        slots = new StoreSlot[row, col];
        int count = 0;
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++, count++)
            {
                slots[i, j] = tempSlots[count];
                slots[i, j].Row = i;
                slots[i, j].Col = j;
                slots[i, j].storePage = this;
            }
        }
    }
    #endregion

    #region Open Page
    public void ChangePage()
    {
        Inventory.Instance.Store.wantChagePage = pageType;


    }
    #endregion

    #region ItemDrop
    /// <summary>
    /// 아이템 드롭을 시도하는 함수
    /// </summary>
    /// <param name="mousePos">현재 마우스 커서의 위치</param>
    /// <param name="dragSlot">드래그 하고 있는 아이템이 담긴 슬롯</param>
    public void TryDropItem(Slot dragSlot)
    {
        if (!Inventory.Instance.PointOnSlot.Type.Equals(SlotType.Store))
        {
            return;
        }

        // 판매 페이지 아닐 경우 리턴
        if (!pageType.Equals(ItemBaseType.Cash))
        {
            return;
        }

        StoreSlot slot = Inventory.Instance.PointOnSlot as StoreSlot;

        // 팔 수 없는 아이템일 경우
        if (dragSlot.Item.SellPrice == -9999)
        {
            return;
        }

        if (!slot.SlotRect.gameObject.activeSelf)
        {
            return;
        }

        if (slot.Item != null)   // 드롭한 슬롯에 아이템이 있을 경우
        {
            SetDropFailUI("아이템을 배치할 수 없습니다");
            return;
        }
        else    // 드롭한 슬롯에 아이템이 없을 경우
        {
            if (!CheckItemDropable(slot.Row, slot.Col, dragSlot))
            {
                SetDropFailUI("아이템을 배치할 수 없습니다");
                return;
            }
            itemDragSlot = dragSlot;
            itemDropSlot = slot;

            Vector2 slotParent = slot.SlotRect.localScale;
            slot.SlotRect.localScale = new Vector2(slotParent.x * dragSlot.Item.Width, slotParent.y * dragSlot.Item.Height);
            SlotActive(slot.Row, slot.Col, dragSlot.Item, false);

            slot.Item = dragSlot.Item;
            dragSlot.Item = null;

            Inventory.Instance.DraggingSlot = null;

            items.Add(slot, slot.Item);

            if (dragSlot.Type.Equals(SlotType.Bag))
            {
                for (int num = 0; num < Inventory.Instance.QuickSlots.Length; num++)
                {
                    QuickSlot quick = null;

                    if (Inventory.Instance.QuickItems.ContainsKey(Inventory.Instance.QuickSlots[num]))
                    {
                        // 퀵슬롯 안에 있는 아이템이 드래그 됐을 때
                        if (Inventory.Instance.QuickItems[Inventory.Instance.QuickSlots[num]] == dragSlot)
                        {
                            quick = Inventory.Instance.QuickSlots[num];
                        }
                    }

                    if (quick != null)
                    {
                        quick.Item = null;
                        Inventory.Instance.InGameQuickSlots[quick.QuickNum].Item = null;
                        Inventory.Instance.QuickItems.Remove(quick);
                        break;
                    }
                }
            }

            ReturnColor(slot);

            if (pageType.Equals(ItemBaseType.Cash))
            {
                TrySellItem();
            }

            return;
        }
    }

    /// <summary>
    /// 현재 선택한 슬롯에 아이템을 넣을 수 있는지 여부 판단 함수
    /// </summary>
    /// <param name="curRow">현재 선택한 슬롯의 행</param>
    /// <param name="curCol">현재 선택한 슬롯의 열</param>
    /// <param name="slot">현재 선택한 슬롯</param>
    /// <returns></returns>

    public bool CheckItemDropable(int curRow, int curCol, Slot slot)
    {
        if (curRow + slot.Item.Height > row || curCol + slot.Item.Width > col)
        {
            return false;
        }

        for (int i = 0; i < slot.Item.Height; i++)
        {
            for (int j = 0; j < slot.Item.Width; j++)
            {
                if (slots[curRow + i, curCol + j].Item != null || !slots[curRow + i, curCol + j].SlotRect.gameObject.activeSelf)
                {
                    if (slots[curRow + i, curCol + j].gameObject.Equals(slot.gameObject))
                    {
                        continue;
                    }
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 현재 선택한 슬롯에 아이템을 넣을 수 있는지 여부 판단 함수
    /// </summary>
    /// <param name="curRow">현재 선택한 슬롯의 행</param>
    /// <param name="curCol">현재 선택한 슬롯의 열</param>
    /// <param name="item">현재 드레그 하고 있는 슬롯 아이템이 담긴 슬롯</param>
    /// <returns></returns>
    public bool CheckItemDropable(int curRow, int curCol, ItemData item)
    {
        if (curRow + item.Height > row || curCol + item.Width > col)
        {
            return false;
        }

        for (int i = 0; i < item.Height; i++)
        {
            for (int j = 0; j < item.Width; j++)
            {
                if (slots[curRow + i, curCol + j].Item != null || !slots[curRow + i, curCol + j].SlotRect.gameObject.activeSelf)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 슬롯의 Active 상태를 변경하는 함수
    /// </summary>
    /// <param name="curRow">현재 아이템이 들어간 슬롯의 행</param>
    /// <param name="curCol">현재 아이템이 들어간 슬롯의 열</param>
    /// <param name="item">넣고자 하는 아이템의 데이터</param>
    /// <param name="active">설정하고자 하는 active의 상태</param>
    public void SlotActive(int curRow, int curCol, ItemData item, bool active)
    {
        for (int i = 0; i < item.Height; i++)
        {
            for (int j = 0; j < item.Width; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }
                slots[curRow + i, curCol + j].SlotRect.gameObject.SetActive(active);
            }
        }
    }
    #endregion

    #region Visualize inventory
    /// <summary>
    /// 드래그 중 아이템 드롭 가능 여부 시각화 해주는 함수
    /// </summary>
    /// <param name="availability">드롭 가능 여부</param>
    /// <param name="slot">드롭될 슬롯</param>
    /// <param name="color">바꾸고자 하는 색깔</param>
    public void ShowAvailability(bool availability, Slot slot, Color color)
    {
        if (Inventory.Instance.DraggingSlot == null)
        {
            return;
        }

        Slot draggingSlot = Inventory.Instance.DraggingSlot;

        if (availability)
        {
            for (int i = 0; i < draggingSlot.Item.Height; i++)
            {
                for (int j = 0; j < draggingSlot.Item.Width; j++)
                {
                    slots[slot.Row + i, slot.Col + j].SlotImage.color = color;
                }
            }
        }
        else
        {
            for (int i = 0; i < draggingSlot.Item.Height && slot.Row + i < row; i++)
            {
                for (int j = 0; j < draggingSlot.Item.Width && slot.Col + j < col; j++)
                {
                    slots[slot.Row + i, slot.Col + j].SlotImage.color = color;
                }
            }
        }
    }

    /// <summary>
    /// 색이 바뀐 슬롯을 다시 원래 슬롯으로 되돌리는 함수
    /// </summary>
    /// <param name="slot">되돌리고자 하는 슬롯 중 기준 슬롯</param>
    public void ReturnColor(Slot slot)
    {
        for (int i = 0; i < slot.Item.Height; i++)
        {
            for (int j = 0; j < slot.Item.Width; j++)
            {
                slots[slot.Row + i, slot.Col + j].SlotImage.color = Color.white;
            }
        }
    }
    #endregion

    #region Sell Item
    /// <summary>
    /// 플레이어가 상점 판매 페이지에 아이템을 드롭할 시 호출되는 메소드. 아이템 판매를 희망하는 물어보는 UI 호출
    /// </summary>
    public void TrySellItem()
    {
        // 판매 페이지만 가능한 기능
        if (!pageType.Equals(ItemBaseType.Cash))
        {
            return;
        }

        if (sellPopUpUI == null)
        {
            Debug.Log("판매 의사 물어보는 팝업창 UI 할당되지 않음.");
            return;
        }

        sellPopUpUI.SetSellUI();
        sellPopUpUI.UI.SetActive(true);
    }

    /// <summary>
    /// 플레이어가 아이템 판매 수락 버튼을 눌렀을 때 아이템 판매를 진행하는 메소드
    /// </summary>
    public void SellItemSuccess()
    {
        Debug.Log("판매 완료");
        FirebaseManager.Instance.curGold += Inventory.Instance.StorePage.ItemDropSlot.Item.SellPrice;
        sellPopUpUI.UI.SetActive(false);
        itemDragSlot = null;
        itemDropSlot = null;
    }

    /// <summary>
    /// 플레이어가 아이템 판매 취소 버튼을 눌렀을 때 드롭 전 상태로 돌아가는 기능 구현한 메소드
    /// </summary>
    public void SellItemFail()
    {
        // itemDragSlot : 다른 슬롯 (가방, 장비, 창고)
        // itemDropSlot : 상점 슬롯

        if (itemDragSlot.Type.Equals(SlotType.Bag)) // 가방에서 드래그한 경우
        {
            itemDragSlot.SlotRect.localScale = itemDropSlot.SlotRect.localScale;
            Inventory.Instance.SlotActive(itemDragSlot.Row, itemDragSlot.Col, itemDropSlot.Item, false);
        }
        else if (itemDragSlot.Type.Equals(SlotType.Storage))    // 창고에서 드래그한 경우
        {
            itemDragSlot.SlotRect.localScale = itemDropSlot.SlotRect.localScale;
            Inventory.Instance.StoragePage.SlotActive(itemDragSlot.Row, itemDragSlot.Col, itemDropSlot.Item, false);
        }

        itemDropSlot.SlotRect.localScale = Vector2.one;
        SlotActive(itemDropSlot.Row, itemDropSlot.Col, itemDropSlot.Item, true);

        itemDragSlot.Item = itemDropSlot.Item;
        
        switch (itemDragSlot.Type)  // 아이템 정보 이전 상태로 돌려주는 작업 진행
        {
            case SlotType.Storage:
                Inventory.Instance.StoragePage.Items.Add((itemDragSlot as StorageSlot), itemDragSlot.Item);
                break;
            default:
                Inventory.Instance.Items.Add(itemDragSlot, itemDragSlot.Item);
                break;
        }
        items.Remove(itemDropSlot);
        itemDropSlot.Item = null;
        sellPopUpUI.UI.SetActive(false);

        itemDragSlot = null;
        itemDropSlot = null;
    }
    #endregion

    #region Buy Item

    public void TargetItem(ItemData targetItem)
    {
        Debug.Log($"curPage : {gameObject.name}");
        TargetItem_ = targetItem;
    }

    public void TryBuyItem()
    {
        if (buyPopUpUI == null)
        {
            Debug.Log("구매 의사 물어보는 팝업창 UI 할당되지 않음.");
            return;
        }

        buyPopUpUI.SetBuyUI();
        buyPopUpUI.UI.SetActive(true);
    }

    public void BuyItemSuccess()
    {   // 아이템 구매 체크
        if(targetItem == null || FirebaseManager.Instance.curGold < targetItem.SellPrice)
        {   // 구매 불가
            SetDropFailUI("금액이 부족합니다.");
            BuyItemFail();
            return;
        }

        Debug.Log("구매 완료");
        buyPopUpUI.UI.SetActive(false);
        itemDragSlot = null;
        buyPopUpUI.BuyButton.onClick.RemoveAllListeners();
        buyPopUpUI.NoBuyButton.onClick.RemoveAllListeners();

        FirebaseManager.Instance.curGold -= TargetItem_ switch
        {
            WearableData wearable => wearable.BuyPrice,
            ConsumablesData consume => consume.BuyPrice,
            _ => 0 
        };




        //TODO... 환급 아이템 데이터와 연결하여 아이템의 판매 금액 데이터만큼 플레이어의 현재 금액 올려줘야 함
    }

    public void BuyItemFail()
    {
        // itemDragSlot : 상점 슬롯
        // itemDropSlot : 다른 슬롯 (가방, 장비, 창고)
        
        Slot itemDropSlot = Inventory.Instance.PointOnSlot;

        itemDragSlot.SlotRect.localScale = itemDropSlot.SlotRect.localScale;
        SlotActive(itemDragSlot.Row, itemDragSlot.Col, itemDropSlot.Item, false);

        itemDropSlot.SlotRect.localScale = Vector2.one;
        if (itemDropSlot.Type.Equals(SlotType.Bag)) // 가방에 드롭 중인 경우
        {
            Inventory.Instance.SlotActive(itemDropSlot.Row, itemDropSlot.Col, itemDropSlot.Item, true);
        }
        else if (itemDropSlot.Type.Equals(SlotType.Storage))    // 창고에 드롭 중인 경우
        {
            Inventory.Instance.StoragePage.SlotActive(itemDropSlot.Row, itemDropSlot.Col, itemDropSlot.Item, true);
        }

        itemDragSlot.Item = itemDropSlot.Item;

        switch (itemDropSlot.Type)  // 아이템 정보 이전 상태로 돌려주는 작업 진행
        {
            case SlotType.Storage:
                Inventory.Instance.StoragePage.Items.Remove((itemDropSlot as StorageSlot));
                break;
            default:
                Inventory.Instance.Items.Remove(itemDropSlot);
                break;
        }

        items.Add((itemDragSlot as StoreSlot), itemDragSlot.Item);
        itemDropSlot.Item = null;
        buyPopUpUI.UI.SetActive(false);

        itemDragSlot = null;
        buyPopUpUI.BuyButton.onClick.RemoveAllListeners();
        buyPopUpUI.NoBuyButton.onClick.RemoveAllListeners();
    }
    #endregion

    #region Drop Fail UI
    /// <summary>
    /// 드롭 실패를 알리는 PopUp UI의 실패 원인 Text 설정 및 UI 활성화 해주는 함수
    /// </summary>
    /// <param name="dropFailText"></param>
    public void SetDropFailUI(string dropFailText)
    {
        Inventory.Instance.Store.DropFailPopUpUI.dropFailText.text = dropFailText;
        Inventory.Instance.Store.DropFailPopUpUI.UI.SetActive(true);
    }
    #endregion
}