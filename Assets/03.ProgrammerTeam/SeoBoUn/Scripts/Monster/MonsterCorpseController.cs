using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 몬스터 시체 컨트롤러 -> 몬스터가 사망했을 때 처리를 위한 컨트롤러
/// </summary>
public class MonsterCorpseController : ItemBoxChecker, IDamageable
{
    Coroutine coroutine;

    public bool IsInteract { get { return isInteract; } set { isInteract = value; } }

    /// <summary>
    /// 사망했을 때, id에 따른 스폰 예정
    /// </summary>
    /// <param name="id"></param>
    public void OnDie(int id)
    {
        //base.Start();

        if (PhotonNetwork.IsMasterClient)
        {   // TODO.. 몬스터 박스 아이디 넣기
            StartCoroutine(DestroyRoutine());
            dropBox.SetBoxTable(id);
        }
        boxLayer = LayerMask.GetMask(new string[] { "Interact", "Monster" });
    }

    public override void Interact(PlayerController player)
    {
        if (!player.photonView.IsMine)
        {   // 예외처리 1. 나(로컬) 이외의 다른 플레이어와는 상호작용 금지
            return;
        }

        if (isInteract)
        {   // 상호작용 중이라면
            if (player == curPlayer)
            {   // 상호작용한 플레이어가 현재 상호작용 진행중인 플레이어와 동일하면
                isClose = true; // 상자 닫기
                SoundManager.instance.PlaySFX(dropBox.Box.closeSound, audioSource);

                if (curPlayer != null)
                {
                    curPlayer.StateController.StateChange(PlayerState.Interact, 0, 0, false, false);
                    curPlayer = null;
                }
                // 상호작용 상황 동기화
                photonView.RPC("SetInteract", RpcTarget.All, false);
                isOpen = false;
                Inventory.Instance.ControlInventory(dropBox, isOpen);
                Inventory.Instance.ItemBox5X5.FreshItemBoxSlots();
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
            if (Inventory.Instance.DraggingSlot != null)
            {
                Inventory.Instance.DraggingSlot.SlotBehaviorOnClose();
            }
            player.UIController.CanOpenInventory = true;
        }
    }

    /// <summary>
    /// 플레이어가 근처에 있을 시 특정 상호작용키(F)를 보여줄 수 있도록
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator ShowImageRoutine()
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
            keyImageRender.enabled = isShow;

            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// 플레이어와 상호작용 시 박스 안의 물건을 보여줄 수 있도록
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator ShowBoxItemRoutine()
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
                SoundManager.instance.PlaySFX(dropBox.Box.closeSound, audioSource);
                
                isClose = true;
                Inventory.Instance.ControlInventory(dropBox, isOpen);
                if (Inventory.Instance.DraggingSlot != null)
                {
                    Inventory.Instance.DraggingSlot.SlotBehaviorOnClose();
                }

                if (Inventory.Instance.ItemBox5X5.Slots != null)
                {
                    Inventory.Instance.ItemBox5X5.FreshItemBoxSlots();
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// 60초후에 몬스터가 사라지도록 설정
    /// </summary>
    /// <returns></returns>
    IEnumerator DestroyRoutine()
    {
        yield return new WaitForSeconds(60f);

        yield return new WaitUntil(() => !isInteract);

        PhotonNetwork.Destroy(GetComponentInParent<BaseMonster>().gameObject);

    }

    /// <summary>
    /// 상자 안의 랜덤한 물건 없애기
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(float damage, DamageType dmgType = DamageType.Normal )
    {
        if (dropBox.BoxItems.Count > 0)
        {
            // dropBox.BoxItems.RemoveAt(Random.Range(0, dropBox.BoxItems.Count));
        }
    }

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
        throw new System.NotImplementedException();
    }

}
