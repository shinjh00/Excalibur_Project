using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 신지혜 / HPBarUI 관련 클래스
/// </summary>
public class HPBarUI : BaseUI
{
    [Tooltip("HPBar_border 이미지 (에디터 할당 필요)")]
    [SerializeField] Image hpBarBorder;
    [Tooltip("HPBar_gauge 이미지 (에디터 할당 필요)")]
    [SerializeField] Image hpBarGauge;
    [Tooltip("HPBar_Transparent 반투명 이미지 (에디터 할당 필요")]
    [SerializeField] Image hpBarTransparent;
    [Tooltip("Player (에디터 할당 필요)")]
    [SerializeField] PlayerHealthController healthController;


    public Image HPBarGauge { get { return  hpBarGauge; } set {  hpBarGauge = value; } }
    float maxHp;
    Vector3 targetVec;

    /// <summary>
    /// 기본 초기화 함수(HpUI 설정)
    /// </summary>
    /// <param name="maxHp"></param>
    public void Init(float maxHp)
    {
        this.maxHp = maxHp;
        hpBarGauge.rectTransform.localScale = Vector3.one;
        hpBarTransparent.rectTransform.localScale = Vector3.one;
        targetVec = Vector3.one;
    }

    /// <summary>
    /// Hp 루틴(현재 체력에서 목표 체력까지 변하도록 설정.)
    /// </summary>
    /// <param name="curHp"></param>
    /// <param name="targetHp"></param>
    /// <returns></returns>
    public IEnumerator HpRoutine(float curHp, float targetHp)
    {
        if (maxHp == 0 && targetHp != 0)
        {
            maxHp = curHp;
        }

        float rate = 0f;
        if (targetHp < 0)
        {
            targetHp = 0f;
        }
        else if(targetHp >= maxHp)
        {   // 회복하는 상황에서 너무 많이 회복되는 경우 방지
            targetHp = maxHp;
        }

        if (curHp == targetHp)
        {   // 현재 체력과 목표치가 같으면 수행하지 않아도 됨.
            yield return null;
        }
        else
        {   
            hpBarGauge.rectTransform.localScale = new Vector3(targetHp / maxHp, 1f, 1f);

            while (rate < 1f)
            {
                rate += 0.1f;
                targetVec.x = Mathf.Lerp(curHp, targetHp, rate) / maxHp;

                hpBarTransparent.rectTransform.localScale = targetVec;
                yield return new WaitForSeconds(0.1f);
            }
        }
       
    }

    /// <summary>
    /// 개발자 : 서보운
    /// <br/> 최대 HP가 바뀌었을 때 반응할 메소드
    /// </summary>
    /// <param name="maxHp"></param>
    public void ChangeMaxHp(float maxHp)
    {
        this.maxHp = maxHp;
    }
     
    public void SetImage(Material mat)
    {
        hpBarBorder.material = mat;
        hpBarGauge.material = mat;
        hpBarTransparent.material = mat;
    }

}
