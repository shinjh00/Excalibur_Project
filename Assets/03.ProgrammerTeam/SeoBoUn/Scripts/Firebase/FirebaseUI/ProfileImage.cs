using Firebase.Database;
using Firebase.Extensions;
using Photon.Pun;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 서보운
/// <br/> 플레이어의 프로필 이미지
/// </summary>
public class ProfileImage : MonoBehaviour
{
    [Tooltip("유저 이미지(에디터 할당 필요)")]
    [SerializeField] Image imageID;
    [Tooltip("유저 닉네임(에디터 할당 필요)")]
    [SerializeField] TMP_InputField nickname;
    [Tooltip("유저 레벨(에디터 할당 필요)")]
    [SerializeField] TMP_Text levelText;
    [Tooltip("유저 탈출횟수(에디터 할당 필요)")]
    [SerializeField] TMP_Text exitCountText;
    [Tooltip("유저 지닌골드(에디터 할당 필요)")]
    [SerializeField] TMP_Text havingGoldText;
    [Tooltip("유저 플레이횟수(에디터 할당 필요)")]
    [SerializeField] TMP_Text killCountText;
    [Tooltip("유저 승리횟수(에디터 할당 필요)")]
    [SerializeField] TMP_Text deathCountText;
    [Tooltip("유저 소개설명글(에디터 할당 필요)")]
    [SerializeField] TMP_InputField introduceText;

    [Tooltip("뒤로가기 버튼(에디터 할당 필요)")]
    [SerializeField] Button backButton;
    [Tooltip("닉네임 변경버튼(에디터 할당 필요)")]
    [SerializeField] Button changeNicknameButton;
    [Tooltip("자기소개 변경버튼(에디터 할당 필요)")]
    [SerializeField] Button changeIntroductButton;

    Coroutine nicknameChangeRoutine;
    Coroutine introduceChangeRoutine;

    GameObject mainPanelBlockImage;

    private void Awake()
    {
        nickname.interactable = false;
        introduceText.interactable = false;
        backButton.onClick.AddListener(BackButton);
        changeNicknameButton.onClick.AddListener(ChangeNickname);
        changeIntroductButton.onClick.AddListener(ChangeIntroduce);

        mainPanelBlockImage = GetComponentInParent<MainPanel>().MainPanelBlockImage;
    }

    private void BackButton()
    {
        gameObject.SetActive(false);
        mainPanelBlockImage.SetActive(false);
    }

    /// <summary>
    /// 파이어 베이스에 프로필 설정 메소드
    /// <br/> imageID, nickname, level, winningGames, havingGold, killCount, deathCount, introduce
    /// </summary>
    public void SetProfile()
    {
        GetData();
    }

    /// <summary>
    /// 파이어 베이스에 요청하여 유저 데이터 로딩
    /// </summary>
    private void GetData()
    {
        FirebaseManager.DB
            .GetReference("PlayerDataTable")
            .Child(FirebaseManager.Instance.curPlayer)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    return;
                }
                else if (task.IsFaulted)
                {
                    return;
                }

                DataSnapshot snapShot = task.Result;

                if (snapShot.Exists)
                {
                    // DataSnapshot temp = snapShot.Child("setting")
                    // IDictionary infodata = (IDictionary)temp.Value;
                    // infodata["sound"]

                    IDictionary nicknameData = (IDictionary)snapShot.Value;
                    nickname.text = $"{(string)nicknameData["nickname"]}";
                    introduceText.text = (string)nicknameData["introduce"];
                    levelText.text = $"LV. {nicknameData["level"]}";
                    exitCountText.text = $"{nicknameData["winningGames"]}";
                    deathCountText.text = $"{nicknameData["winningGames"]}"; // DeathCount 데이터 가져오기
                    havingGoldText.text = $"{nicknameData["havingGold"]}";
                    killCountText.text = $"{nicknameData["playedSession"]}";  // KillCount 데이터 가져오기
                    // level = int.Parse(playSessionText.text); -> 이유는 모르겠으나 이렇게 이용하여야 값이 읽힘
                    // level = Convert.ToInt32(nicknameData["playedSession"]); // -> firebase에는 64비트래요. 32비트로 변환
                    // imageID.sprite = Resources.Load<Sprite>(CsvParser.Instance.returnPath((int)nicknameData["introduce"]));
                }
            });
    }

    private void ChangeNickname()
    {
        nickname.interactable = true;
        nickname.ActivateInputField();

        nicknameChangeRoutine = StartCoroutine(ChangeNickNameRoutine());
    }

    IEnumerator ChangeNickNameRoutine()
    {   // 엔터를 누르거나 아무 곳이나 클릭하면 설정 종료
        yield return new WaitUntil(() => (Input.GetKeyDown(KeyCode.Return) || (Input.GetMouseButtonDown(0))));
        nickname.interactable = false;

        FirebaseManager.DB
            .GetReference("PlayerDataTable")
            .Child(FirebaseManager.Instance.curPlayer)
            .Child("nickname")
            .SetValueAsync(nickname.text)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    return;
                }
                else if (task.IsFaulted)
                {
                    return;
                }
            });
        Debug.Log($"MyNickName : {nickname.text}");
        PhotonNetwork.LocalPlayer.NickName = nickname.text;
    }

    private void ChangeIntroduce()
    {
        introduceText.interactable = true;
        introduceText.ActivateInputField();

        introduceChangeRoutine = StartCoroutine(ChangeIntroduceRoutine());
    }

    IEnumerator ChangeIntroduceRoutine()
    {   // 엔터를 누르거나 아무 곳이나 클릭하면 설정 종료
        yield return new WaitUntil(() => (Input.GetKeyDown(KeyCode.Return) || (Input.GetMouseButtonDown(0))));
        introduceText.interactable = false;

        FirebaseManager.DB
            .GetReference("PlayerDataTable")
            .Child(FirebaseManager.Instance.curPlayer)
            .Child("introduce")
            .SetValueAsync(introduceText.text)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    return;
                }
                else if (task.IsFaulted)
                {
                    return;
                }
            });
    }
}
