using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자: 이예린 / 소비 아이템에 대한 데이터를 가질 스크립터블 오브젝트
/// </summary>
public class ConsumablesData : ItemData
{
    [Header("Consumable Item Information")]
    [SerializeField] ConsumablesType consumablesType;
    [SerializeField] int buyPrice;
    [SerializeField] int healPerSec;
    [SerializeField] float heal;
    [SerializeField] IncrementalStat stat;
    [SerializeField] float statUp;
    [SerializeField] int duration;
    [SerializeField] int casting;
    [SerializeField] int equipSoundId;
    [SerializeField] int useSoundId;

    #region Get_Set
    public ConsumablesType ConsumablesType { get { return consumablesType; } set { consumablesType = value; } }
    public int BuyPrice { get { return buyPrice; } set { buyPrice = value; } }
    public int HealPerSec { get { return healPerSec; } set { healPerSec = value; } }
    public float Heal { get { return heal; } set { heal = value; } }
    public IncrementalStat Stat { get { return stat; } set { stat = value; } }
    public float StatUp { get { return statUp; } set { statUp = value; } }
    public int Duration { get { return duration; } set { duration = value; } }
    public int Casting { get { return casting; } set { casting = value; } }
    public int EquipSoundID { get { return equipSoundId; } set { equipSoundId = value; } }
    public int UseSoundID { get { return useSoundId; } set { useSoundId = value; } }
    #endregion
}