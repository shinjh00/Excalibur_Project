using Photon.Pun;
using System.Collections;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필 /관 트랩
/// </summary>
public class CoffinTrap : Trap
{
    [SerializeField] float jump = 2f;
    [SerializeField] float jumpTime = 1f;
    [SerializeField] GameObject rangeObj;
    PlayerHealthController PHC;
    PlayerStateController playerState;
    protected override void Activate(bool isActivated)
    {
        StartCoroutine(WaitForTargetPlayer(isActivated));
    }
    IEnumerator WaitForTargetPlayer(bool isActivated)
    {
        yield return new WaitUntil(() => targetPlayer != null);
        if (isActivated)
        {
            SoundManager.instance.PlaySFX(activeSound, audioSource);
            if (anim != null)
            {
                anim.SetTrigger("Trap");
            }
            if (targetPlayer != null)
            {
                targetPlayer.StateController.StateChange(PlayerState.invincible, 0, 5, true, false);
                playerState = targetPlayer.GetComponent<PlayerStateController>();
                PHC = targetPlayer.GetComponent<PlayerHealthController>();
                PHC.enabled = false;
                if (targetPlayer.photonView.IsMine)
                {
                    StartCoroutine(jumpRoutine(targetPlayer));
                    StartCoroutine(CoolTimeStart());
                }
            }
        }
    }
    /// <summary>
    /// 해당 트랩 쿨타임
    /// </summary>
    /// <returns></returns>
    IEnumerator CoolTimeStart()
    {
        yield return new WaitForSeconds(coolTime);
        photonView.RPC("SetActive", RpcTarget.All, false, -1);
    }
    /// <summary>
    /// 베지어곡선으로 자연스럽게 관 트랩에 끌려가는 코루틴
    /// </summary>
    /// <param name=
    ///
    /// ></param>
    /// <returns></returns>
    IEnumerator jumpRoutine(PlayerController player)
    {
        Vector3 hitPos = player.transform.position;
        Vector3 coffin = transform.position;
        float t = 0;
        while (t < jumpTime)
        {
            player.transform.position = BezierCurve.Bezier(hitPos, new Vector3(hitPos.x, coffin.y + jump, 0), coffin + new Vector3(0, jump, 0), coffin, t);
            t += Time.deltaTime / jumpTime;
            yield return null;
        }
        player.StateController.trapResistStart = true;
        player.StateController.PlayerSpriteColorChange(Color.clear);
        //    playerState.StateChange(PlayerState.invincible,0, 0, false, false);
        photonView.RPC("OnSleep", RpcTarget.All);
        player.Rigid.bodyType = RigidbodyType2D.Static;
    }
    [PunRPC]
    public void OnSleep()
    {
        playerState.StateChange(PlayerState.Sleep, this, coolTime, true, false);
    }
    [PunRPC]
    public void TrapOn()
    {
        playerState.PlayerSpriteColorChange(Color.white);
        playerState.StateChange(PlayerState.Idle, 0, 0, true, false);
        playerState.StateChange(PlayerState.Sleep, 0, 0, false, false);
        playerState.StateChange(PlayerState.invincible, 0, 0, false, false);
        targetPlayer.Rigid.bodyType = RigidbodyType2D.Dynamic;
        if (playerState.sleepRoutine != null)
        {
            Debug.Log(playerState.sleepRoutine);
            StopCoroutine(playerState.sleepRoutine);
            playerState.sleepRoutine = null; // 초기화
        }
        playerState.t = 1000;
        targetPlayer = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (playerLayer.Contain(collision.gameObject.layer) && !coolChecker)
        {
            targetPlayer = collision.GetComponent<PlayerController>();
            if (targetPlayer.StateController.CurState.Contain(PlayerState.Dead | PlayerState.Groggy))
            {
                return;
            }
            if (targetPlayer.photonView.IsMine)
            {
                photonView.RPC("SetActive", RpcTarget.All, true, targetPlayer.photonView.ViewID);
            }
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
    [PunRPC]
    public override void SetActive(bool active, int viewID)
    {
        coolChecker = active;
        IsActivate = active;
        if (active)
        {
            if (viewID != -1 || viewID != 0)
            {
                targetPlayer = GameManager.Ins.FindPlayer(viewID);
                GameManager.Ins.Message($"Find : {viewID}");
            }
        }
        else
        {
            coolChecker = false;
            anim.SetTrigger("TrapOn");
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