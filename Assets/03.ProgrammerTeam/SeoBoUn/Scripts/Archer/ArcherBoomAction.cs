using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// <br/>아쳐의 붐 액션(폭탄 발사하고 터트리기)
/// <br/>화살이 몬스터/플레이어에 적중한 경우 1.5초 뒤에 3초 동안 스킬 재사용 가능
/// <br/>3초 안에 스킬을 재사용하면 재사용하는 즉시 화살이 폭발
/// </summary>
public class ArcherBoomAction : MonoBehaviour
{
    bool isUse;
    bool isAttached;    // 부착했는가
    bool isBoom;        // 터트릴건가
    [Tooltip("터트릴 수 있는 시간(기본 1.5초")]
    [SerializeField] float boomTime;

    Coroutine boomRoutine;

    public bool IsAttached { get { return isAttached; } set { isAttached = value; if (value) StartBoomRoutine(); } }
    public bool IsBoom { get { return isBoom; } set { isBoom = value;} }
    public bool IsUse { get { return isUse; } }

    public void StartBoomRoutine()
    {
        isBoom = false;
        boomRoutine = StartCoroutine(BoomRoutine());
        isUse = true;
    }

    IEnumerator BoomRoutine()
    {
        yield return new WaitForSeconds(boomTime);

        yield return new WaitUntil(() => (!isAttached) || (Input.GetKeyDown(KeyCode.E)));
        isUse = false;
        isBoom = true;
    }
}
