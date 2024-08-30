using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

/// <summary>
/// 개발자: 이예린/ Food item casting bar UI 구현
/// </summary>
public class FoodItemBar : MonoBehaviour
{
    [Tooltip("FoodItemBar_border 이미지 (에디터 할당 필요)")]
    [SerializeField] Image foodBarBorder;
    [Tooltip("FoodItemBar_gauge 이미지 (에디터 할당 필요)")]
    [SerializeField] Image foodBarGauge;
    [Tooltip("Current food item 이미지 (에디터 할당 필요)")]
    [SerializeField] Image currentFoodItem;

    Coroutine foodItemBarRoutine;
    Vector3 currentVec;

    /// <summary>
    /// Food item casting bar 세팅 및 routine 시작하는 함수
    /// </summary>
    /// <param name="casting">아이템 casting 사간</param>
    /// <param name="itemImage">아이템 이미지</param>
    public void StartFoodItmeRoutine(int casting, Sprite itemImage)
    {
        currentVec = new Vector3(0f, 1f);

        foodBarGauge.rectTransform.localScale = currentVec;
        currentFoodItem.sprite = itemImage;

        foodBarBorder.gameObject.SetActive(true);
        foodBarGauge.gameObject.SetActive(true);
        currentFoodItem.gameObject.SetActive(true);

        foodItemBarRoutine = StartCoroutine(FoodItmeRoutine(casting));
    }

    /// <summary>
    /// Food item casting bar routine 종료 및 UI 비활성화 하는 함수
    /// </summary>
    public void StopFoodItmeRoutine()
    {
        StopCoroutine(foodItemBarRoutine);

        foodBarBorder.gameObject.SetActive(false);
        foodBarGauge.gameObject.SetActive(false);
        currentFoodItem.gameObject.SetActive(false);
    }

    /// <summary>
    /// Food item casting bar routine
    /// </summary>
    /// <param name="casting">아이템 casting 사간</param>
    /// <returns></returns>
    private IEnumerator FoodItmeRoutine(int casting)
    {
        float timeSinceFoodUse = 0;

        if (casting == 0)
        {
            yield return null;
        }
        else
        {
            float incrementPerSecond = 1f / (float)casting;

            while (timeSinceFoodUse < 1f)
            {
                timeSinceFoodUse += incrementPerSecond;
                currentVec.x = timeSinceFoodUse;
                foodBarGauge.rectTransform.localScale = currentVec;
                yield return new WaitForSeconds(1f);
            }
        }

        foodBarBorder.gameObject.SetActive(false);
        foodBarGauge.gameObject.SetActive(false);
        currentFoodItem.gameObject.SetActive(false);
    }
}