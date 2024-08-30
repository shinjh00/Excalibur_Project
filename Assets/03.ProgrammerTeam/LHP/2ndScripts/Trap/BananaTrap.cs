using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필 / 바나나함정 클래스
/// </summary>
public class BananaTrap : Trap
{
    Rigidbody2D rb;
    [SerializeField] float slideTime = 2f;
    [SerializeField] float slideSpeed = 7f;

    /* public override void CreateTrap(Vector2 pos)
     {
         PhotonNetwork.InstantiateRoomObject("6.Prefab/Trap/BananaTrap", pos, Quaternion.identity);
     }*/
    [PunRPC]
    protected override void Init()
    {
        base.Start();
 //       range = CsvParser.Instance.TrapDic[trapId].range;
        BoxCollider2D thisCol = GetComponent<BoxCollider2D>();
        thisCol.size = new Vector2(range, range);

    }
    protected override void Activate(bool IsActivated)
    {

        if (IsActivated)
        {
            SoundManager.instance.PlaySFX(activeSound,audioSource);
            if (anim != null)
                anim.SetTrigger("Trap");
            if (targetPlayer != null)
            {
                rb = targetPlayer.gameObject.GetComponent<Rigidbody2D>();
                targetPlayer.PlayerAnim.SetFloat("Move", 0);
                targetPlayer.MoveController.enabled = false;
                targetPlayer.SkillController.enabled = false;


                if (targetPlayer.photonView.IsMine)
                {
                    StartCoroutine(BananaRoutine());
                }
            }
            

        }
        else
        {
            targetPlayer = null;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (playerLayer.Contain(collision.gameObject.layer))
        {
            targetPlayer = collision.GetComponent<PlayerController>();
            if (targetPlayer.photonView.IsMine)
            {
                photonView.RPC("SetActive", RpcTarget.All, true, targetPlayer.photonView.ViewID);

            }
        }
    }
    /// <summary>
    /// 미끄러지는 루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator BananaRoutine()
    {
        float t = 0;

        Vector2 slideDirection = targetPlayer.MoveController.MoveDir.normalized * slideSpeed;
        LayerMask layer = playerLayer | wall | monster; // 감지할 레이어 설정
        targetPlayer.StateController.StateChange(PlayerState.Silence, 0, 0, true, false);
        while (t < slideTime)
        {
            rb.velocity = slideDirection;
            t += Time.deltaTime;

            // 플레이어의 현재 위치에서 미끄러지는 방향으로 오버랩 실행
            Collider2D[] cols = Physics2D.OverlapBoxAll((Vector2)targetPlayer.transform.position + targetPlayer.MoveController.MoveDir.normalized*0.5f,new Vector2(1f,1f), 0, layer);

            foreach(Collider2D col in cols)
            {
                if (col.gameObject != null)
                {
                    // 충돌한 게임 오브젝트가 targetPlayer가 아니면 루프 탈출
                    if (col.gameObject != targetPlayer.gameObject)
                    {
                        rb.velocity = targetPlayer.MoveController.MoveDir.normalized * 0.1f;

                       Rigidbody2D rb2 =  col.gameObject.GetComponent<Rigidbody2D>();
                        if(rb2 != null)
                        {
                            rb2.velocity = Vector2.zero;
                        }

                        break;
                    }
                }
            }

            yield return null;
        }
        targetPlayer.MoveController.enabled = true;
        targetPlayer.SkillController.enabled = true;
        rb.velocity = Vector2.zero;
        targetPlayer.StateController.StateChange(PlayerState.Silence, 0, 0, false, false);
        if (!targetPlayer.MoveController.MoveAction.IsPressed())
        {
            targetPlayer.MoveController.MovingStop();
            
        }
        photonView.RPC("SetActive", RpcTarget.All, false,targetPlayer.photonView.ViewID);
        targetPlayer = null;
        // 플레이어 컨트롤러 다시 활성화

    }

    [PunRPC]
    public override void SetActive(bool active, int viewID )
    {
        IsActivate = active; 
        if (active)
        {
            if (viewID != -1 || viewID != 0)
            {
                targetPlayer = GameManager.Ins.FindPlayer(viewID);
                GameManager.Ins.Message($"Find : {viewID}");
            }
        }

    }
    [PunRPC]
    void SetTrap(int trapID)
    {
        range = CsvParser.Instance.TrapDic[trapID].range;
        coolTime = CsvParser.Instance.TrapDic[trapID].coolTime;
        damage = CsvParser.Instance.TrapDic[trapID].perDamage;
        knockBack = CsvParser.Instance.TrapDic[trapID].knockBackDis;
        activeSound = CsvParser.Instance.TrapDic[trapID].trapSound;
        Init();
    }
}
