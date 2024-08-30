using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageFloater : MonoBehaviour
{
    [SerializeField] GameObject systemMessagePrefab;
    [SerializeField] Transform messageTransform;
    [SerializeField] ScrollRect systemRect;
    Coroutine messageDownRoutine;


    public void ShowMessage(string message)
    {
        if (this == null || !gameObject.activeInHierarchy)
        {
            Debug.LogWarning("GameManager is destroyed or inactive, ignoring ShowMessage RPC.");
            return;
        }
        GameObject instance = Instantiate(systemMessagePrefab, messageTransform);
        MessageResizer resizer = instance.GetComponent<MessageResizer>();
        resizer.targetText.text = message;
        CanvasGroup canvasGroup = instance.GetComponent<CanvasGroup>();

        /* if (messageDownRoutine != null)
           {
               StopCoroutine(messageDownRoutine);
           }
           messageDownRoutine = StartCoroutine(MessageDown());*/
        systemRect.verticalScrollbar.value = 0f;
        StartCoroutine(MessageFading(canvasGroup,0.5f));


     /*   IEnumerator MessageDown()
        {   
            float d = 0.5f;
            float s = systemRect.verticalScrollbar.value;
            float targetValue = 0f;
            for (float t = 0; t < d; t += Time.deltaTime)
            {
                systemRect.verticalScrollbar.value = Mathf.Lerp(s, targetValue, t / d);
                yield return null;
            }
        }*/
        IEnumerator MessageFading(CanvasGroup canvas, float d)
        {
            
            for (float t = 0; t < d; t += Time.deltaTime)
            {
                systemRect.verticalScrollbar.value = 0f;
                canvas.alpha = t / d;
                yield return null;
            }
            canvas.alpha = 1f;


            yield return new WaitForSeconds(3f);

            for (float t = 0; t < d; t += Time.deltaTime)
            {
                systemRect.verticalScrollbar.value = 0f;
                canvas.alpha = 1 - t / d;
                yield return null;
            }
            canvas.alpha = 0f;


            Destroy(canvas.gameObject);
            systemRect.verticalScrollbar.value = 0f;
        }
    }

    /// <summary>
    /// 엑스칼리버를 주웠을 때 실행할 메소드(엑스칼리버 경고)
    /// </summary>
    public void ShowMessageExcalibur()
    {
        if (this == null || !gameObject.activeInHierarchy)
        {
            Debug.LogWarning("GameManager is destroyed or inactive, ignoring ShowMessage RPC.");
            return;
        }
        GameObject instance = Instantiate(systemMessagePrefab, messageTransform);
        MessageResizer resizer = instance.GetComponent<MessageResizer>();
        resizer.targetText.text = $"누군가가 엑스칼리버를 주웠습니다.";
        CanvasGroup canvasGroup = instance.GetComponent<CanvasGroup>();

        systemRect.verticalScrollbar.value = 0f;
        StartCoroutine(MessageFading(canvasGroup, 1f));

        IEnumerator MessageFading(CanvasGroup canvas, float d)
        {

            for (float t = 0; t < d; t += Time.deltaTime)
            {
                systemRect.verticalScrollbar.value = 0f;
                canvas.alpha = t / d;
                yield return null;
            }
            canvas.alpha = 1f;


            yield return new WaitForSeconds(5f);

            for (float t = 0; t < d; t += Time.deltaTime)
            {
                systemRect.verticalScrollbar.value = 0f;
                canvas.alpha = 1 - t / d;
                yield return null;
            }
            canvas.alpha = 0f;


            Destroy(canvas.gameObject);
            systemRect.verticalScrollbar.value = 0f;
        }
    }
}
