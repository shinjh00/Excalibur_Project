using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 몬스터 길찾기를 위한 Graph 클래스
/// <br/>현재는 사용하지 않음
/// </summary>
public class Graph
{
    public List<MonsterEdge> edges;    // 그래프의 간선
    public List<ASNode> nodes;    // 그래프의 노드

    public Graph()
    {
        nodes = new List<ASNode>();
        edges = new List<MonsterEdge>();
    }

    /// <summary>
    /// 그래프에 노드를 추가하기 위한 메소드
    /// </summary>
    /// <param name="pos">추가하는 노드(타일)의 위치</param>
    public void AddNode(Vector3 pos)
    {
        ASNode newNode = new ASNode(nodes.Count, pos);
        nodes.Add(newNode);
    }

    /// <summary>
    /// 새로운 간선을 추가하기 위한 메소드
    /// </summary>
    /// <param name="prev">시작(이전) 노드</param>
    /// <param name="next">간선을 이을 목표 노드</param>
    public void AddEdge(ASNode prev, ASNode next)
    {
        MonsterEdge newEdge = new MonsterEdge(prev, next, 1f);
        edges.Add(newEdge);
    }

    /// <summary>
    /// 그래프에서 간선이 이어져 있는지 확인하기 위한 메소드
    /// </summary>
    /// <param name="prev"></param>
    /// <param name="next"></param>
    /// <returns>반환값은 이어져 있다면 true, 아니라면 false</returns>
    public bool IsNodeConnected(ASNode prev, ASNode next)
    {
        foreach(MonsterEdge edge in edges)
        {
            if (edge.prev == prev && edge.next == next)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 하나의 노드(target)에서 갈 수 있는 모든 노드(타일) 찾기
    /// </summary>
    /// <param name="target"></param>
    /// <returns>반환값은 리스트로 해당 모든 노드들</returns>
    public List<ASNode> Neighbors(ASNode target)
    {
        List<ASNode> result = new List<ASNode>();

        foreach(MonsterEdge edge in edges)
        {
            if(edge.prev == target)
            {
                result.Add(edge.next);
            }
        }

        return result;
    }

    /// <summary>
    /// 거리(가중치)를 반환할 메소드
    /// </summary>
    /// <param name="prev"></param>
    /// <param name="next"></param>
    /// <returns>반환값은 float로 두 노드 사이의 거리(가중치)</returns>
    public float Distance(ASNode prev, ASNode next)
    {
        foreach(MonsterEdge edge in edges)
        {
            if(edge.prev == prev && edge.next == next)
            {
                return edge.GetWeight();
            }
        }

        return Mathf.Infinity;
    }

    private Vector2[] Direction =
{
        new Vector2(0, 1),
        new Vector2(0, -1),
        new Vector2(1, 0),
        new Vector2(-1, 0)
    };

    public bool PathFinding(ASNode start, ASNode end)
    {
        PriorityQueue<ASNode, int> nextPQ = new PriorityQueue<ASNode, int>();
        ASNode startNode = start;
        nextPQ.Enqueue(startNode, startNode.f);

        while (nextPQ.Count() > 0)
        {
            ASNode nextNode = nextPQ.Dequeue();
            // 2. 방문한 노드는 방문했다고 표기하기.
            
        }

        return false;
    }
}

/// <summary>
/// 개발자 : 서보운
/// 몬스터 길찾기를 위한 Node 클래스
/// </summary>
public class ASNode
{
    public int index;       // 노드의 번호
    public Vector3 pos;     // 노드(타일)로 취급되는 게임 오브젝트의 위치
    public Vector3 parent;

    public int f;
    public int g;
    public int h;

    private bool isObject = false;
    /// <summary>
    /// 해당 노드(타일)위에 오브젝트가 있는지에 대한 여부
    /// true : 오브젝트가 있는 상태, false : 노드(타일)위에 비어있는 상태
    /// </summary>
    public bool IsObject { get { return isObject; } set { isObject = value; } }

    public ASNode(int index, Vector3 pos)
    {
        this.index = index;
        this.pos = pos;
        isObject = false;
    }

    public ASNode(Vector3 pos, Vector3 parent, int g, int h)
    {
        this.pos = pos;
        this.parent = parent;
        this.f = g + h;
        this.g = g;
        this.h = h;
    }
}

/// <summary>
/// 개발자 : 서보운
/// 몬스터 길찾기를 위한 Edge 클래스
/// </summary>
public class MonsterEdge
{
    public ASNode prev;   // 이전(시작) 노드
    public ASNode next;   // 다음 노드

    private float weight;   // 가중치(defalut : 1)

    public MonsterEdge(ASNode prev, ASNode next, float weight)
    {
        this.prev = prev;
        this.next = next;
        this.weight = weight;
    }

    /// <summary>
    /// 가중치를 반환할 메소드
    /// </summary>
    /// <returns></returns>
    public float GetWeight()
    {
        // 다음 위치에 갈 수 있는 상태라면(오브젝트가 없다면)
        if(next.IsObject)
        {
            return Mathf.Infinity;
        }

        return weight;
    }
}

#region PrevPathFinding
/*
/// <summary>
/// 시작 지점(start)으로 부터 끝 지점(end)까지 길을 찾아주는 메소드
/// </summary>
/// <param name="start"></param>
/// <param name="end"></param>
/// <returns>반환값은 List(Node) </returns>
public List<ASNode> GetPath(ASNode start, ASNode end)
{
    List<ASNode> path = new List<ASNode>(20000);

    if (start == end)
    {
        path.Add(start);
        return path;
    }

    List<ASNode> openList = new List<ASNode>();                             // 탐색할 모든 노드(타일)들
    Dictionary<ASNode, ASNode> previous = new Dictionary<ASNode, ASNode>();     // 시작 지점부터 끝 지점까지 저장할 딕셔너리
    Dictionary<ASNode, float> distances = new Dictionary<ASNode, float>();  // 시작 노드로부터 다른 노드까지의 거리를 반환할 딕셔너리
                                                                            // ex) distances[nodes[5]] => start부터 5번 노드로 가는 가중치 값(float)

    for (int i = 0; i < nodes.Count; i++)
    {
        openList.Add(nodes[i]);

        distances.Add(nodes[i], float.PositiveInfinity);
    }

    // 시작점에서 시작점으로 가는 가중치는 0f
    distances[start] = 0f;

    while (openList.Count > 0)
    {
        // 거리순 정렬
        openList = openList.OrderBy(x => distances[x]).ToList();

        // 가장 거리가 짧은 타일 꺼내기(시작은 start)
        ASNode current = openList[0];

        openList.Remove(openList[0]);

        // 만일 도착점 위치라면
        if (current == end)
        {
            // previous가 들고있는 모든 타일을 꺼내면서
            while (previous.ContainsKey(current))
            {
                path.Add(current);
                // 해당 노드의 이전 노드 꺼내기
                current = previous[current];
            }
            // 시작점을 넣고
            path.Add(current);
            // 역순으로 정렬
            path.Reverse();
            break;
        }

        // 현재 노드(타일)에서 갈 수 있는 모든 타일을 검사
        foreach (ASNode neighbor in Neighbors(current))
        {
            // 현재 타일에서 다음 타일까지의 거리를 계산하고
            float distance = Distance(current, neighbor);

            // 다음 거리는 가중치 + 거리
            float nextDistance = distances[current] + distance;

            // 만약 다음 거리가 더 짧다면
            if (nextDistance < distances[neighbor])
            {
                // 갱신하고 다음 위치로 갈 거리로 계산
                distances[neighbor] = nextDistance;
                // 어디에서 왔는지 길 갱신
                previous[neighbor] = current;
            }
        }
    }

    return path;
}
*/
#endregion