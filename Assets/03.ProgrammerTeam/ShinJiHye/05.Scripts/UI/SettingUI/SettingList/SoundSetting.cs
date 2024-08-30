using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 신지혜
/// 옵션 > 사운드 설정 관련 스크립트
/// </summary>
public class SoundSetting : MonoBehaviour
{
    [Header("각 Slider와 MuteButton(Button, _normal, _pressed) 할당")]

    [SerializeField] Slider masterVol_slider;
    [SerializeField] Button masterVol_muteButton;
    [SerializeField] GameObject masterVol_muteButton_normal;
    [SerializeField] GameObject masterVol_muteButton_pressed;

    [SerializeField] Slider bgmVol_slider;
    [SerializeField] Button bgmVol_muteButton;
    [SerializeField] GameObject bgmVol_muteButton_normal;
    [SerializeField] GameObject bgmVol_muteButton_pressed;

    [SerializeField] Slider sfxVol_slider;
    [SerializeField] Button sfxVol_muteButton;
    [SerializeField] GameObject sfxVol_muteButton_normal;
    [SerializeField] GameObject sfxVol_muteButton_pressed;

    bool isMuteMaster;
    bool isMuteBGM;
    bool isMuteSFX;

    private void Start()
    {
        SetAddListener();
        SetValues();
        SetMuteMasterButton();
        SetMuteBGMButton();
        SetMuteSFXButton();
    }

    /// <summary>
    /// Slider, Button에 AddListener 할당
    /// </summary>
    private void SetAddListener()
    {
        masterVol_slider.onValueChanged.AddListener(SetMasterVol);
        bgmVol_slider.onValueChanged.AddListener(SetBGMVol);
        sfxVol_slider.onValueChanged.AddListener(SetSFXVol);

        masterVol_muteButton.onClick.AddListener(MuteMasterVol);
        bgmVol_muteButton.onClick.AddListener(MuteBGMVol);
        sfxVol_muteButton.onClick.AddListener(MuteSFXVol);
    }

    /// <summary>
    /// 설정창 -> 사운드탭 열었을 때 기존값 유지해서 세팅
    /// </summary>
    private void SetValues()
    {
        masterVol_slider.value = Mathf.Pow(10, (SoundManager.instance.curMasterVol / 20));
        bgmVol_slider.value = Mathf.Pow(10, (SoundManager.instance.curBGMVol / 20));
        sfxVol_slider.value = Mathf.Pow(10, (SoundManager.instance.curSFXVol / 20));

        isMuteMaster = SoundManager.instance.isMuteMaster;
        isMuteBGM = SoundManager.instance.isMuteBGM;
        isMuteSFX = SoundManager.instance.isMuteSFX;
    }

    /// <summary>
    /// 마스터 볼륨 음소거 버튼 모양
    /// </summary>
    private void SetMuteMasterButton()
    {
        masterVol_muteButton_normal.SetActive(!isMuteMaster);
        masterVol_muteButton_pressed.SetActive(isMuteMaster);
    }

    /// <summary>
    /// 배경음악 음소거 버튼 모양
    /// </summary>
    private void SetMuteBGMButton()
    {
        bgmVol_muteButton_normal.SetActive(!isMuteBGM);
        bgmVol_muteButton_pressed.SetActive(isMuteBGM);
    }

    /// <summary>
    /// 효과음 음소거 버튼 모양
    /// </summary>
    private void SetMuteSFXButton()
    {
        sfxVol_muteButton_normal.SetActive(!isMuteSFX);
        sfxVol_muteButton_pressed.SetActive(isMuteSFX);
    }


    #region SetVol
    /// <summary>
    /// 마스터 볼륨 조절 메소드
    /// </summary>
    /// <param name="value"></param>
    private void SetMasterVol(float value)
    {
        SoundManager.instance.SetVolume(SoundType.MasterVol, value);
    }

    /// <summary>
    /// 배경음악 볼륨 조절 메소드
    /// </summary>
    /// <param name="value"></param>
    private void SetBGMVol(float value)
    {
        SoundManager.instance.SetVolume(SoundType.BGMVol, value);
        SoundManager.instance.saveUserData.wholeSound = (int)value;
    }

    /// <summary>
    /// 효과음 볼륨 조절 메소드
    /// </summary>
    /// <param name="value"></param>
    private void SetSFXVol(float value)
    {
        SoundManager.instance.SetVolume(SoundType.SFXVol, value);
        SoundManager.instance.saveUserData.effectSound = (int)value;
    }
    #endregion


    #region MuteVol
    /// <summary>
    /// 마스터 볼륨 음소거 메소드
    /// </summary>
    private void MuteMasterVol()
    {
        if (!isMuteMaster)  // isMute : false -> true
        {
            SoundManager.instance.prevMasterVol = SoundManager.instance.curMasterVol;
            masterVol_slider.value = masterVol_slider.minValue;
        }
        else  // isMute : true -> false
        {
            masterVol_slider.value = Mathf.Pow(10, (SoundManager.instance.prevMasterVol / 20));
        }
        SoundManager.instance.MuteVolume(SoundType.MasterVol, isMuteMaster);
        isMuteMaster = !isMuteMaster;
        SoundManager.instance.isMuteMaster = isMuteMaster;
        SetMuteMasterButton();
    }

    /// <summary>
    /// 배경음악 음소거 메소드
    /// </summary>
    private void MuteBGMVol()
    {
        if (!isMuteBGM)
        {
            SoundManager.instance.prevBGMVol = SoundManager.instance.curBGMVol;
            bgmVol_slider.value = bgmVol_slider.minValue;
        }
        else
        {
            bgmVol_slider.value = Mathf.Pow(10, (SoundManager.instance.prevBGMVol / 20));
        }
        SoundManager.instance.MuteVolume(SoundType.BGMVol, isMuteBGM);
        isMuteBGM = !isMuteBGM;
        SoundManager.instance.isMuteBGM = isMuteBGM;
        SoundManager.instance.saveUserData.wholeSoundOff = isMuteBGM ? 1 : 0;
        SetMuteBGMButton();
    }

    /// <summary>
    /// 효과음 음소거 메소드
    /// </summary>
    private void MuteSFXVol()
    {
        if (!isMuteSFX)
        {
            SoundManager.instance.prevSFXVol = SoundManager.instance.curSFXVol;
            sfxVol_slider.value = sfxVol_slider.minValue;
        }
        else
        {
            sfxVol_slider.value = Mathf.Pow(10, (SoundManager.instance.prevSFXVol / 20));
        }
        SoundManager.instance.MuteVolume(SoundType.SFXVol, isMuteSFX);
        isMuteSFX = !isMuteSFX;
        SoundManager.instance.isMuteSFX = isMuteSFX;
        SoundManager.instance.saveUserData.effectSoundOff = isMuteSFX ? 1 : 0;
        SetMuteSFXButton();
    }
    #endregion
}
