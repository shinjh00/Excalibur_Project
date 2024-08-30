using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 움직임 컨트롤러
/// </summary>
public class PlayerMoveController : BaseController
{
    [SerializeField] Vector2 moveDir;

    [SerializeField] float moveSpeed;

    public Action onMove;
    bool isDash;
    LayerMask wallLayer;

    Collider2D col;
    public float MoveSpeed { get { return moveSpeed; } set { moveSpeed = value; photonView.RPC("UpdateMoveStatSnyc", RpcTarget.All, PlayerStat.MoveSpeed, value); } }
    public bool IsDash { get { return isDash; } set { isDash = value; } }
    public Vector2 MoveDir { get { return moveDir; } }
    private InputAction moveAction;
    public InputAction MoveAction { get { return moveAction; } }

    /// <summary>
    /// 개발자 : 서보운
    /// <br/> MoveController에 대한 동기화 메소드들
    /// </summary>
    /// <param name="stat"></param>
    /// <param name="value"></param>
    [PunRPC]
    private void UpdateMoveStatSnyc(PlayerStat stat, float value)
    {
        switch(stat)
        {
            case PlayerStat.MoveSpeed:
                moveSpeed = (float)value;
                break;
        }
    }

    /// <summary>
    /// owner에 있는 플레이어 데이터 참조용 메소드
    /// </summary>
    protected override void GetStat()
    {
        moveSpeed = owner.PlayerClassData.moveSpeed;
        var inputActionAsset = GetComponent<PlayerInput>().actions;
        moveAction = inputActionAsset.FindAction("Move");
        col = GetComponent<Collider2D>();
        wallLayer = LayerMask.GetMask("wall");
    }

    private void FixedUpdate()
    {
         if (owner != null && !owner.StateController.CurState.Contain(PlayerState.Interact|PlayerState.Dead|PlayerState.Sleep))
         {
            Move(moveDir);
            /*   if (moveAction.IsPressed())
               {
                   owner.StateController.StateChange(PlayerState.Idle, 0, 0, false, false);
                   owner.StateController.StateChange(PlayerState.Move, 0, 0, true, false); // 상태 변경
               }
               else
               {
                   owner.StateController.StateChange(PlayerState.Idle, 0, 0, true, false);
                   owner.StateController.StateChange(PlayerState.Move, 0, 0, false, false  ); // 상태 변경
               }*/
        }


    }

    /// <summary>
    /// 기본 이동 함수
    /// </summary>
    public void Move(Vector2 moveDir)
    {
        this.moveDir = moveDir;
        owner.SetAnimator("Move", moveDir.magnitude);

        transform.Translate(this.moveDir * moveSpeed * Time.fixedDeltaTime, Space.World);
        onMove?.Invoke();
    }
    
    public void SetMoveDir(Vector2 moveDir)
    {
        this.moveDir = moveDir;
        onMove?.Invoke();
       
    }
    /// <summary>
    /// 이동속도를 바꿔주는 루틴
    /// </summary>
    /// <param name="slowValue"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public IEnumerator MoveSpeedChange(float slowValue, float time)
    {
        if(owner.StatController.ContainBuff(StatLevel.MoveSpeed, slowValue))
        {
            owner.StatController.RemoveBuffStat(StatLevel.MoveSpeed, slowValue);
        }
        owner.StatController.AddBuffStat(StatLevel.MoveSpeed, slowValue);
        yield return new WaitForSeconds(time);
        owner.StatController.RemoveBuffStat(StatLevel.MoveSpeed, slowValue);
        owner.StateController.StateChange(PlayerState.Slow, 0, 0, false, false);
    }
    /// <summary>
    /// 이동 속도에 대한 값을 정지시키는 메소드
    /// </summary>
    public void MovingStop()
    {
        moveDir = Vector2.zero;
    }

    // TODO : 천천히 이동 구현


    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 대쉬 상태일 땐 벽을 제외한 모든 충돌체를 무시
        if (!isDash)
        {
            return;
        }

        if (wallLayer.Contain(collision.gameObject.layer))
        {
            return;
        }

        StartCoroutine(IgnoreRoutine(col, col));
    }

    IEnumerator IgnoreRoutine(Collider2D playerCollider, Collider2D other)
    {
        Physics2D.IgnoreCollision(playerCollider, other, true);
        Physics2D.IgnoreCollision(other, playerCollider, true);

        yield return new WaitForSeconds(0.15f);

        Physics2D.IgnoreCollision(playerCollider, other, false);
        Physics2D.IgnoreCollision(other, playerCollider, false);
    }
}
