using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 서보운
/// <br/> 엑스칼리버 탈출 포인트를 위한 스크립트
/// </summary>
public class ExcaliburExitPoint : MonoBehaviourPun, IInteractable
{
    [Tooltip("탈출구 애니메이션(에디터 할당 필요)")]
    [SerializeField] Animator animator;
    [Tooltip("상호작용 키 이미지(에디터 할당 필요)")]
    [SerializeField] SpriteRenderer keyImageRender;
    [Tooltip("탈출 게이지(에디터 할당 필요")]
    [SerializeField] Image exitGauge;
    [SerializeField] Image backGround;

    // 1. 플레이어가 스폰된 위치로는 탈출구가 될 수 없음..
    [SerializeField] PlayerController spawnPlayer;

    [Tooltip("탈출에 필요한 시간")]
    [SerializeField] float exitTime;
    [Tooltip("탈출이 가능한지 검사")]
    [SerializeField] bool isExit;

    Collider2D[] colliders;
    PlayerController curPlayer;
    LayerMask playerLayer;

    Coroutine showImageRoutine;
    Coroutine exitRoutine;

    float showDistance = 1f;
    float gaugeTime;    // 탈출을 위해 누르고 있던 시간
    bool isShow;

    public bool IsExit { get { return isExit; } }

    private void Start()
    {
        colliders = new Collider2D[10];
        playerLayer = LayerMask.GetMask("Player");

        exitGauge.rectTransform.localScale = new Vector3(0f, 1f, 1f);
        exitGauge.gameObject.SetActive(false);
        backGround.gameObject.SetActive(false);
        showImageRoutine = StartCoroutine(ShowImageRoutine());
    }

    /// <summary>
    /// 탈출구 포인트를 활성화 시키기 위한 메소드
    /// </summary>
    public void ActiveExitPoint()
    {
        photonView.RPC("ActiveExitPoint_Sync", RpcTarget.All);
    }

    /// <summary>
    /// 탈출구 포인트를 비활성화 시키기 위한 메소드
    /// </summary>
    public void DisableExitPoint()
    {
        Debug.Log("비활성화");
        photonView.RPC("DisableExitPoint_Sync", RpcTarget.All);
    }

    /// <summary>
    /// 엑스칼리버 유저와 상호작용 가능. 5초간 상호작용 후 탈출
    /// </summary>
    /// <param name="player"></param>
    public void Interact(PlayerController player)
    {
        if (!isExit)
        {   // 탈출이 가능한 상황에만 가능
            player.StateController.StateChange(PlayerState.Interact, 0f, 0f, false, false);
            return;
        }

        if (player.PlayerClassData.classType != ClassType.Excalibur)
        {   // 엑스칼리버 유저만 탈출 가능
            player.StateController.StateChange(PlayerState.Interact, 0f, 0f, false, false);
            return;
        }

        if (curPlayer != null)
        {   // 만약 중간에 멈춘다면
            StopExitRoutine();
        }
        else
        {
            curPlayer = player;
            curPlayer.StateController.HitEvent += StopExitRoutine;
            exitRoutine = StartCoroutine(ExitRoutine());
        }
    }

    /// <summary>
    /// 탈출 루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator ExitRoutine()
    {
        exitGauge.gameObject.SetActive(true);
        backGround.gameObject.SetActive(true);
        while(gaugeTime < 5f)
        {
            if (curPlayer != null)
            {
                /*
                if(Vector2.SqrMagnitude(curPlayer.transform.position - transform.position) > showDistance * showDistance)
                {   // 피격당하거나 멀어지면 강제로 정지(아마 슈퍼아머라 그런 일은..?)
                    curPlayer.StateController.StateChange(PlayerState.Interact, 0f, 0f, false, false);
                    StopCoroutine(exitRoutine);
                }
                */
                gaugeTime += Time.deltaTime;
                exitGauge.rectTransform.localScale = new Vector3(gaugeTime / exitTime, 1f, 1f);
            }
            yield return null;
        }

        ExitSucess();


    }
    async void ExitSucess()
    {
        Debug.Log("Exit Success");
        List<Player> players = PhotonNetwork.LocalPlayer.AllyPlayers();
        foreach (var p in players)
        {
            Debug.Log($"player : {p} start");
          await p.SetPropertyAsync<bool>(DefinePropertyKey.VICTORY, true);
            Debug.Log($"player : {p} success");
        }
        Debug.Log($"Localplayer start");
        await PhotonNetwork.LocalPlayer.SetPropertyAsync<bool>(DefinePropertyKey.VICTORY, true);            // 엑스칼리버 플레이어를 게임 승리로
        Debug.Log($"Localplayer success");
        GameManager.Ins.GameOver();
    }
    /// <summary>
    /// 피격 중 탈출 루틴 정지를 위한 메소드
    /// </summary>
    private void StopExitRoutine()
    {
        if (exitRoutine != null)
        {
            StopCoroutine(exitRoutine);
        }
        curPlayer.StateController.HitEvent -= StopExitRoutine;
        gaugeTime = 0f;
        exitGauge.gameObject.SetActive(false);
        backGround.gameObject.SetActive(false);
        curPlayer = null;
    }

    /// <summary>
    /// 키(F) 이미지를 보여주기 위한 루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowImageRoutine()
    {
        while (true)
        {
            // 1. 주변의 플레이어 검사
            isShow = false;
            int size = Physics2D.OverlapCircleNonAlloc(transform.position, showDistance, colliders, playerLayer);

            for (int i = 0; i < size; i++)
            {   // 2. 주변의 플레이어가 있었다면
                PlayerController target = colliders[i].GetComponent<PlayerController>();

                if (target.PlayerClassData.classType == ClassType.Excalibur)
                {   // 3. 그중 엑스칼리버를 들고 있었다면
                    isShow = true;
                }
            }

            keyImageRender.gameObject.SetActive(isShow);

            yield return new WaitForSeconds(0.1f);
        }
    }

    [PunRPC]
    private void ActiveExitPoint_Sync()
    {
        isExit = true;
        animator.SetTrigger("Active");
        showImageRoutine = StartCoroutine(ShowImageRoutine());
    }

    [PunRPC]
    private void DisableExitPoint_Sync()
    {
        isExit = false;
        animator.SetTrigger("Disable");
        StopAllCoroutines();
    }
}
