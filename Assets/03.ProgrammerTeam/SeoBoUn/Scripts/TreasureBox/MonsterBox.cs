using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 몬스터 박스
/// </summary>
public class MonsterBox : DropBox
{
    protected override void Start()
    {
        // pv = GetComponentInParent<PhotonView>();
        boxData = CsvParser.Instance.BoxDic[boxID];
        boxData.dropTable = CsvParser.Instance.BoxDropTableDic[boxData.dropTableID];
        
        if (PhotonNetwork.IsMasterClient)
        {
            // SetBoxTable();
        }
    }
}
