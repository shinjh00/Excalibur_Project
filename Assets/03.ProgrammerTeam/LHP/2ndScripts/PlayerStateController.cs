using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 개발자 : 이형필 / 플레이어의 상태를 참조하고 변환하기 위한 클래스
/// </summary>
public class PlayerStateController : BaseController, IStateable
{
    [SerializeField] SpriteRenderer playerEmoji;
    [Tooltip("캐릭터가 보여질 렌더러 전부 할당")]
    [SerializeField] SpriteRenderer[] playerSprite;
    [SerializeField] Image[] hpBorders;
    [Tooltip("캐릭터캔버스의 HPBorder 할당")]
    [SerializeField] GameObject hpBorder;
    [Tooltip("Resources/2.Sprite/Slot/UI_Graphic_Assets의 SleepEmoji 참조")]
    [SerializeField] Sprite sleepEmoji;
    [Tooltip("Resources/2.Sprite/Slot/UI_Graphic_Assets의 GroggyEmoji 참조")]
    [SerializeField] Sprite GroggyEmoji;
    [SerializeField] public bool trapResistStart;
    [SerializeField] public float t = 0;
    [SerializeField] PlayerState curState;


    [Header("GROGGY SETTING")]
    [Tooltip("부활했을때 체력")]
    [SerializeField] float groggyRevive;
    [Tooltip("그로기 게이지가 차는 사이 시간")]
    [SerializeField] float groggyTerm;
    [Tooltip("그로기 게이지가 차는 양")]
    [SerializeField] float groggyClearValue;
    [Tooltip("그로기에서 사망 시 까지 걸리는 시간")]
    [SerializeField] int groggyCount;
    Coroutine groggyCheck;
    Rigidbody2D playerRigid;
    Action Updates;
    private void Start()
    {
        Updates += Test;
    }
    void Update()
    => Updates?.Invoke();
    void Test()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if(owner.photonView.IsMine)
            owner.HealthController.TakeDamage(200);
        }
    }
    protected override void GetStat()
    {
        StateChange(PlayerState.Idle, 0, 0, true, true);
        playerRigid = gameObject.GetComponent<Rigidbody2D>();
        if (owner.PlayerClassData.classType == ClassType.Excalibur)
        {
            StateChange(PlayerState.SuperArmor, 0, 0, true, false);
        }
    }

    Coroutine slowCorouine;
    Coroutine invincibleRoutine;
    Coroutine knockBackRoutine;
    Coroutine hitRoutine;
    Coroutine attackRoutine;
    public Coroutine sleepRoutine;

    public PlayerState CurState { get { return curState; } set { curState = value; } }
    /// <summary>
    /// 플레이어 렌더러의 색을 바꿈
    /// </summary>
    /// <param name="color">바꿀 색</param>
    public void PlayerSpriteColorChange(Color color)
    {
        if (owner.photonView.IsMine)
        {
            float[] colorData = new float[] { color.r, color.g, color.b, color.a };
            photonView.RPC("RPC_SetColor", RpcTarget.All, colorData);
        }

    }
    [PunRPC]
    public void RPC_SetColor(float[] colorData)
    {
        Color color = new Color(colorData[0], colorData[1], colorData[2], colorData[3]);
        foreach (var sprite in playerSprite)
        {
            sprite.color = color;
        }
    }
    public void MaterialSet(Material mat)
    {
        foreach (var sprite in playerSprite)
        {
            sprite.material = mat;
        }
        foreach (var image in hpBorders)
        {
            image.material = mat;
        }
    }
    /// <summary>
    /// hp보더 껐다켰다 할수 있도록
    /// </summary>
    /// <param name="set"></param>
    public void BorderSetter(bool set)
    {
        hpBorder.SetActive(set);
    }
    /// <summary>
    /// 플레이어의 상태를 바꿈
    /// </summary>
    /// <param name="state">바꿀 상태</param>
    /// <param name="param">상태이상에 적용할 값</param>
    /// <param name="time">상태이상에 적용할 시간</param>
    /// <param name="addState">상태이상을 적용할 것인지 해제할 것인지</param>
    /// <param name="fixThisState">true로 할 시 이 상태이상만 적용시킨다</param>
    public void StateChange(PlayerState state, object param, float time, bool addState, bool fixThisState)
    {
        if (curState.Contain(PlayerState.Dead | PlayerState.Groggy))
        {
            if (state == PlayerState.Dead && addState == false)       //사망에서 상태변화는 부활하는 경우를 제외하고 전부 무시
            {
                curState = PlayerState.Idle;
            }
            else if (state == PlayerState.Groggy && addState == false) //그로기에서 상태변화는 부활하는 경우를 제외하고 전부 무시
            {
                Debug.Log("Groggy Cleared");
                Updates -= CheckOtherPlayer;
                curState = PlayerState.Idle;
            }
            else if (state == PlayerState.Dead && addState == true)
            {
                curState = PlayerState.Dead;
                //    StateChange(PlayerState.invincible, 0, 999, true, true);
            }
            else
            {
                return;
            }
        }
        // 1. 강제 적용 상태이상 검사. -> fixThisState에 따라 설정
        PlayerState prevState = curState;
        curState = fixThisState ? state : (addState ? (curState | state) : (curState & ~state));
      /*  if(prevState != curState )
        {
            GameManager.Ins.Message($"{state} => {addState}");
        }*/

        if (addState)
        {
            if (owner != null)
            {   // 플레이어 컨트롤러 입장에서 상태가 바뀌었을 때 작업을 진행하도록 설정하기 위한 메소드
                owner.AddStateEvent(state);
            }
            switch (state)
            {
                case PlayerState.Idle:  // Idle State.
                    ChangeEmoji(PlayerState.Idle);

                    if (playerRigid != null && playerRigid.bodyType == RigidbodyType2D.Static)
                    {
                        playerRigid.bodyType = RigidbodyType2D.Dynamic;
                    }
                    break;
                case PlayerState.Move:  // Move State. 
                    break;
                case PlayerState.Slow:  // Slow State. (거미알 전용)
                    if (slowCorouine != null)
                    {
                        StopCoroutine(slowCorouine);
                    }
                    slowCorouine = StartCoroutine(owner.MoveController.MoveSpeedChange((float)param, time));
                    break;
                case PlayerState.Sleep: // Sleep State. (관 전용)
                    if (owner.photonView.IsMine)
                    {
                        owner.InSleep();
                        sleepRoutine = StartCoroutine(SleepRoutine(param, time));
                    }

                    ChangeEmoji(PlayerState.Sleep);
                    break;
                case PlayerState.AttackStart:   // AttackStart
                    // Idle 상태는 빠져나오고
                    StateChange(PlayerState.Idle, 0, 0, false, false);
                    if (owner.photonView.IsMine)
                    {   // 움직이는 동안에 이동 속도는 0.5배(디버프)
                        owner.StatController.AddBuffStat(StatLevel.MoveSpeed, 0.5f);
                    }
                    break;
                case PlayerState.AttackExit:    // AttackExit
                    if (attackRoutine != null)
                    {   // 어택 루틴이 있다면 정지
                        StopCoroutine(attackRoutine);
                    }   // AttackExit보다 Attcking + AttackExit 동시에 진행
                    attackRoutine = StartCoroutine(AttackRoutine());
                    break;
                case PlayerState.SkillStart:
                    StateChange(PlayerState.Idle, 0, 0, false, false);
                    if (owner.PlayerClassData.classType != ClassType.Excalibur)
                    {   // 엑스칼리버 유저는 움직이면서 스킬을 사용함.
                        owner.MoveController.enabled = false;
                      //  owner.MoveController.MovingStop();
                    }
                    break;
                case PlayerState.SkillExit:
                    owner.MoveController.enabled = true;
                    if (owner.MoveController.MoveAction.IsPressed())
                    {
                        owner.MoveController.Move(owner.MoveController.MoveDir);
                    }
                    StateChange(PlayerState.SkillStart, 0, 0, false, false);
                    StateChange(PlayerState.SkillExit, 0, 0, false, false);
                    StateChange(PlayerState.Idle, 0, 0, true, false);
                    break;
                case PlayerState.Silence:       // 바나나 트랩용
                    break;
                case PlayerState.Interact:      // 상호작용
                    owner.MoveController.MovingStop();
                    owner.SetAnimator("Move", 0);
                    break;
                case PlayerState.Puzzle:
                    break;
                case PlayerState.UnAttackable:
                    break;
                case PlayerState.Knockback:
                    if (CurState.Contain(PlayerState.invincible))
                    {   // 무적 상태에서는 넉백 불가
                        return;
                    }
                    if (param is float invincibleTime)
                    {   // 무적 시간과 넉백 시간을 적용
                        photonView.RPC("KnockBack", RpcTarget.All, time, invincibleTime);
                    }
                    break;
                case PlayerState.invincible:
                    if (time > 99)
                    {   // 사망 상태에 대한 예외 처리. (사망하면 999)
                        return;
                    }
                    if (invincibleRoutine != null)
                    {   // 무적 루틴이 있었다면
                        StopCoroutine(invincibleRoutine);
                    }
                    invincibleRoutine = StartCoroutine(Invincible(time));
                    break;
                case PlayerState.Dead:
                    // 사망 시 무적 처리.
                    //  StateChange(PlayerState.invincible, 0, 999, true, true);
                    break;
                case PlayerState.Hit:
                    if (CurState.Contain(PlayerState.invincible))
                    {   // 무적 상태라면 진행하지 않음.
                        return;
                    }
                    if (!curState.Contain(PlayerState.SuperArmor))
                    {   // 슈퍼 아머는 어택도, 스킬도 끊기지 않음
                        HitEvent?.Invoke(); // AttackExit 할당
                    }
                    if (hitRoutine != null)
                    {
                        StopCoroutine(hitRoutine);
                    }
                    hitRoutine = StartCoroutine(HitRoutine(time));
                    break;
                case PlayerState.Metamorph:
                    break;
                case PlayerState.Maintain:
                    break;
                case PlayerState.Lobby:
                    break;
                case PlayerState.Chat:
                    break;

                case PlayerState.SuperArmor:    // 추가 작업. 슈퍼아머 작업
                    // Hit에서 데미지는 적용되나, 넉백이 되지 않으며
                    // 모든 공격과 스킬이 끊기지 않음
                    break;
                case PlayerState.Groggy:
                    Debug.Log("State Change Groggy");
                    ChangeEmoji(PlayerState.Groggy);
                    if (owner.photonView.IsMine)
                    {
                        GameManager.Ins.CheckOtherPlayerGroggy(PhotonNetwork.LocalPlayer);
                        Updates += CheckOtherPlayer;
                        if (Updates != null)
                        {
                            foreach (var d in Updates.GetInvocationList())
                            {
                                Debug.Log($"Method: {d.Method.Name}, Target: {d.Target}");
                            }
                        }
                        else
                        {
                            Debug.Log("Updates is null");
                        }
                        if (groggyCheck != null)
                        {
                            Debug.Log("Already Groggy");
                            StopCoroutine(groggyCheck);
                        }
                        groggyCheck = StartCoroutine(GroggyCheckRoutine());
                    }

                    Color color = Color.red;
                    //  owner.photonView.RPC("SetHpBarColor",RpcTarget.All,(color.r,color.g,color.b,color.a));
                    break;

            }
        }

    }

    public Action HitEvent;

    IEnumerator AttackRoutine()
    {
        if (owner.photonView.IsMine)
        {
            owner.StatController.RemoveBuffStat(StatLevel.MoveSpeed, 0.5f);
        }
        StateChange(PlayerState.AttackStart, 0, 0, false, false);
        StateChange(PlayerState.AttackExit, 0, 0, false, false);
        StateChange(PlayerState.Idle, 0, 0, true, false);
        yield return null;
    }
    [PunRPC]
    public void KnockBack(float time, float invincibleTime)
    {
        if (knockBackRoutine != null)
        {
            StopCoroutine(knockBackRoutine);
        }
        if (!curState.Contain(PlayerState.SuperArmor))
        {   // 슈퍼아머 상태에서는 넉백은 적용되지 않고 무적만 적용됨.
            knockBackRoutine = StartCoroutine(KnockBackRoutine(time));
        }
        StateChange(PlayerState.invincible, 0, invincibleTime, true, false);
    }
    IEnumerator KnockBackRoutine(float time)
    {
        owner.AttackController.enabled = false;
        owner.MoveController.enabled = false;
        owner.PlayerAnim.SetBool("Hit", true);
        yield return new WaitForSeconds(time);
        owner.PlayerAnim.SetBool("Hit", false);
        StateChange(PlayerState.Knockback, 0, 0, false, false);
        owner.AttackController.enabled = true;
        owner.MoveController.enabled = true;
    }
    IEnumerator Invincible(float time)
    {
        yield return new WaitForSeconds(time);
        StateChange(PlayerState.invincible, 0, 0, false, false);
    }

    /// <summary>
    /// 상태이상 수면에 걸렸을 경우 실행시킬 루틴
    /// <br/> param : 관 오브젝트, time : 상태이상 시간
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator SleepRoutine(object param, float time)
    {
        t = time;
        while (t > 0)
        {
            t -= Time.deltaTime;
            yield return null;
        }
        owner.AllControllerStart();
        CoffinTrap coffin = param as CoffinTrap;
        if (coffin != null)
        {
            coffin.photonView.RPC("TrapOn", RpcTarget.All);
        }
    }
    /// <summary>
    /// 피격상태 루틴 time이후 피격상태 종료
    /// </summary>
    /// <param name="time">피격상태 지속시간</param>
    /// <returns></returns>
    IEnumerator HitRoutine(float time)
    {
        if (CurState.Contain(PlayerState.Interact|PlayerState.Maintain ) )
        {
            StartCoroutine(GameManager.Ins.CanvasColorChange(1, Color.red));
        }

        yield return new WaitForSeconds(time);
        StateChange(PlayerState.Hit, 0, 0, false, false);
        StateChange(PlayerState.Idle, 0, 0, true, false);
    }
    /// <summary>
    /// 플레이어의 이모지를 상태이상에 따라 표기해주는 메서드
    /// </summary>
    /// <param name="state"></param>
    void ChangeEmoji(PlayerState state)
    {
        if (owner.photonView.IsMine)
        {
            photonView.RPC("EmojiSync", RpcTarget.All, state);
        }

    }
    [PunRPC]
    private void EmojiSync(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                playerEmoji.sprite = null;
                t = -1;
                trapResistStart = false;
                break;
            case PlayerState.Sleep:
                playerEmoji.sprite = sleepEmoji;
                break;
            case PlayerState.Groggy:
                playerEmoji.sprite = GroggyEmoji;
                break;
        }

    }


    bool activeGroggyClear;


    IEnumerator GroggyCheckRoutine()
    {
        GameManager.Ins.Message("Groggy Dead Routine Start");
        yield return new WaitForSeconds(groggyCount);
        GameManager.Ins.Message("Groggy Dead Routine Check");
        if (CurState.Contain(PlayerState.Groggy))
        {

            StateChange(PlayerState.Dead, 0, 0, true, true);
            GameManager.Ins.Message("TIME OUT YOU DIE");
            ChangeEmoji(PlayerState.Idle);
            GameManager.Ins.DieEvent();
        }
    }
    void CheckOtherPlayer()
    {
        Debug.Log("Check");
        Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, 2f, LayerMask.GetMask("Player"));
        Debug.Log($"length : {players.Length}");
        if (activeGroggyClear)
        {
            Debug.Log($"Already active");

            if (players.Length <= 1)
            {
                ClearGroggy(false);
            }
            return;
        }
        else
        {
            for (int i = 0; i < players.Length; i++)
            {
                PlayerController targetPlayer = players[i].GetComponent<PlayerController>();

                if (targetPlayer != null && targetPlayer.photonView.Owner.IsTeammate() && targetPlayer != owner)
                {

                    ClearGroggy(true);
                }
            }
        }



    }
    Coroutine groggyRoutine;
    void ClearGroggy(bool Active)
    {
        if (Active)
        {

            activeGroggyClear = true;
            groggyRoutine = StartCoroutine(ClearGroggyRoutine());

        }
        else
        {
            FailGroggyClear();

        }
    }
    IEnumerator ClearGroggyRoutine()
    {
        GameManager.Ins.Message("Groggy Clearing..");
        ChangeEmoji(PlayerState.Idle);
        while (owner.HealthController.Hp < owner.HealthController.MaxHp)
        {
            yield return new WaitForSeconds(groggyTerm);
            owner.HealthController.Hp += (owner.HealthController.MaxHp * groggyClearValue);
        }
        owner.photonView.RPC("SuccessGroggyClear",RpcTarget.All);
        yield break;
    }
    [PunRPC]
    void SuccessGroggyClear()
    {
        GameManager.Ins.Message("Clear groggy Success");
        owner.SetAnimator("Revive");
        owner.AllControllerStart();
        owner.HealthController.Hp = (owner.HealthController.MaxHp * groggyRevive);
        owner.MoveController.MovingStop();
        owner.StateController.StateChange(PlayerState.Groggy, 0, 0, false, false);
        Color color = Color.white;
        //   photonView.RPC("SetHpBarColor", RpcTarget.All, (color.r, color.g, color.b, color.a));
    }
    void FailGroggyClear()
    {
        ChangeEmoji(PlayerState.Groggy);
        GameManager.Ins.Message("Clear groggy fail");
        if (groggyRoutine != null)
        {
            StopCoroutine(groggyRoutine);
        }
        activeGroggyClear = false;
        owner.HealthController.Hp = 0f;
    }
}
