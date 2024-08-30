using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 신지혜
/// 게임 내 사운드 할당 스크립트
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Tooltip("SoundManager 할당")]
    [SerializeField] AudioSource bgmPlayer;

    [Tooltip("Config > AudioMixer 할당")]
    public AudioMixer mixer;

    [Header("Init~Vol에만 초기값 할당 (-40 ~ 0)")]
    public float initMasterVol;
    public float initBGMVol;
    public float initSFXVol;

    public float curMasterVol;
    public float curBGMVol;
    public float curSFXVol;

    public float prevMasterVol;
    public float prevBGMVol;
    public float prevSFXVol;

    float prevVol;

    public bool isMuteMaster;
    public bool isMuteBGM;
    public bool isMuteSFX;

    // 추가 개발(서보운) 파이어베이스 세팅을 위한 데이터
    public SettingSaveData saveUserData;

    private void Awake()
    {
        instance = this;

        if (mixer == null)
        {
            mixer = Resources.Load<AudioMixer>($"5.Sound/AudioMixer");
        }
    }

    private void Start()
    {
        mixer.SetFloat("MasterVol", initMasterVol);
        mixer.SetFloat("BGMVol", initBGMVol);
        mixer.SetFloat("SFXVol", initSFXVol);

        curMasterVol = initMasterVol;
        curBGMVol = initBGMVol;
        curSFXVol = initSFXVol;

        isMuteMaster = false;
        isMuteBGM = false;
        isMuteSFX = false;

        saveUserData = new SettingSaveData();
    }

    /// <summary>
    /// ID에 맞는 사운드 클립 가져오는 메소드
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public AudioClip SetAudioClip(int id)
    {
        //int id = CsvParser.Instance.ResourceDic.FirstOrDefault(x => x.Value.name.Equals(name)).Key;
        AudioClip clip = Resources.Load<AudioClip>(CsvParser.Instance.returnPath(id));
        return clip;
    }

    /// <summary>
    /// 배경음악 재생
    /// </summary>
    /// <param name="id"></param>
    public void PlayBGM(int id)
    {
        //---게임 딜레이 시, 게임 시작 시, 씬 전환 시 아래와 같이 코드 추가
        //---SoundManager.instance.PlayBGM("Background_Ready");

        if (bgmPlayer != null)
        {
            // bgmPlayer.outputAudioMixerGroup = mixer.FindMatchingGroups("BGM").FirstOrDefault();
            bgmPlayer.clip = SetAudioClip(id);
            bgmPlayer.Play();
        }
        else
        {
            return;
        }
    }

    /// <summary>
    /// 효과음 재생
    /// </summary>
    /// <param name="id"></param>
    /// <param name="audioSource"> 사운드 넣을 오디오 소스 </param>
    public void PlaySFX(int id, AudioSource audioSource)
    {
        //---SoundManager.instance.PlaySFX("Attack", owner.audioSource);

        if (audioSource != null)
        {
            audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX").FirstOrDefault();
            AudioClip sfxClip = SetAudioClip(id);
            audioSource.clip = sfxClip;
            audioSource.PlayOneShot(sfxClip);
        }
        else
        {
            return;
        }
    }

    public Coroutine sfxLoopRoutine;

    /// <summary>
    /// 효과음 반복 재생
    /// </summary>
    /// <param name="id"></param>
    /// <param name="audioSource"></param>
    /// <param name="loopTime"> 몇 초마다 반복할 지 </param>
    public void PlaySFXLoop(int id, AudioSource audioSource, float loopTime)
    {
        //---SoundManager.instance.PlaySFX("Move", owner.audioSource, 0.5f);

        if (audioSource != null)
        {
            audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX").FirstOrDefault();
            AudioClip sfxClip = SetAudioClip(id);
            audioSource.clip = sfxClip;
            sfxLoopRoutine = StartCoroutine(SFXLoopRoutine(audioSource, sfxClip, loopTime));
        }
        else
        {
            return;
        }
    }

    public void StopSFXLoop()
    {
        StopCoroutine(sfxLoopRoutine);
    }

    IEnumerator SFXLoopRoutine(AudioSource audioSource, AudioClip sfxClip, float loopTime)
    {
        while (true)
        {
            audioSource.PlayOneShot(sfxClip);
            yield return new WaitForSeconds(loopTime);
        }
    }


    #region VolumeControl
    /// <summary>
    /// 볼륨 조절 메소드
    /// </summary>
    /// <param name="soundType"> 오디오 믹서 종류 </param>
    /// <param name="vol"> 볼륨 조절 슬라이더의 value값 </param>
    public void SetVolume(SoundType soundType, float vol)
    {
        mixer.SetFloat(soundType.ToString(), Mathf.Log10(vol) * 20);
        switch (soundType)
        {
            case SoundType.MasterVol:
                curMasterVol = Mathf.Log10(vol) * 20;
                break;
            case SoundType.BGMVol:
                curBGMVol = Mathf.Log10(vol) * 20;
                break;
            case SoundType.SFXVol:
                curSFXVol = Mathf.Log10(vol) * 20;
                break;
        }
    }

    /// <summary>
    /// 음소거 메소드
    /// </summary>
    /// <param name="soundType"> 오디오 믹서 종류 </param>
    public void MuteVolume(SoundType soundType, bool isMute)
    {
        if (!isMute)  // isMute : false -> true
        {
            mixer.SetFloat(soundType.ToString(), -80);
        }
        else  // isMute : true -> false
        {
            switch (soundType)
            {
                case SoundType.MasterVol:
                    prevVol = prevMasterVol;
                    break;
                case SoundType.BGMVol:
                    prevVol = prevBGMVol;
                    break;
                case SoundType.SFXVol:
                    prevVol = prevSFXVol;
                    break;
            }
            mixer.SetFloat(soundType.ToString(), prevVol);
        }
    }
}
#endregion

/// <summary>
/// 오디오 믹서 종류. 오디오 믹서의 파라미터 이름과 일치해야함
/// </summary>
public enum SoundType
{
    MasterVol, BGMVol, SFXVol
}
