using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자: 이예린 / 아이템 박스 생성 클래스
/// </summary>
public class ItemBox : MonoBehaviour
{
    [SerializeField] Transform box;
    [SerializeField] int row;
    [SerializeField] int col;

    [SerializeField] ItemBoxSlot[,] slots;

    public ItemBoxSlot[,] Slots { get { return slots; } }

    #region Item Set
    /// <summary>
    /// 개발자 : 서보운
    /// 게임 세팅을 위한 메소드. 게임이 시작되면 드랍 테이블에 따라 아이템 선정
    /// </summary>
    public void SetBox(List<ItemData> itemData)
    {
        StartCoroutine(SettingBox(itemData));
    }

    public void SetBox(DropBox box)
    {
        StartCoroutine(SettingBox(box));
    }

    /// <summary>
    /// 개발자 : 서보운
    /// 행과 열을 통해 특정 슬롯을 반환하는 메소드
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    public ItemBoxSlot FindSlot(int row, int col)
    {
        for(int i = 0; i < slots.GetLength(0); i++)
        {
            for(int j = 0; j < slots.GetLength(1); j++)
            {
                if (slots[i, j].Col == col && slots[i, j].Row == row)
                {
                    return slots[i, j];
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 아이템 박스가 처음 열릴 때 아이템 박스 세팅하는 코루틴
    /// </summary>
    /// <param name="itemData">박스에 넣을 아이템 데이터 리스트</param>
    /// <returns></returns>
    private IEnumerator SettingBox(List<ItemData> itemData)
    {
        yield return new WaitUntil(() => Inventory.Instance != null);
        Inventory.Instance.ItemBox5X5 = this;

        box.gameObject.SetActive(true);
        if (slots == null)
        {
            ItemBoxSlot[] tempSlots = box.GetComponentsInChildren<ItemBoxSlot>();
            slots = new ItemBoxSlot[row, col];
            int count = 0;
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++, count++)
                {
                    slots[i, j] = tempSlots[count];
                    slots[i, j].Row = i;
                    slots[i, j].Col = j;
                    slots[i, j].box = this;
                    slots[i, j].bagRect = transform as RectTransform;
                }
            }
        }
        StartCoroutine(StartSetItem(itemData));
    }

    /// <summary>
    /// 열린 적 있는 아이템 박스 아이템 세팅 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator SettingBox(DropBox box)
    {
        yield return new WaitUntil(() => Inventory.Instance != null);
        if (box == null)
            yield break;
        Inventory.Instance.ItemBox5X5 = this;
        
        box.gameObject.SetActive(true);

        for(int i = 0; i < box.BoxItems.Count; i++)
        {
            ItemBoxSlot slot = FindSlot(box.SlotRow[i], box.SlotCol[i]);

            slot.Item = box.BoxItems[i];
            slot.SlotRect.localScale = new Vector2(box.BoxItems[i].Width * Inventory.Instance.DefaultScale.x, box.BoxItems[i].Height * Inventory.Instance.DefaultScale.y);
            SlotActive(slot.Row, slot.Col, box.BoxItems[i], false);

            //box.SlotDatas[i].Item = box.BoxItems[i];
            //box.SlotDatas[i].SlotRect.localScale = new Vector2(box.BoxItems[i].Width, box.BoxItems[i].Height);
            //SlotActive(box.SlotDatas[i].Row, box.SlotDatas[i].Col, box.BoxItems[i], false);
        }
    }

    /// <summary>
    /// 아이템 박스 안 아이템 배치하는 함수
    /// </summary>
    public void SetItems(List<ItemData> itemData)
    {
        foreach (ItemBoxSlot slot in slots)
        {
            slot.Item = null;
        }

        for (int count = 0; count < itemData.Count; count++)
        {
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    if (count >= itemData.Count)
                    {
                        return;
                    }

                    if (CheckItemDropable(i, j, itemData[count]))
                    {
                        Vector2 slotParent = slots[i, j].SlotRect.localScale;
                        slots[i, j].SlotRect.localScale = new Vector2(slotParent.x * itemData[count].Width, slotParent.y * itemData[count].Height);
                        SlotActive(i, j, itemData[count], false);
                        slots[i, j].Item = itemData[count++];
                    }
                }
            }
        }
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

    /// <summary>
    /// 아이템 박스 UI가 꺼졌을 때 아이템 박스 슬롯 상태를 아이템이 들어가기 전 초기 상태로 돌려주는 메소드
    /// </summary>
    public void FreshItemBoxSlots()
    {
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (slots[i, j].Item != null)
                {
                    SlotActive(i, j, slots[i, j].Item, true);
                    slots[i, j].Item = null;
                    slots[i, j].SlotRect.localScale = new Vector2(1f, 1f);
                }
            }
        }
    }

    IEnumerator StartSetItem(List<ItemData> itemData)
    {
        yield return new WaitForSeconds(0.1f);
        SetItems(itemData);
    }
    #endregion

    #region Item Drop
    /// <summary>
    /// 아이템 드롭을 시도하는 함수
    /// </summary>
    /// <param name="mousePos">현재 마우스 커서의 위치</param>
    /// <param name="dragSlot">드래그 하고 있는 아이템이 담긴 슬롯</param>
    public void TryDropItem(Vector2 mousePos, Slot dragSlot)
    {
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                // 스크린상의 특정 포인트가 지정된 사각 범위 꼭지점 안에 포함되는지 결과를 리턴
                if (RectTransformUtility.RectangleContainsScreenPoint(slots[i, j].Rect, mousePos) && slots[i, j] != dragSlot)
                {
                    if (!slots[i, j].SlotRect.gameObject.activeSelf)
                    {
                        return;
                    }

                    if (slots[i, j].Item != null)   // 드롭한 슬롯에 아이템이 있을 경우
                    {
                        // 아이템의 너비와 높이가 같을 경우만 슬롯 교환 가능
                        if (slots[i, j].Item.Width != dragSlot.Item.Width || slots[i, j].Item.Height != dragSlot.Item.Height)
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

                            Inventory.Instance.Items[dragSlot] = slots[i, j].Item;
                        }
                        else if (dragSlot.Type.Equals(SlotType.Equipment))
                        {
                            Inventory.Instance.Items[dragSlot] = slots[i, j].Item;
                        }

                        Vector2 slotParent = dragSlot.SlotRect.localScale;
                        dragSlot.SlotRect.localScale = new Vector2(slotParent.x * dragSlot.Item.Width, slotParent.y * dragSlot.Item.Height);

                        ItemData tempItem = slots[i, j].Item;
                        slots[i, j].Item = dragSlot.Item;
                        dragSlot.Item = tempItem;
                        Inventory.Instance.DraggingSlot = null;

                        ReturnColor(slots[i, j]);

                        return;
                    }
                    else   // 드롭한 슬롯에 아이템이 없는 경우
                    {
                        if (!CheckItemDropable(i, j, dragSlot))
                        {
                            return;
                        }
                        Vector2 slotParent = slots[i, j].SlotRect.localScale;
                        slots[i, j].SlotRect.localScale = new Vector2(slotParent.x * dragSlot.Item.Width, slotParent.y * dragSlot.Item.Height);
                        SlotActive(i, j, dragSlot.Item, false);

                        slots[i, j].Item = dragSlot.Item;
                        dragSlot.Item = null;

                        Inventory.Instance.DraggingSlot = null;

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

                        ReturnColor(slots[i, j]);

                        return;
                    }
                }
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