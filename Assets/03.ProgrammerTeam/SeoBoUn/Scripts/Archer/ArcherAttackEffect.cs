using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 아쳐 어택 이펙트용 스크립트
/// </summary>
public class ArcherAttackEffect : MonoBehaviour
{
    [Tooltip("어택 이펙트를 가진 오브젝트 할당 필요")]
    [SerializeField] AttackEffect effect;

    /// <summary>
    /// 외부에서 실행할 애니메이션 이벤트
    /// <br/>(어택 시작시 이펙트 오브젝트 활성화)
    /// </summary>
    public void StartEffect()
    {
        effect.StartEffect();
        // effect.gameObject.SetActive(true);
    }

    /// <summary>
    /// 외부에서 실행할 애니메이션 이벤트
    /// <br/>(어택 종료시 이펙트 오브젝트 비활성화)
    /// </summary>
    public void EndEffect()
    {
        // effect.gameObject.SetActive(false);
    }

}
