using System;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필 / 범위 스킬 사용 전에 범위를 표기해주는 클래스
/// </summary>
public class RangeChecker : MonoBehaviour
{
    /// <summary>
    /// 스킬을 시전을 할 수 있는 범위체커를 움직일 수 있는 최대거리
    /// </summary>
    float inRangeScale;
    /// <summary>
    /// 좌클릭시 나올 스킬을 담는 액션
    /// </summary>
    Action<object> skillExcute;
    /// <summary>
    /// 우클릭시 레인지체커를 취소시키는 로직을 발동할 액션
    /// </summary>
    Action skillCancle;
    /// <summary>
    /// 마우스를 따라 다닐 레인지표시
    /// </summary>
    [SerializeField] Transform selectRange;
    /// <summary>
    /// 레인지표시가 움직일 수 있는 범위체커
    /// </summary>
    [SerializeField] Transform inRange;
    [SerializeField] Transform aoe;
    [SerializeField] Transform arrow;

    public PlayerController owner;
    public SkillData skill;
    Vector3 mousePosition;
    Vector3 ownerPosition;
    Vector3 direction;
    public Transform SelectedRange { get { return selectRange; } }

    /// <summary>
    /// 범위 스킬이었을때 로직 초기화해주는 함수
    /// </summary>
    /// <param name="rangeType">범위 타입</param>
    /// <param name="rangeScale">범위 크기</param>
    /// <param name="inRange">레인지체커가 움직일수있는 반경</param>
    /// <param name="skill">구현 할 스킬</param>
    /// <param name="player">사용하는 플레이어</param>
    public void InitSkill(RangeType rangeType, Vector3 rangeScale, float inRange, SkillData skill, PlayerController player)
    {
        switch (rangeType)
        {

            case RangeType.AoE:
                selectRange = aoe;
                selectRange.localScale = rangeScale;
                break;
            case RangeType.NonTarget:
                selectRange = arrow;
                selectRange.localScale = rangeScale;
                break;

        }
        owner = player;
        this.skill = skill;
        selectRange.gameObject.SetActive(true);
        this.inRange.localScale = new Vector3(inRange * 2, inRange * 2);
        inRangeScale = inRange;

        skillCancle += Cancle;
    }
    private void FixedUpdate()
    {
        if(owner.StateController.CurState.Contain(PlayerState.Silence))
        {
            skillCancle?.Invoke();
        }
        else
        {
            FollowTarget();
        }

    }

    void FollowTarget()
    {
         mousePosition = GameManager.Ins.MousePos;

        ownerPosition = owner.transform.position;

        direction = mousePosition - ownerPosition;

        float distance = direction.magnitude;
        inRange.position = ownerPosition;
        selectRange.transform.up = direction;
        if (distance > inRangeScale)
        {
            direction = direction.normalized * inRangeScale;
            selectRange.position = ownerPosition + direction;
        }
        else
        {
            selectRange.position = mousePosition;
        }
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            skillExcute?.Invoke(direction);
            skillCancle?.Invoke();
        }
        else if (Input.GetMouseButton(1))
        {
            skillCancle?.Invoke();
        }
    }
    /// <summary>
    /// 캔슬됐을때 나올 메서드
    /// </summary>
    public void Cancle()
    {
        owner.AttackController.enabled = true;
        skill.Casting = false;
        if (gameObject != null)
            Destroy(gameObject);
    }
    /// <summary>
    /// 레인지체커로 인한 좌클릭시 사용할 스킬 참조
    /// </summary>
    /// <param name="action"></param>
    public void AddSkill(Action<object> action)
    {
        skillExcute += action;
    }


}
public enum RangeType { NULL, AoE, NonTarget }
