using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상점 아이템 데이블 구조체
/// </summary>
public struct StoreItemData : ICsvReadable
{
    public ItemBaseType type;
    public Rank rank;
    public float prob;
    public void CsvRead(string[] elements)
    {
        type = (ItemBaseType)int.Parse(elements[0]);
        rank = (Rank)int.Parse(elements[1]);
        prob = float.Parse(elements[2]);
    }
}