using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 개발자: 이예린 / 환급 아이템의 데이터를 저장하는 구조체
/// </summary>
public struct CashStatData : ICsvReadable, ICreatableSO
{
    public int id;
    public string name;
    public Rank rank;
    public int xLength;
    public int yLength;
    public int sellPrice;
    public int detailTextid;
    public int image;

    public void CsvRead(string[] elements)
    {
        id = int.Parse(elements[0]);
        name = elements[1];
        rank = (Rank)int.Parse(elements[2]);
        xLength = int.Parse(elements[3]);
        yLength = int.Parse(elements[4]);
        sellPrice = int.Parse(elements[5]);
        detailTextid = int.Parse(elements[6]);
        image = int.Parse(elements[7]);

    }
    public void CreateScriptableObject()
    {
        CashData item = ScriptableObject.CreateInstance<CashData>();

        item.Id = id;
        item.Name = name;
        item.ItemType = ItemBaseType.Cash;
        item.Rank = rank;
        item.Width = xLength;
        item.Height = yLength;
        item.SellPrice = sellPrice;
        item.DetailTextId = detailTextid;
        item.ItemImage = Resources.Load<Sprite>(CsvParser.Instance.returnPath(image));
        //TODO... 이미지 Sprite 넣어주는 작업 필요
        AddItemList(item);
#if UNITY_EDITOR
        AssetDatabase.CreateAsset(item, $"Assets/Resources/9.ScriptableObj/Item/Cash/{item.Id}.asset");
#endif
    }

    public void AddItemList(ItemData item)
    {
        CsvParser.Instance.ItemList.ItemDic.Add(item.Id, item);
        SpawnItemList.Instance.SpawnItemsList[(int)item.Rank + 16].spawnItemList.Add(item);
    }
}