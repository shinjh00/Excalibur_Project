using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatedDamage : MonoBehaviour
{
    [SerializeField]public TMP_Text dmgUi;


    public void Init()
    {
        dmgUi = GetComponent<TMP_Text>();
        gameObject.SetActive(false);
    }
}
