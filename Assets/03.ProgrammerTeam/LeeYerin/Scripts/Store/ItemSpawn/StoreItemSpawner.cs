using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자: 이예린/ 상점 페이지(장비, 소비품) 아이템 스폰 및 배치하는 기능 구현
/// </summary>
public class StoreItemSpawner : MonoBehaviour
{
    [SerializeField] ItemBaseType spawnItemType;
    [SerializeField] List<float> probs;
    StorePage storePage;

    #region Unity Event
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => (Inventory.Instance.Store != null) && Inventory.Instance.Store.StorePagesCount == 3);

        SpawnItemInStore();

        yield return new WaitUntil(() => (storePage.Slots != null));

        SetItem();  // 아이템 슬롯에 배치
    }
    #endregion

    #region Spawn item

    /// <summary>
    /// 상점 페이지로 아이템 스폰 진행하는 함수
    /// </summary>
    public void SpawnItemInStore()
    {
        if (storePage != null && storePage.SpwanItems.Count != 0)
        {
            storePage.SpwanItems.Clear();
        }

        switch (spawnItemType)  // 페이지 종류에 따라
        {
            case ItemBaseType.Wearable:
                if (storePage == null)
                {
                    storePage = Inventory.Instance.Store.WearablePage;
                }
                // 장비 아이템 데이터 10개 스폰
                for (; storePage.SpwanItems.Count < 10;)
                {
                    EquipmentSpawn();
                }
                break;
            case ItemBaseType.Consumable:
                if (storePage == null)
                {
                    storePage = Inventory.Instance.Store.ConsumablePage;
                }
                // 장비 아이템 데이터 5개 스폰
                for (; storePage.SpwanItems.Count < 5;)
                {
                    ConsumableSpawn();
                }
                break;
            default:
                if (storePage == null)
                {
                    storePage = Inventory.Instance.Store.CashPage;
                }
                break;
        }
    }

    /// <summary>
    /// 장비 테이블에 따른 장비 아이템 스폰결정 메소드
    /// </summary>
    private void EquipmentSpawn()
    {
        float rand = Random.Range(0, 1f);
        float totalWeight = 0f;
        Rank spawnRank = Rank.None;

        // 드랍 테이블의 장비 가중치에 대한 확률값을 전부 확인하면서
        for (int i = 0; i < probs.Count; i++)
        {
            totalWeight += probs[i];
            if (rand <= totalWeight)
            {   // 하나의 장비 랭크 결정
                spawnRank = (Rank)i;
                break;
            }
        }

        // 만약 저 위의 확률을 전부 뚫지 못했다면 장비는 없음
        if (spawnRank == Rank.None)
        {
            return;
        }

        List<ItemData> curList = SpawnItemList.Instance.SpawnItemsList[(int)spawnRank + (int)SpawnItemType.Equipment * (int)Rank.Size].spawnItemList;

        if (curList == null)
        {
            return;
        }

        if (curList.Count == 0)
        {
            return;
        }

        storePage.SpwanItems.Add(curList[Random.Range(0, curList.Count)]);
    }

    /// <summary>
    /// 소비품 테이블에 따른 소비 아이템 스폰결정 메소드
    /// </summary>
    private void ConsumableSpawn()
    {
        float rand = Random.Range(0, 1f);
        float totalWeight = 0f;
        Rank spawnRank = Rank.None;

        // 드랍 테이블의 소모품 가중치에 대한 확률값을 전부 확인하면서
        for (int i = 0; i < probs.Count; i++)
        {
            totalWeight += probs[i];
            if (rand <= totalWeight)
            {   // 하나의 소비품 랭크 결정
                spawnRank = (Rank)i;
                break;
            }
        }

        // 만약 저 위의 확률을 전부 뚫지 못했다면 장비는 없음
        if (spawnRank == Rank.None)
        {
            return;
        }

        List<ItemData> curList;

        switch (Random.Range(0, 3))
        {
            case 0:
                curList = SpawnItemList.Instance.SpawnItemsList[(int)spawnRank + (int)SpawnItemType.Potion * (int)Rank.Size].spawnItemList;
                break;
            case 1:
                curList = SpawnItemList.Instance.SpawnItemsList[(int)spawnRank + (int)SpawnItemType.Food * (int)Rank.Size].spawnItemList;
                break;
            default:
                curList = SpawnItemList.Instance.SpawnItemsList[(int)spawnRank + (int)SpawnItemType.BuffPotion * (int)Rank.Size].spawnItemList;
                break;
        }

        if (curList == null)
        {
            return;
        }

        if (curList.Count == 0)
        {
            return;
        }

        storePage.SpwanItems.Add(curList[Random.Range(0, curList.Count)]);
    }
    #endregion

    #region Set item in slot
    /// <summary>
    /// 상점 페이지에 아이템 배치하는 메소드
    /// </summary>
    public void SetItem()
    {
        if (storePage.Items.Count != 0)
        {
            foreach (var item in storePage.Items)
            {
                item.Key.SlotRect.localScale = Vector2.one;
                storePage.SlotActive(item.Key.Row, item.Key.Col, item.Key.Item, true);
                item.Key.Item = null;
            }
            storePage.Items.Clear();
        }

        // 판매 페이지일 경우 아이템 스폰하지 않음
        if (storePage.Slots == null || spawnItemType.Equals(ItemBaseType.Cash))
        {
            return;
        }

        for (int count = 0; count < storePage.SpwanItems.Count; count++)
        {
            for (int i = 0; i < storePage.Row; i++)
            {
                for (int j = 0; j < storePage.Col; j++)
                {
                    if (count >= storePage.SpwanItems.Count)
                    {
                        return;
                    }

                    if (storePage.CheckItemDropable(i, j, storePage.SpwanItems[count]))
                    {
                        Vector2 slotParent = storePage.Slots[i, j].SlotRect.localScale;
                        storePage.Slots[i, j].SlotRect.localScale
                            = new Vector2(slotParent.x * storePage.SpwanItems[count].Width, slotParent.y * storePage.SpwanItems[count].Height);
                        storePage.SlotActive(i, j, storePage.SpwanItems[count], false);
                        storePage.Slots[i, j].Item = storePage.SpwanItems[count++];
                        storePage.Items.Add(storePage.Slots[i, j], storePage.Slots[i, j].Item);
                    }
                }
            }
        }
    }
    #endregion
}
