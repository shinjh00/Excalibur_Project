using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필 // 퍼즐1을 생성하는 클래스
/// </summary>

public class Puzzle_1 : Puzzle
{/// <summary>
/// 활성화 되어있는 정점
/// </summary>
    public List<PuzzlePoint> activeVertices = new List<PuzzlePoint>();
    /// <summary>
    /// 이전 정점
    /// </summary>
    public PuzzlePoint previousVertex;
    /// <summary>
    /// 현재 정점
    /// </summary>
    public PuzzlePoint currentVertex;

    public PuzzleLine line;
    public List<PuzzleLine> edges = new List<PuzzleLine>();
   // public List<PuzzleLine> allEdges = new List<PuzzleLine>();
    [SerializeField]PuzzlePoint[] points;




    protected override void Start()
    {
        base.Start();
        points = GetComponentsInChildren<PuzzlePoint>();

        
        // 정점 간의 연결 및 엣지 설정
        
        foreach (var vertex in points)
        {
            vertex.puzzle = this;
            vertex.Init();

            foreach (var connectedVertex in vertex.connectedVertices)
            {
                //allEdges.Add(vertex.connectedEdges[connectedVertex]);
                
                if (vertex.connectedEdges.ContainsKey(connectedVertex))
                {
                    if (!IsEdgeDup(vertex.connectedEdges[connectedVertex]))
                    {
                        edges.Add(vertex.connectedEdges[connectedVertex]);
                    }
                    else
                    {
                        Destroy(vertex.connectedEdges[connectedVertex].gameObject);
                    }
                }
     
            }
        }
    }
    /// <summary>
    /// 모든 라인이 활성화 되었는지 체크
    /// </summary>
    [PunRPC]
    public void CheckAllLine()
    {
        foreach(PuzzleLine l in edges)
        {
            if (!l.IsCheck)
            {
                return;
            }
            
        }
        foreach(var edge in edges)
        {
            edge.ChangeColor(Color.black);
        }
        foreach(var point in points)
        {
            point.gameObject.SetActive(false);
        }
        SoundManager.instance.PlaySFX(1650084, audioSource);
        if (photonView.IsMine)
        {
            SpawnBox();
        }

                    
    }
    [PunRPC]
    private void ResetColors()
    {
        Debug.Log("Reset");

        foreach (var vertex in points)
        {
            vertex.IsActive = false;
        }

        // 모든 엣지의 색상 초기화
        foreach (var edge in edges)
        {
            edge.ResetColor();
        }
        currentVertex = null;
        previousVertex = null;
        activeVertices.Clear();
    }
    /// <summary>
    /// 중복체크가 끝난 엣지들이 트리거되면 활성화
    /// </summary>
    /// <param name="vertex1"></param>
    /// <param name="vertex2"></param>
    public void ChangeEdgeColor(PuzzlePoint vertex1, PuzzlePoint vertex2)
    {
        if(vertex1 == vertex2)
        {
            return;
        }
        foreach (var edge in edges)
        {
            if ((edge.vertex1 == vertex1 && edge.vertex2 == vertex2) ||
                (edge.vertex1 == vertex2 && edge.vertex2 == vertex1))
            {
                if(edge.IsAlreadyCheck == 1)
                {
                    photonView.RPC("ResetColors", RpcTarget.All);
                }
                else
                {
                    edge.IsCheck = true;
                    break;
                }

            }
        }
    }
    /// <summary>
    /// 정점간 라인 인스턴스ID 중복체크, 중복 시 false반환
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public bool IsEdgeDup(PuzzleLine line)
    {
        List<PuzzlePoint> vertexPair = new List<PuzzlePoint>
        {
            line.vertex1,
            line.vertex2
        };

        vertexPair.Sort((v1, v2) => v1.GetInstanceID().CompareTo(v2.GetInstanceID()));
        if (edges.Count < 1)
        {
            return false;
        }
        foreach (var existingLine in edges)
        {
            
            List<PuzzlePoint> existingVertexPair = new List<PuzzlePoint>
            {
                existingLine.vertex1,
                existingLine.vertex2
            };

            existingVertexPair.Sort((v1, v2) => v1.GetInstanceID().CompareTo(v2.GetInstanceID()));

            if (vertexPair[0].GetInstanceID() == existingVertexPair[0].GetInstanceID() &&
                vertexPair[1].GetInstanceID() == existingVertexPair[1].GetInstanceID())
            {
                return true;
            }
        }

        return false;
    }

}
