using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필 / 카메라 등이 플레이어를 따라갈 수 있게 만드는 클래스
/// </summary>
public class CopyPos : MonoBehaviour
{
    [SerializeField] bool x, y, z;
    [SerializeField] public Transform target;

    private void Update()
    {
        if(!target) return;

        transform.position = new Vector3((x ? target.position.x : transform.position.x),
                                          (y ? target.position.y : transform.position.y),
                                          (z ? target.position.z : transform.position.z));
    }
}
