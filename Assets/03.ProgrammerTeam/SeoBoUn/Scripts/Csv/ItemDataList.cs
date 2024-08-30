using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 아이템 데이터 리스트
/// 모든 아이템을 딕셔너리(ItemDic)에 저장하고 id를 통해 해당 아이템 데이터를 반환
/// <br/>
/// ex) ItemDic[200001] => ID가 200001인 아이템 데이터를 반환 
/// </summary>
public class ItemDataList : MonoBehaviour
{
    Dictionary<int, ItemData> itemDic = new Dictionary<int, ItemData>();
    Dictionary<int, Sprite> spriteDic;
    [SerializeField] ItemDataStruct[] itemDataStruct;
    [SerializeField] List<Sprite> spriteList;
    [SerializeField] List<ItemData> itemData = new List<ItemData>();

    public Dictionary<int, ItemData> ItemDic { get { return itemDic; } }
    public Dictionary<int, Sprite> SpriteDic { get { return spriteDic; } }

    private void Start()
    {
        spriteDic = new Dictionary<int, Sprite>();
        spriteList = new List<Sprite>();
    }

    public void AddItem()
    {
        foreach(ItemData item in ItemDic.Values)
        {
            itemData.Add(item);
        }
    }

    /*/// <summary>
    /// 개발자: 이예린/ 생성한 아이템 스크립터블 오브젝트 데이터 리스트에 넣어주는 작업 진행하는 메소드
    /// </summary>
    /// <param name="item"></param>
    public void AddItemList(ItemData item)
    {
        switch (item.ItemType)
        {
            case ItemBaseType.Wearable:
                itemDic.Add(item.Id, item);
                SpawnItemList.Instance.SpawnItemsList[(int)item.Rank].spawnItemList.Add(item);
                break;
            case ItemBaseType.Cash:
                itemDic.Add(item.Id, item);
                SpawnItemList.Instance.SpawnItemsList[(int)item.Rank + 16].spawnItemList.Add(item);
                break;
            case ItemBaseType.Consumable:
                itemDic.Add(item.Id, item);
                switch ((item as ConsumablesData).ConsumablesType)
                {
                    case ConsumablesType.HpPotion:
                        SpawnItemList.Instance.SpawnItemsList[(int)item.Rank + 4].spawnItemList.Add(item);
                        break;
                    case ConsumablesType.Food:
                        SpawnItemList.Instance.SpawnItemsList[(int)item.Rank + 8].spawnItemList.Add(item);
                        break;
                    case ConsumablesType.Buffpotion:
                        SpawnItemList.Instance.SpawnItemsList[(int)item.Rank + 12].spawnItemList.Add(item);
                        break;
                }
                break;
        }
    }*/
}

[Serializable]
public struct ItemDataStruct
{
    public int id;
    public ItemData itemData;
}