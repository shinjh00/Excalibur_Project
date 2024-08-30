using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingImage : MonoBehaviour
{
    [SerializeField] Button backButton;

    GameObject mainPanelBlockImage;

    private void Awake()
    {
        backButton.onClick.AddListener(BackButton);

        mainPanelBlockImage = GetComponentInParent<MainPanel>().MainPanelBlockImage;
    }

    /// <summary>
    /// 뒤로가기 버튼(아웃게임에서 사용)
    /// </summary>
    private void BackButton()
    {
        gameObject.SetActive(false);
        mainPanelBlockImage.SetActive(false);

        // 뒤로가기 버튼을 누를 때 해당 정보는 저장되어 파이어 베이스로 올라감.

        /*
        FirebaseManager.DB
            .GetReference("PlayerDataTable")
            .Child(FirebaseManager.Instance.curPlayer)
            .Child("settingData")
            .SetValueAsync(SoundManager.instance.saveUserData)
            .ContinueWithOnMainThread(task =>
            {
                if(task.IsCanceled)
                {
                    return;
                }
                else if(task.IsFaulted)
                {
                    return;
                }
            });
        */
    }
}
