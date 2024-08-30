using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbushBoxHitPoint : MonoBehaviour,IDamageable
{
   public AmbushBox owner;

    public void FloatingDamage(float damage)
    {
        return;
    }

    public void TakeDamage(float damage,DamageType dmgType = DamageType.Normal)
    {
        owner.Unmetamorph();
    }

    public void TakeKnockBack(Vector3 attackPos, float knockBackDistance, float knockBackSpeed)
    {
        Debug.Log("TakeKB");
        return;
    }

    public void TakeKnockBackFromSkill(Vector3 mouseDir, Vector3 attackPos, float knockBackDistance, float knockBackSpeed)
    {
        Debug.Log("TakeDamageKBFS");
        return;
    }
}
