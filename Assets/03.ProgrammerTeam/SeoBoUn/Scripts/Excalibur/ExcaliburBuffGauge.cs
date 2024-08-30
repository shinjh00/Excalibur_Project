using System.Collections;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// <br/> 엑스칼리버 버프 게이지용 스크립트
/// </summary>
public class ExcaliburBuffGauge : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRender;
    [SerializeField] Material ownerMat;
    [Tooltip("캐릭터와 떨어져서 표시되는 위치(1, 1, 0)")]
    [SerializeField] Vector3 offset;

    Coroutine tracingRoutine;

    /// <summary>
    /// 버프 시작할 때 호출할 메소드
    /// </summary>
    public void StartBuff()
    {
        MatChange();
        animator.SetTrigger("Buff");
        tracingRoutine = StartCoroutine(TracingPlayer());
    }

    public void EndBuff()
    {
        animator.SetTrigger("EndBuff");

        if (tracingRoutine != null)
        {
            StopCoroutine(tracingRoutine);
        }

        spriteRender.enabled = false;
    }

    private IEnumerator TracingPlayer()
    {
        while (true)
        {
            if (GameManager.Ins.excaliburPlayer == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                transform.position = GameManager.Ins.excaliburPlayer.transform.position + offset;

                yield return null;
            }
        }
    }

    public void MatChange()
    {
        spriteRender.material = ownerMat;
    }
}
