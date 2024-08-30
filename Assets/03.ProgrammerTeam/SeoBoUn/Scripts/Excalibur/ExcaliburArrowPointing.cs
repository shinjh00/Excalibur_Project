using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// <br/> 엑스칼리버 탈출구 표시를 위한 화살표 스크립트
/// <br/> 엑스칼리버 유저는 탈출구를, 다른 유저는 엑스칼리버를 가리킴.
/// </summary>
public class ExcaliburArrowPointing : MonoBehaviour
{
    [Tooltip("포인터가 가리킬 위치(디버깅용 에디터 할당 x)")]
    [SerializeField] Transform targetPos;
    [Tooltip("캐릭터의 위치(에디터 할당 필요)")]
    [SerializeField] Transform ownerPos;
    [Tooltip("화살표 렌더러(에디터 할당 필요)")]
    [SerializeField] SpriteRenderer spriteRender;
    Coroutine pointingRoutine;

    private void Start()
    {
        spriteRender.enabled = false;
    }

    /// <summary>
    /// 현재 클래스타입에 따라서 위치를 가리키기 위한 메소드
    /// </summary>
    /// <param name="curPlayerType"></param>
    public void TargetingArrow(ClassType curPlayerType)
    {
        spriteRender.enabled = true;
        if (pointingRoutine != null)
        {
            StopCoroutine(pointingRoutine);
        }

        if(curPlayerType == ClassType.Excalibur)
        {
            TargetingExit();
        }
        else
        {
            TargetingExcalibur();
        }
    }

    /// <summary>
    /// 탈출구를 가리키는 메소드
    /// </summary>
    private void TargetingExit()
    {
        if (PhotonNetwork.IsMasterClient)
        {   // 마스터 클라이언트는 해당 포인트의 위치를 알고 있으므로 바로 참조
            targetPos = GameManager.Ins.ExitPointController.CurActivePoint.transform;
        }
        else
        {   
            // 그 외의 클라이언트는 활성화 된 포인트를 모름. 찾아야 함.
            for(int i = 0; i < GameManager.Ins.ExitPointController.ExitPoints.Count; i++)
            {
                if (GameManager.Ins.ExitPointController.ExitPoints[i].IsExit)
                {
                    targetPos = GameManager.Ins.ExitPointController.ExitPoints[i].transform;
                }
            }
        }
        pointingRoutine = StartCoroutine(TargetingPoint());
    }

    /// <summary>
    /// 엑스칼리버를 가리키는 메소드
    /// </summary>
    private void TargetingExcalibur()
    {
        targetPos = GameManager.Ins.excaliburPlayer.transform;
        pointingRoutine = StartCoroutine(TargetingPoint());
    }

    IEnumerator TargetingPoint()
    {
        while (true)
        {
            if(targetPos == null)
            {
                yield return new WaitUntil(() => GameManager.Ins.ExitPointController.CurActivePoint != null);
                targetPos = GameManager.Ins.ExitPointController.CurActivePoint.transform;
            }
            
            Vector2 targetDir = (targetPos.position - ownerPos.position).normalized;
            transform.up = targetDir;

            transform.position = ownerPos.position + (Vector3)targetDir;

            yield return new WaitForSeconds(0.1f);
        }
    }
}
