using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개(멍멍)발자 : 신지혜
/// 기술사 고유 스킬1 창던지기 만발사수
/// 창을 던져서 맞은 오브젝트에 데미지를 주고 내 위치로 끌어옴
/// </summary>
public class KnightThrowSkill : MonoBehaviour
{
    KnightAttackController owner;
    Spear spear;
    public float damage;
    public LayerMask damageableMask;

    public Transform targetCollision;
    public bool isThrowing;
    public bool isGrapped;
    Vector2 grapPos;
    float grapTime;

    Vector3 startPos;  // Spear 초기 위치
    float throwDuration = 0.15f;  // 창 날아가는 시간
    float returnDuration = 0.3f;  // 창 돌아오기 전 딜레이 시간


    /// <summary>
    /// Spear 관련 정보 세팅
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="mouseDir"></param>
    /// <param name="skill1SpearSpeed"></param>
    /// <param name="skill1HitboxWidth"></param>
    /// <param name="skill1HitboxHeight"></param>
    /// <param name="damage"></param>
    public void SetInfo(KnightAttackController owner, Spear spear, float damage)
    {
        this.owner = owner;
        this.spear = spear;
        this.damage = damage;
        damageableMask = owner.DamageableMask;
    }

    /// <summary>
    /// 기사 창 던지기 메소드
    /// <br/>spear 위치 조금씩 틀어짐 추후 수정 필요**************
    /// </summary>
    /// <param name="targetPos"></param>
    public void ThrowSpear(Vector3 targetPos)
    {
        startPos = spear.transform.position;  // 초기 위치
        targetCollision = null;
        StartCoroutine(ThrowSpearRoutine(targetPos));
    }

    /// <summary>
    /// 기사 창 날리기 코루틴
    /// </summary>
    /// <param name="targetPos"> 창 던질 때 목표 위치 </param>
    /// <returns></returns>
    IEnumerator ThrowSpearRoutine(Vector3 targetPos)
    {
        SoundManager.instance.PlaySFX(1650019, owner.KnightAudioSource);
        // 창이 날아감
        float t = 0f;
        float rate = 0f;
        while (t <= throwDuration)
        {
            t += Time.deltaTime;
            rate = t / throwDuration;
            if(rate > 1f)
            {
                rate = 1f;
            }

            if (isGrapped)
            {
                grapPos = Vector2.Lerp(startPos, targetPos, rate);
                grapTime = t;
                spear.transform.position = grapPos;
                break;
            }
            else
            {
                spear.transform.position = Vector2.Lerp(startPos, targetPos, rate);
                grapTime = throwDuration;
                yield return null;
            }
            
        }

        // 일정 시간 뒤 다시 시작점으로 돌아가기
        yield return new WaitForSeconds(returnDuration);

        isThrowing = false;

        SoundManager.instance.PlaySFX(1650018, owner.KnightAudioSource);
        // 창이 돌아옴
        t = 0f;
        rate = 0f;
        while (t <= grapTime)
        {
            if (isGrapped)
            {
                t += Time.deltaTime;
                rate = t / grapTime;
                if (rate > 1f)
                {
                    rate = 1f;
                }
                spear.transform.position = Vector2.Lerp(grapPos, startPos, rate);
                targetCollision.position = Vector2.Lerp(grapPos, startPos, rate * 0.6f);
                yield return null;
            }
            else
            {
                t += Time.deltaTime;
                rate = t / grapTime;
                if (rate > 1f)
                {
                    rate = 1f;
                }
                spear.transform.position = Vector2.Lerp(targetPos, startPos, rate);
                yield return null;
            }
        }
        isGrapped = false;

        owner.AttackPoint.IsAttacking = false;
        owner.enabled = true;
        spear.transform.position = owner.transform.position;
    }

    /// <summary>
    /// 포톤뷰 체크용 메소드
    /// </summary>
    /// <returns></returns>
    public bool CheckOwner()
    {
        if(!owner.photonView.IsMine)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
