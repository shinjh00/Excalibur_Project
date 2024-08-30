using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 개발자: 이예린 / 창고 관리 클래스
/// </summary>
public class Storage : MonoBehaviour
{
    [SerializeField] GameObject storagePagePrefab;
    [SerializeField] List<StoragePage> storagePages = new List<StoragePage>();
    [SerializeField] Transform pages;
    [SerializeField] TMP_Text pageText;
    public GameObject UI;
    public StoragePage StoragePage { get { return storagePages[currentPage]; } }

    [SerializeField] int currentPage;

    [SerializeField] UploadStorageData saveData;

    public Action<bool> UIChange;

    #region Unity Event
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => (Inventory.Instance != null && Inventory.Instance.Player != null));
        Inventory.Instance.Storage = this;
        currentPage = 0;
        UpDatePage();
        storagePages[currentPage].gameObject.SetActive(true);

        UIChange += UpLoadData;
    }

    public void UpLoadData(bool value)
    {
        if (!value)
        {
            FirebaseManager.Instance.UpLoadStore_StorageData();
        }
    }

    #endregion

    #region Page
    /// <summary>
    /// 창고 페이지 올리는 메소드
    /// </summary>
    public void PageUp()
    {
        // 페이지를 더 올릴 수 없다면
        if (currentPage + 1 >= storagePages.Count)
        {
            return;
        }

        // 현재 보여주던 창고 페이지 비활성화
        storagePages[currentPage++].gameObject.SetActive(false);
        // 다음 창고 페이지 활성화
        storagePages[currentPage].gameObject.SetActive(true);
        // 창고 페이지 Text 업데이트
        pageText.text = $"{currentPage + 1}/{storagePages.Count}";
    }

    /// <summary>
    /// 창고 페이지 내리는 메소드
    /// </summary>
    public void PageDown()
    {
        // 페이지를 더 내릴 수 없다면
        if (currentPage - 1 < 0)
        {
            return;
        }

        // 현재 보여주던 창고 페이지 비활성화
        storagePages[currentPage--].gameObject.SetActive(false);
        // 다음 창고 페이지 활성화
        storagePages[currentPage].gameObject.SetActive(true);
        // 창고 페이지 Text 업데이트
        UpDatePage();
    }

    /// <summary>
    /// 창고 페이지 추가
    /// </summary>
    public void PageAdd()
    {
        GameObject addedPage = Instantiate(storagePagePrefab);
        addedPage.transform.parent = pages.transform;
        addedPage.transform.localPosition = Vector2.zero;
        addedPage.transform.localScale = Vector2.one;

        storagePages.Add(addedPage.GetComponent<StoragePage>());

        // 창고 페이지 Text 업데이트
        UpDatePage();
    }
    #endregion

    #region Text
    private void UpDatePage()
    {
        pageText.text = $"{currentPage + 1}/{storagePages.Count}";
    }
    #endregion

    #region Upload Data
    /// <summary>
    /// 창고 아이템 관련 데이터 업로드해야할 시 호출하는 메소드. 창고의 상태 테이터 리스트로 반환
    /// </summary>
    /// <returns></returns>
    public UploadStorageData GetUploadStoreDatas()
    {
        UploadStorageData uploadStorageDatas = new UploadStorageData();

        for (int i = 0; i < storagePages.Count - 1; i++)
        {
            foreach (var item in storagePages[i].Items)
            {
                uploadStorageDatas.SetData(item.Value.Id, i, item.Key.Row, item.Key.Col);
            }
        }

        return uploadStorageDatas;
    }

    public void DownLoadStoreagData()
    {
        saveData = new UploadStorageData();

        FirebaseManager.DB
            .GetReference("PlayerDataTable")
            .Child(FirebaseManager.Instance.curPlayer)
            .Child("StorageData")
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if(task.IsCanceled)
                {
                    return;
                }
                else if(task.IsFaulted)
                {
                    return;
                }

                DataSnapshot snapShot = task.Result;

                if(snapShot.Exists)
                {
                    saveData = JsonUtility.FromJson<UploadStorageData>(snapShot.GetRawJsonValue());
                    FillStorage(saveData);
                }
            });
    }

    private void FillStorage(UploadStorageData saveData)
    {
        int max = 0;

        for(int i = 0; i < saveData.itemPage.Count; i++)
        {
            if(max < saveData.itemPage[i])
            {   // 1. 최대 페이지 저장
                max = saveData.itemPage[i];
            }
        }
        max++;

        for (int i = 0; i < storagePages.Count - 1; i++)
        {
            storagePages[i].OpenPage();
        }

        for(int i = 0; i < saveData.itemId.Count; i++)
        {
            ItemData targetItem = CsvParser.Instance.ItemList.ItemDic[saveData.itemId[i]];
            int posX = saveData.posX[i];
            int posY = saveData.posY[i];

            storagePages[saveData.itemPage[i]].Slots[posX, posY].Item = targetItem;
            storagePages[saveData.itemPage[i]].Items.Add(storagePages[saveData.itemPage[i]].Slots[posX, posY], targetItem);

            Vector2 slotParent = storagePages[saveData.itemPage[i]].Slots[posX, posY].SlotRect.localScale;
            storagePages[saveData.itemPage[i]].Slots[posX, posY].SlotRect.localScale = new Vector2(slotParent.x * storagePages[saveData.itemPage[i]].Slots[posX, posY].Item.Width, slotParent.y * storagePages[saveData.itemPage[i]].Slots[posX, posY].Item.Height);
            storagePages[saveData.itemPage[i]].SlotActive(posX, posY, storagePages[saveData.itemPage[i]].Slots[posX, posY].Item, false);
        }
    }

    #endregion
}

[Serializable]
public struct UploadStorageData
{
    public List<int> itemId;
    public List<int> itemPage;
    public List<int> posX;
    public List<int> posY;

    public void SetData(int itemId, int itemPage, int posX, int posY)
    {
        if (this.itemId == null)
        {
            this.itemId = new List<int>();
        }
        if (this.posX == null)
        {
            this.posX = new List<int>();
        }
        if (this.posY == null)
        {
            this.posY = new List<int>();
        }
        if (this.itemPage == null)
        {
            this.itemPage = new List<int>();
        }

        this.itemId.Add(itemId);
        this.posX.Add(posX);
        this.posY.Add(posY);
        this.itemPage.Add(itemPage);
    }
}