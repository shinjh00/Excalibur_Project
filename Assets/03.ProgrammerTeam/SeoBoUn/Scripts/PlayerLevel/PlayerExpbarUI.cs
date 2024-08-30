using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 서보운
/// <br/> 캐릭터의 경험치 바UI에 대한 스크립트
/// </summary>
public class PlayerExpbarUI : MonoBehaviour
{
    [SerializeField] Image expbarImage;

    public Action ChangeMaxExp;

    int nextExp;
    Vector3 targetVec;

    /// <summary>
    /// 초기화 메소드
    /// </summary>
    public void Init(int nextExp)
    {
        targetVec = Vector3.up + Vector3.forward;

        expbarImage.rectTransform.localScale = targetVec;

        this.nextExp = nextExp;
    }

    /// <summary>
    /// 최대 경험치 설정 메소드
    /// </summary>
    /// <param name="nextExp"></param>
    public void SetNextExp(int nextExp)
    {
        this.nextExp = nextExp;
    }

    /// <summary>
    /// Exp바를 움직일 코루틴
    /// </summary>
    /// <param name="curExp"></param>
    /// <param name="targetExp"></param>
    /// <returns></returns>
    public IEnumerator SetExp(int curExp, int targetExp)
    {
        // curExp : 현재 경험치, targetExp : 목표로 하는 경험치량
        float rate = 0f;

        if (targetExp < nextExp)
        {   // 레벨업을 하지 않아도 괜찮은 경우
            while (rate < 1f)
            {
                rate += Time.deltaTime;

                if (rate > 1f)
                {
                    rate = 1f;
                }

                targetVec.x = Mathf.Lerp(curExp, targetExp, rate) / (float)nextExp;

                expbarImage.rectTransform.localScale = targetVec;

                yield return null;
            }
        }
        else
        {   // 그렇지 않은 경우.(레벨업을 하고 추가로 경험치를 얻는 경우)

            // 1. 먼저 레벨업을 할 때 까지 끝까지 경험치 바를 채우기
            while(rate < 1f)
            {
                rate += Time.deltaTime;

                if(rate > 1f)
                {
                    rate = 1f;
                }

                targetVec.x = Mathf.Lerp(curExp, nextExp, rate) / (float)nextExp;

                expbarImage.rectTransform.localScale = targetVec;

                yield return null;
            }

            // 2. 경험치 바를 다시 0으로 바꾸기
            targetVec.x = 0f;
            expbarImage.rectTransform.localScale = targetVec;

            // 3. 남은 경험치를 채우기
            int remainExp = targetExp - nextExp;
            ChangeMaxExp?.Invoke();
            rate = 0f;
            
            while (rate < 1f)
            {
                rate += Time.deltaTime;

                if(rate > 1f)
                {
                    rate = 1f;
                }

                targetVec.x = Mathf.Lerp(0f, remainExp, rate) / (float)nextExp;

                expbarImage.rectTransform.localScale = targetVec;

                yield return null;
            }
        }
    }
}
