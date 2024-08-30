using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.SocialPlatforms;

/// <summary>
/// 미믹 몬스터의 스크립트
/// </summary>
public class Monster_Mimic : BaseMonster, IInteractable
{
    [SerializeField] AudioSource[] monsterAudio;

    Coroutine showRoutine;

    [SerializeField] SpriteRenderer keyImageRender;
    [SerializeField] float showDistance;
    bool isShow;

    protected override void Awake()
    {
        base.Awake();

        fsm = new MonsterStateMachine();

        fsm.AddState("Idle", new IdleState(this));
        fsm.AddState("Trace", new TraceState(this));
        fsm.AddState("Attack", new AttackState(this));
        fsm.AddState("Alert", new AlertState(this));
        fsm.AddState("Die", new DieState(this));
        fsm.AddState("Hit", new HitState(this));
        fsm.AddState("Return", new ReturnState(this));
        fsm.AddState("Patrol", new PatrolState(this));
        fsm.AddState("Standby", new StandbyState(this));

        // Die는 HP가 0인경우 무조건 전이
        fsm.AddAnyStateTransition("Die", () =>
        {
            return monsterData.hp <= 0;
        });

        fsm.AddAnyStateTransition("Hit", () =>
        {
            return isDamage;
        });

        // Idle -> Alert
        fsm.AddTransition("Idle", "Alert", () =>
        {
            return CheckPlayer();
        });

        fsm.AddTransition("Idle", "Patrol", () =>
        {
            return isPatrol;
        });

        fsm.AddTransition("Patrol", "Idle", () =>
        {
            return !isPatrol;
        });

        // Patrol -> Alert
        fsm.AddTransition("Patrol", "Alert", () =>
        {
            return CheckPlayer();
        });

        // Alert -> Idle
        fsm.AddTransition("Alert", "Idle", () =>
        {
            return !CheckPlayer();
        });

        // Alert -> Patrol
        fsm.AddTransition("Alert", "Patrol", () =>
        {
            return !CheckPlayer();
        });

        // Alert -> Trace
        fsm.AddTransition("Alert", "Trace", () =>
        {
            return startTrace;
        });

        // Trace -> Idle
        fsm.AddTransition("Trace", "Idle", () =>
        {
            return !CheckPlayer();
        });

        // Trace -> Attack
        fsm.AddTransition("Trace", "Attack", () =>
        {
            return CheckAttackRange();
        });

        // Attack -> Trace
        fsm.AddTransition("Attack", "Trace", () =>
        {
            return (!CheckAttackRange() && !IsAttacking);
        });

        // Hit -> Idle
        fsm.AddTransition("Hit", "Trace", () =>
        {
            return !isDamage;
        });
    }

    protected override void Start()
    {
        monsterData = CsvParser.Instance.MonsterDic[id];
        SetAnimator("Standby");
        fsm.Init("Standby");
        base.Start();
        
        colliders = new Collider2D[10];
        monsterUI.gameObject.SetActive(false);
        curDectionRange = monsterData.detectionRange;
        showRoutine = StartCoroutine(ShowImageRoutine());

        animator.SetTrigger("Standby");
    }

    /// <summary>
    /// 미믹 상호작용 메소드
    /// 미믹은 상호작용시 바로 플레이어를 추격
    /// </summary>
    public void Interact(PlayerController player)
    {
        if (isDie)
        {
            corpseController.Interact(player);
        }
        else
        {
            targetPos = player;

            player.StateController.StateChange(PlayerState.Interact, 0, 0, false, false);
            fsm.ChangeState("Trace");
            monsterUI.gameObject.SetActive(true);
            keyImageRender.gameObject.SetActive(false);
            StopCoroutine(showRoutine);
        }
    }

    public override void TakeDamage(float damage, DamageType dmgType = DamageType.Normal )
    {
        monsterUI.gameObject.SetActive(true);
        keyImageRender.gameObject.SetActive(false);
        StopCoroutine(showRoutine);
        base.TakeDamage(damage);
    }

    IEnumerator ShowImageRoutine()
    {
        while (true)
        {
            isShow = false;
            int size = Physics2D.OverlapCircleNonAlloc(transform.position, showDistance, colliders, playerLayer);

            for (int i = 0; i < size; i++)
            {
                PlayerController target = colliders[i].GetComponent<PlayerController>();

                if (target != null)
                {
                    isShow = true;
                    break;
                }
            }
            keyImageRender.gameObject.SetActive(isShow);

            yield return new WaitForSeconds(0.1f);
        }
    }

    [PunRPC]
    public override void CheckSound()
    {
        if (curState == MonsterState.Die)
        {
            monsterAudio[0].Play();
        }

        if (curState == MonsterState.Hit)
        {
            monsterAudio[1].Play();
        }

        if (curState == MonsterState.Attack)
        {
            monsterAudio[2].Play();
        }
    }
}
