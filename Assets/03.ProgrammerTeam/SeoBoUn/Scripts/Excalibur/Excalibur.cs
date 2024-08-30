using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 서보운
/// <br/> 엑스칼리버 무기 스크립트
/// <br/>1. 0세대 방에 생성되며 4분 뒤 획득 가능한 상태로 변경됨
/// (이 때, 빛나는 모습이 되어야 하고 시스템 메세지 알림이 필요)
/// <br/>2. 플레이어가 근접하면 F 키로 상호작용 표시 및 가능 (상호작용시 살짝 Y축 위로 움직여야 함)
/// 가장 먼저 상호작용한 플레이어 변신(일시적 무적 및 직업 변경)
/// <br/>3. 사망한 플레이어는 엑스칼리버 아이템을 떨어뜨리며 동일한 과정을 거침(단, 움직이는 것은 없이 변신만)
/// </summary>
public class Excalibur : MonoBehaviourPun, IInteractable
{
    [Tooltip("현재 장착한 플레이어(에디터 할당 x)")]
    [SerializeField] PlayerController curController;

    [Tooltip("상호작용 키 이미지 렌더러(에디터 할당 필요)")]
    [SerializeField] SpriteRenderer keyImageRender;
    [Tooltip("엑스칼리버 이미지 렌더러(에디터 할당 필요)")]
    [SerializeField] SpriteRenderer excaliburImage;
    [Tooltip("엑스칼리버 뽑는 게이지 표시용(에디터 할당 필요)")]
    [SerializeField] Image excaliburGauge;
    [Tooltip("엑스칼리버 버프 게이지 표시용(에디터 할당 필요)")]
    [SerializeField] ExcaliburBuffGauge excaliburBuffGauge;

    [Tooltip("엑스칼리버 얻을 수 있을 때의 색(에디터 할당 필요)")]
    [SerializeField] Sprite activeImage;
    [Tooltip("엑스칼리버 변신 이펙트")]
    [SerializeField] Animator animator;

    [Tooltip("엑스칼리버 획득 가능 시간(에디터 할당 필요")]
    [SerializeField] float excaliburTime;
    [Tooltip("엑스칼리버를 뽑기 위해서 상호작용을 진행해야 하는 시간(에디터 할당 필요)")]
    [SerializeField] float excaliburPoolTime;

    [SerializeField] ExcaliburData curData;

    PlayerController excaliburPlayer;
    Collider2D[] colliders; // 오버랩을 진행하기 위한 콜라이더
    LayerMask playerLayer;  // 플레이어 레이어 체크용
    Coroutine showImageRoutine;
    Coroutine firstPullOutRoutine;

    float startTime = 0f;
    float time = 0f;
    float distance = 1f;    // 상호작용 거리
    bool isGet;             // 얻을 수 있는 상태인가
    bool isFirstController; // 처음 획득한 유저인가
    bool isShow;            // 상호작용 키 이미지를 보여주어야 하는 상황인가

    [SerializeField] int level;
    [SerializeField] int[] statLevel;
    [SerializeField] float[] equip;

    /// <summary>
    /// 엑스칼리버 데이터 세팅 메소드
    /// </summary>
    /// <param name="id"></param>
    public void SetExcaliburData(int id)
    {
        curData = CsvParser.Instance.ExcaliburDic[id];

        startTime = (float)PhotonNetwork.CurrentRoom.GetProperty<double>(DefinePropertyKey.STARTTIME);

        if (curData.lockedImageID != -9999)
        {
            excaliburImage.sprite = Resources.Load<Sprite>(CsvParser.Instance.returnPath(curData.lockedImageID));
        }

        StartCoroutine(ExcaliburGetTime());
    }

    private void Start()
    {
        colliders = new Collider2D[10];
        playerLayer = LayerMask.GetMask("Player");
        keyImageRender.gameObject.SetActive(false);
        excaliburBuffGauge.gameObject.SetActive(false);
        isFirstController = true;
        statLevel = new int[5];
        excaliburPlayer = null;
    }

    public void Interact(PlayerController player)
    {
        if (!isGet)
        {   // 얻을 수 없는 상태
            player.StateController.StateChange(PlayerState.Interact, 0, 0, false, false);
            return;
        }

        if (curController != null)
        {   // 누군가 상호작용중이라면
            player.StateController.StateChange(PlayerState.Interact, 0, 0, false, false);
            return;
        }

        curController = player;

        if (isFirstController)
        {   // 처음 뽑히는 상황이라면
            firstPullOutRoutine = StartCoroutine(FirstPullOutRoutine());
        }
        else
        {   // 아니라면
            StartCoroutine(PullOutRoutine());
        }
    }
    
    /// <summary>
    /// 엑스칼리버를 얻기까지 걸리는 시간용 루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator ExcaliburGetTime()
    {
        isGet = false;

        // 현재 시간 - (실제)시작 시간
        float diffTime = (float)PhotonNetwork.Time - startTime;
        float targetTime = curData.excaliburActiveTime - diffTime;

        yield return new WaitForSeconds(targetTime);
        photonView.RPC("CanGetExcalibur", RpcTarget.All);
    }

    /// <summary>
    /// 엑스칼리버가 처음 뽑히는 루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator FirstPullOutRoutine()
    {
        // 1. 위로 살짝 들리는 이펙트 있었으면 함.
        Debug.Log("뽑기 시작");

        time = 0f;
        while (true)
        {
            if (Input.GetKey(KeyCode.F))
            {   // 누르고 있다면 시간 증가
                time += Time.deltaTime;
            }
            else
            {   // 아니라면 감소
                time = (time - (Time.deltaTime * 3f)) > 0f ? (time - Time.deltaTime) : 0f;
                curController.StateController.StateChange(PlayerState.Interact, 0, 0, false, false);
            }
            // 위로 살짝 들리기
            transform.position = (Vector2.up * time) * 0.75f;
            // excaliburGauge.rectTransform.localScale = new Vector3(time / excaliburPoolTime, 1, 1);

            if (time > excaliburPoolTime)
            {
                curController.StateController.StateChange(PlayerState.Interact, 0, 0, false, false);
                StartCoroutine(ChangeExcaliburJobFirst(curController));
                break;
            }

            yield return null;
        }

        // 2. 이펙트와 함께 뽑은 플레이어를 변신시킴

    }

    /// <summary>
    /// 그 이후 엑스칼리버를 드는 유저의 루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator PullOutRoutine()
    {
        // 단순 이펙트와 함께 뽑은 플레이어를 변신
        StartCoroutine(ChangeExcaliburJob(curController));

        yield return null;
    }

    /// <summary>
    /// 상호작용 이미지 체크 루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowImageRoutine()
    {
        while (true)
        {
            // 1. 주변의 플레이어 검사
            isShow = false;
            yield return new WaitUntil(() => transform != null);
            int size = Physics2D.OverlapCircleNonAlloc(transform.position, distance, colliders, playerLayer);

            for (int i = 0; i < size; i++)
            {   // 2. 주변의 플레이어가 있었다면
                PlayerController target = colliders[i].GetComponent<PlayerController>();

                if (target != null)
                {
                    isShow = true;
                    break;
                }
            }
            keyImageRender.gameObject.SetActive(isShow);

            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// 엑스칼리버 직업으로 변경
    /// </summary>
    private IEnumerator ChangeExcaliburJobFirst(PlayerController player)
    {   // 엑스칼리버 캐릭터로 변경
        // 일단 엑스칼리버 캐릭터 스폰

        // 문제점. 호스트가 아닌 플레이어가 먹었을 시 스탯 레벨이 동기화가 되어 있지 않음.
        level = player.StatController.PlayerLevel;
        statLevel = player.StatController.StatLevels;
        equip = new float[] { player.StatController.equipMaxHpRate, player.StatController.equipAtkDamageRate, player.StatController.equipAtkSpeedRate, player.StatController.equipDefenseRate, player.StatController.equipMoveSpeedRate };

        curController = null;
        animator.SetTrigger("Pull");
        // 기존 캐릭터는 삭제
        PhotonNetwork.Destroy(player.gameObject);
        // 엑스칼리버 캐릭터를 소환하고
        excaliburPlayer = PhotonNetwork.Instantiate("6.Prefab/Character/ExcaliburChar", player.transform.position, Quaternion.identity).GetComponent<PlayerController>();

        excaliburPlayer.HealthController.OnExcaliburDie += (ExcaliburDie);
        photonView.RPC("PullOutExcalibur", RpcTarget.All, excaliburPlayer.photonView.ViewID);
        yield return new WaitForSeconds(1f);
        ApplyExcaliburStat(excaliburPlayer);
        StartCoroutine(BuffRoutine(excaliburPlayer));

        yield return new WaitUntil(() => excaliburPlayer.UIController.inventoryUI != null);
        Inventory.Instance.SetInventorySetting(excaliburPlayer);
        
        // 원격에 있는 엑스칼리버들만 해당 메소드를 실행함. 전원이 실행해야 함..
        photonView.RPC("GetExcalibur", RpcTarget.MasterClient);
    }

    // 2번째부터는 버프 없이 변경으로 진행
    private IEnumerator ChangeExcaliburJob(PlayerController player)
    {   // 엑스칼리버 캐릭터로 변경
        // 일단 엑스칼리버 캐릭터 스폰

        // 문제점. 호스트가 아닌 플레이어가 먹었을 시 스탯 레벨이 동기화가 되어 있지 않음.
        level = player.StatController.PlayerLevel;
        statLevel = player.StatController.StatLevels;
        equip = new float[] { player.StatController.equipMaxHpRate, player.StatController.equipAtkDamageRate, player.StatController.equipAtkSpeedRate, player.StatController.equipDefenseRate, player.StatController.equipMoveSpeedRate };

        curController = null;
        animator.SetTrigger("Pull");
        // 기존 캐릭터는 삭제
        PhotonNetwork.Destroy(player.gameObject);
        // 엑스칼리버 캐릭터를 소환하고
        excaliburPlayer = PhotonNetwork.Instantiate("6.Prefab/Character/ExcaliburChar", player.transform.position, Quaternion.identity).GetComponent<PlayerController>();
        yield return new WaitUntil(() => excaliburPlayer != null);

        excaliburPlayer.HealthController.OnExcaliburDie += (ExcaliburDie);
        photonView.RPC("PullOutExcalibur", RpcTarget.All, excaliburPlayer.photonView.ViewID);
        yield return new WaitForSeconds(1f);

        ApplyExcaliburStat(excaliburPlayer);

        yield return new WaitUntil(() => excaliburPlayer.UIController.inventoryUI != null);
        Inventory.Instance.SetInventorySetting(excaliburPlayer);


        // 원격에 있는 엑스칼리버들만 해당 메소드를 실행함. 전원이 실행해야 함..
        photonView.RPC("GetExcalibur", RpcTarget.MasterClient);
    }

    /// <summary>
    /// 엑스칼리버 스탯으로 변경 및 적용
    /// </summary>
    /// <param name="curPlayer"></param>
    private void ApplyExcaliburStat(PlayerController curPlayer)
    {
        // 기존의 스탯 상황을 인계(레벨, 스탯 포인트, 장비들..)
        for (int i = 0; i < statLevel.Length; i++)
        {
            curPlayer.StatController.StatLevels[i] = statLevel[i];
        }

        curPlayer.StatController.ChangeLevel(level);                                      // 이전 레벨 적용
        curPlayer.StatController.ChangeExcaliburStat(equip);                              // 이전 장비 스탯 적용
        curPlayer.StatController.ApplyStat();                                             // 실제 스탯에 따른 적용 메소드
        curPlayer.StateController.StateChange(PlayerState.SuperArmor, 0, 0, true, false); // 슈퍼아머 적용
    }

    public void ExcaliburDie(Vector3 pos)
    {
        Debug.Log("사망");
        photonView.RPC("ExcaliburDie_Sync", RpcTarget.All, pos);
    }

    IEnumerator BuffRoutine(PlayerController curPlayer)
    {   // 엑스칼리버를 처음 뽑는 유저 버프
        // 고정 수치값으로 변경 / (기본스탯 + 레벨스탯) * 장비 퍼센트 * 버프값 + 고정 수치값
        curPlayer.StatController.AddBuffStat(StatLevel.Atk, 100);
        curPlayer.StatController.AddBuffStat(StatLevel.Def, 170);
        yield return new WaitForSeconds(120f);

        curPlayer.StatController.RemoveBuffStat(StatLevel.Atk, 100);
        curPlayer.StatController.RemoveBuffStat(StatLevel.Def, 170);
        photonView.RPC("EndBuff_Sync", RpcTarget.All);
    }

    [PunRPC]
    private void PullOutExcalibur(int viewID)
    {
        GameManager.Ins.Init();
        if (isFirstController)
        {   // 처음 뽑은 유저상황 동기화 필요
            isFirstController = false;
            excaliburBuffGauge.gameObject.SetActive(true);
            ExcaliburAttackPoint attackPoint = GameManager.Ins.excaliburPlayer.AttackController.AttackPoint as ExcaliburAttackPoint;
            attackPoint.BuffAnim.enabled = true;
            excaliburBuffGauge.StartBuff();
        }

        excaliburImage.enabled = false;
        keyImageRender.enabled = false;
        curController = null;
        isGet = false;
    }

    [PunRPC]
    private void ExcaliburDie_Sync(Vector3 pos)
    {
        if (excaliburBuffGauge.gameObject.activeSelf)
        {
            photonView.RPC("EndBuff_Sync", RpcTarget.All);
            excaliburBuffGauge.gameObject.SetActive(false);
        }

        transform.position = pos;
        excaliburImage.enabled = true;
        keyImageRender.enabled = true;
        isGet = true;
        excaliburPlayer = null;
        photonView.RPC("ChangeMagneticFieldOwner", RpcTarget.All, pos);
        GameManager.Ins.magneticField.MagneticPos.position = pos;
        GameManager.Ins.ExcaliburDie();
    }

    [PunRPC]
    private void GetExcalibur()
    {
        photonView.RPC("ChangeMagneticFieldOwner", RpcTarget.All);
        photonView.RPC("TargetingPoint", RpcTarget.All);
    }

    [PunRPC]
    private void ChangeMagneticFieldOwner()
    {
        if (GameManager.Ins.excaliburPlayer != null)
        {
            GameManager.Ins.magneticField.owner = GameManager.Ins.excaliburPlayer.gameObject;
            if (!GameManager.Ins.magneticField.getExcalibur && GameManager.Ins.magneticField.CurentPhase == 0)
            {
                GameManager.Ins.magneticField.getExcalibur = true;
                GameManager.Ins.magneticField.StartSafeZoneShrinkers();
            }
        }
    }

    [PunRPC]
    private void CanGetExcalibur()
    {
        showImageRoutine = StartCoroutine(ShowImageRoutine());

        if (curData.activeImageID == -9999)
        {
            excaliburImage.sprite = activeImage;
        }
        else
        {
            excaliburImage.sprite = activeImage;
            // excaliburImage.sprite = Resources.Load<Sprite>(CsvParser.Instance.returnPath(curData.activeImageID));
        }
        isGet = true;
    }

    [PunRPC]
    private void ChangeMagneticFieldOwner(Vector3 pos)
    {
        if (excaliburPlayer == null)
        {
            GameManager.Ins.magneticField.owner = null;
            GameManager.Ins.magneticField.excaliburPos = pos;
        }
    }

    [PunRPC]
    private void TargetingPoint()
    {
        GameManager.Ins.GetExcalibur();
    }

    [PunRPC]
    private void EndBuff_Sync()
    {
        excaliburBuffGauge.EndBuff();
        ExcaliburAttackPoint attackPoint = GameManager.Ins.excaliburPlayer.AttackController.AttackPoint as ExcaliburAttackPoint;
        attackPoint.BuffAnim.gameObject.SetActive(false);
    }
}
