using System.Collections;
using UnityEngine;

public class AttackAnima : MonoBehaviour
{
    [SerializeField] GameObject weapon;
    Coroutine ult;

    private void AttackStart()
    {
        if (weapon.activeSelf)
        {
            weapon.SetActive(false);
        }
    }

    private void AttackEnd()
    {
        if (!weapon.activeSelf)
        {
            weapon.SetActive(true);
        }
    }

    private void UltStart()
    {
        ult = StartCoroutine(Ult());
    }

    private void UltEnd()
    {
        StopCoroutine(ult);
    }

    IEnumerator Ult()
    {
        while (true) 
        {
            transform.parent.position = transform.position;
            yield return null;
        }
    }
}
