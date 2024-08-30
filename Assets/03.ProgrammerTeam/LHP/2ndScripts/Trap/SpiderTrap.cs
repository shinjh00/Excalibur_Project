
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필 / 거미함정을 구현하는 클래스
/// </summary>
public class SpiderTrap : Trap
{
    Coroutine coroutine;
    [SerializeField] float slowValue = 0.1f;
    [SerializeField] float slowTime;
    [SerializeField] GameObject rangeObj;
    protected override void Activate(bool isActivated )
    {



        if (isActivated)
        {
            SoundManager.instance.PlaySFX(activeSound, audioSource);
            if (anim != null)
                anim.SetTrigger("Trap");
            if (coroutine != null)
                StopCoroutine(coroutine);
            coroutine = StartCoroutine(CoolTimeStart());
            if(targetPlayer != null)
            {
                GameManager.Ins.Message("ActiveTrap");
                coolChecker = true;
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, playerLayer);
                if (hits.Length > 0)
                {
                    foreach (Collider2D hit in hits)
                    {
                        IStateable stateAble = hit.GetComponent<IStateable>();
                        if (stateAble != null)
                        {
                            stateAble.StateChange(PlayerState.Slow, slowValue, slowTime, true, false);
                        }
                    }
                }
            }
        }
        else
        {
            targetPlayer = null;
        }
    }
    [PunRPC]
    protected override void Init()
    {
        base.Start();
        CircleCollider2D coffinRange = GetComponent<CircleCollider2D>();
        coffinRange.radius = range;
        rangeObj.transform.localScale = new Vector2(range, range);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, range);
    }
  /*  public override void CreateTrap(Vector2 pos)
    {
        PhotonNetwork.InstantiateRoomObject("6.Prefab/Trap/SpiderTrap", pos, Quaternion.identity);
    }*/
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (playerLayer.Contain(collision.gameObject.layer) && !coolChecker && !IsActivate)
        {
            targetPlayer = collision.GetComponent<PlayerController>();
            
            if (targetPlayer.photonView.IsMine)
            {
                photonView.RPC("SetActive", RpcTarget.All, true, targetPlayer.photonView.ViewID);
            }
        }
    }
    /// <summary>
    /// 해당 트랩 쿨타임
    /// </summary>
    /// <returns></returns>
    IEnumerator CoolTimeStart()
    {
        yield return new WaitForSeconds(1f);
        sr.color = Color.clear;
        yield return new WaitForSeconds(coolTime - 2);
        sr.color = Color.white;
        if (anim != null)
            anim.SetTrigger("TrapOn");
        yield return new WaitForSeconds(2);
        coolChecker = false;
        photonView.RPC("SetActive", RpcTarget.All, false, -1);
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
