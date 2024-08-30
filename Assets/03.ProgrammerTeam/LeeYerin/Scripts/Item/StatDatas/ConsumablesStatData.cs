using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 개발자: 이예린 / 소비 아이템의 데이터를 저장하는 구조체
/// </summary>
[Serializable]
public struct ConsumablesStatData : ICsvReadable, ICreatableSO
{
    public int id;
    public string name;
    public ConsumablesType type;
    public Rank rank;
    public int xLength;
    public int yLength;
    public int buyPrice;
    public int sellPrice;
    public int healPerSec;
    public IncrementalStat stat;
    public float statUp;
    public int duration;
    public float heal;
    public int casting;
    public int detailTextid;
    public int image;
    public int equipSoundId;
    public int useSoundId;

    public void CsvRead(string[] elements)
    {
        id = int.Parse(elements[0]);
        name = elements[1];
        type = (ConsumablesType)int.Parse(elements[2]);
        rank = (Rank)int.Parse(elements[3]);
        xLength = int.Parse(elements[4]);
        yLength = int.Parse(elements[5]);
        buyPrice = int.Parse(elements[6]);
        sellPrice = int.Parse(elements[7]);
        healPerSec = int.Parse(elements[8]);
        stat = (IncrementalStat)int.Parse(elements[9]);
        statUp = int.Parse(elements[10]);
        duration = int.Parse(elements[11]);
        heal = float.Parse(elements[12]);
        casting = int.Parse(elements[13]);
        detailTextid = int.Parse(elements[14]);
        image = int.Parse(elements[15]);
        equipSoundId = int.Parse(elements[16]);
        useSoundId = int.Parse(elements[17]);
    }

    /// <summary>
    /// ConsumablesType Item ScriptableObject 생성해주는 함수
    /// </summary>
    public void CreateScriptableObject()
    {
        ConsumablesData item = ScriptableObject.CreateInstance<ConsumablesData>();

        item.Id = id;
        item.Name = name;
        item.ItemType = ItemBaseType.Consumable;
        item.ConsumablesType = type;
        item.Rank = rank;
        item.Width = xLength;
        item.Height = yLength;
        item.BuyPrice = buyPrice;
        item.SellPrice = sellPrice;
        item.DetailTextId = detailTextid;
        item.ItemImage = Resources.Load<Sprite>(CsvParser.Instance.returnPath(image));
        item.EquipSoundID = equipSoundId;
        item.UseSoundID = useSoundId;
        //TODO... 이미지 Sprite 넣어주는 작업 필요
        switch (type) 
        {
            case ConsumablesType.HpPotion:
                item.HealPerSec = healPerSec;
                item.Duration = duration;

                break;
            case ConsumablesType.Food:
                item.Heal = heal;
                item.Casting = casting;

                break;
            case ConsumablesType.Buffpotion:
                item.Stat = stat;
                item.StatUp = statUp;
                item.Duration = duration;

                break;
        }

        AddItemList(item);
#if UNITY_EDITOR
        AssetDatabase.CreateAsset(item, $"Assets/Resources/9.ScriptableObj/Item/Consumable/{item.Id}.asset");
#endif
    }

    public void AddItemList(ItemData item)
    {
        CsvParser.Instance.ItemList.ItemDic.Add(item.Id, item);
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
    }
}
