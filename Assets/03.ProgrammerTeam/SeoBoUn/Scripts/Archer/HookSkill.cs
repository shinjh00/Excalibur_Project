using System.Collections;
using UnityEngine;


/// <summary>
/// 개발자 : 서보운
/// <br/> 아쳐 스킬3 프리팹 스크립트
/// </summary>
public class HookSkill : MonoBehaviour
{
    PlayerController owner;
    ArcherHookAction playerAction;

    Transform grapPos;
    Vector2 targetPos;

    float damage;
    float shootRange;
    float knockBackDis;
    float knockBackSpeed;
    float shootSpeed;
    bool attackStart;
    bool isGrappling;   // 잡았는가
    bool isCollide;

    float invTime;
    float kbTime;

    public float InvTime { get { return invTime; } }
    public float KbTime { get { return kbTime; } }

    [Tooltip("갈고리에 걸릴 수 있는 레이어 설정 필요")]
    [SerializeField] LayerMask hookLayer;
    [Tooltip("갈고리에서 날라갈 후크 선")]
    [SerializeField] LineRenderer hookLine;

    LayerMask wallLayer;

    Coroutine coroutine;

    public bool IsCollider { get { return isCollide; } set { isCollide = value; } }

    /// <summary>
    /// 후크 스킬 초기화 메소드
    /// <br/> 사정거리, 넉백거리, 넉백스피드, 발사속도, 무적시간, 넉백시간
    /// </summary>
    /// <param name="shootRange"></param>
    /// <param name="knockBackDis"></param>
    /// <param name="knockBackSpeed"></param>
    /// <param name="shootSpeed"></param>
    /// <param name="invTime"></param>
    /// <param name="kbTime"></param>
    public void SetInit(float shootRange, float knockBackDis, float knockBackSpeed, float shootSpeed, float invTime, float kbTime)
    {
        hookLine = GetComponent<LineRenderer>();
        hookLine.positionCount = 2;
        this.shootRange = shootRange;
        this.knockBackDis = knockBackDis;
        this.knockBackSpeed = knockBackSpeed;
        this.shootSpeed = shootSpeed;
        this.invTime = invTime;
        this.kbTime = kbTime;
        wallLayer = LayerMask.GetMask("Wall");
    }

    /// <summary>
    /// 스킬 초기화 메소드
    /// <br/> 방향, 사용하는 유저, 데미지
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="owner"></param>
    /// <param name="damage"></param>
    public void SetInfo(object parameter, PlayerController owner, float damage)
    {
        this.transform.up = (Vector3)parameter;
        this.owner = owner;
        this.damage = damage;
        owner.HealthController.OnHit.AddListener(HitStop);
        if (playerAction == null)
        {
            playerAction = owner.GetComponent<ArcherHookAction>();
        }
    }

    /// <summary>
    /// 맞았을 때 스킬 강제 종료.
    /// </summary>
    private void HitStop()
    {
        playerAction.EndHookAction();
        StopAllCoroutines();
        Destroy(gameObject);
    }

    /// <summary>
    /// 후크 루틴(날아가서 걸리는 것 까지 진행)
    /// </summary>
    /// <returns></returns>
    public IEnumerator HookRoutine()
    {
        Vector2 startPosition = transform.position;
        targetPos = startPosition + (Vector2)transform.up * shootRange;
        Vector2 endPos = Vector2.zero;

        float rate = 0;
        float time = Vector2.Distance(startPosition, targetPos) / shootSpeed;

        hookLine.SetPosition(0, transform.position);
        hookLine.SetPosition(1, transform.position);

        while (rate < 1f)
        {
            if (isGrappling)
            {   // 잡았다면
                yield break;
            }

            rate += Time.deltaTime / time;
            if (rate > 1f)
            {
                rate = 1f;
            }
            endPos = Vector2.Lerp(startPosition, targetPos, rate); /*
            time += 10000f;
            time / Time.deltaTime;                                             */
            transform.position = endPos;
            hookLine.SetPosition(0, startPosition);
            hookLine.SetPosition(1, endPos);
            yield return null;
        }

        if (!isGrappling)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 후크 충돌 확인용(벽이나 플레이어 / 몬스터에 닿았을 시)
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hookLayer.Contain(collision.gameObject.layer))
        {
            if (collision.gameObject.Equals(owner.gameObject))
            {
                return;
            }

            isGrappling = true;

            if (wallLayer.Contain(collision.gameObject.layer))
            {
                StartCoroutine(WallHookAction(transform.position));
            }
            else
            {
                // TODO.. 벽에 닿았을 때 transform이 0이 나옴. 
                // 벽에 닿았을 때를 분리하여 벽에는 position으로 변경하여야 함

                if (owner.photonView.IsMine)
                {
                    /*
                    IDamageable damagable = collision.GetComponent<IDamageable>();
                    PlayerController targetPlayer = collision.GetComponent<PlayerController>();
                    if (targetPlayer != null)
                    {
                        if (targetPlayer.photonView.Owner.IsTeammate())
                        {
                            return;
                        }
                    }
                    IStateable stateable = collision.GetComponent<IStateable>();

                    if (damagable != null)
                    {
                        damagable.TakeDamage(damage * 1.8f);
                        damagable.TakeKnockBack(transform.position, knockBackDis, knockBackSpeed);
                    }
                    if (stateable != null)
                    {
                        Debug.Log(stateable);
                        stateable.StateChange(PlayerState.Knockback, InvTime, KbTime + 0.2f, true, false);
                    }
                    */
                }
                grapPos = collision.gameObject.transform;

                StartCoroutine(HookAction());
            }
        }

    }

    /// <summary>
    /// 플레이어나 몬스터에 닿았을 때
    /// </summary>
    /// <returns></returns>
    IEnumerator HookAction()
    {
        playerAction.StartHookAction(this);

        Vector2 startPos = owner.transform.position;
        Vector2 endPos = grapPos.position;

        float rate = 0f;
        float time = Vector2.Distance(startPos, endPos) / (shootSpeed);

        while (rate < 1f)
        {
            if (isCollide)
            {   // 충돌하였다면
                break;
            }
            if (owner.StateController.CurState.Contain(PlayerState.Hit))
            {
                playerAction.EndHookAction();
                break;
            }
            rate += Time.deltaTime / time;
            if (rate > 1f)
            {
                rate = 1f;
            }

            owner.transform.position = Vector2.Lerp(startPos, endPos, rate);
            hookLine.SetPosition(0, owner.transform.position);

            endPos = grapPos.position;
            hookLine.SetPosition(1, endPos);
            yield return null;
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// 벽에 걸렸을 때
    /// </summary>
    /// <param name="grapPos"></param>
    /// <returns></returns>
    IEnumerator WallHookAction(Vector3 grapPos)
    {
        playerAction.StartHookAction(this);

        Vector2 startPos = owner.transform.position;
        Vector2 endPos = grapPos;

        float rate = 0f;
        float time = Vector2.Distance(startPos, endPos) / (shootSpeed);

        while (rate < 1f)
        {
            if (isCollide)
            {   // 충돌하였다면
                break;
            }

            rate += Time.deltaTime / time;
            if (rate > 1f)
            {
                rate = 1f;
            }

            owner.transform.position = Vector2.Lerp(startPos, endPos, rate);
            hookLine.SetPosition(0, owner.transform.position);

            endPos = grapPos;
            hookLine.SetPosition(1, endPos);
            yield return null;
        }

        Destroy(gameObject);
    }
}
