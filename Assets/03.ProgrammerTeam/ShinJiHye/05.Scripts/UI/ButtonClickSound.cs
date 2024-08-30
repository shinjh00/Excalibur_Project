using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 버튼 클릭 사운드 스크립트
/// </summary>
public class ButtonClickSound : MonoBehaviour
{
    [SerializeField] AudioSource uiAudioSource;
    [SerializeField] Button[] buttons;

    /// <summary>
    /// AudioSource 세팅 및 버튼 클릭 시 사운드 재생
    /// </summary>
    private void Start()
    {
        uiAudioSource = GetComponent<AudioSource>();
        buttons = GetComponentsInChildren<Button>();

        uiAudioSource.outputAudioMixerGroup = SoundManager.instance.mixer.FindMatchingGroups("SFX").FirstOrDefault();
        uiAudioSource.playOnAwake = false;
        uiAudioSource.loop = false;

        foreach (Button button in buttons)
        {
            button.onClick.AddListener(() => SoundManager.instance.PlaySFX(1650069, uiAudioSource));
        }
    }
}
