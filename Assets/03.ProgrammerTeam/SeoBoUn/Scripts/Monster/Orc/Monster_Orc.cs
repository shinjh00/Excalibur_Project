using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 오크 몬스터의 스크립트
/// </summary>
public class Monster_Orc : BaseMonster, IInteractable
{
    [SerializeField] AudioSource[] monsterAudio;

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
        base.Start();

        fsm.Init("Idle");

        colliders = new Collider2D[10];
        curDectionRange = monsterData.detectionRange;
    }

    public void Interact(PlayerController player)
    {
        if (isDie)
        {
            corpseController.Interact(player);
        }
        else
        {
            player.StateController.StateChange(PlayerState.Interact, 0, 0, false, false);
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
