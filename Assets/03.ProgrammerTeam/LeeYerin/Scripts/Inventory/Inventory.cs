using Firebase.Database;
using Firebase.Extensions;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 개발자: 이예린 / Inventory 관련 클래스
/// </summary>
public class Inventory : MonoBehaviour
{
    static Inventory instance;
    public static Inventory Instance => instance;

    [Header("Component")]
    [SerializeField] RectTransform rect;
    [SerializeField] VerticalLayoutGroup verticalLayoutGroup;
    [SerializeField] Transform bag;
    [SerializeField] Transform quick;
    [SerializeField] Transform equipment;
    [SerializeField] int row;
    [SerializeField] int col;
    Vector3 defaultScale; // 슬롯의 기본 scale;

    [SerializeField] PlayerController player;

    [SerializeField] Slot[,] slots;
    [SerializeField] QuickSlot[] quickSlots;
    [SerializeField] QuickSlot[] inGameQuickSlots;
    [SerializeField] EquipmentSlot[] equipmentSlots;
    [SerializeField] ItemBox itemBox5X5;
    [SerializeField] ItemBox itemBox6X6;
    [SerializeField] Storage storage;
    [SerializeField] Store store;
    [SerializeField] Dictionary<Slot, ItemData> items = new Dictionary<Slot, ItemData>();
    [SerializeField] Dictionary<QuickSlot, Slot> quickItems = new Dictionary<QuickSlot, Slot>();

    [SerializeField] Slot draggingSlot;
    [SerializeField] Slot pointOnSlot;

    public bool canUseHpPotion;
    public bool canUseFood;
    public bool canUseBuffPotion;

    public bool isOpened;
    public bool isInventoryOpen;

    [SerializeField] InventorySaveData saveData;

    [Header("UI")]
    [SerializeField] ApplyTextData textData;

    [SerializeField] List<ItemData> debuggingData = new List<ItemData>();
    [SerializeField] List<int> debuggingX = new List<int>(); 
    [SerializeField] List<int> debuggingY = new List<int>();
    [SerializeField] List<WearablesType> type = new List<WearablesType>();

    #region Property
    public RectTransform Rect => rect;
    public VerticalLayoutGroup VerticalLayout => verticalLayoutGroup;
    public int Row => row;
    public int Col => col;
    public Vector3 DefaultScale => defaultScale;
    public PlayerController Player
    {
        get
        {
            return player;
        }
        set
        {
            player = value;
            if (player != null)
                switch (player.PlayerClassData.classType)
                {
                    case ClassType.Warrior:
                        equipmentSlots[0].EquipmentSlotType = WearablesType.OneHanded;
                        break;
                    case ClassType.Archer:
                        equipmentSlots[0].EquipmentSlotType = WearablesType.Bow;
                        break;
                    case ClassType.Wizard:
                        equipmentSlots[0].EquipmentSlotType = WearablesType.Wand;
                        break;
                    case ClassType.Knight:
                        equipmentSlots[0].EquipmentSlotType = WearablesType.Spear;
                        break;
                }
        }
    }
    public QuickSlot[] QuickSlots => quickSlots;
    public QuickSlot[] InGameQuickSlots => inGameQuickSlots;
    public EquipmentSlot[] EquipmentSlots => equipmentSlots;
    public Dictionary<Slot, ItemData> Items => items;
    public Dictionary<QuickSlot, Slot> QuickItems => quickItems;
    public Slot DraggingSlot { get { return draggingSlot; } set { draggingSlot = value; } }
    public Slot PointOnSlot { get { return pointOnSlot; } set { pointOnSlot = value; } }
    public ItemBox ItemBox5X5 { get { return itemBox5X5; } set { itemBox5X5 = value; } }
    public ItemBox ItemBox6X6 { get { return itemBox6X6; } set { itemBox6X6 = value; } }
    public Storage Storage { get { return storage; } set { storage = value; } }
    public StoragePage StoragePage { get { return storage.StoragePage; } }
    public Store Store { get { return store; } set { store = value; } }
    public StorePage StorePage { get { return store.StorePage; } }
    public ApplyTextData TextData => textData;
    #endregion

    #region Unity Events
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Start()
    {
        ControlInventoryUI(false);
        SetInventory();
        canUseHpPotion = true;
        canUseFood = true;
        canUseBuffPotion = true;

        yield return new WaitUntil(() => Player != null);

        if (Player != null)
        {
            Debug.Log("다운");
            DownLoadInventoryData();
        }
    }
    #endregion

    #region Setting
    /// <summary>
    /// 인벤토리 초기 세팅
    /// </summary>
    private void SetInventory()
    {
        Slot[] tempSlots = bag.transform.GetComponentsInChildren<Slot>();
        slots = new Slot[row, col];
        int count = 0;
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++, count++)
            {
                slots[i, j] = tempSlots[count];
                slots[i, j].Row = i;
                slots[i, j].Col = j;
                slots[i, j].bagRect = bag as RectTransform;
            }
        }

        defaultScale = quickSlots[0].SlotRect.localScale;
    }

    /// <summary>
    /// Inventory 안에 있는 슬롯 내용 정리 함수
    /// </summary>
    public void FreshSlots()
    {
        foreach (Slot slot in slots)
        {
            slot.Item = null;
        }

        foreach (Slot slot in quickSlots)
        {
            slot.Item = null;
        }

        foreach (Slot slot in equipmentSlots)
        {
            slot.Item = null;
        }
    }
    #endregion

    #region Inventory UI
    /// <summary>
    /// 특정 상자(보물상자, 몬스터상자)를 열 때 인벤토리를 여닫는 메소드
    /// </summary>
    /// <param name="targetBox">어떤 상자를 여는지</param>
    /// <param name="value"></param>
    public void ControlInventory(DropBox targetBox, bool value)
    {
        ControlInventoryUI(value);

        if (targetBox == null)
            return;
        itemBox5X5.gameObject.SetActive(value);

        if (value)
        {
            if (!targetBox.IsOpen)
            {
                targetBox.IsOpen = true;
                itemBox5X5.SetBox(targetBox.BoxItems);
            }
            else
            {
                itemBox5X5.SetBox(targetBox);
            }
        }
        else
        {
            if (ItemBox5X5.Slots == null)
            {
                return;
            }

            targetBox.SetData(itemBox5X5.Slots);
        }
    }

    /// <summary>
    /// 인벤토리 UI를 컨트롤하는 메소드
    /// </summary>
    /// <param name="value"></param>
    private void ControlInventoryUI(bool value)
    {
        bag.gameObject.SetActive(value);
        quick.gameObject.SetActive(value);
        equipment.gameObject.SetActive(value);
    }

    public void DownLoadData()
    {
        store.DownLoadStoreData();
        storage.DownLoadStoreagData();
    }

    public void OpenOutGameInvenUI()
    {
        ControlInventoryUI(true);
        storage.UI.SetActive(true);
        store.UI.SetActive(true);
        DownLoadData();
        // Inventory.Instance.DownLoadInventoryData();
    }

    public void CloseOutGameInvenUI()
    {
        if (draggingSlot != null)
        {
            draggingSlot.SlotBehaviorOnClose();
        }
        ControlInventoryUI(false);
        storage.UI.SetActive(false);
        storage.UIChange?.Invoke(false);
        store.UI.SetActive(false);
        FirebaseManager.Instance.UpLoadInventoryData();
        FirebaseManager.Instance.UploadCurGold();
    }
    #endregion

    #region Drop Item

    ///////////////////////////////////////////////////////////////////////////
    ///                                                                     ///
    ///                              Bag                                    ///
    ///                                                                     ///
    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 아이템 드롭을 시도하는 함수
    /// </summary>
    /// <param name="mousePos">현재 마우스 커서의 위치</param>
    /// <param name="dragSlot">드래그 하고 있는 아이템이 담긴 슬롯</param>
    public bool TryDropItem(Vector2 mousePos, Slot dragSlot)
    {
        // 현재 드래그 하는 슬롯에 있는 아이템이 소비 아이템일 경우 퀵슬롯에 드롭 시도
        if (dragSlot.Item.ItemType.Equals(ItemBaseType.Consumable))
        {
            if (!TryDropQuick(mousePos, dragSlot))
            {
                return true;
            }
        }
        else if (dragSlot.Item.ItemType.Equals(ItemBaseType.Wearable))
        {
            if (!TryDropEquipment(mousePos, dragSlot))
            {
                return true;
            }
        }

        if (!pointOnSlot.Type.Equals(SlotType.Bag))
        {
            return false;
        }

        if (!pointOnSlot.SlotRect.gameObject.activeSelf)
        {
            return false;
        }

        if (pointOnSlot.Item != null)   // 드롭한 슬롯에 아이템이 있을 경우
        {
            if (dragSlot.Type.Equals(SlotType.Store))   // 상점 슬롯인 경우
            {
                StorePage.SetDropFailUI("아이템을 배치할 수 없습니다");
                return false;
            }

            Vector2 slotParent;
            ItemData tempItem;

            // 같은 종류의 장비 아이템 교환일 경우
            if (draggingSlot.Type.Equals(SlotType.Equipment) && pointOnSlot.Item.ItemType.Equals(ItemBaseType.Wearable))
            {
                if (draggingSlot.WearableItem.WearablesType.Equals(pointOnSlot.WearableItem.WearablesType))
                {
                    EquipmentSlot equipmentSlot = dragSlot as EquipmentSlot;
                    equipmentSlot.EquipmentEffects(false);

                    tempItem = pointOnSlot.Item;
                    pointOnSlot.Item = dragSlot.Item;
                    dragSlot.Item = tempItem;
                    draggingSlot = null;

                    items[dragSlot] = dragSlot.Item;
                    items[pointOnSlot] = pointOnSlot.Item;

                    equipmentSlot.EquipmentEffects(true);

                    ReturnColor(pointOnSlot);

                    return true;
                }
            }

            // 아이템의 너비와 높이가 같을 경우만 슬롯 교환 가능
            if (pointOnSlot.Item.Width != dragSlot.Item.Width || pointOnSlot.Item.Height != dragSlot.Item.Height)
            {
                return false;
            }

            // 퀵슬롯에 있는 아이템 교환인지 확인
            for (int num = 0; num < quickSlots.Length; num++)
            {
                if (pointOnSlot.ConsumableItem == null)
                {
                    break;
                }

                QuickSlot dropInQuick = null;
                // 드롭하고자 하는 슬롯이 퀵슬롯에 있는 아이템일 경우
                if (quickItems.ContainsKey(quickSlots[num]))
                {
                    if (quickItems[quickSlots[num]] == pointOnSlot)
                    {
                        dropInQuick = quickSlots[num];
                    }
                }

                if (dropInQuick != null)
                {
                    // 드래그한 슬롯의 아이템이 퀵슬롯에 넣을 수 있는 아이템이 아닐 경우
                    if (dragSlot.ConsumableItem == null)
                    {
                        // 드래그한 슬롯이 가방 슬롯이 아니라면
                        if (!dragSlot.Type.Equals(SlotType.Bag))
                        {
                            quickItems.Remove(dropInQuick);
                            dropInQuick.Item = null;
                            inGameQuickSlots[num].Item = null;
                            break;
                        }
                        // 가방 슬롯이라면 가방 안 위치는 교환, 퀵슬롯 아이템 관리에는 추가 안 함
                        else
                        {
                            quickItems[dropInQuick] = dragSlot;
                            break;
                        }
                    }
                    // 소비 아이템이 들어있지만 가방 슬롯이 아닐 경우
                    else if (!dragSlot.Type.Equals(SlotType.Bag))
                    {
                        quickItems.Remove(dropInQuick);
                        dropInQuick.Item = null;
                        inGameQuickSlots[num].Item = null;
                        break;
                    }

                    quickItems[dropInQuick] = dragSlot;
                    break;
                }
            }

            for (int num = 0; num < quickSlots.Length; num++)
            {
                if (!dragSlot.Type.Equals(SlotType.Bag) || !dragSlot.Item.ItemType.Equals(ItemBaseType.Consumable))
                {
                    break;
                }

                QuickSlot dragInQuick = null;

                if (quickItems.ContainsKey(quickSlots[num]))
                {
                    // 드래그 하는 슬롯의 아이템이 퀵슬롯에 있을 경우
                    if (quickItems[quickSlots[num]] == dragSlot)
                    {
                        dragInQuick = quickSlots[num];
                    }
                }

                if (dragInQuick != null)
                {
                    // 드롭 슬롯의 아이템이 퀵슬롯에 넣을 수 있는 아이템이 아닐 경우
                    if (!pointOnSlot.Item.ItemType.Equals(ItemBaseType.Consumable))
                    {
                        quickItems[dragInQuick] = pointOnSlot;
                        break;
                    }

                    quickItems[dragInQuick] = pointOnSlot;
                    break;
                }
            }

            slotParent = dragSlot.SlotRect.localScale;
            dragSlot.SlotRect.localScale = new Vector2(slotParent.x * dragSlot.Item.Width, slotParent.y * dragSlot.Item.Height);

            tempItem = pointOnSlot.Item;
            pointOnSlot.Item = dragSlot.Item;
            dragSlot.Item = tempItem;
            draggingSlot = null;

            if (dragSlot.Type.Equals(SlotType.Bag))
            {
                items[dragSlot] = dragSlot.Item;
            }

            items[pointOnSlot] = pointOnSlot.Item;

            ReturnColor(pointOnSlot);

            return true;
        }
        else   // 드롭한 슬롯에 아이템이 없는 경우
        {
            if (!CheckItemDropable(pointOnSlot.Row, pointOnSlot.Col, dragSlot))
            {
                if (store != null)
                {
                    StorePage.SetDropFailUI("아이템을 배치할 수 없습니다");
                }
                return false;
            }

            if (dragSlot.Type.Equals(SlotType.Store))
            {
                StorePage.itemDragSlot = dragSlot;
                StorePage.TryBuyItem();
            }

            Vector2 slotParent = pointOnSlot.SlotRect.localScale;
            pointOnSlot.SlotRect.localScale = new Vector2(slotParent.x * dragSlot.Item.Width, slotParent.y * dragSlot.Item.Height);
            SlotActive(pointOnSlot.Row, pointOnSlot.Col, dragSlot.Item, false);

            if (dragSlot.Type.Equals(SlotType.Equipment))   // 드래그하고 있는 슬롯이 장비 슬롯일 경우
            {
                EquipmentSlot equipmentSlot = dragSlot as EquipmentSlot;
                equipmentSlot.EquipmentEffects(false);
            }
            else if (dragSlot.Type.Equals(SlotType.Bag))
            {
                // 퀵슬롯에 있는 아이템의 이동인지 체크
                foreach (var quick in quickItems)
                {
                    QuickSlot tempSlot = null;
                    if (quick.Value == dragSlot)
                    {
                        tempSlot = quick.Key;
                    }

                    if (tempSlot != null)
                    {
                        quickItems.Remove(tempSlot);
                        quickItems.Add(tempSlot, pointOnSlot);
                        break;
                    }
                }
            }

            pointOnSlot.Item = dragSlot.Item;
            dragSlot.Item = null;

            items.Add(pointOnSlot, pointOnSlot.Item);

            draggingSlot = null;

            ReturnColor(pointOnSlot);

            return true;
        }
    }

    /// <summary>
    /// 현재 선택한 슬롯에 아이템을 넣을 수 있는지 여부 판단 함수
    /// </summary>
    /// <param name="curRow">현재 선택한 슬롯의 행</param>
    /// <param name="curCol">현재 선택한 슬롯의 열</param>
    /// <param name="slot">현재 드레그 하고 있는 슬롯 아이템이 담긴 슬롯</param>
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

    ///////////////////////////////////////////////////////////////////////////
    ///                                                                     ///
    ///                            Quick Slot                               ///
    ///                                                                     ///
    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 퀵슬롯에 아이템 드롭 시도하는 함수. TryItemDrop()에서 계속 탐색 이어가야 하는지 여부 리턴
    /// </summary>
    /// <param name="mousePos">현재 마우스 커서의 위치</param>
    /// <param name="dragSlot">드래그 하고 있는 아이템이 담긴 슬롯</param>
    /// <returns></returns>
    public bool TryDropQuick(Vector2 mousePos, Slot dragSlot)
    {
        for (int i = 0; i < quickSlots.Length; i++)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(quickSlots[i].Rect, mousePos) && quickSlots[i] != dragSlot)
            {
                if (!quickSlots[i].canDrag)
                {
                    return false;
                }

                if (!dragSlot.Type.Equals(SlotType.Bag) && !dragSlot.Type.Equals(SlotType.Quick))
                {
                    return false;
                }

                if (!dragSlot.Item.ItemType.Equals(ItemBaseType.Consumable))
                {
                    return false;
                }

                if (!(dragSlot.Item.Width.Equals(1) && dragSlot.Item.Height.Equals(1)))
                {
                    return false;
                }

                if (quickSlots[i].Item == null) // 드롭하고자 하는 퀵슬롯에 아이템이 없을 경우
                {
                    for (int num = 0; num < quickItems.Count; num++)
                    {
                        if (!quickItems.ContainsKey(quickSlots[num]))
                        {
                            continue;
                        }

                        if (quickItems[quickSlots[num]] == dragSlot)    // 이미 해당 아이템이 퀵슬롯에 있을 경우
                        {
                            return false;
                        }
                    }

                    if (dragSlot.Type.Equals(SlotType.Quick))
                    {
                        QuickSlot dragQuick = dragSlot as QuickSlot;
                        SoundManager.instance.PlaySFX(dragSlot.ConsumableItem.EquipSoundID, Player.audioSource);  // 퀵슬롯 장착 사운드 실행

                        quickSlots[i].Item = dragQuick.Item;
                        inGameQuickSlots[i].Item = quickSlots[i].Item;
                        quickItems.Add(quickSlots[i], quickItems[dragQuick]);

                        quickItems.Remove(dragQuick);
                        dragQuick.Item = null;
                        inGameQuickSlots[dragQuick.QuickNum].Item = null;

                        return false;
                    }

                    quickSlots[i].Item = dragSlot.Item;
                    inGameQuickSlots[i].Item = quickSlots[i].Item;
                    quickItems.Add(quickSlots[i], dragSlot);
                    SoundManager.instance.PlaySFX(draggingSlot.ConsumableItem.EquipSoundID, Player.audioSource);  // 퀵슬롯 장착 사운드 실행
                }
                else    // 드롭하고자 하는 퀵슬롯에 아이템이 있을 경우 (퀵슬롯끼리의 교환)
                {
                    SoundManager.instance.PlaySFX(dragSlot.ConsumableItem.EquipSoundID, Player.audioSource);  // 퀵슬롯 장착 사운드 실행
                    QuickSlot dragQuick = dragSlot as QuickSlot;

                    Slot dropSlotValue = quickItems[quickSlots[i]];
                    Slot dragSlotValue = quickItems[dragQuick];

                    quickItems.Remove(quickSlots[i]);
                    quickItems.Remove(dragQuick);

                    ItemData tempItem = quickSlots[i].Item;
                    quickSlots[i].Item = dragSlot.Item;
                    dragSlot.Item = tempItem;

                    quickItems.Add(quickSlots[i], dragSlotValue);
                    quickItems.Add(dragQuick, dropSlotValue);

                    inGameQuickSlots[quickSlots[i].QuickNum].Item = quickSlots[i].Item;
                    inGameQuickSlots[dragQuick.QuickNum].Item = quickSlots[dragQuick.QuickNum].Item;
                    return false;
                }

                return false;
            }
        }
        return true;
    }

    ///////////////////////////////////////////////////////////////////////////
    ///                                                                     ///
    ///                           Equipment Slot                            ///
    ///                                                                     ///
    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 장비 슬롯에 아이템 드롭을 시도하는 함수. TryItemDrop()에서 계속 탐색 이어가야 하는지 여부 리턴
    /// </summary>
    /// <param name="mousePos">현재 마우스 커서의 위치</param>
    /// <param name="dragSlot">드래그 하고 있는 아이템이 담긴 슬롯</param>
    /// <returns></returns>
    public bool TryDropEquipment(Vector2 mousePos, Slot dragSlot)
    {
        for (int i = 0; i < equipmentSlots.Length; i++)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(equipmentSlots[i].Rect, mousePos) && equipmentSlots[i] != dragSlot)
            {
                if (dragSlot.Type.Equals(SlotType.Store))
                {
                    return false;
                }

                if (!dragSlot.Item.ItemType.Equals(ItemBaseType.Wearable))
                {
                    return false;
                }

                WearableData item = dragSlot.Item as WearableData;

                if (!equipmentSlots[i].EquipmentSlotType.Equals(item.WearablesType))
                {
                    return false;
                }

                if (equipmentSlots[i].Item == null)
                {
                    SoundManager.instance.PlaySFX(dragSlot.WearableItem.EquipSoundID, Player.audioSource);  // 장비 슬롯 장착 사운드 실행
                    equipmentSlots[i].Item = dragSlot.Item;
                    draggingSlot.Item = null;

                    items.Add(equipmentSlots[i], equipmentSlots[i].Item);
                    items.Remove(dragSlot);
                }
                else
                {
                    SoundManager.instance.PlaySFX(dragSlot.WearableItem.EquipSoundID, Player.audioSource);  // 장비 슬롯 장착 사운드 실행
                    equipmentSlots[i].EquipmentEffects(false);

                    ItemData tempItem = equipmentSlots[i].Item;
                    equipmentSlots[i].Item = dragSlot.Item;
                    draggingSlot.Item = tempItem;

                    items[dragSlot] = dragSlot.Item;
                    items[equipmentSlots[i]] = equipmentSlots[i].Item;
                }

                equipmentSlots[i].EquipmentEffects(true);

                return false;
            }
        }
        return true;
    }

    #endregion

    #region Use Item In Bag
    /// <summary>
    /// 인벤토리 안 Bag에 있는 소비 아이템 사용 키 반응 호출
    /// </summary>
    /// <param name="value">마우스 오른쪽</param>
    private void OnUseItem(InputValue value)
    {
        UseBagItem();
    }

    /// <summary>
    /// 인벤토리 안 Bag 안에 있는 소모 아이템 사용하는 함수. 마우스 위치에 맞는 슬롯을 찾아 슬롯 안에 있는 아이템 사용
    /// </summary>
    private void UseBagItem()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(slots[i, j].Rect, eventData.position))
                {
                    if (slots[i, j].ConsumableItem == null)
                    {
                        Debug.Log($"[{i}, {j}] ConsumableItem == null");
                        return;
                    }

                    slots[i, j].UseItem();
                    return;
                }
            }
        }
    }
    #endregion

    #region Use Item In QuickSlot
    private void OnQuick1(InputValue value)
    {
        if (quickSlots[0].Item == null)
        {
            return;
        }
        quickSlots[0].UseItem();
    }

    private void OnQuick2(InputValue value)
    {
        if (quickSlots[1].Item == null)
        {
            return;
        }
        quickSlots[1].UseItem();
    }

    private void OnQuick3(InputValue value)
    {
        if (quickSlots[2].Item == null)
        {
            return;
        }
        quickSlots[2].UseItem();
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
        if (draggingSlot == null)
        {
            return;
        }

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

    #region When player get Excalibur
    /// <summary>
    /// 유저가 엑스칼리버 뽑을 시 필요한 인벤토리 관련 재설정 진행하는 메소드
    /// </summary>
    /// <param name="player"></param>
    public void SetInventorySetting(PlayerController player)
    {
        this.player = player;
        // 가방 슬롯 아이템 사용 관련 이벤트 재연결
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                slots[i, j].SetSlotEvent();
            }
        }
        // 퀵슬롯 아이템 사용 관련 이벤트 재연결
        for (int i = 0; i < quickSlots.Length; i++)
        {
            quickSlots[i].SetSlotEvent();
        }

        player.StateController.HitEvent += player.UIController.inventoryUI.ChangeHitState;
    }
    #endregion

    #region Upload Data
    /// <summary>
    /// 인벤토리 아이템 관련 데이터 업로드해야할 시 호출하는 메소드. 인벤토리의 상태 테이터 리스트로 반환
    /// </summary>
    /// <returns></returns>
    public InventorySaveData GetUploadInventoryDatas()
    {
        InventorySaveData inventorySaveDatas = new InventorySaveData();

        foreach (var item in items)
        {
            if (!item.Value.ItemType.Equals(ItemBaseType.Wearable))
            {
                foreach (var quick in quickItems)
                {
                    if (quick.Value == item.Key)
                    {
                        inventorySaveDatas.SetData(item.Value.Id, item.Key.Row, item.Key.Col, WearablesType.Null, quick.Key.QuickNum);
                        break;
                    }
                }

                inventorySaveDatas.SetData(item.Value.Id, item.Key.Row, item.Key.Col, WearablesType.Null, -9999);
            }
            else
            {
                WearableData wearableData = item.Value as WearableData;
                if (wearableData.WearablesType < WearablesType.Armor)
                {
                    if (equipmentSlots[0] == item.Key)
                    {
                        inventorySaveDatas.SetData(item.Value.Id, item.Key.Row, item.Key.Col, wearableData.WearablesType, -9999);
                    }
                    else
                    {
                        inventorySaveDatas.SetData(item.Value.Id, item.Key.Row, item.Key.Col, WearablesType.Null, -9999);
                    }
                }
                else
                {
                    if (equipmentSlots[(int)wearableData.WearablesType + 1] == item.Key)
                    {
                        inventorySaveDatas.SetData(item.Value.Id, item.Key.Row, item.Key.Col, wearableData.WearablesType, -9999);
                    }
                    else
                    {
                        inventorySaveDatas.SetData(item.Value.Id, item.Key.Row, item.Key.Col, WearablesType.Null, -9999);
                    }
                }
            }
        }

        return inventorySaveDatas;
    }

    /// <summary>
    /// 인벤토리 아이템 관련 데이터 다운로드해야할 시 호출하는 메소드. 
    /// </summary>
    /// <returns></returns>
    [ContextMenu("DownloadInvenData")]
    public void DownLoadInventoryData()
    {
        Debug.Log($"DownLoad InvenData {FirebaseManager.Instance.curClass} ");
        if (FirebaseManager.Instance.curClass == ClassType.Excalibur)
        {
            return;
        }

        saveData = new InventorySaveData();
        FirebaseManager.DB
            .GetReference("PlayerDataTable")
            .Child(FirebaseManager.Instance.curPlayer)
            .Child("characterData")
            .Child(FirebaseManager.Instance.curClass.ToString())
            .Child("detailData")
            .Child("inventoryData")
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    return;
                }
                else if (task.IsFaulted)
                {
                    return;
                }

                DataSnapshot snapShot = task.Result;

                if (snapShot.Exists)
                {
                    saveData = JsonUtility.FromJson<InventorySaveData>(snapShot.GetRawJsonValue());
                    FirebaseManager.Instance.InvenData = saveData;
                }
                else
                {
                    FirebaseManager.Instance.InvenData = new InventorySaveData();
                    FreshSlots();
                }
                FillInventory(FirebaseManager.Instance.InvenData);
            }); 
    }

    private void FillInventory(InventorySaveData saveData)
    {
        FreshSlots();

        debuggingData.Clear();
        debuggingX.Clear();
        debuggingY.Clear();
        type.Clear();

        Debug.Log($"saveData : {saveData.itemID.Count}");
        for (int i = 0; i < saveData.itemID.Count; i++)
        {
            ItemData targetItem = CsvParser.Instance.ItemList.ItemDic[saveData.itemID[i]];
            int posX = saveData.posX[i];
            int posY = saveData.posY[i];

            debuggingData.Add(targetItem);
            debuggingX.Add(posX);
            debuggingY.Add(posY);

            type.Add(saveData.slot[i]);

            if (saveData.slot[i] != WearablesType.Null)
            {   // 1. 장비 슬롯에 있는지 검사(-> 슬롯에 있는 경우 0보다 작은 경우에는 0번 아닌 경우에는 차례대로 넣어줘야 함
                int slotNum = (int)saveData.slot[i]; // -4 ~ -1 -> 0번으로, 0이상은 하나씩 증가
                switch (slotNum)
                {
                    case -4:
                    case -3:
                    case -2:
                    case -1:
                        slotNum = 0;
                        break;
                    default:
                        slotNum = slotNum + 1;
                        break;
                }
                EquipmentSlots[slotNum].Item = targetItem;
                Debug.Log($"{slotNum} {EquipmentSlots[slotNum].EquipmentSlotType}, {EquipmentSlots[slotNum].Item.Name}");
                // items.Add(EquipmentSlots[slotNum], targetItem);
            }
            else
            {
                Debug.Log("논 슬롯 아이템");
                slots[posX, posY].Item = targetItem;

                if (saveData.quickSlot_Number[i] != -9999)
                {
                    quickSlots[saveData.quickSlot_Number[i]].Item = targetItem;

                    quickItems.Add(quickSlots[saveData.quickSlot_Number[i]], slots[posX, posY]);
                }

                Vector2 slotParent = slots[posX, posY].SlotRect.localScale;
                slots[posX, posY].SlotRect.localScale = new Vector2(slotParent.x * slots[posX, posY].Item.Width, slotParent.y * slots[posX, posY].Item.Height);
                SlotActive(posX, posY, slots[posX, posY].Item, false);
            }
        }
    }
    #endregion
}