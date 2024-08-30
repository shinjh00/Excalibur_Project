using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizStartButton : MonoBehaviour, IInteractable
{
    public bool isStart;
    public Puzzle_5 puzzle;
    public int idx = 0;
    [SerializeField] SpriteRenderer spriteRenderer;
    public Collider2D col;
    public void Init()
    {
        col = GetComponent<Collider2D>();
    }
    public void Interact(PlayerController player)
    {
        player.StateController.StateChange(PlayerState.Interact, 0, 0, false, false);
        col.enabled = false;
        spriteRenderer.enabled = false;
        if (!isStart)
        {
           
            puzzle.photonView.RPC("StartQuiz", Photon.Pun.RpcTarget.All);


        }
        else if(!puzzle.isStartQuiz)
        {

            puzzle.photonView.RPC("LoadQuiz", Photon.Pun.RpcTarget.All,idx);
        }

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        PlayerController player = collision.GetComponent<PlayerController>();
        if(player != null && player.photonView.IsMine)
        {
            spriteRenderer.enabled = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null && player.photonView.IsMine)
        {
            spriteRenderer.enabled = false;
        }
    }
}
