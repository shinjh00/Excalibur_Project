using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// 개발자: 이예린 / ScriptableObject 생성 가능 여부
/// </summary>
public interface ICreatableSO
{
    /// <summary>
    /// ScriptableObject 생성해주는 함수
    /// </summary>
    public void CreateScriptableObject();

    /// <summary>
    /// 만들어진 데이터 Item List에 넣어주는 작업 진행하는 메소드
    /// </summary>
    public void AddItemList(ItemData item);
}