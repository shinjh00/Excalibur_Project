using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자: 이예린/ 상점 관리 클래스
/// </summary>
public class Store : MonoBehaviour
{
    [Header("Page")]
    [SerializeField] List<StorePage> inStorePages = new List<StorePage>();
    [SerializeField] List<PageButtons> inStoreButton = new List<PageButtons>();

    [Header("Timer")]
    [SerializeField] TMP_Text timerText;
    [SerializeField] int resetTime;
    [SerializeField] StoreDropFailUI dropFailPopUpUI;
    public Coroutine resetTimer;
    public int timeCount = 0;
    public bool canReset = false;

    [Header("Spwner")]
    public StoreItemSpawner wearableSpwner;
    public StoreItemSpawner consumableSpwner;
    public StoreItemSpawner cashSpwner;

    [Header("UI")]
    public GameObject UI;

    Dictionary<ItemBaseType, StorePage> storePages = new Dictionary<ItemBaseType, StorePage>();
    Dictionary<ItemBaseType, Image> buttons = new Dictionary<ItemBaseType, Image>();

    public ItemBaseType wantChagePage;
    ItemBaseType currentPage;

    [SerializeField] UploadStoreData downLoadData;

    #region Property
    public StoreDropFailUI DropFailPopUpUI { get { return dropFailPopUpUI; } set { dropFailPopUpUI = value; } }
    public int StorePagesCount => storePages.Count;
    public StorePage WearablePage => storePages[ItemBaseType.Wearable];
    public StorePage ConsumablePage => storePages[ItemBaseType.Consumable];
    public StorePage CashPage => storePages[ItemBaseType.Cash];
    public StorePage StorePage { get { return storePages[currentPage];  }}
    #endregion

    #region Unity Event
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => (Inventory.Instance != null && Inventory.Instance.Player != null));

        if (Inventory.Instance.Store == null)
        {
            Inventory.Instance.Store = this;
        }

        for (int i = 0; i < inStorePages.Count; i++)
        {
            storePages.Add(inStorePages[i].PageType, inStorePages[i]);
            buttons.Add(inStoreButton[i].pageType, inStoreButton[i].buttonImage);
            inStoreButton[i].buttonImage.color = Color.gray;
        }
        currentPage = ItemBaseType.Wearable;
        storePages[currentPage].gameObject.SetActive(true);
        buttons[currentPage].color = Color.white;
        StartItemResetTimer();
    }
    #endregion

    #region Open Page
    public void OpenPage()
    {
        // 이미 열려있는 페이지라면
        if (wantChagePage.Equals(currentPage))
        {
            return;
        }

        buttons[currentPage].color = Color.gray;
        buttons[wantChagePage].color = Color.white;
        Debug.Log($"CurrentPage : {currentPage}");
        Debug.Log($"Active Page : {wantChagePage}");
        storePages[currentPage].buyPopUpUI.curItem -= storePages[currentPage].TargetItem;
        storePages[currentPage].gameObject.SetActive(false);
        storePages[wantChagePage].gameObject.SetActive(true);
        currentPage = wantChagePage;
        storePages[wantChagePage].buyPopUpUI.curItem += storePages[wantChagePage].TargetItem;

    }
    #endregion

    #region PopUp Close
    public void PopUpClose()
    {
        DropFailPopUpUI.UI.SetActive(false);
    }
    #endregion

    #region Item reset timer
    public void StartItemResetTimer()
    {
        //TODO... timeCount를 마지막 아웃게임 접속을 기준으로
        timeCount = 0;
        resetTimer = StartCoroutine(ItemResetTimer());
    }

    IEnumerator ItemResetTimer()
    {
        timerText.text = $"{resetTime}:00";
        canReset = false;
        while (timeCount < resetTime)
        {
            yield return new WaitForSeconds(60f);
            timeCount++;
            if ((resetTime - timeCount) / 10 != 0)
            {
                timerText.text = $"{resetTime - timeCount}:00";
            }
            else
            {
                timerText.text = $"0{resetTime - timeCount}:00";
            }
        }

        if (Inventory.Instance.DraggingSlot != null && Inventory.Instance.DraggingSlot.Type.Equals(SlotType.Store))
        {
            Inventory.Instance.DraggingSlot.SlotBehaviorOnClose();
        }

        if (StorePage.buyPopUpUI.UI.activeSelf)
        {
            Slot itemDropSlot = Inventory.Instance.PointOnSlot;

            itemDropSlot.SlotRect.localScale = Vector2.one;
            if (itemDropSlot.Type.Equals(SlotType.Bag))
            {
                Inventory.Instance.SlotActive(itemDropSlot.Row, itemDropSlot.Col, itemDropSlot.Item, true);
                Inventory.Instance.Items.Remove(itemDropSlot);
            }
            else if (itemDropSlot.Type.Equals(SlotType.Storage))
            {
                Inventory.Instance.StoragePage.SlotActive(itemDropSlot.Row, itemDropSlot.Col, itemDropSlot.Item, true);
                Inventory.Instance.StoragePage.Items.Remove(itemDropSlot as StorageSlot);
            }
            itemDropSlot.Item = null;
            StorePage.BuyItemSuccess();
            /*buyPopUpUI.UI.SetActive(false);
            itemDragSlot = null;
            buyPopUpUI.BuyButton.onClick.RemoveAllListeners();
            buyPopUpUI.NoBuyButton.onClick.RemoveAllListeners();*/
        }

        // 자동으로 아이템 새로고침 실시
        wearableSpwner.SpawnItemInStore();
        wearableSpwner.SetItem();
        consumableSpwner.SpawnItemInStore();
        consumableSpwner.SetItem();
        cashSpwner.SpawnItemInStore();
        cashSpwner.SetItem();

        canReset = true;
        timeCount = 0;

        StartItemResetTimer();
    }
    #endregion

    #region Upload Data
    /// <summary>
    /// 파이어 베이스에 상점 데이터 올려야 할 시 호출하는 메소드. 상점 페이지들의 상태 데이터 리스트로 반환
    /// </summary>
    /// <returns></returns>
    public UploadStoreData GetUploadStoreDatas()
    {
        UploadStoreData storeDatas = new UploadStoreData();

        foreach (var page in storePages)
        {
            foreach (var item in page.Value.Items)
            {
                storeDatas.SetData((int)page.Key, item.Value.Id, item.Key.Row, item.Key.Col);
            }
        }

        return storeDatas;
    }

    /// <summary>
    /// 씬 전환 혹은 게임 종료 시 실행시켜줘야 하는 메소드
    /// </summary>
    public UploadStoreTime UploadCurrentTime()
    {
        if (timeCount != resetTime)
        {
            StopCoroutine(resetTimer);
        }

        UploadStoreTime time = new UploadStoreTime();
        time.remainingTime = resetTime - timeCount;

        DateTimeOffset now = DateTimeOffset.UtcNow;
        int unixTimestamp = (int)now.ToUnixTimeSeconds();

        time.closeTime = unixTimestamp;

        return time;
        //TODO... 현재 시간을 firebase에 올리는 작업 필요
        //이후 플레이어가 다시 게임에 접속하거나 인게임에서 아웃게임으로 돌아왔을 때 그 시간을 비교해서 30분 지남을 체크해야 함
    }

    public void DownLoadStoreData()
    {
        downLoadData = new UploadStoreData();

        FirebaseManager.DB
            .GetReference("PlayerDataTable")
            .Child(FirebaseManager.Instance.curPlayer)
            .Child("StoreData")
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
                    downLoadData = JsonUtility.FromJson<UploadStoreData>(snapShot.GetRawJsonValue());
                    FillStore(downLoadData);
                }
            });
    }

    private void FillStore(UploadStoreData storeData)
    {
        for (int i = 0; i < storeData.id.Count; i++)
        {
            ItemData targetItem = CsvParser.Instance.ItemList.ItemDic[storeData.id[i]];
            int posX = storeData.row[i];
            int posY = storeData.col[i];

            Slot targetSlot = new Slot();

            if(storeData.pageType[i] == 0)
            {   // 장비
                WearablePage.Slots[posX, posY].Item = targetItem;
                WearablePage.Items.Add(WearablePage.Slots[posX, posY], targetItem);
                targetSlot = WearablePage.Slots[posX, posY];

                Vector2 slotParent = targetSlot.SlotRect.localScale;
                targetSlot.SlotRect.localScale = new Vector2(slotParent.x * targetSlot.Item.Width, slotParent.y * targetSlot.Item.Height);
                WearablePage.SlotActive(posX, posY, targetSlot.Item, false);
            }
            else if (storeData.pageType[i] == 1)
            {   // 소비
                ConsumablePage.Slots[posX, posY].Item = targetItem;
                ConsumablePage.Items.Add(ConsumablePage.Slots[posX, posY], targetItem);
                targetSlot = ConsumablePage.Slots[posX, posY];
                ConsumablePage.SlotActive(posX, posY, targetSlot.Item, false);
            }
            else//  if (storeData.pageType[i] == 2)
            {   // 판매 페이지
                CashPage.Slots[posX, posY].Item = targetItem;
                CashPage.Items.Add(CashPage.Slots[posX, posY], targetItem);
                targetSlot = CashPage.Slots[posX, posY];
                CashPage.SlotActive(posX, posY, targetSlot.Item, false);
            }
        }

    }
    #endregion
}

/// <summary>
///  파이어 베이스에 올릴 상점 아이템 데이터
/// </summary>
[Serializable]
public struct UploadStoreData
{
    public List<int> pageType;
    public List<int> id;
    public List<int> row;
    public List<int> col;

    public void SetData(int pageType, int id, int row, int col)
    {
        if (this.pageType == null)
        {
            this.pageType = new List<int>();
        }
        if (this.id == null)
        {
            this.id = new List<int>();
        }
        if (this.row == null)
        {
            this.row = new List<int>();
        }
        if (this.col == null)
        {
            this.col = new List<int>();
        }

        this.pageType.Add(pageType);
        this.id.Add(id);
        this.row.Add(row);
        this.col.Add(col);
    }

}

/// <summary>
/// 파이어 베이스에 올릴 상점 시간 데이터
/// </summary>
public struct UploadStoreTime
{
    public int remainingTime;
    public int closeTime;
}

[Serializable]
public struct PageButtons
{
    public Image buttonImage;
    public ItemBaseType pageType;
}