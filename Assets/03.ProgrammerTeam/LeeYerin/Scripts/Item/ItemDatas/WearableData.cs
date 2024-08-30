using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WearableData : ItemData
{
    [Header("Wearable Information")]
    [SerializeField] WearablesType wearablesType;
    [SerializeField] int buyPrice;
    [SerializeField] List<IncrementalStat> stats = new List<IncrementalStat>();
    [SerializeField] List<float> statValues = new List<float>();
    [SerializeField] int equipSoundId;

    #region Get_Set
    public WearablesType WearablesType { get { return wearablesType; } set { wearablesType = value; } }
    public int BuyPrice { get { return buyPrice; } set { buyPrice = value; } }
    public List<IncrementalStat> Stats { get { return stats; } set { stats = value; } }
    public List<float> StatValues { get { return statValues; } set { statValues = value; } }
    public int EquipSoundID { get { return equipSoundId; } set { equipSoundId = value; } }
    #endregion
}