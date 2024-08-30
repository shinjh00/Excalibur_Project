using Photon.Pun;
using UnityEngine;

public class Puzzle4_StartButton : MonoBehaviour,IInteractable
{
    public Puzzle_4 puzzle;
    public PlayerController player;

    public void ExitPuzzle()
    {
        // puzzle.checker.SetActive(false);
        player.StateController.StateChange(PlayerState.Puzzle, 0, 0, false, false);
        player.StateController.StateChange(PlayerState.Interact, 0, 0, false, false);
      
    }

    public void Interact(PlayerController player)
    {
        Debug.Log($"puzzle Activate : {puzzle.isStart}  ");
        if (!puzzle.isStart)
        {

            this.player = player;
            player.CameraMove(puzzle.transform);
            player.HealthController.OnHit.AddListener(puzzle.Exit);
           
            if (player.photonView.IsMine)
            {
                puzzle.GameStart(player);
            }
        }
        else
        {
            Debug.Log("already Start");
            if(this.player == player)
            {
                Debug.Log("already Start player is Exit");
                puzzle.Exit();
            }
            else
            {
                Debug.Log(" already other player is");
                player.StateController.StateChange(PlayerState.Interact, 0, 0, false, false);
            }
        }
    }

}
