using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필 / 가중치에 따른 랜덤선택(높으면 잘뽑힘)
/// </summary>
/// <typeparam name="T"></typeparam>
public class WeightedRandom<T>
{
    public Dictionary<T,float> itemDic = new Dictionary<T,float>();
    float totalWeight = 0f;

    public void Add(T item, float weight )
    {
        itemDic.Add(item, weight);
        totalWeight += weight;
    }

    public void Remove(T item )
    {
        float weight;
        if(itemDic.TryGetValue(item, out weight) )
        {
            totalWeight -= weight;
            itemDic.Remove(item);
        }
    }
    public void Clear()
    {
        itemDic.Clear();
        totalWeight = 0f;
    }

    public bool ChangeWeight(T item, float weight )
    {
        if ( itemDic.ContainsKey(item) )
        {
            itemDic [item] = weight;
            return true;
        }
        return false;
    }
    /// <summary>
    /// 각 항목의 가중치를 전체 가중치로 나누어 비율을 구한 뒤, 무작위로 선택된 값을 가중치의 누적 비율과 비교하여 항목을 선택
    /// </summary>
    /// <returns></returns>
    public T GetItem()
    {
        if ( totalWeight <= 0 )
            return default(T);

        Dictionary<T,float> ratioDic = new Dictionary<T, float> ();
        foreach(KeyValuePair<T,float> pair in itemDic)          //각 항목을
            ratioDic.Add(pair.Key, pair.Value / totalWeight);   //전체 가중치에서 키의 가중치를 나눈값인 비율딕셔너리에 추가

        float pivot = Random.value;                     //0부터 1 난수
        float acc = 0f;

        foreach(KeyValuePair <T,float> pair in ratioDic)   // 각 항목의 비율을 반복하여 누적 확률을 계산하고, 피벗값보다 크거나 같은 누적 확률을 가진 항목을 선택
        {
            acc += pair.Value;                              //벨류가 낮더라도 반복문 돌면서 확률이 누적되면 픽업 될 가능성이 존재함
            if(pivot<=acc)
                return pair.Key;
        }
        return default(T);
    }
}
