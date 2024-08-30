using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatBubble : MonoBehaviour
{
    [SerializeField]TMP_Text text;
    [SerializeField] Image image;
    [SerializeField] float hidingTime = 5f;

    public void SetText(string s,Player player)
    {
        text.text = s;
        Color textColor = text.color;
        textColor.a = 1f; // 초기 알파값은 1로 설정
        text.color = textColor;

        Color imageColor = image.color;
        imageColor.a = 1f; // 초기 알파값은 1로 설정
        image.color = imageColor;

        if(player == PhotonNetwork.LocalPlayer)
        {
            image.color = Color.green;
        }

    }

    public IEnumerator FadeOutTextAndImage()
    {
        Debug.Log("StartFadeOut");
        float startAlpha = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < hidingTime)
        {
            if (text == null || image == null || !gameObject.activeInHierarchy)
            {
                yield break;
            }

            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / hidingTime);

            if (text != null)
            {
                Color textColor = text.color;
                textColor.a = alpha;
                text.color = textColor;
            }

            if (image != null)
            {
                Color imageColor = image.color;
                imageColor.a = alpha;
                image.color = imageColor;
            }

            yield return null; 
        }
        if (gameObject != null && gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
        }
    }

}
