using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// <br/> 아쳐 스킬1 프리팹 스크립트
/// </summary>
public class ArrowSkill : MonoBehaviour
{
    PlayerController owner;
    Vector2 targetPos;

    float damage;           // 스킬의 데미지
    float shootRange;       // 스킬의 사정거리
    float knockBackDis;     // 스킬의 넉백거리
    float knockBackSpeed;   // 스킬의 넉백스피드
    float shootSpeed;       // 스킬의 속도
    bool attackStart;

    float invTime;          // 무적 시간
    float kbTime;           // 넉백 시간

    List<IDamageable> damageableList = new List<IDamageable>();
    Collider2D[] damagableColliders = new Collider2D[20];

    [SerializeField] GameObject arrowObject;
    [SerializeField] GameObject arrowPointObject;
    [SerializeField] LayerMask targetLayer;

    Coroutine shootRoutine;

    /// <summary>
    /// 화살 스킬에 대한 초기값 설정 메소드
    /// <br/>사거리, 넉백거리, 넉백스피드, 발사속도, 무적 시간, 넉백 시간
    /// </summary>
    /// <param name="shootRange"></param>
    /// <param name="knockBackDis"></param>
    /// <param name="knockBackSpeed"></param>
    /// <param name="shootSpeed"></param>
    public void SetInit(float shootRange, float knockBackDis, float knockBackSpeed, float shootSpeed, float invTime, float kbTime)
    {
        this.shootRange = shootRange;
        this.knockBackDis = knockBackDis;
        this.knockBackSpeed = knockBackSpeed;
        this.shootSpeed = shootSpeed;
        this.invTime = invTime;
        this.kbTime = kbTime;
    }

    /// <summary>
    /// 화살을 발사할 때 설정할 메소드
    /// <br/> 어느 방향으로, 누가 사용하며, 데미지에 대한 부분
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="owner"></param>
    /// <param name="damage"></param>
    public void SetInfo(object parameter, PlayerController owner, float damage)
    {
        this.transform.up = (Vector3)parameter;
        this.owner = owner;
        this.damage = damage;
    }

    public IEnumerator ArrowCreateRoutine()
    {
        arrowObject.SetActive(true);
        damageableList.Clear();
        yield return null;
        shootRoutine = StartCoroutine(ArrowShootRoutine(shootSpeed));
    }

    IEnumerator ArrowShootRoutine(float shootSpeed)
    {
        attackStart = true;
        float rate = 0;

        Vector2 startPosition = transform.position;
        // 목표 위치는 내(로컬) 머리방향 * 날아갈 거리
        targetPos = startPosition + (Vector2)transform.up * shootRange;
        float time = Vector2.Distance(startPosition, targetPos) / shootSpeed;

        while(rate < 1f)
        {
            if(this == null)
            {
                yield break;
            }

            rate += Time.deltaTime / time;

            if (rate > 1f)
            {
                rate = 1f;
            }

            transform.position = Vector2.Lerp(startPosition, targetPos, rate);
            yield return null;
        }

        if(this != null)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(!attackStart)
        {   // 오브젝트 준비 시간동안에는 데미지 불가능
            return;
        }

        if(!owner.photonView.IsMine)
        {
            return;
        }
        if (targetLayer.Contain(collision.gameObject.layer))
        {
            if(collision.gameObject.Equals(owner.gameObject))
            {
                return;
            }

            IStateable stateable = collision.GetComponent<IStateable>();
            PlayerController targetPlayer = collision.GetComponent<PlayerController>();
            if (targetPlayer != null)
            {
                if (targetPlayer.photonView.Owner.IsTeammate())
                {
                    return;
                }
            }
            IDamageable damagable = collision.GetComponent<IDamageable>();
            if(damagable != null && !damageableList.Contains(damagable))
            {
                damageableList.Add(damagable);
                damagable.TakeDamage(damage);
                damagable.TakeKnockBack(owner.transform.position, knockBackDis, knockBackSpeed);

                if(shootRoutine != null)
                {
                    StopCoroutine(shootRoutine);
                }
                if (stateable != null)
                {
                    Debug.Log(stateable);
                    stateable.StateChange(PlayerState.Knockback, invTime, kbTime, true, false);
                }
                Destroy(gameObject);
            }

        }
    }
}
