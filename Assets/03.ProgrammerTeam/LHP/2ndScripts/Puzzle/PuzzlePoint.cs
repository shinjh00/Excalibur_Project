using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필 //  퍼즐1의 정점
/// </summary>

public class PuzzlePoint : MonoBehaviourPun
{
    bool isActive = false;
    public List<PuzzlePoint> connectedVertices = new List<PuzzlePoint>();
    public Dictionary<PuzzlePoint, PuzzleLine> connectedEdges = new Dictionary<PuzzlePoint, PuzzleLine>();
    public Puzzle_1 puzzle;
    [SerializeField] public SpriteRenderer sr;

    public bool IsActive { get { return isActive; } set { isActive = value; Change(value); } }

    public void Init()
    {
        sr = GetComponent<SpriteRenderer>();
        foreach (PuzzlePoint p in connectedVertices)
        {
            PuzzleLine l = Instantiate(puzzle.line, puzzle.transform.position, Quaternion.identity, puzzle.transform);
            connectedEdges[p] = l;
            p.connectedEdges[this] = l; // 반대편 정점에도 엣지 추가
            l.SetVertices(this, p);
            // puzzle.allEdges.Add(l);
        }
        connectedVertices.Add(this);
    }
    public void Change(bool value)
    {

        if (value)
        {
            SoundManager.instance.PlaySFX(1650082, puzzle.audioSource);
            sr.color = Color.blue;
            puzzle.previousVertex = puzzle.currentVertex;
            puzzle.currentVertex = this;
            puzzle.activeVertices.Add(this);
            if (puzzle.currentVertex != null && puzzle.previousVertex != null)
            {

                if (puzzle.previousVertex.connectedVertices.Contains(puzzle.currentVertex))
                {
                    puzzle.ChangeEdgeColor(puzzle.previousVertex, puzzle.currentVertex);
                    if (puzzle.photonView.IsMine)
                    {
                        puzzle.photonView.RPC("CheckAllLine", RpcTarget.All);
                    }

                }
                else
                {
                    puzzle.currentVertex = null;
                    puzzle.previousVertex = null;
                    if (puzzle.photonView.IsMine)
                    {
                        puzzle.photonView.RPC("ResetColors", RpcTarget.All);
                    }
                }
            }
        }
        else
        {
            sr.color = Color.white;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (puzzle.bothLayer.Contain(collision.gameObject.layer))
        {
            if (photonView.IsMine)
            {
                photonView.RPC("ChangeField", RpcTarget.All);
            }
        }
    }
    [PunRPC]
    public void ChangeField()
    {
        IsActive = true;
    }
}
