using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleGolemFov : MonoBehaviour
{
    [SerializeField] float viewRadius = 5f; // 시야 거리
    [SerializeField] float viewAngle = 90f; // 시야 각도
    [SerializeField] LayerMask targetMask; // 감지할 대상 레이어
    public Puzzle_3 puzzle;
    [SerializeField] int segmentCount = 20;
    [SerializeField] LineRenderer line;
    [SerializeField] HashSet<PlayerController> detectedPlayers = new HashSet<PlayerController>();
    public Vector2 hallwayPos;

    public void Init()
    {
        targetMask = LayerMask.GetMask("Player");
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.positionCount = segmentCount + 2;
        DrawVisionCone();
        if (puzzle.photonView.IsMine)
        {
            Vector2 hallwayPos = puzzle.room.GetRoomHallWay();
            puzzle.photonView.RPC("GetHallWayPos", RpcTarget.All,hallwayPos);
        }
    }

    private void Update()
    {
       
            DetectTargets();
        

    }
    void DrawVisionCone()
    {
        Vector3 leftBoundary = DirFromAngle(-viewAngle / 2);
        Vector3 rightBoundary = DirFromAngle(viewAngle / 2);
        line.SetPosition(0, Vector3.zero);
        line.SetPosition(1, rightBoundary * viewRadius);
        for (int i = 2; i <= segmentCount; i++)
        {
            float t = i / (float)segmentCount;
            Vector3 point = BezierCurve.Bezier(rightBoundary * viewRadius, new Vector3((rightBoundary * viewRadius).x, viewRadius + 1), new Vector3((leftBoundary * viewRadius).x, viewRadius + 1), leftBoundary * viewRadius, t);
            line.SetPosition(i, point);
        }
        line.SetPosition(segmentCount + 1, leftBoundary * viewRadius);
    }

    public Vector3 DirFromAngle(float angleInDegrees)
    {
        angleInDegrees += transform.eulerAngles.z + 90;
        return new Vector3(Mathf.Cos(Mathf.Deg2Rad * angleInDegrees), Mathf.Sin(Mathf.Deg2Rad * angleInDegrees), 0);
    }

    public bool IsInView(Transform target)
    {
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        float dstToTarget = Vector3.Distance(transform.position, target.position);

        if (dstToTarget < viewRadius)
        {
            float angleToTarget = Vector3.Angle(transform.up, dirToTarget);
            if (angleToTarget < viewAngle / 2)
            {
                return true;
            }
        }
        return false;
    }

    [SerializeField] bool hasStateChanged;
    PlayerController player;
    public void DetectTargets()
    {
        // 현재 범위 내의 모든 타겟을 검색
        Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(transform.position, viewRadius, targetMask);
        HashSet<PlayerController> playersInView = new HashSet<PlayerController>();

        // 새로 범위에 들어온 플레이어를 확인
        foreach (Collider2D target in targetsInViewRadius)
        {
            PlayerController player = target.GetComponent<PlayerController>();
            if (player != null && IsInView(target.transform))
            {
                // 범위 안에 있고 시야 내에 있는 플레이어를 추가(검사해쉬셋)
                if (!playersInView.Contains(player))
                {
                    playersInView.Add(player);
                }
            }
        }
        if (playersInView.Count > 0)
        {
            foreach (PlayerController addPlayer in playersInView)
            {
                if (!detectedPlayers.Contains(addPlayer))
                {   //검사 해쉬셋에 처음 들어오면 함수 할당
                    addPlayer.onAddStateEvent +=  OnPlayerStateChanged;
                }
            }
        }

        // 상태가 변경된 경우 처리
        if (hasStateChanged)
        {
            if (player.photonView.IsMine)
            {
                puzzle.damagedTarget = player.transform;

                puzzle.photonView.RPC("Teleport", RpcTarget.All, hallwayPos);
            }

        }
        foreach (PlayerController p in detectedPlayers)
            {
                if (!playersInView.Contains(p))
                {
                    // 범위 밖으로 나간 플레이어의 이벤트 핸들러를 해제
                    p.onAddStateEvent -= OnPlayerStateChanged;
                    hasStateChanged = false;
                }
            }
            detectedPlayers = playersInView;
        


    }

/// <summary>
/// 상태 변경된 플레이어를 알리고 해당 플레이어를 텔레포트
/// </summary>
/// <param name="player">해당 플레이어</param>
/// <param name="newState">변경된 상태</param>
    private void OnPlayerStateChanged(PlayerController player,PlayerState newState)
    {
        this.player = player;
            hasStateChanged = true;

    }




}
