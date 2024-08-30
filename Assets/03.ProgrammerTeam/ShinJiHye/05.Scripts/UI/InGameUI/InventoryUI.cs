using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 신지혜 / 인벤토리 UI
/// </summary>
public class InventoryUI : BaseUI
{
    [Tooltip("InventoryButton 할당")]
    [SerializeField] Button inventoryButton;
    [Tooltip("InventoryButton_normal 할당")]
    [SerializeField] GameObject inventoryButton_normal;
    [Tooltip("InventoryButton_pressed 할당")]
    [SerializeField] GameObject inventoryButton_pressed;

    public Button InventoryButton { get { return inventoryButton; } set {  inventoryButton = value; } }

    protected override void Awake()
    {
        // Bind() 사용 안함
        inventoryButton_normal.SetActive(true);
        inventoryButton_pressed.SetActive(false);
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => (Inventory.Instance != null && Inventory.Instance.Player != null));
        Inventory.Instance.Player.UIController.inventoryUI = this;
        inventoryButton.onClick.AddListener(Inventory.Instance.Player.UIController.OpenInventoryUI);

        Inventory.Instance.Player.StateController.HitEvent += ChangeHitState;
    }

    /// <summary>
    /// 인벤토리 창 열기
    /// </summary>
    public void OpenInventory()
    {
        Inventory.Instance.isInventoryOpen = !Inventory.Instance.isInventoryOpen;
        inventoryButton_normal.SetActive(!Inventory.Instance.isInventoryOpen);  // false : 기본 아이콘 비활성화
        inventoryButton_pressed.SetActive(Inventory.Instance.isInventoryOpen);  // true : pressed 아이콘 활성화
        Inventory.Instance.ControlInventory(null, Inventory.Instance.isInventoryOpen); // true : Inventory 열림

        if (Inventory.Instance.isInventoryOpen)
        {
            Inventory.Instance.Player.StateController.StateChange(PlayerState.Maintain, 0, 0, true, false);
            Inventory.Instance.Player.MoveController.SetMoveDir(Vector2.zero);
        }
        else
        {
            Inventory.Instance.Player.StateController.StateChange(PlayerState.Maintain, 0, 0, false, false);
        }
    }

    /// <summary>
    /// 개발자 : 서보운
    /// <br/> 만약 정비 상태에서 맞았을 때 인벤토리가 닫히도록 설정할 메소드
    /// </summary>
    public void ChangeHitState()
    {
        if(Inventory.Instance.Player.StateController.CurState.Contain(PlayerState.Maintain))
        {
            OpenInventory();
        }
    }
}