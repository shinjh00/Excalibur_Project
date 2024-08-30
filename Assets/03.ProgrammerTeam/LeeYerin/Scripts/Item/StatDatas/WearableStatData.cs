using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 개발자: 이예린 / 장비 아이템의 데이터를 저장하는 구조체
/// </summary>
[Serializable]
public struct WearableStatData : ICsvReadable, ICreatableSO
{
    public int id;
    public string name;
    public WearablesType type;
    public Rank rank;
    public int xLength;
    public int yLength;
    public int buyPrice;
    public int sellPrice;
    public IncrementalStat stat1;
    public float stat1Value;
    public IncrementalStat stat2;
    public float stat2Value;
    public IncrementalStat stat3;
    public float stat3Value;
    public IncrementalStat stat4;
    public float stat4Value;
    public int detailTextid;
    public int image;
    public int equipSoundId;

    public void CsvRead(string[] elements)
    {
        id = int.Parse(elements[0]);
        name = elements[1];
        type = (WearablesType)int.Parse(elements[2]);
        rank = (Rank)int.Parse(elements[3]);
        xLength = int.Parse(elements[4]);
        yLength = int.Parse(elements[5]);
        buyPrice = int.Parse(elements[6]);
        sellPrice = int.Parse(elements[7]);
        if (int.Parse(elements[8]) == -9999)
        {
            stat1 = IncrementalStat.Null;
        }
        else
        {
            stat1 = (IncrementalStat)int.Parse(elements[8]);
        }
        stat1Value = float.Parse(elements[9]);
        if (int.Parse(elements[10]) == -9999)
        {
            stat2 = IncrementalStat.Null;
        }
        else
        {
            stat2 = (IncrementalStat)int.Parse(elements[10]);
        }
        stat2Value = float.Parse(elements[11]);
        if (int.Parse(elements[12]) == -9999)
        {
            stat3 = IncrementalStat.Null;
        }
        else
        {
            stat3 = (IncrementalStat)int.Parse(elements[12]);
        }
        stat3Value = float.Parse(elements[13]);
        if (int.Parse(elements[14]) == -9999)
        {
            stat4 = IncrementalStat.Null;
        }
        else
        {
            stat4 = (IncrementalStat)int.Parse(elements[14]);
        }
        stat4Value = float.Parse(elements[15]);
        detailTextid = int.Parse(elements[16]);
        image = int.Parse(elements[17]);
        equipSoundId = int.Parse(elements[18]);

    }
    public void CreateScriptableObject()
    {
        WearableData item = ScriptableObject.CreateInstance<WearableData>();

        item.Id = id;
        item.Name = name;
        item.ItemType = ItemBaseType.Wearable;
        item.WearablesType = type;
        item.Rank = rank;
        item.Width = xLength;
        item.Height = yLength;
        item.BuyPrice = buyPrice;
        item.SellPrice = sellPrice;
        item.DetailTextId = detailTextid;
        item.ItemImage = Resources.Load<Sprite>(CsvParser.Instance.returnPath(image));
        item.EquipSoundID = equipSoundId;

        if (!stat1.Equals(IncrementalStat.Null))
        {
            item.Stats.Add(stat1);
            item.StatValues.Add(stat1Value);
        }
        if (!stat2.Equals(IncrementalStat.Null))
        {
            item.Stats.Add(stat2);
            item.StatValues.Add(stat2Value);
        }
        if (!stat3.Equals(IncrementalStat.Null))
        {
            item.Stats.Add(stat3);
            item.StatValues.Add(stat3Value);
        }
        if (!stat4.Equals(IncrementalStat.Null))
        {
            item.Stats.Add(stat4);
            item.StatValues.Add(stat4Value);
        }
        AddItemList(item);
#if UNITY_EDITOR
        AssetDatabase.CreateAsset(item, $"Assets/Resources/9.ScriptableObj/Item/Wearable/{item.Id}.asset");
#endif
    }

    public void AddItemList(ItemData item)
    {
        CsvParser.Instance.ItemList.ItemDic.Add(item.Id, item);
        SpawnItemList.Instance.SpawnItemsList[(int)item.Rank].spawnItemList.Add(item);
    }
}