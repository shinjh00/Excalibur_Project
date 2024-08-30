using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 몬스터의 행동에 대한 행동머신
/// </summary>
public class MonsterStateMachine
{
    // 몬스터의 현재 상태
    public string curState { get; private set; }
    // 몬스터의 상태를 관리할 딕셔너리(string, state)
    private Dictionary<string, BaseState> stateDic;
    // 전이 조건에 대한 구조체를 들고 있을 리스트
    private List<Transition> anyStateTransition;

    /// <summary>
    /// 기본 생성자
    /// 몬스터의 행동은 딕셔너리(string, baseState)로 관리
    /// 전이(Transition) 구조체를 관리
    /// </summary>
    public MonsterStateMachine()
    {
        stateDic = new Dictionary<string, BaseState>();
        anyStateTransition = new List<Transition>();
    }

    /// <summary>
    /// 몬스터의 행동머신에 상태 추가
    /// </summary>
    /// <param name="key">행동 문자열</param>
    /// <param name="value">상태 클래스(BaseState)</param>
    public void AddState(string key, BaseState value)
    {
        stateDic.Add(key, value);
    }

    /// <summary>
    /// 전이 조건을 추가(특정 상태에서 바로 전이)
    /// </summary>
    /// <param name="state">상태</param>
    /// <param name="condition">조건(결과물이 bool값인 메소드)</param>
    public void AddAnyStateTransition(string state, Func<bool> condition)
    {
        anyStateTransition.Add(new Transition(state, condition));
    }

    /// <summary>
    /// 전이 조건을 추가
    /// </summary>
    /// <param name="start">현재 전이 상태</param>
    /// <param name="end">전이 목표 상태</param>
    /// <param name="condition">조건(결과물이 bool값인 메소드)</param>
    public void AddTransition(string start, string end, Func<bool> condition)
    {
        stateDic[start].transitions.Add(new Transition(end, condition));
    }

    /// <summary>
    /// 행동 머신 초기화 
    /// </summary>
    /// <param name="entry">전이가 들어갈 문자열</param>
    public void Init(string entry)
    {
        curState = entry;
        stateDic[entry].Enter();
    }

    /// <summary>
    /// 행동 머신의 Update
    /// 기본 Update를 수행하고 전이 조건을 검사
    /// 1. 특정 상황에서의 바로 전이(any)
    /// 2. 모든 전이 상황을 검사
    /// </summary>
    public void Update()
    {
        stateDic[curState].Update();

        foreach(var transition in anyStateTransition)
        {
            if(transition.condition())
            {
                ChangeState(transition.end);
                return;
            }
        }

        foreach(var transition in stateDic[curState].transitions)
        {
            if(transition.condition())
            {
                ChangeState(transition.end);
                return;
            }
        }
    }

    /// <summary>
    /// 행동머신의 LateUpdate를 수행
    /// </summary>
    public void LateUpdate()
    {
        stateDic[curState].LateUpdate();
    }

    /// <summary>
    /// 행동머신의 FixedUpdate를 수행
    /// </summary>
    public void FixedUpdate()
    {
        stateDic[curState].FixedUpdate();
    }

    /// <summary>
    /// 상태 변환 메소드
    /// </summary>
    /// <param name="nextState">대상으로 할 상태</param>
    public void ChangeState(string nextState)
    {
        stateDic[curState].Exit();
        curState = nextState;
        stateDic[curState].Enter();
    }
}

/// <summary>
/// 전이에 대한 정보를 포함할 구조체
/// </summary>
public struct Transition
{
    public string end;              // 전이 목표 대상
    public Func<bool> condition;    // 전이 조건의 함수(반환형은 bool타입)

    /// <summary>
    /// 생성자
    /// </summary>
    /// <param name="end">문자열</param>
    /// <param name="condition">반환형은 bool타입 메소드</param>
    public Transition(string end, Func<bool> condition)
    {
        this.end = end;
        this.condition = condition;
    }
}