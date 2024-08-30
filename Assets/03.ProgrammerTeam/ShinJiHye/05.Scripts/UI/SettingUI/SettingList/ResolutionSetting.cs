using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 신지혜 / 해상도 설정 스크립트
/// </summary>
public class ResolutionSetting : MonoBehaviour
{
    List<Resolution> selectedResolutions = new List<Resolution>();
    FullScreenMode screenMode;
    int refreshRate;

    [SerializeField] TMP_Dropdown resolDropdown;
    [SerializeField] int resolutionNum;

    [SerializeField] Toggle screenModeToggle;
    [SerializeField] bool isFullScreen;

    private void Start()
    {
        InitResolution();
        resolDropdown.onValueChanged.AddListener(DropboxOptionChange);
        screenModeToggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    /// <summary>
    /// 해상도 초기 설정
    /// </summary>
    private void InitResolution()
    {
        Resolution[] allResolutions = Screen.resolutions;  // 모든 해상도 가져오기

        // 대표적으로 사용하는 해상도 정의
        List<Vector2Int> commonResolutions = new List<Vector2Int>
        {
            new Vector2Int(1280, 720),
            new Vector2Int(1920, 1080),
            new Vector2Int(2560, 1440),
        };

        // 특정 해상도 필터링
        foreach (Resolution res in allResolutions)
        {
            foreach (Vector2Int commonRes in commonResolutions)
            {
                if (res.width == commonRes.x && res.height == commonRes.y)
                {
                    selectedResolutions.Add(res);
                    break;
                }
            }
        }

        resolDropdown.options.Clear();  // 기존 드롭다운 리스트 요소 모두 제거

        int optionNum = 0;

        foreach (Resolution res in selectedResolutions)
        {
            // 선택된 해상도의 주사율 구하기
            refreshRate = (int)(res.refreshRateRatio.numerator / res.refreshRateRatio.denominator);

            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
            option.text = $"{res.width} x {res.height} {refreshRate}hz";  // 드롭다운 목록에 넣을 text
            resolDropdown.options.Add(option);  // 드롭다운 옵션에 추가

            // 드롭다운 value 정수 순서대로 할당
            if (res.width == Screen.width && res.height == Screen.height)
            {
                resolDropdown.value = optionNum;
            }
            optionNum++;
        }

        resolDropdown.RefreshShownValue();  // 드롭다운 리스트가 변경되었으니 새로고침 해줌
    }

    /// <summary>
    /// 드롭다운 옵션 변경 시 실행 메소드
    /// </summary>
    /// <param name="x"> 드롭다운 value </param>
    public void DropboxOptionChange(int x)
    {
        resolutionNum = x;
        Screen.SetResolution(selectedResolutions[resolutionNum].width, selectedResolutions[resolutionNum].height, screenMode);
    }

    /// <summary>
    /// Toggle 값에 따라 화면 모드 변경 메소드
    /// </summary>
    /// <param name="isOn"></param>
    private void OnToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow; // 전체 화면 모드
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed; // 창 모드
        }
    }
}
