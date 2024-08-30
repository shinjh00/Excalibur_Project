using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 플레이어 상호작용 컨트롤러
/// </summary>
public class PlayerInteractController : BaseController
{
    IInteractable target;
    IInteractable temp;
    [SerializeField] Collider2D[] colliders;

    float minDist = 100f;
    float interactDist = 50f;

    protected override void GetStat()
    {
        owner = GetComponent<PlayerController>();
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => (owner != null) && (owner.isSetting));

        colliders = new Collider2D[20];
    }

    public void Interact(PlayerController player)
    {
        minDist = 100f;
        // 상호작용
        // 오버랩 검사 : 범위 안에 있는 상호작용 가능 오브젝트 스캔
        // 오브젝트들 배열에 담기, 가장 가까운 오브젝트 검사
        //      foreach 반복문으로 - Distance(player.transform.position, gameObject.transform.position)
        // 그 오브젝트의 Interaction() 호출
       /* if (player.StateController.CurState.Contain(PlayerState.Puzzle) )
        {
            return;
        }*/
        if (player.StateController.CurState.Contain(PlayerState.Interact) )
        {
            player.StateController.StateChange(PlayerState.Interact, 0, 0, false, false);
            target.Interact(player);
            target = null;
            return;
        }

        int size = Physics2D.OverlapCircleNonAlloc(transform.position, 3f, colliders);

        for (int i = 0; i < size; i++)
        {
            // 1. 상호작용 가능한 대상인지 검사
            temp = colliders[i].GetComponent<IInteractable>();

            // 2. 거리를 측정하고
            interactDist = Vector2.Distance(transform.position, colliders[i].transform.position);

            // 3. 최단 거리인지 찾기
            if(temp != null && minDist > interactDist)
            {
                minDist = interactDist;
                target = temp;
            }
        }

        if (target != null)
        {
            player.StateController.StateChange(PlayerState.Interact, 0, 0, true, false);
            target.Interact(player);
        }
    }
}
