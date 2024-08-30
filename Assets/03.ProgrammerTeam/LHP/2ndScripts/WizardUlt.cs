using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필 / 마법사 궁극기 프리팹에 적용하는 클래스
/// </summary>
public class WizardUlt : MonoBehaviour
{
    Animator anim;
    List<IDamageable> damageableList = new List<IDamageable>();
    List<IStateable> stateables = new List<IStateable>();
    Collider2D[] damageableColliders = new Collider2D[20];
    public LayerMask damagableMask;
    public float dmg;
    public float knockBackDis;
    public float knockBackSpeed;
    public float kbTime;
    public float invTime;
    PlayerController owner;
    private void Start()
    {
        anim = GetComponent<Animator>();
    }
    private void Update()
    {
        if(owner != null)
        {
            return;
        }
        Destroy(gameObject);
    }
    /// <summary>
    /// 프리팹 생성 후 호출할 스킬 로직
    /// </summary>
    /// <param name="targetPos">궁극기 타격지점</param>
    /// <param name="width">피격 범위 가로</param>
    /// <param name="height">피격 범위 세로</param>
    /// <param name="owner">시전자</param>
    /// <param name="spasticity">경직 유무</param>
    /// <returns></returns>
    public IEnumerator Active(Vector2 targetPos,float width,float height,PlayerController owner,bool spasticity)
    {
        Vector2 StartPos = transform.position;
        this.owner = owner;
float t = 0;
        while(t < 1)
        {
            t += Time.deltaTime;
            transform.position = Vector2.Lerp(StartPos, targetPos, t);
            yield return null;
        }
        transform.position = targetPos;
        anim.SetTrigger("Explode");
        transform.rotation = Quaternion.identity;
        float animationLength = 0.5f;
        IStateable stateable = null;
        if (owner.photonView.IsMine)
        {
            int overlapCount = Physics2D.OverlapBoxNonAlloc(targetPos, new Vector2(width, height), 0, damageableColliders, damagableMask);

            // 마우스 커서와 공격 가능한 객체 위치의 각도 계산하여 범위 안에 있는 공격 가능한 객체들만 damageableList에 담기
            for (int i = 0; i < overlapCount; i++)
            {
                // 자신이 체크되는 거 방지
                if (damageableColliders[i].gameObject.Equals(owner.gameObject))
                {
                    continue;
                }
                // 공격 가능한 객체들 리스트에 담기
                IDamageable damageable = damageableColliders[i].GetComponent<IDamageable>();
                stateable = damageableColliders[i].GetComponent<IStateable>();
                PlayerController player = damageableColliders[i].GetComponent<PlayerController>();
                if (player != null)
                {
                    if (player.StateController.CurState.Contain(PlayerState.invincible))
                    {
                        continue;
                    }
                }

                if (damageable != null)
                {
                    damageableList.Add(damageable);
                }
            }
        }
        if (stateable != null &&!spasticity)
        {
            stateables.Add(stateable);
        }
        foreach (var damageable in damageableList)
        {
            damageable.TakeDamage(dmg);
        }
        foreach (var state in stateables)
        {
            state.StateChange(PlayerState.Knockback, invTime, kbTime, true, false);
        }
        yield return new WaitForSeconds(animationLength);
        Destroy(gameObject);

    }

}
