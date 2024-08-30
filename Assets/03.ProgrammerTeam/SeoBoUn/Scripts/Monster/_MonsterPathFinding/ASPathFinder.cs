using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// A* 기반 길찾기
/// <br/>현재는 사용하지 않음
/// </summary>
public class ASPathFinder
{
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

        }

        return false;
    }
}
