using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class QuizButton : MonoBehaviour
{

   public List<PlayerController> players = new List<PlayerController> ();
    [SerializeField] BoxCollider2D area;
   public void GetPlayersInList()
    {
        Collider2D[] cols = Physics2D.OverlapBoxAll(area.bounds.center, area.bounds.size, 0,LayerMask.GetMask("Player"));
        foreach (Collider2D col in cols)
        {
            PlayerController player = col.GetComponent<PlayerController>();
            if (player != null)
            {
                players.Add(player);
            }

        }
    }

    public List<PlayerController> SetPlayers(PlayerState state,bool add)
    {
        foreach (PlayerController p in players)
        {
            p.StateController.StateChange(state, 0, 0,add, false);
        }

        return players;
    }
}
