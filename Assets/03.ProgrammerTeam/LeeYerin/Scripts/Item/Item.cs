using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Item : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] ItemData item;
    [SerializeField] SpriteRenderer image;

    public ItemData ItemData => item;

    Vector3 DefaultPos;

    private void Start()
    {
        image.sprite = item.ItemImage;
    }

    /// <summary>
    /// 아이템 사용 함수
    /// </summary>
    protected virtual void UseItem()
    {
    }

    #region Drag and Drop
    public void OnBeginDrag(PointerEventData eventData)
    {
        DefaultPos = transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(eventData.position);
        transform.position = new Vector3(mousePos.x, mousePos.y, 0f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Inventory.Instance.TryDropItem(transform.position, this);
        transform.position = DefaultPos;
    }
    #endregion
}
