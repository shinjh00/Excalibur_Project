using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightEffect : MonoBehaviour
{
    [SerializeField] Animator effectAnim;

    private void AttackEffect()
    {
        effectAnim.SetTrigger("Knight_Effect_Attack");
    }

    private void Skill1Effect()
    {
        effectAnim.SetTrigger("Knight_Effect_Skill1");
    }

    private void Skill2Effect()
    {
        effectAnim.SetTrigger("Knight_Effect_Skill2");
    }
}
