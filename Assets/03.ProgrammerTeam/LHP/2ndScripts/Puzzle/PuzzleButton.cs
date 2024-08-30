using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필 // 퍼즐2에 활용될 버튼
/// </summary>
public class PuzzleButton : MonoBehaviour
{
    public Puzzle_2 puzzle;
    public bool onButton = false;
    [SerializeField] PuzzleFire fire;
    Collider2D[] cols = new Collider2D[4];

    SpriteRenderer fireSr;
    public SpriteRenderer sr;
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        fireSr = fire.GetComponent<SpriteRenderer>();
        fire.Animaion("Clear", true);


    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(puzzle != null)
        {
            if (puzzle.bothLayer.Contain(collision.gameObject.layer) && !puzzle.clear && !onButton)
            {

                ButtonSetOn();
            }
        }

    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!puzzle.clear)
        {
            cols = Physics2D.OverlapBoxAll(transform.position, Vector2.one, 0, puzzle.playerLayer);
            if (cols.Length < 1)
            {
                sr.color = Color.white;
                fire.Animaion("Clear", true);
                fire.circleLight.SetActive(false);
                onButton = false;
            }
        }

    }
    public void ButtonSetOn()
    {
        if (!onButton)
        {
            onButton = true;
            fire.Animaion("Clear", false);
            fire.circleLight.SetActive(true);
            puzzle.photonView.RPC("CheckButton", Photon.Pun.RpcTarget.All);
            sr.color = Color.red;
            SoundManager.instance.PlaySFX(1650082, puzzle.audioSource);
        }

    }

}
