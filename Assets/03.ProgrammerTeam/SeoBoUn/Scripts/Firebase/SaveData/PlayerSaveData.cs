using System;
using UnityEngine.AI;

/// <summary>
/// 창고 세이브 데이터 구조체(아이템 하나를 위해 사용)
/// <br/> itemID, itemPage, posX, posY 
/// </summary>
[Serializable]
public struct StorageSaveData
{
    public int itemID;
    public int itemPage;
    public int posX;
    public int posY;

    public StorageSaveData(int itemID, int itemPage, int posX, int posY)
    {
        this.itemID = itemID;
        this.itemPage = itemPage;
        this.posX = posX;
        this.posY = posY;
    }
}
