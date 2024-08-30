using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 개발자 : 서보운
/// 몬스터의 길을 관리할 GridManager
/// <br/>현재는 사용하지 않음
/// </summary>
public class MonsterGridManager : MonoBehaviour
{
    [SerializeField] Tilemap grid;

    public Graph graph;

    private int prevIndex;
    private int nextIndex;

    
    private void Start()
    {
       // InitGraph();
    }
    
    /// <summary>
    /// 그래프 초기화 메소드
    /// 
    /// </summary>
    /*
    public void InitGraph()
    {
        graph = new Graph();

        // 그래프의 모양을 코드에서 가져올 수 있도록 cellBounds 활용
        for (int x = grid.cellBounds.xMin; x < grid.cellBounds.xMax; x++)
        {
            for(int y = grid.cellBounds.yMin; y < grid.cellBounds.yMax; y++)
            {
                // grid는 cell로써 관리되고, cell좌표는 int값만 허용 및 배열처럼 관리
                Vector3Int localPosition = new Vector3Int(x, y, 0);

                if(grid.HasTile(localPosition))
                {
                    Vector3 pos = grid.CellToWorld(localPosition);

                    // 왼쪽 아래가 기준이므로 중점을 가운데로 둬야 정확히 움직임
                    pos.x += grid.cellSize.x / 2;
                    pos.y += grid.cellSize.y / 2;

                    // 타일이 있으므로 해당 지점은 그래프의 노드가 됨
                    graph.AddNode(pos);
                }
            }
        }

        foreach (ASNode prev in graph.nodes)
        {
            foreach(ASNode next in graph.nodes)
            {
                // 두 노드(타일)간의 거리가 1이면 -> 연결하는 간선 추가
                if(Vector3.Distance(prev.pos, next.pos) <= 1.0f && prev != next)
                {
                    graph.AddEdge(prev, next);
                }
            }
        }

        foreach (ASNode node in graph.nodes)
        {
            // 만약 연결되어 있는 노드가 없는 단절된 노드는
            if (graph.Neighbors(node).Count == 0)
            {
                // 그래프에서 삭제
                graph.nodes.Remove(node);
            }
        }
    }

    /// <summary>
    /// 위치에 해당하는 가장 인접한 타일 찾기
    /// </summary>
    /// <param name="pos"></param>
    /// <returns>반환값은 노드</returns>
    public ASNode GetCurNode(Vector3 pos)
    {
        float distance = float.MaxValue;
        ASNode curNode = null;

        foreach(ASNode node in graph.nodes)
        {
            if(distance > Vector3.SqrMagnitude(pos - node.pos))
            {
                distance = Vector3.Magnitude(pos - node.pos);
                curNode = node;
            }
        }

        return curNode;
    }
    */
}
