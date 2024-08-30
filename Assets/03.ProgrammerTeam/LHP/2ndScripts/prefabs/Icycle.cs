using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필 / 마법사 스킬2 프리팹 클래스
/// </summary>
public class Icycle : MonoBehaviour
{
    public bool attackStart = false;

    public PlayerController owner;
    public float damage;
    public Vector2 targetPos;
    public float shootArriveTime;
    public float shootRange;
    public float knockBackDis;
    public float knockBackSpeed;
    public float kbTime;
    public float invTime;
    protected List<IDamageable> damageableList = new List<IDamageable>();
    protected Collider2D[] damageableColliders = new Collider2D[20];
    public float delay = 1;

    [SerializeField] GameObject ice1;
    [SerializeField] GameObject ice2;
    [SerializeField] GameObject ice3;
    [SerializeField] LayerMask targetLayer;

    Coroutine shootRoutine;
    public BoxCollider2D thisCol;

    /// <summary>
    /// 발사체가 자연스럽게 생기게 만드는 애니메이션 루틴
    /// </summary>
    /// <returns></returns>
    public IEnumerator IceCreateRoutine()
    {
        thisCol = GetComponent<BoxCollider2D>();
        thisCol.enabled = false;
        ice1.SetActive(true);
        yield return new WaitForSeconds(delay*0.3f);
        ice2.SetActive(true);
        yield return new WaitForSeconds(delay * 0.3f);
        ice3.SetActive(true);
        yield return new WaitForSeconds(delay * 0.3f);

        thisCol.enabled = true;
        shootRoutine = StartCoroutine(IceShootRoutine(shootArriveTime));
    }
    /// <summary>
    /// 발사체 사출하는 루틴
    /// </summary>
    /// <param name="shootTime"></param>
    /// <returns></returns>
    IEnumerator IceShootRoutine(float shootArriveTime)
    {
        float t = 0;
        Vector2 startPosition = transform.position;
        targetPos = startPosition + (Vector2)transform.up * shootRange;
        while (t < shootArriveTime)
        {
            if (this == null) // 이 오브젝트가 파괴되었는지 확인
            {
                yield break;
            }

            t += Time.deltaTime;
            transform.position = Vector2.Lerp(startPosition, targetPos, t / shootArriveTime);
            yield return null;
        }

        if (this != null)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
       
        if (targetLayer.Contain(collision.gameObject.layer))
        {
            if (collision.gameObject.Equals(owner.gameObject))
            {
                return;
            }
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                if(player.StateController.CurState.Contain(PlayerState.invincible))
                {
                    return;
                }
                if (player.StateController.CurState.Contain(PlayerState.Metamorph))
                {
                    return;
                }
                    if (player.photonView.Owner.IsTeammate())
                    {
                        return;
                    }
            }
            IStateable stateable = null;
            Vector2 targetDir = (targetPos - (Vector2)transform.position).normalized;
            if (owner.photonView.IsMine)
            {
                IDamageable damageable = collision.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                    
                    damageable.TakeKnockBack(transform.position, knockBackDis, knockBackDis/knockBackSpeed);

                    if (shootRoutine != null)
                    {
                        StopCoroutine(shootRoutine);
                    }
                }

            }
            stateable = collision.GetComponent<IStateable>();
            if (stateable != null)
            {
                stateable.StateChange(PlayerState.Knockback, invTime, kbTime, true, false);
            }
            Destroy(gameObject);
        }
    }
}
