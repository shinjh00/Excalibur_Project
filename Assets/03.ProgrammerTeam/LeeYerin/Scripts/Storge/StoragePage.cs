using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자: 이예린 / 창고 각 페이지 관련 내용 구현 클래스 (창고 슬롯 관련 기능 구현)
/// </summary>
public class StoragePage : MonoBehaviour
{
    [SerializeField] Transform storage;
    [SerializeField] int row = 6;
    [SerializeField] int col = 6;
    Dictionary<StorageSlot, ItemData> items = new Dictionary<StorageSlot, ItemData>();
    [SerializeField] GameObject pageOpen;
    [SerializeField] Button pageOpenButton;

    public bool isOpend;

    StorageSlot[,] slots;

    public StorageSlot[,] Slots => slots;
    public Dictionary<StorageSlot, ItemData> Items => items;

    #region Unity Event
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => (Inventory.Instance.Storage != null));

        StorageSlot[] tempSlots = storage.GetComponentsInChildren<StorageSlot>();

        slots = new StorageSlot[row, col];
        int count = 0;
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++, count++)
            {
                slots[i, j] = tempSlots[count];
                slots[i, j].Row = i;
                slots[i, j].Col = j;
                slots[i, j].storagePage = this;
            }
        }

        pageOpenButton.onClick.AddListener(Inventory.Instance.Storage.PageAdd);
    }
    #endregion

    #region Page Open
    public void OpenPage()
    {
        pageOpen.SetActive(false);
        isOpend = true;
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
        if (!Inventory.Instance.PointOnSlot.Type.Equals(SlotType.Storage))
        {
            return;
        }

        if (!isOpend)
        {
            return;
        }

        StorageSlot slot = Inventory.Instance.PointOnSlot as StorageSlot;

        if (!slot.SlotRect.gameObject.activeSelf)
        {
            return;
        }

        if (slot.Item != null)   // 드롭한 슬롯에 아이템이 있을 경우
        {
            if (dragSlot.Type.Equals(SlotType.Store))   // 상점 슬롯일 경우
            {
                Inventory.Instance.StorePage.SetDropFailUI("아이템을 배치할 수 없습니다");
                return;
            }

            // 아이템의 너비와 높이가 같을 경우만 슬롯 교환 가능
            if (slot.Item.Width != dragSlot.Item.Width || slot.Item.Height != dragSlot.Item.Height)
            {
                return;
            }

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

                Inventory.Instance.Items[dragSlot] = slot.Item;
                items[slot] = dragSlot.Item;
            }
            else if (dragSlot.Type.Equals(SlotType.Equipment))
            {
                Inventory.Instance.Items[dragSlot] = slot.Item;
                items[slot] = dragSlot.Item;
            }

            Vector2 slotParent = dragSlot.SlotRect.localScale;
            dragSlot.SlotRect.localScale = new Vector2(slotParent.x * dragSlot.Item.Width, slotParent.y * dragSlot.Item.Height);

            ItemData tempItem = slot.Item;
            slot.Item = dragSlot.Item;
            dragSlot.Item = tempItem;
            Inventory.Instance.DraggingSlot = null;

            ReturnColor(slot);

            return;
        }
        else    // 드롭한 슬롯에 아이템이 없을 경우
        {
            if (!CheckItemDropable(slot.Row, slot.Col, dragSlot))
            {
                Inventory.Instance.StorePage.SetDropFailUI("아이템을 배치할 수 없습니다");
                return;
            }
            Vector2 slotParent = slot.SlotRect.localScale;
            slot.SlotRect.localScale = new Vector2(slotParent.x * dragSlot.Item.Width, slotParent.y * dragSlot.Item.Height);
            SlotActive(slot.Row, slot.Col, dragSlot.Item, false);

            if (dragSlot.Type.Equals(SlotType.Store))
            {
                Inventory.Instance.StorePage.itemDragSlot = dragSlot;
                Inventory.Instance.StorePage.TryBuyItem();
            }

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
}