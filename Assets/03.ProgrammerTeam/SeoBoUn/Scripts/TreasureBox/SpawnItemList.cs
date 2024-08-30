using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 개발자 : 서보운
/// 아이템 목록을 들고있을 클래스
/// </summary>
public class SpawnItemList : MonoBehaviour
{
    [Tooltip("장비, 포션, 식량, 버프포션, 환금")]
    [SerializeField] List<SpawnItem> spawnItems;
    private static SpawnItemList instance;
    public static SpawnItemList Instance { get { return instance; } }
    public List<SpawnItem> SpawnItemsList { get { return spawnItems; } }


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

}
// Rank + SpawnItemType * Rank.Size

// 0 -> equipment normal...
// 1 -> equipment rare...
// 2
// 3
// 4 -> potion normal...

/// <summary>
/// 스폰할 아이템 타입(장비, 포션, 식량, 버프포션, 환금)
/// </summary>
public enum SpawnItemType
{
    Equipment,
    Potion,
    Food,
    BuffPotion,
    Cashback,
    Size
}

/// <summary>
/// 스폰할 아이템 모음( Rank + SpawnItemType * Rank.Size )
/// <br/>spawnItemsList[0] -> normal 장비
/// spawnItemList[2] -> artifact 장비
/// spawnItemList[5] -> normal 포션...
/// </summary>
[Serializable]
public struct SpawnItem
{
    public List<ItemData> spawnItemList;
}