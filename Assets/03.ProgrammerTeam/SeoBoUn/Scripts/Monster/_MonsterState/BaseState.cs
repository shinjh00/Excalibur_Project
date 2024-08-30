using System.Collections.Generic;

/// <summary>
/// 개발자 : 서보운
/// 몬스터의 베이스 상태
/// </summary>
public class BaseState
{
    public List<Transition> transitions;
    protected BaseMonster owner;

    /// <summary>
    /// 생성자
    /// </summary>
    public BaseState()
    {
        transitions = new List<Transition>();
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void LateUpdate() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }
}

/// <summary>
/// 몬스터 상태를 표현할 열거형
/// <br/>Idle, Patrol, Alert, Trace, Attack, Return, Standby, Hit, Die
/// </summary>
public enum MonsterState
{
    Idle,
    Patrol,
    Alert,
    Trace,
    Attack,
    Return,
    Standby,
    Hit,
    Die
}