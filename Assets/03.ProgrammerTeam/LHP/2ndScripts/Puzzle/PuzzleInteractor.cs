using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleInteractor : MonoBehaviour,IInteractable
{
    Puzzle_3 puzzle;



    public void Init(Puzzle_3 puzzle)
    {
        this.puzzle = puzzle;
    }
    public void Interact(PlayerController player)
    {
        if (puzzle.isDeactivated)
        {
            player.StateController.StateChange(PlayerState.Interact, 0, 0, false, false);
            return;
        }
        if (!player.photonView.IsMine)
        {
            player.StateController.StateChange(PlayerState.Interact, 0, 0, false, false);
            return;
        }

        player.StateController.StateChange(PlayerState.Interact, 0, 0, false, false);
        puzzle.photonView.RPC("ClearGolem", RpcTarget.All);
        Destroy(this);
    }
}
