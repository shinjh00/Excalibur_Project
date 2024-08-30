using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필  //  퍼즐1에서 정점을 잇는 라인
/// </summary>
public class PuzzleLine : MonoBehaviour,IPunObservable
{
    LineRenderer lineRenderer;
    public PuzzlePoint vertex1;
    public PuzzlePoint vertex2;
    bool isCheck = false;
    public bool IsCheck { get { return isCheck; } set { isCheck = value; ChangeColor(value); } }
    public int IsAlreadyCheck = 0;
    public void SetVertices(PuzzlePoint v1, PuzzlePoint v2)
    {
        vertex1 = v1;
        vertex2 = v2;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, vertex1.transform.localPosition);
        lineRenderer.SetPosition(1, vertex2.transform.localPosition);
    }

    public void ChangeColor(Color color)
    {
        
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        isCheck = true;
    }
    public void ChangeColor(bool isCheck)
    {
        if (isCheck)
        {
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            IsAlreadyCheck++;
        }
        else
        {
            IsAlreadyCheck = 0;
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = Color.white;
        }
    }
    public void ResetColor()
    {
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        IsAlreadyCheck = 0;
        isCheck = false;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isCheck);
            stream.SendNext(IsAlreadyCheck);

        }
        else
        {
            IsCheck = (bool)stream.ReceiveNext();
            IsAlreadyCheck = (int)stream.ReceiveNext();
            
        }
    }
}
