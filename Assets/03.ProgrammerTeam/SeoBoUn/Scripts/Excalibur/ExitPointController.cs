using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// <br/>탈출구 포인트를 관리하기 위한 컨트롤러
/// </summary>
public class ExitPointController : MonoBehaviourPun
{
    [Tooltip("탈출 포인트를 관리하기 위한 리스트")]
    [SerializeField] List<ExcaliburExitPoint> exitPoints = new List<ExcaliburExitPoint>();

    // 현재 활성화 된 탈출 포인트
    ExcaliburExitPoint curActivePoint;

    Collider2D[] colliders = new Collider2D[20];

    bool isFirstExit;

    public List<ExcaliburExitPoint> ExitPoints { get { return exitPoints; } }

    public ExcaliburExitPoint CurActivePoint { get 
        { 
            if(curActivePoint == null)
            {
                for(int i = 0; i < exitPoints.Count; i++)
                {
                    if (exitPoints[i].IsExit)
                    {
                        curActivePoint = exitPoints[i];
                    }
                }
            }

            return curActivePoint; } }

    private void Start()
    {
        GameManager.Ins.ChangeExcalibur += ActiveExitPoint;
        isFirstExit = true;
    }

    /// <summary>
    /// 탈출 포인트를 스폰할 메소드
    /// <br/> pos : 스폰될 위치
    /// </summary>
    /// <param name="pos"></param>
    public void SpawnExitPoint(Vector3 pos)
    {
        ExcaliburExitPoint instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Etc/ExcaliburExitPoint", pos, Quaternion.identity).GetComponent<ExcaliburExitPoint>();

        AddExitPoint(instance);
    }

    /// <summary>
    /// 엑스칼리버 포인트를 관리하기 위해 리스트에 넣어주는 함수
    /// <br/> 추가하고 자동으로 해당 탈출 포인트는 비활성화
    /// </summary>
    /// <param name="point"></param>
    private void AddExitPoint(ExcaliburExitPoint point)
    {
        exitPoints.Add(point);
        point.DisableExitPoint();
    }

    /// <summary>
    /// 랜덤한 하나의 포인트를 활성화하는 메소드
    /// </summary>
    public void ActiveExitPoint()
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (isFirstExit)
        {   // 처음 활성화되는 경우 + 자기 스폰 위치는 탈출구가 될 수 없음.
            isFirstExit = false;
            curActivePoint = exitPoints[UnityEngine.Random.Range(0, exitPoints.Count)];
            curActivePoint.ActiveExitPoint();
        }
        else
        {
            curActivePoint.DisableExitPoint();  // 기존 포인트는 비활성화

            ExcaliburExitPoint temp;
            do
            {   // 중복된 포인트는 뽑히면 안됨.
                temp = exitPoints[UnityEngine.Random.Range(0, exitPoints.Count)];
            } while (curActivePoint == temp);

            curActivePoint = temp;
            curActivePoint.ActiveExitPoint();   // 활성화 시키기
        }
    }

    /// <summary>
    /// 엑스칼리버 유저가 사망했을 때 기존의 포인트는 비활성화
    /// </summary>
    public void DisableExitPoint()
    {
        curActivePoint.DisableExitPoint();
    }

    /// <summary>
    /// 활성화 된 위치를 동기화하기 위한 메소드
    /// </summary>
    public ExcaliburExitPoint Sync_ActivePoint()
    {
        for(int i = 0; i < exitPoints.Count; i++)
        {
            if (exitPoints[i].IsExit)
            {
                curActivePoint = exitPoints[i];
                return exitPoints[i];
            }
        }
        return null;
    }

    /// <summary>
    /// 마스터 클라이언트가 아닌 다른 클라이언트에서 탈출 포인트 동기화 메소드
    /// </summary>
    public void Sync_ExitPoint()
    {
        ExcaliburExitPoint[] points = FindObjectsOfType<ExcaliburExitPoint>();
        exitPoints = points.ToList();
    }
}
