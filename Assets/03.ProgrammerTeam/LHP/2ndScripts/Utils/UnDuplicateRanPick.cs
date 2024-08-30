using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

/// <summary>
/// 리스트 중에서 랜덤으로 하나 가져오고 리스트에서 뺌
/// </summary>
/// <typeparam name="T"></typeparam>
public class UnDuplicateRanPick<T>
{
    public List<T> itemList = new List<T>();
    /// <summary>
    ///  리스트를 초기화하고 다시 리스트에 추가
    /// </summary>
    /// <param name="items"></param>
    public void SetItem( List<T> items )
    {
        itemList.Clear();
        AddItem(items);
    }
    public void AddItem (List<T> items )
    {
        for ( int i = 0; i < items.Count; i++ )
        {
            itemList.Add(items [i]);
        }
    }/// <summary>
     /// 랜덤으로 하나를 가져오고 리스트에서 지워서 중복되지 않게 함
     /// </summary>
     /// <returns></returns>
    public T GetItem()
    {
        if ( itemList.Count == 0 )
            return default(T);
        int ran = Random.Range(0, itemList.Count);

        T item = itemList [ran];
        itemList.RemoveAt(ran);
        return item;
    }/// <summary>
     /// 비었으면 true반환
     /// </summary>
     /// <returns></returns>
    public bool IsEmpty()
    {
        return itemList.Count == 0;
    }
    
    
}
