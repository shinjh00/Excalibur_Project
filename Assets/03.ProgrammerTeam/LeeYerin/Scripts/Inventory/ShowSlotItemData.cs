using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 개발자: 이예린/ 마우스 포인터 올렸을 시 아이템 정보 UI 띄우는 기능 구현
/// </summary>
public class ShowSlotItemData : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Slot slot;
    [SerializeField] InformationUI informationUI;
    [SerializeField] InformationUI activeEquipmentUI;

    #region Unity Events
    private void Start()
    {
        slot = transform.GetComponentInChildren<Slot>();
    }

    private void OnDisable()
    {
        if (slot.Item != null)
        {
            InfoUIDisable();
        }
    }

    private void Update()
    {
        if (informationUI.pointOn)
        {
            informationUI.pointOn = false;
            InfoUIDisable();
        }
    }
    #endregion

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (slot.Item != null && Inventory.Instance.DraggingSlot == null)
        {
            informationUI.SetInformationUI(slot);
            slot.SlotRect.SetAsLastSibling();
            slot.bagRect.SetAsLastSibling();
            informationUI.gameObject.SetActive(true);

            if (activeEquipmentUI == null)
            {
                return;
            }

            if (slot.WearableItem != null)
            {
                int element = (int)slot.WearableItem.WearablesType < 0 ? 0 : (int)slot.WearableItem.WearablesType + 1;
                if (Inventory.Instance.EquipmentSlots[element].Item == null)
                {
                    return;
                }
                activeEquipmentUI.SetInformationUI(Inventory.Instance.EquipmentSlots[element]);
                activeEquipmentUI.gameObject.SetActive(true);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (slot.Item != null)
        {
            InfoUIDisable();
        }
    }

    public void InfoUIDisable()
    {
        informationUI.gameObject.SetActive(false);

        if (activeEquipmentUI == null)
        {
            return;
        }

        if (activeEquipmentUI.gameObject.activeSelf)
        {
            activeEquipmentUI.gameObject.SetActive(false);
        }
    }
}
