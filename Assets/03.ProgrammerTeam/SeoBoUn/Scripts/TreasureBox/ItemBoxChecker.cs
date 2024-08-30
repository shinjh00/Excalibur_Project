using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 아이템 박스 플레이어 체크용 스크립트
/// F키에 대한 상호작용과 상호작용 키를 보여줄 오브젝트에 붙어있을 예정.
/// </summary>
public class ItemBoxChecker : MonoBehaviourPun, IInteractable, IImageViewer
{
    [SerializeField] private Animator animator;
    [SerializeField] protected SpriteRenderer keyImageRender;
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected LayerMask boxLayer;
    [SerializeField] protected Collider2D[] colliders;
    [SerializeField] protected Collider2D[] boxColliders;
    [SerializeField] protected DropBox dropBox;
    [SerializeField] protected PlayerController curPlayer;

    [SerializeField] protected float showDistance;
    [SerializeField] protected AudioSource audioSource;

    protected bool isShow;
    protected bool isImageShow;
    protected bool isOpen;
    protected bool isClose;
    protected bool isInteract;

    protected Coroutine showRoutine;
    protected Coroutine showBoxRoutine;

    protected Vector3 targetPos = Vector3.zero;

    protected void Start()
    {
        ShowImage();
        showBoxRoutine = StartCoroutine(ShowBoxItemRoutine());

        colliders = new Collider2D[10];
        boxColliders = new Collider2D[10];
    }

    /// <summary>
    /// 특정 상호작용 키를 보여주는 루틴
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator ShowImageRoutine()
    {
        while (true)
        {
            // 1. 주변의 플레이어 검사
            isShow = false;
            int size = Physics2D.OverlapCircleNonAlloc(transform.position, showDistance, colliders, playerLayer);

            for (int i = 0; i < size; i++)
            {   // 2. 주변의 플레이어가 있었다면
                PlayerController target = colliders[i].GetComponent<PlayerController>();

                if (target != null)
                {
                    if (isInteract)
                    {   // 상호작용 중이라면
                        isShow = false;
                        break;
                    }
                    targetPos = target.transform.position;
                    isShow = true;
                    break;
                }
            }

            // 3. 만약 플레이어가 있는 상황에서. 주변의 박스와의 거리를 측정하기
            int boxSize = Physics2D.OverlapCircleNonAlloc(transform.position, showDistance + 5f, boxColliders, boxLayer);
            // 나와 플레이어의 거리 측정
            float dist = Vector2.Distance(targetPos, transform.position);

            for (int j = 0; j < boxSize; j++)
            {
                float boxDis = Vector2.Distance(targetPos, boxColliders[j].transform.position);

                if (dist > boxDis)
                {   // 박스들 중 나보다 짧은 애가 있다면
                    isShow = false;
                }
            }
            keyImageRender.gameObject.SetActive(isShow);

            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// 박스 안의 아이템을 닫아야 할지 결정하는 루틴
    /// 
    /// 1. 주변의 플레이어를 검사.
    /// 2. 만약 상호작용하고 있는 플레이어(curPlayer)가 있다면 계속 보여주기
    /// 3. 만약 해당 플레이어가 없다면 박스 안의 아이템을 닫아버리기
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator ShowBoxItemRoutine()
    {
        while (true)
        {
            bool playerCheck = false;
            int size = Physics2D.OverlapCircleNonAlloc(transform.position, showDistance, colliders, playerLayer);

            if (size != 0)
            {
                for (int i = 0; i < size; i++)
                {
                    PlayerController target = colliders[i].GetComponent<PlayerController>();

                    if (target == curPlayer)
                    {   // 만약 오버랩안에 플레이어가 있었다면 상호작용하는 플레이어가 존재
                        playerCheck = true;
                    }
                }
            }

            if (!playerCheck && !isClose && curPlayer != null)
            {
                curPlayer.StateController.StateChange(PlayerState.Interact, 0, 0, false, false);
                curPlayer = null;
                
                photonView.RPC("SetInteract", RpcTarget.All, false);
                isOpen = false;
                isClose = true;
                SoundManager.instance.PlaySFX(dropBox.Box.closeSound, audioSource);
                Inventory.Instance.ControlInventory(dropBox, isOpen);
                if(Inventory.Instance.ItemBox5X5.Slots != null)
                {
                    Inventory.Instance.ItemBox5X5.FreshItemBoxSlots();
                }
                SetAnimator("IsClose");
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// 박스 상호작용 스크립트
    /// 상호작용키(F)를 눌렀을 때 애니메이션 및 박스 안의 아이템 보여주기
    /// </summary>
    public virtual void Interact(PlayerController player)
    {
        if (!player.photonView.IsMine)
        {   // 예외처리 1. 나(로컬) 이외의 다른 플레이어와는 상호작용 금지
            return;
        }

        if (isInteract)
        {
            if (player == curPlayer)
            {
                isClose = true;
                if (curPlayer != null)
                {
                    curPlayer.StateController.StateChange(PlayerState.Interact, 0, 0, false, false);
                    curPlayer = null;
                }
                photonView.RPC("SetInteract", RpcTarget.All, false);
                isOpen = false;
                SoundManager.instance.PlaySFX(dropBox.Box.closeSound, audioSource);
                Inventory.Instance.ControlInventory(dropBox, isOpen);
                Inventory.Instance.ItemBox5X5.FreshItemBoxSlots();
                SetAnimator("IsClose");
                if (Inventory.Instance.DraggingSlot != null)
                {
                    Inventory.Instance.DraggingSlot.SlotBehaviorOnClose();
                }
                player.UIController.CanOpenInventory = true;
            }
            else
            {
                player.StateController.StateChange(PlayerState.Interact, 0, 0, false, false);
            }

            return;
        }

        // 인벤토리 열린 상태일 땐 아이템 박스 열리지 않도록 처리
        if (Inventory.Instance.isInventoryOpen)
        {
            return;
        }

        if (curPlayer == null)
        {   // 상호작용 하고 있는 플레이어를 설정 & 박스 열림 설정
            isOpen = !isOpen;
            curPlayer = player;
            photonView.RPC("SetInteract", RpcTarget.All, true);
        }
        else if (curPlayer == player)
        {   // 상호작용 가능한 플레이어만 닫힘 설정 가능
            isOpen = !isOpen;
        }

        if (isOpen)
        {
            isClose = false;
            SoundManager.instance.PlaySFX(dropBox.Box.closeSound, audioSource);
            Inventory.Instance.ControlInventory(dropBox, isOpen);
            SetAnimator("IsOpen");

            if (!Inventory.Instance.isOpened)   // 인벤토리가 처음 열리는 상황이라면
            {
                Inventory.Instance.isOpened = true;
                StartCoroutine(TurnOffVerticalLayout());
            }
            player.UIController.CanOpenInventory = false;
        }
        else
        {
            isClose = true;
            SoundManager.instance.PlaySFX(dropBox.Box.openSound, audioSource);
            if (curPlayer != null)
            {
                curPlayer.StateController.StateChange(PlayerState.Interact, 0, 0, false, false);
                curPlayer = null;
            }
            photonView.RPC("SetInteract", RpcTarget.All, false);
            Inventory.Instance.ControlInventory(dropBox, isOpen);
            Inventory.Instance.ItemBox5X5.FreshItemBoxSlots();
            SetAnimator("IsClose");
            if (Inventory.Instance.DraggingSlot != null)
            {
                Inventory.Instance.DraggingSlot.SlotBehaviorOnClose();
            }
            player.UIController.CanOpenInventory = true;
        }
    }

    [PunRPC]
    protected void SetInteract(bool value)
    {
        isInteract = value;
    }

    /// <summary>
    /// 이미지를 보여주는 루틴
    /// </summary>
    public void ShowImage()
    {
        showRoutine = StartCoroutine(ShowImageRoutine());
    }

    public void SetAnimator(string name, bool value)
    {
        photonView.RPC("SetAnimatorSnyc", RpcTarget.All, name, value);
    }

    [PunRPC]
    protected void SetAnimatorSnyc(string name, bool value)
    {
        animator.SetBool(name, value);
    }

    public void SetAnimator(string name)
    {
        photonView.RPC("SetAnimatorSnyc", RpcTarget.All, name);
    }

    [PunRPC]
    protected void SetAnimatorSnyc(string name)
    {
        animator.SetTrigger(name);
    }

    /// <summary>
    /// 개발자: 이예린 / 인벤토리의 VerticalLayoutGroup를 0.1초 후 꺼주는 코루틴
    /// </summary>
    /// <returns></returns>
    protected IEnumerator TurnOffVerticalLayout()
    {
        yield return new WaitForSeconds(0.1f);
        Inventory.Instance.VerticalLayout.enabled = false;
    }
}
