using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 개발자: 이예린 / 아이템 정보 UI 관련 클래스
/// </summary>
public class InformationUI : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] Image image;
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] TextMeshProUGUI rank;
    [SerializeField] TextMeshProUGUI type;
    [SerializeField] List<TextMeshProUGUI> stats;
    [SerializeField] TextMeshProUGUI consumItemStat;
    [SerializeField] TextMeshProUGUI detailText;
    [SerializeField] TextMeshProUGUI price;
    [SerializeField] TextMeshProUGUI whoseWeapon;
    [SerializeField] Slot slot;
    [SerializeField] RectTransform rect;

    public bool pointOn;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => (Inventory.Instance != null));

        Inventory.Instance.TextData.ApplyTextEffect(itemName, 1500020);
        Inventory.Instance.TextData.ApplyTextEffect(whoseWeapon, 1500032);
        for (int i = 0; i < stats.Count; i++)
        {
            Inventory.Instance.TextData.ApplyTextEffect(stats[i], 1500032);
        }
        Inventory.Instance.TextData.ApplyTextEffect(consumItemStat, 1500032);
        Inventory.Instance.TextData.ApplyTextEffect(price, 1500033);
        Inventory.Instance.TextData.ApplyTextEffect(type, 1500034);

    }

    public void SetInformationUI(Slot infoSlot)
    {
        Inventory.Instance.TextData.ApplyTextEffect(itemName, 1500020);
        Inventory.Instance.TextData.ApplyTextEffect(whoseWeapon, 1500032);
        for (int i = 0; i < stats.Count; i++)
        {
            Inventory.Instance.TextData.ApplyTextEffect(stats[i], 1500032);
        }
        Inventory.Instance.TextData.ApplyTextEffect(consumItemStat, 1500032);
        Inventory.Instance.TextData.ApplyTextEffect(price, 1500033);
        Inventory.Instance.TextData.ApplyTextEffect(type, 1500034);

        switch (infoSlot.Item.Rank) // 아이템 타입 텍스트 효과 적용
        {
            case Rank.Normal:
                Inventory.Instance.TextData.ApplyTextEffect(rank, 1500022);
                rank.text = "일반";
                break;
            case Rank.Rare:
                Inventory.Instance.TextData.ApplyTextEffect(rank, 1500023);
                rank.text = "희귀";
                break;
            case Rank.Artifact:
                Inventory.Instance.TextData.ApplyTextEffect(rank, 1500024);
                rank.text = "유물";
                break;
            case Rank.Legendary:
                Inventory.Instance.TextData.ApplyTextEffect(rank, 1500025);
                rank.text = "전설";
                break;
        }

        if (infoSlot.WearableItem != null)
        {
            WhoseWeapon(infoSlot);
        }
        else
        {
            whoseWeapon.text = "";
        }

        if (infoSlot.Item.DetailTextId != -9999)    // 아이템 설명 텍스트 적용
        {
            Inventory.Instance.TextData.ApplyText(detailText, infoSlot.Item.DetailTextId);
        }
        else
        {
            detailText.text = "";
        }

        image.sprite = infoSlot.Item.ItemImage;
        itemName.text = infoSlot.Item.Name;

        switch (infoSlot.Item.ItemType)
        {
            case ItemBaseType.Consumable:
                type.text = "소모품";
                statText(infoSlot, infoSlot.ConsumableItem.ConsumablesType);
                price.text = $"구매 : {infoSlot.ConsumableItem.BuyPrice}G\n판매 : {infoSlot.Item.SellPrice}G";
                break;
            case ItemBaseType.Wearable:
                type.text = "장비";
                statText(infoSlot, infoSlot.WearableItem.Stats);
                price.text = $"구매 : {infoSlot.WearableItem.BuyPrice}G\n판매 : {infoSlot.Item.SellPrice}G";
                break;
            case ItemBaseType.Cash:
                type.text = "환급품";
                for (int i = 0; i < stats.Count; i++)
                {
                    stats[i].text = "";
                }
                price.text = $"판매 : {infoSlot.Item.SellPrice}G";
                break;
        }

        // 슬롯 크기에 상관없이 항상 아이템 정보 UI 크기 같게 유지
        rect.localScale
            = new Vector2(2 * Inventory.Instance.DefaultScale.x / slot.SlotRect.localScale.x,
            2 * Inventory.Instance.DefaultScale.y / slot.SlotRect.localScale.y);
        image.rectTransform.localScale
            = new Vector2(1 / (slot.SlotRect.localScale.x * rect.localScale.x),
            1 / (slot.SlotRect.localScale.y * rect.localScale.y));
    }
    public void statText(Slot slot, ConsumablesType type)
    {
        switch (type)
        {
            case ConsumablesType.HpPotion:
                consumItemStat.text = $"초당 HP +{slot.ConsumableItem.HealPerSec} 지속시간 {slot.ConsumableItem.Duration}초";
                break;
            case ConsumablesType.Food:
                consumItemStat.text = $"HP +{slot.ConsumableItem.Heal}% ";
                break;
            case ConsumablesType.Buffpotion:
                consumItemStat.text = $"{slot.ConsumableItem.Stat}스탯 + {slot.ConsumableItem.StatUp} 지속시간 {slot.ConsumableItem.Duration}초 ";
                break;
        }

        for (int i = 0; i < stats.Count; i++)
        {
            stats[i].text = "";
        }
    }

    public void statText(Slot slot, List<IncrementalStat> itemStats)
    {
        consumItemStat.text = "";
        int count = 0;
        for (; count < itemStats.Count; count++)
        {
            switch (itemStats[count])
            {
                case IncrementalStat.Offensive:
                    if (slot.WearableItem.StatValues[count] > 0)
                    {
                        stats[count].text = $"{count + 1}. 공격력 : +{slot.WearableItem.StatValues[count]}";
                    }
                    else
                    {
                        stats[count].text = $"{count + 1}. 공격력 : -{slot.WearableItem.StatValues[count]}";
                    }
                    break;
                case IncrementalStat.Defense:
                    if (slot.WearableItem.StatValues[count] > 0)
                    {
                        stats[count].text = $"{count + 1}. 방어력 : +{slot.WearableItem.StatValues[count]}";
                    }
                    else
                    {
                        stats[count].text = $"{count + 1}. 방어력 : -{slot.WearableItem.StatValues[count]}";
                    }
                    break;
                case IncrementalStat.AttackSpeed:
                    if (slot.WearableItem.StatValues[count] > 0)
                    {
                        stats[count].text = $"{count + 1}. 공격속도 : +{slot.WearableItem.StatValues[count]}";
                    }
                    else
                    {
                        stats[count].text = $"{count + 1}. 공격속도 : -{slot.WearableItem.StatValues[count]}";
                    }
                    break;
                case IncrementalStat.HP:
                    if (slot.WearableItem.StatValues[count] > 0)
                    {
                        stats[count].text = $"{count + 1}. 체력 : +{slot.WearableItem.StatValues[count]}";
                    }
                    else
                    {
                        stats[count].text = $"{count + 1}. 체력 : -{slot.WearableItem.StatValues[count]}";
                    }
                    break;
                case IncrementalStat.MoveSpeed:
                    if (slot.WearableItem.StatValues[count] > 0)
                    {
                        stats[count].text = $"{count + 1}. 이동속도 : +{slot.WearableItem.StatValues[count]}";
                    }
                    else
                    {
                        stats[count].text = $"{count + 1}. 이동속도 : -{slot.WearableItem.StatValues[count]}";
                    }
                    break;
            }
        }
        for (; count < stats.Count; count++)
        {
            stats[count].text = "";
        }
    }

    private void WhoseWeapon(Slot slot)
    {
        switch (slot.WearableItem.WearablesType)
        {
            case WearablesType.Bow:
                whoseWeapon.text = "궁수만 착용 가능";
                break;
            case WearablesType.Wand:
                whoseWeapon.text = "마법사만 착용 가능";
                break;
            case WearablesType.OneHanded:
                whoseWeapon.text = "전사만 착용 가능";
                break;
            case WearablesType.Spear:
                whoseWeapon.text = "기사만 착용 가능";
                break;
            default:
                whoseWeapon.text = "";
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointOn = true;
    }
}
