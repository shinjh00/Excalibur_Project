using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;

/// <summary>
/// 개발자 : 서보운
/// 기사 고유 스킬2 돌격 앞으로
/// <br/> 돌진 사거리 안의 플레이어/몬스터에게 사용
/// <br/> 사용 시 플레이어, 몬스터에게 충돌시 적용
/// </summary>
public class KnightRushSkill : MonoBehaviour
{
    [SerializeField] BoxCollider2D rushCollider2D;
    LayerMask targetLayer;
    LayerMask wallLayer;
    
    bool isUsing;           // 사용 여부
    float damage;           // skill2 데미지
    float knockBackDis;     // skill2 넉백 거리
    float knockBackTime;    // skill2 넉백 시간
    float moveSpeed;        // skill2 이동 속도
    float moveDistance;     // skill2 이동 거리

    public void SetInit(float moveSpeed, float moveDistance, float knockBackDis, float knockBackTime)
    {
        // rushCollider2D = GetComponent<BoxCollider2D>();
        targetLayer = LayerMask.GetMask(new string[] { "Monster", "Player" });
        wallLayer = LayerMask.GetMask("Wall");

        this.moveSpeed = moveSpeed;
        this.moveDistance = moveDistance;
        this.knockBackDis = knockBackDis;
        this.knockBackTime = knockBackTime;
    }

    public void SetInfo(float damage)
    {
        this.damage = damage;
    }

    public void UseRushSkill(PlayerController player, object direction)
    {
        isUsing = true;
        rushCollider2D.enabled = true;
        if (player.photonView.IsMine)
        {
            player.StartCoroutine(RushRoutine(player, direction));
        }
        else
        {
            isUsing = false;
            rushCollider2D.enabled = false;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!rushCollider2D.enabled)
        {   // 사용중이 아니라면 밑의 로직은 실행하지 않도록 설정
            return;
        }

        if(!isUsing)
        {
            return;
        }

        if (targetLayer.Contain(collision.gameObject.layer))
        {   // 만약 충돌한 상대가 타겟중 하나(몬스터, 플레이어)이면
            Debug.Log(collision.gameObject.name);
            PlayerController targetPlayer = collision.GetComponent<PlayerController>();
            if (targetPlayer != null)
            {
                if (targetPlayer.photonView.Owner.IsTeammate())
                {
                    return;
                }
            }
            IDamageable target = collision.gameObject.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damage);
                target.TakeKnockBack(collision.transform.position, knockBackDis, knockBackTime);
                StartCoroutine(HitRoutine(collision));
            }
            IStateable statable = collision.gameObject.GetComponent<IStateable>();
            if (statable != null)
            {
                statable.StateChange(PlayerState.Knockback, knockBackTime, 0.5f, true, false);
            }
        }
    }

    IEnumerator RushRoutine(PlayerController player, object direction)
    {
        Debug.Log("돌진 루틴");
        Vector2 moveDir = ((Vector3)direction).normalized;
        Vector2 startPos = player.transform.position;
        Vector2 targetPos = Vector2.zero;
        RaycastHit2D hitInfo = Physics2D.Raycast(startPos, moveDir, moveDistance, wallLayer);

        if(hitInfo)
        {
            targetPos = hitInfo.point - (moveDir * 0.5f);
        }
        else
        {
            targetPos = startPos + moveDir * moveDistance;
        }

        float rate = 0f;
        float time = Vector2.Distance(player.transform.position, targetPos) / moveSpeed;
        
        while(rate < 1f)
        {
            rate += Time.deltaTime / time;

            if(rate > 1f)
            {
                rate = 1f;
            }

            player.transform.position = Vector2.Lerp(startPos, targetPos, rate);

            yield return null;
        }

        isUsing = false;
        rushCollider2D.enabled = false;
    }

    IEnumerator HitRoutine(Collider2D collision)
    {
        float t = 0;

        while(t < 0.3f)
        {
            t += Time.deltaTime;

            collision.transform.Rotate(Vector3.forward, 30f);

            yield return null;
        }

        collision.transform.rotation = Quaternion.identity;

    }
}
