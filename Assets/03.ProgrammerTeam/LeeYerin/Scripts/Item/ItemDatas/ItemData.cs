using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자: 이예린 / Inventory Item 관련 데이터 ScriptableObject
/// </summary>
// [CreateAssetMenu(fileName = "Item", menuName = "Item/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Information")]
    [SerializeField] int id;
    [SerializeField] string itemName;
    [SerializeField] ItemBaseType type;
    [SerializeField] Rank rank;
    [SerializeField] Sprite itemImage;
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] int sellPrice;
    [SerializeField] int detailTextId;

    #region Get_Set
    public int Id { get { return id; } set { id = value; } }
    public string Name { get { return itemName; } set { itemName = value; } }
    public ItemBaseType ItemType { get { return type; } set { type = value; } }
    public Rank Rank { get { return rank; } set { rank = value; } }
    public Sprite ItemImage { get { return itemImage; } set { itemImage = value; } }
    public int Width { get { return width; } set { width = value; } }
    public int Height { get { return height; } set { height = value; } }
    public int SellPrice { get { return sellPrice; } set { sellPrice = value; } }
    public int DetailTextId { get {  return detailTextId; } set {  detailTextId = value; } }
    #endregion
}

/// <summary>
/// 아이템 기본 종류
/// </summary>
public enum ItemBaseType
{
    Consumable,
    Wearable,
    Cash
}

/// <summary>
/// 소비 아이템 종류
/// </summary>
public enum ConsumablesType
{
    HpPotion,
    Buffpotion,
    Food
}

/// <summary>
/// 장비 아이템 종류
/// </summary>
public enum WearablesType
{
    Bow = - 4,
    Wand,
    OneHanded,
    Spear,
    Armor,
    Head,
    Arm,
    Leg,
    Null = -9999
}

/// <summary>
/// 아이템 사용으로 증가할 stat 종류
/// </summary>
public enum IncrementalStat
{
    Offensive,
    Defense,
    AttackSpeed,
    HP,
    MoveSpeed,
    Null
}