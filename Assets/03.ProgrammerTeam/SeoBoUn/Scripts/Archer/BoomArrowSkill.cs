using System.Collections;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// <br/> 아쳐 스킬2 프리팹 스크립트
/// </summary>
public class BoomArrowSkill : MonoBehaviour
{
    PlayerController owner;
    Vector2 targetPos;

    float damage;
    float shootRange;
    float knockBackDis;
    float knockBackSpeed;
    float shootSpeed;
    bool attackStart;
    float invTime;
    float kbTime;

    IDamageable target;

    [Tooltip("BoomArrow 스크립트 할당 필요")]
    [SerializeField] BoomArrow boomArrowPrefab;
    [Tooltip("몬스터 레이어만 설정 필요")]
    [SerializeField] LayerMask monsterLayer;
    [Tooltip("플레이어 레이어만 설정 필요")]
    [SerializeField] LayerMask playerLayer;

    Coroutine shootRoutine;
    ArcherBoomAction boomAction;

    /// <summary>
    /// 폭발 스킬에 대한 초기값 설정 메소드
    /// <br/>사거리, 넉백거리, 넉백스피드, 발사속도, 무적 시간, 넉백 시간
    /// </summary>
    /// <param name="shootRange"></param>
    /// <param name="knockBackDis"></param>
    /// <param name="knockBackSpeed"></param>
    /// <param name="shootSpeed"></param>
    /// <param name="invTime"></param>
    /// <param name="kbTime"></param>
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
    /// 스킬에 대한 기본 속성 설정
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="owner"></param>
    /// <param name="damage"></param>
    public void SetInfo(object parameter, PlayerController owner, float damage)
    {
        this.owner = owner;
        boomAction = owner.GetComponent<ArcherBoomAction>();
        this.damage = damage;
    }

    /// <summary>
    /// 폭탄 화살 생성 루틴(외부에서 실행)
    /// </summary>
    /// <returns></returns>
    public IEnumerator BoomArrowCreateRoutine()
    {
        boomArrowPrefab.gameObject.SetActive(true);
        boomArrowPrefab.SetInfo(owner);
        yield return new WaitForSeconds(0.15f);
        shootRoutine = StartCoroutine(BoomArrowShootRoutine());
    }

    /// <summary>
    /// 실제 폭탄 화살이 날라가는 루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator BoomArrowShootRoutine()
    {
        attackStart = true;
        Vector2 startPosition = transform.position;
        targetPos = startPosition + (Vector2)transform.up * shootRange;
        float rate = 0;
        float time = Vector2.Distance(startPosition, targetPos) / shootSpeed;

        while (rate < 1f)
        {
            rate += Time.deltaTime / time;

            if (rate > 1f)
            {
                rate = 1f;
            }

            transform.position = Vector2.Lerp(startPosition, targetPos, rate);
            yield return null;
        }

        if (this != null)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 무언가 화살이 충돌했을 때 폭탄을 심기위한 메소드
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!attackStart)
        {
            return;
        }

        if (collision.gameObject.Equals(owner.gameObject))
        {
            return;
        }

        Debug.Log(collision.gameObject.name);
        if (monsterLayer.Contain(collision.gameObject.layer))
        {
            BaseMonster target = collision.GetComponent<BaseMonster>();

            if (target != null)
            {
                target.TakeDamage(damage);
                target.TakeKnockBackFromSkill(targetPos, transform.position, knockBackDis, knockBackSpeed);
                boomArrowPrefab.SetBoom(target);
                boomAction.IsAttached = true;
            }

            Destroy(gameObject);
        }

        if (playerLayer.Contain(collision.gameObject.layer))
        {
            PlayerController target = collision.GetComponent<PlayerController>();
            if (target.StateController.CurState.Contain(PlayerState.Hit))
            {
                return;
            }
            if (!owner.photonView.IsMine)
            {
                return;
            }
            if (target.photonView.Owner.IsTeammate())
            {
                return;
            }
            if (target != null)
            {
                IDamageable damageable = target.GetComponent<IDamageable>();
                IStateable stateable = target.GetComponent<IStateable>();

                damageable.TakeDamage(damage);
                damageable.TakeKnockBackFromSkill(targetPos, transform.position, knockBackDis, knockBackSpeed);
                boomArrowPrefab.SetBoom(target);
                boomAction.IsAttached = true;
                if (stateable != null)
                {
                    Debug.Log(stateable);
                    stateable.StateChange(PlayerState.Knockback, invTime, kbTime, true, false);
                }
            }

            Destroy(gameObject);
        }
    }

}
