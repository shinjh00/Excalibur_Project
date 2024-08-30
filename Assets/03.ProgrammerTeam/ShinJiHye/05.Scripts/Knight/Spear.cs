using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 신지혜 / 기사 Spear의 정보 및 동작 관련 클래스
/// </summary>
public class Spear : MonoBehaviour
{
    //[Tooltip("Spear 할당")]
    //[SerializeField] Animator spearAnim;
    [Tooltip("SpearPoint 할당")]
    [SerializeField] GameObject spearPoint;

    //public Animator SpearAnim { get { return spearAnim; } set { spearAnim = value; } }
    public GameObject SpearPoint { get { return spearPoint; } set { spearPoint = value; } }

}
