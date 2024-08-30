using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PuzzleFire : MonoBehaviour
{
    [SerializeField]public GameObject circleLight;
    [SerializeField] public SpriteRenderer sr;
    [SerializeField] Animator animator;


    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();    
    }

    public void Animaion(string s,bool b)
    {
        animator.SetBool(s, b);
    }
}
