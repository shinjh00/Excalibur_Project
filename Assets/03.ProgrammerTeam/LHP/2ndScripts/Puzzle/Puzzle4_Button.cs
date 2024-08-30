using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle4_Button : MonoBehaviour,IPunObservable
{
    SpriteRenderer spriteRenderer;
    bool isCheck = false;

    public bool IsCheck { get { return isCheck; } set { isCheck = value; ColorChange(value); } }

    public void Init()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Color.white;
    }
    void ColorChange(bool check)
    {
        if (spriteRenderer == null)
            return;
        if (check)
        {
            spriteRenderer.color = Color.red;

        }
        else
        {
            spriteRenderer.color = Color.white;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(IsCheck);

        }
        else
        {
            IsCheck = (bool)stream.ReceiveNext();
        }
    }
}
