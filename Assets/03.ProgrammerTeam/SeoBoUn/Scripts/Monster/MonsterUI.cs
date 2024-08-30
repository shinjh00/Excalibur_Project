using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 서보운
/// <br/> 몬스터의 UI(HpBar...)
/// </summary>
public class MonsterUI : MonoBehaviour
{
    [Tooltip("HpBar 이미지(에디터 할당 필요)")]
    [SerializeField] Image hpBarImage;
    [Tooltip("HpBar 반투명 이미지(에디터 할당 필요")]
    [SerializeField] Image hpBarImage_Transparent;

    float maxHp;

    Vector3 targetVec;

    /// <summary>
    /// 기본 초기화 함수(HpUI 설정)
    /// </summary>
    /// <param name="maxHp"></param>
    public void Init(float maxHp)
    {
        this.maxHp = maxHp;

        hpBarImage.rectTransform.localScale = Vector3.one;
        hpBarImage_Transparent.rectTransform.localScale = Vector3.one;
        targetVec = Vector3.one;
    }

    /// <summary>
    /// Hp 루틴(현재 체력에서 목표 체력까지 변하도록 설정)
    /// </summary>
    /// <param name="curHp"></param>
    /// <param name="targetHp"></param>
    /// <returns></returns>
    public IEnumerator HpRoutine(float curHp, float targetHp)
    {
        if(maxHp == 0 && targetHp != 0)
        {
            maxHp = curHp;
        }

        float rate = 0f;
        if (targetHp < 0)
        {
            targetHp = 0f;
        }
        if (curHp == targetHp)
        {
            yield return null;
        }
        else
        {
            hpBarImage.rectTransform.localScale = new Vector3(targetHp / maxHp, 1f, 1f);

            while (rate < 1f)
            {
                rate += 0.1f;
                targetVec.x = Mathf.Lerp(curHp, targetHp, rate) / maxHp;

                hpBarImage_Transparent.rectTransform.localScale = targetVec;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
