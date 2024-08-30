using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 개발자 : 서보운
/// ArcherSkill2 한발에 두방을 위한 프리팹 오브젝트
/// </summary>
public class BoomArrow : MonoBehaviour
{
    Transform targetTransform;
    ArcherBoomAction boomAction;

    [Tooltip("BoomArrow 프리팹 할당 필요(부착용)")]
    [SerializeField] BoomArrow boomPrefab;
    PlayerController owner;
    Collider2D[] colliders;

    [Tooltip("폭발까지 필요한 시간(Default : 3f)")]
    [SerializeField] float boomTime;
    [Tooltip("폭발 범위(Defalut : 2f")]
    [SerializeField] float boomRange;

    [Tooltip("폭발에 데미지를 입을 레이어 설정 필요")]
    [SerializeField] LayerMask targetLayer;

    float invTime = 0.5f;
    float kbTime = 0.3f;

    [Tooltip("폭탄 부착 이미지렌더러")]
    [SerializeField] SpriteRenderer boomRender;
    [Tooltip("폭탄 부착 이미지")]
    [SerializeField] Sprite boomImage;
    [Tooltip("폭탄 폭발 애니메이터")]
    [SerializeField] Animator animator;

    /// <summary>
    /// 폭발 위치 설정
    /// </summary>
    /// <param name="target"></param>
    public void SetTransform(Transform target)
    {
        targetTransform = target;
    }

    /// <summary>
    /// 시작 시 owner설정 (폭발에서 피하기 위함)
    /// </summary>
    /// <param name="owner"></param>
    public void SetInfo(PlayerController owner)
    {
        this.owner = owner;
        boomAction = owner.GetComponent<ArcherBoomAction>();
        colliders = new Collider2D[10];
    }

    /// <summary>
    /// 폭탄 설정 메소드(플레이어)
    /// </summary>
    /// <param name="player"></param>
    public void SetBoom(PlayerController player)
    {
        // 플레이어의 자식으로 따라갈 예정.
        targetTransform = player.transform;
        BoomArrow boom = Instantiate(boomPrefab, player.transform);
        boom.SetInfo(owner);
        boom.transform.position += Vector3.up;
        boom.StartBoomRoutine();
        boom.SetTransform(targetTransform);
    }

    /// <summary>
    /// 폭탄 설정 메소드(몬스터)
    /// </summary>
    /// <param name="monster"></param>
    public void SetBoom(BaseMonster monster)
    {
        targetTransform = monster.transform;
        BoomArrow boom = Instantiate(boomPrefab, monster.transform);
        boom.SetInfo(owner);
        boom.transform.position += Vector3.up;
        boom.StartBoomRoutine();
        boom.SetTransform(targetTransform);
    }

    private void BoomImageRender()
    {
        boomRender.enabled = true;
    }


    /// <summary>
    /// 실제 폭발 루틴을 시작하기 위한 메소드
    /// </summary>
    public void StartBoomRoutine()
    {
        StartCoroutine(BoomRoutine());
    }

    /// <summary>
    /// 폭발 루틴.
    /// <br/> 3초 뒤에 터지거나 한번 더 스킬키를 눌러서 터트림
    /// </summary>
    /// <returns></returns>
    IEnumerator BoomRoutine()
    {
        float startTime = Time.time;
        boomAction.IsBoom = false;

        yield return new WaitUntil(() => (Time.time >= startTime + boomTime) || boomAction.IsBoom);
        if (!owner.photonView.IsMine)
        {
            yield break;
        }
        animator.enabled = true;
        int size = Physics2D.OverlapCircleNonAlloc(targetTransform.position, boomRange, colliders, targetLayer);

        for (int i = 0; i < size; i++)
        {
            // 폭탄을 쏜 아쳐는 데미지를 입지 않음
            if (colliders[i].gameObject.Equals(owner.gameObject))
            {
                continue;
            }
            PlayerController targetPlayer = colliders[i].GetComponent<PlayerController>();
            if (targetPlayer != null)
            {
                if (targetPlayer.photonView.Owner.IsTeammate())
                {
                    continue;
                }
            }
            IStateable stateable = colliders[i].GetComponent<IStateable>();
            if (stateable != null)
            {
                Debug.Log(stateable);
                stateable.StateChange(PlayerState.Knockback, invTime, kbTime, true, false);
            }
            IDamageable target = colliders[i].GetComponent<IDamageable>();

            if (target != null)
            {
                target.TakeDamage(owner.AttackController.AtkDamage * 0.8f);
                target.TakeKnockBack(colliders[i].transform.right, owner.PlayerClassData.knockBackDistance, owner.PlayerClassData.knockBackSpeed);
            }
        }
        boomAction.IsAttached = false;
        boomAction.IsBoom = false;
        Destroy(gameObject, 0.5f);
    }
}
