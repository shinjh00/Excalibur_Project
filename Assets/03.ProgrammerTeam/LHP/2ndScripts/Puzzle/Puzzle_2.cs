using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 개발자 : 이형필 // 퍼즐2를 생성하는 클래스
/// </summary>
public class Puzzle_2 : Puzzle, IPunObservable
{
    [SerializeField] List<PuzzleButton> teamButtons = new List<PuzzleButton>();
    [SerializeField] List<PuzzleButton> IndButtons = new List<PuzzleButton>();
    [SerializeField] List<PuzzleButton> buttons = new List<PuzzleButton>();
    [SerializeField] public bool clear = false;

    protected override void Start()
    {
        base.Start();

        if (PhotonNetwork.CurrentRoom.GetProperty<bool>(DefinePropertyKey.TEAMGAME))
        {
            buttons = teamButtons;
        }
        else
        {
            buttons = IndButtons;
        }

        foreach(PuzzleButton b in buttons)
        {
            b.gameObject.SetActive(true);
            b.puzzle = this;
        }

    }
    [PunRPC]
    public void CheckButton()
    {
        foreach (var button in buttons)
        {
            if (!button.onButton)
            {
                return;
            }
        }
        if (photonView.IsMine)
        {
            SpawnBox();
            clear = true;
            foreach (var button in buttons)
            {
                button.onButton = true;
                button.ButtonSetOn();
            }
        }
        SoundManager.instance.PlaySFX(1650084, audioSource);

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (stream.IsWriting)
        {
            stream.SendNext(clear);
        }
        else if (stream.IsReading)
        {
            clear = (bool)stream.ReceiveNext();
        }
    }
}
