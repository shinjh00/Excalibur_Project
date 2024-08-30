using UnityEngine;

/// <summary>
/// 개발자 : 신지혜
/// 메인 카메라 뷰포트내에 있는 오브젝트만 사운드 재생
/// 사운드를 들을 오브젝트에 해당 스크립트 컴포넌트로 달아줘야 함 (직접 플레이하게 될 캐릭터들만)
/// </summary>
public class AudioWithinViewport : MonoBehaviour
{
    [Tooltip("해당 오브젝트의 Audio Source 할당")]
    [SerializeField] AudioSource sfxAudioSource;
    Camera mainCamera;
    bool isPlay;

    private void Start()
    {
        mainCamera = Camera.main;
        sfxAudioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // 오브젝트의 월드 좌표를 뷰포트 좌표로 변환AudioWithinViewportAudioWithinViewport
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(transform.position);

        // 오브젝트가 화면 안에 있는지 확인
        bool isOnScreen = viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1;

        if (isOnScreen != isPlay)
        {
            if (isOnScreen)
            {
                sfxAudioSource.volume = 1.0f;
                //sfxAudioSource.PlayOneShot(sfxAudioSource.clip, 1.0f);
            }
            else
            {
                sfxAudioSource.volume = 0f;
                //sfxAudioSource.Pause();
            }
            isPlay = isOnScreen;
        }
    }
}
