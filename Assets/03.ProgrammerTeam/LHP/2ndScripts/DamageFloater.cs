using Firebase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageFloater : MonoBehaviour
{
    [SerializeField] FloatedDamage[] damageUis = new FloatedDamage[6];
    [SerializeField] float varianeStrength = 3f;


    private void Start()
    {
        
        foreach(FloatedDamage damage in damageUis)
        {
            damage.Init();
        }
    }


    public void Floating(float dmg)
    {
        for (int i = 0; i < damageUis.Length; i++)
        {
            if (damageUis[i].gameObject.activeSelf)
            {
                continue;
            }
            damageUis[i].gameObject.SetActive(true);
            if(dmg == -1)
            {
                damageUis[i].dmgUi.text = "Invincible";
            }
            else
            {
                damageUis[i].dmgUi.text = ((int)dmg).ToString();
            }
            damageUis[i].dmgUi.color = Color.red;
            StartCoroutine(FloatingDamage(damageUis[i]));
            break;

        }
    }
    public void Floating(string message,Color color)
    {
        for (int i = 0; i < damageUis.Length; i++)
        {
            if (damageUis[i].gameObject.activeSelf)
            {
                continue;
            }
            damageUis[i].gameObject.SetActive(true);
            damageUis[i].dmgUi.text = message;
            damageUis[i].dmgUi.color = color;
            StartCoroutine(FloatingMessage(damageUis[i]));
            break;

        }
    }

    IEnumerator FloatingDamage(FloatedDamage dmgText)
    {
        Vector3 startPos = dmgText.transform.position;
        float r = Random.Range(1, varianeStrength + 1);
        Vector3 point1 = startPos + new Vector3(0, r, 0); 
        float r2 = Random.Range(-varianeStrength, varianeStrength + 1);
        Vector3 point2 = startPos + new Vector3(r2, r, 0); 
        Vector3 point3 = startPos + new Vector3(r2, 0, 0);

        float t = 0;
        while(t < 1)
        {
           dmgText.transform.position = BezierCurve.Bezier(startPos, point1, point2, point3,t);
            t += Time.deltaTime;
            yield return null;
        }
        dmgText.gameObject.SetActive(false);
        dmgText.transform.position = transform.position;
        
    }
    IEnumerator FloatingMessage(FloatedDamage dmgText)
    {
        float t = 0;
        Vector2 startPos = dmgText.transform.position;
        Vector2 targetPos = (Vector2)dmgText.transform.position + new Vector2(0, 1);
        while (t < 1)
        {
            dmgText.transform.position = Vector2.Lerp(startPos, targetPos, t);
            t += Time.deltaTime;
            yield return null;
        }
        dmgText.gameObject.SetActive(false);
        dmgText.transform.position = transform.position;
    }
}
public enum MessageType { Exp,Level}