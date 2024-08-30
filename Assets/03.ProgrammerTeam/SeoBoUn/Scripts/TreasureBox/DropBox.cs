using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 드랍 박스(보물상자, 몬스터, 플레이어) 베이스 스크립트
/// <br/>보물상자나 몬스터가 사망했을 때 나올 파밍의 드랍 테이블을 결정
/// </summary>
public class DropBox : MonoBehaviourPun, IDamageable
{
    [Tooltip("박스 ID")]
    [SerializeField] protected int boxID;
    [Tooltip("박스 ID에 따른 데이터 결정(csv)")]
    [SerializeField] protected BoxData boxData;

    [Tooltip("박스 안의 아이템")]
    [SerializeField] protected List<ItemData> boxItems;
    [Tooltip("박스 안의 슬롯 정보")]
    [SerializeField] protected List<Slot> slotData;
    [Tooltip("박스 슬롯 정보(열)")]
    [SerializeField] protected int[] slotRow;
    [Tooltip("박스 슬롯 정보(행)")]
    [SerializeField] protected int[] slotCol;
    [Tooltip("박스 아이템 목록(ID값)")]
    [SerializeField] protected int[] itemId;

    bool isOpen;
    int[] curCount;

    public BoxData Box { get { return boxData; } }
    public List<ItemData> BoxItems { get { return boxItems; } }
    public List<Slot> SlotDatas { get { return slotData; } }
    public int[] SlotRow { get { return slotRow; } }
    public int[] SlotCol { get { return slotCol; } }
    public bool IsOpen { get { return isOpen; } set { isOpen = value; } }

    protected virtual void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // SetBoxTable();
        }
    }

    /// <summary>
    /// 개발자 : 서보운
    /// 외부에서 스폰을 진행할 메소드(박스 안의 아이템 설정 및 배치)
    /// </summary>
    public void SetBoxTable(int id)
    {
        boxData = CsvParser.Instance.BoxDic[id];
        boxData.dropTable = CsvParser.Instance.BoxDropTableDic[boxData.dropTableID];
        boxItems = new List<ItemData>();

        StartCoroutine(BoxSpawn());
    }

    /// <summary>
    /// 열린 뒤 닫힐 때 해당 슬롯과 아이템을 저장할 메소드
    /// </summary>
    /// <param name="slots"></param>
    public void SetData(ItemBoxSlot[,] slots)
    {
        photonView.RPC("SetBoxClear", RpcTarget.All, slots.GetLength(0) * slots.GetLength(1), slots.GetLength(0) * slots.GetLength(1));

        int index = 0;
        foreach (ItemBoxSlot slot in slots)
        {
            if (slot.Item != null)
            {
                slotData.Add(slot);
                boxItems.Add(slot.Item);

                slotCol[index] = slot.Col;
                slotRow[index] = slot.Row;

                index++;
            }
        }

        itemId = new int[boxItems.Count];
        boxData.boxItemsID.Clear();
        for(int i = 0; i < boxItems.Count; i++)
        {
            itemId[i] = boxItems[i].Id;
            boxData.boxItemsID.Add(boxItems[i].Id);
        }

        photonView.RPC("DropBoxSync", RpcTarget.Others, slotCol, slotRow, itemId);
    }

    /// <summary>
    /// 드랍박스 동기화 메소드
    /// </summary>
    /// <param name="data"></param>
    [PunRPC]
    private void DropBoxSync(int[] col, int[] row, int[] itemId)
    {
        this.itemId = new int[itemId.Length];

        this.slotCol = col;
        this.slotRow = row;
        this.itemId = itemId;

        for(int i = 0; i < itemId.Length; i++)
        {
            boxItems.Add(CsvParser.Instance.ItemList.ItemDic[itemId[i]]);
        }
    }

    [PunRPC]
    private void SetBoxClear(int col, int row)
    {
        boxItems.Clear();
        slotData.Clear();
        itemId = null;
        slotCol = new int[col];
        slotRow = new int[row];
    }

    /// <summary>
    /// 내부 실제 박스 스폰 메소드
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator BoxSpawn()
    {
        boxItems = new List<ItemData>();
        curCount = new int[(int)SpawnItemType.Size];

        yield return new WaitForSeconds(0.1f);
        EquipmentSpawn();
        PotionSpawn();
        FoodSpawn();
        BuffPotionSpawn();
        CashbackSpawn();

        if (boxData.dropTable.isConfirm)
        {
            ConfirmSpawn();
        }

        itemId = new int[boxItems.Count];
        boxData.boxItemsID.Clear();
        for (int i = 0; i < boxItems.Count; i++)
        {
            itemId[i] = boxItems[i].Id;
            boxData.boxItemsID.Add(boxItems[i].Id);
        }

        // FirebaseManager.Instance.Link.SetStructData(boxData);
        photonView.RPC("BoxSpawnSnyc", RpcTarget.Others, itemId);
    }

    /// <summary>
    /// 박스 스폰 동기화
    /// </summary>
    /// <param name="itemId"></param>
    [PunRPC]
    private void BoxSpawnSnyc(int[] itemId)
    {
        boxItems.Clear();
        boxData.boxItemsID.Clear();
        this.itemId = new int[itemId.Length];
        for (int i = 0; i < itemId.Length; i++)
        {
            this.itemId[i] = itemId[i];
            boxItems.Add(CsvParser.Instance.ItemList.ItemDic[itemId[i]]);
            boxData.boxItemsID.Add(itemId[i]);
        }
    }

    /// <summary>
    /// 장비 테이블에 따른 장비 아이템 스폰결정 메소드
    /// </summary>
    protected void EquipmentSpawn()
    {
        // 만약 현재 스폰된 아이템 장비의 수가 드랍 테이블의 최대 수와 같다면
        if (curCount[(int)SpawnItemType.Equipment] == boxData.dropTable.maxEquipmentCount)
            return;

        float rand = UnityEngine.Random.Range(0, 1f);
        float totalWeight = 0f;
        Rank spawnRank = Rank.None;

        // 드랍 테이블의 장비 가중치에 대한 확률값을 전부 확인하면서
        for(int i = 0; i < boxData.dropTable.equipmentWeights.Length; i++)
        {
            totalWeight += boxData.dropTable.equipmentWeights[i];
            if(rand <= totalWeight)
            {   // 하나의 장비 랭크 결정
                spawnRank = (Rank)i;
                break;
            }
        }

        // 만약 저 위의 확률을 전부 뚫지 못했다면 장비는 없음
        if (spawnRank == Rank.None)
        {
            return;
        }
        // 장비 리스트중 하나의 장비 꺼내고
        List<ItemData> curList = SpawnItemList.Instance.SpawnItemsList[(int)spawnRank + (int)SpawnItemType.Equipment * (int)Rank.Size].spawnItemList;

        if (curList == null)
            return;

        // 박스 아이템에 추가하고 현재 소지 수 추가 
        boxItems.Add(curList[UnityEngine.Random.Range(0, curList.Count)]);
        curCount[(int)SpawnItemType.Equipment]++;
    }

    /// <summary>
    /// 포션 테이블에 따른 포션 아이템 스폰결정 메소드
    /// </summary>
    protected void PotionSpawn()
    {
        if (curCount[(int)SpawnItemType.Potion] == boxData.dropTable.maxPotionCount)
            return;

        float rand = UnityEngine.Random.Range(0, 1f);
        float totalWeight = 0f;
        Rank spawnRank = Rank.None;

        for(int i = 0; i < boxData.dropTable.potionWeights.Length; i++)
        {
            totalWeight += boxData.dropTable.potionWeights[i];
            if(rand <= totalWeight)
            {
                spawnRank = (Rank)i;
                break;
            }
        }

        if (spawnRank == Rank.None)
        {
            return;
        }

        List<ItemData> curList = SpawnItemList.Instance.SpawnItemsList[(int)spawnRank + (int)SpawnItemType.Potion * (int)Rank.Size].spawnItemList;

        if (curList.Count == 0)
            return;

        boxItems.Add(curList[UnityEngine.Random.Range(0, curList.Count)]);
        curCount[(int)SpawnItemType.Potion]++;
    }

    /// <summary>
    /// 식량 테이블에 따른 식량 아이템 스폰결정 메소드
    /// </summary>
    protected void FoodSpawn()
    {
        if (curCount[(int)SpawnItemType.Food] == boxData.dropTable.maxFoodCount)
            return;

        float rand = UnityEngine.Random.Range(0, 1f);
        float totalWeight = 0f;
        Rank spawnRank = Rank.None;

        for(int i = 0; i < boxData.dropTable.foodWeights.Length; i++)
        {
            totalWeight += boxData.dropTable.foodWeights[i];
            if(rand <= totalWeight)
            {
                spawnRank = (Rank)i;
                break;
            }
        }

        if (spawnRank == Rank.None)
        {
            return;
        }

        List<ItemData> curList = SpawnItemList.Instance.SpawnItemsList[(int)spawnRank + (int)SpawnItemType.Food * (int)Rank.Size].spawnItemList;

        if (curList.Count == 0)
            return;

        boxItems.Add(curList[UnityEngine.Random.Range(0, curList.Count)]);
        curCount[(int)SpawnItemType.Food]++;
    }

    /// <summary>
    /// 버프 포션 테이블에 따른 버프 포션 아이템 스폰결정 메소드
    /// </summary>
    protected void BuffPotionSpawn()
    {
        if (curCount[(int)SpawnItemType.BuffPotion] == boxData.dropTable.maxBuffPotionCount)
            return;

        float rand = UnityEngine.Random.Range(0, 1f);
        float totalWeight = 0f;
        Rank spawnRank = Rank.None;

        for (int i = 0; i < boxData.dropTable.buffPotionWeight.Length; i++)
        {
            totalWeight += boxData.dropTable.buffPotionWeight[i];
            if (rand <= totalWeight)
            {
                spawnRank = (Rank)i;
                break;
            }
        }

        if (spawnRank == Rank.None)
        {
            return;
        }

        List<ItemData> curList = SpawnItemList.Instance.SpawnItemsList[(int)spawnRank + (int)SpawnItemType.BuffPotion * (int)Rank.Size].spawnItemList;

        if (curList.Count == 0)
            return;

        boxItems.Add(curList[UnityEngine.Random.Range(0, curList.Count)]);
        curCount[(int)SpawnItemType.BuffPotion]++;
    }

    /// <summary>
    /// 환금 테이블에 따른 환금 아이템 스폰결정 메소드
    /// </summary>
    protected void CashbackSpawn()
    {
        if (curCount[(int)SpawnItemType.Cashback] == boxData.dropTable.maxCashbackCount)
            return;

        float rand = UnityEngine.Random.Range(0, 1f);
        float totalWeight = 0f;
        Rank spawnRank = Rank.None;

        for (int i = 0; i < boxData.dropTable.cashbackWeights.Length; i++)
        {
            totalWeight += boxData.dropTable.cashbackWeights[i];
            if (rand <= totalWeight)
            {
                spawnRank = (Rank)i;
                break;
            }
        }

        if (spawnRank == Rank.None)
        {
            return;
        }

        List<ItemData> curList = SpawnItemList.Instance.SpawnItemsList[(int)spawnRank + (int)SpawnItemType.Cashback * (int)Rank.Size].spawnItemList;

        if (curList.Count == 0)
            return;

        boxItems.Add(curList[UnityEngine.Random.Range(0, curList.Count)]);
        curCount[(int)SpawnItemType.Cashback]++;
    }

    /// <summary>
    /// 확정 스폰을 위한 메소드
    /// </summary>
    protected void ConfirmSpawn()
    {
        for (int i = 0; i < boxData.dropTable.confirmCount; i++)
        {
            ConfirmStruct curStruct = boxData.dropTable.confirmStructs[i];

            List<ItemData> curList = SpawnItemList.Instance.SpawnItemsList[(int)curStruct.rank + (int)curStruct.type * (int)Rank.Size].spawnItemList;

            if (curList.Count == 0)
                continue;

            for (int j = 0; j < curStruct.count; j++)
            {
                boxItems.Add(curList[UnityEngine.Random.Range(0, curList.Count)]);
            }
        }
    }

    /// <summary>
    /// 상자 아이템 하나씩 없애기
    /// </summary>
    public void TakeDamage(float damage, DamageType dmgType = DamageType.Normal )
    {
        if(BoxItems.Count > 0)
        {
            BoxItems.RemoveAt(UnityEngine.Random.Range(0, BoxItems.Count));
        }
    }

    /// <summary>
    /// 상자는 넉백 불가능
    /// </summary>
    public void TakeKnockBack(Vector3 attackPos, float knockBackDistance, float knockBackSpeed)
    {
        return;
    }

    public void TakeKnockBackFromSkill(Vector3 mouseDir, Vector3 attackPos, float knockBackDistance, float knockBackSpeed)
    {
        return;
    }

    public void FloatingDamage(float damage)
    {
        throw new NotImplementedException();
    }
}