using Cinemachine;
using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 개발자 : 신지혜, 이형필, 서보운, 이예린
/// 플레이어 컨트롤러 스크립트
/// 전체 컨트롤러를 담당할 스크립트
/// </summary>
public class PlayerController : BaseController
{
    #region Controller Component
    [Header("Controller")]
    [SerializeField] PlayerMoveController moveController;           // 움직임 관련 컨트롤러
    [SerializeField] PlayerAttackController attackController;       // 어택 관련 컨트롤러
    [SerializeField] PlayerSkillController skillController;         // 스킬 관련 컨트롤러
    [SerializeField] PlayerHealthController healthController;       // 체력 관련 컨트롤러
    [SerializeField] PlayerInteractController interactController;   // 상호작용 관련 컨트롤러
    [SerializeField] PlayerUIController uiController;               // UI 관련 컨트롤러
    [SerializeField] PlayerStateController stateController;
    [SerializeField] PlayerStatController statController;         // 레벨과 경험치 관련 컨트롤러
    #endregion

    [SerializeField] PlayerClassData playerClassData;  // 플레이어 직업별 스탯 데이터
    [SerializeField] PlayerStatData playerStatData;
    [SerializeField] int id;
    [SerializeField] CinemachineVirtualCamera virtualCamera;

    [SerializeField] PlayerInput input;
    [SerializeField] HPBarUI hpBarUI;
    [SerializeField] FoodItemBar foodItemBar;
    Slot usingFoodSlot;

    [Tooltip("(직업)Renderer 할당 (필수)")]
    [SerializeField] Animator playerAnim;
    [Tooltip("(직업)Renderer 하위에 (직업)Background 할당 (선택)")]
    [SerializeField] Animator playerBgEffectAnim;
    Rigidbody2D _rigid;
    CircleCollider2D _collider;

    public Coroutine useFoodItem;

    protected AttackType attackType;
    // 캐릭터 필드 데이터(사운드 및 이미지 관련)
    [SerializeField] PlayerFieldData fieldData;

    public Action inputKey;
    // 세팅이 종료되기 전 초기화를 막아주기 위한 불변수
    public bool isSetting;
    [Tooltip("캐릭터 내 Fov 할당 필요")]
    [SerializeField] public VisionController visionController;
    [SerializeField] public CinemachineVirtualCamera VirtualCamera {  get { return virtualCamera; } set { virtualCamera = value; }  }

    public Action<Vector2> camMoveEvent;
    public bool targetCamOther;
    public ChatBubble chatBubble;
    public Vector2 targetPos;
    #region Property
    public PlayerClassData PlayerClassData { get { return playerClassData; } set { playerClassData = value; } }
    public Animator PlayerAnim { get { return playerAnim; } set { playerAnim = value; } }
    public Animator PlayerBgEffectAnim { get { return playerBgEffectAnim; } set { playerBgEffectAnim = value; } }
    public Rigidbody2D Rigid { get { return _rigid; } set { _rigid = value; } }
    public CircleCollider2D Collider { get { return _collider; } set { _collider = value; } }
    public PlayerMoveController MoveController { get { return moveController; } }
    public PlayerHealthController HealthController { get { return healthController; } }
    public PlayerSkillController SkillController { get { return skillController; } }
    public PlayerAttackController AttackController { get { return attackController; } }
    public PlayerStateController StateController { get { return stateController; } }
    public PlayerStatController StatController { get { return statController; } }
    public PlayerUIController UIController { get { return uiController; } }
    public PlayerFieldData FieldData { get { return fieldData; } }
    #endregion
    public int ID { get { return id; } }

    public WaitRoomPanel inWaitRoom;
    public delegate void PlayerStateChangeHandler(PlayerController player, PlayerState newState);

    public event PlayerStateChangeHandler onAddStateEvent;

    public AudioSource audioSource;

    public Coroutine chatBubbleRoutine;
    protected void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
        _collider = GetComponent<CircleCollider2D>();
        _collider.radius = playerClassData.playerColRadius;
        audioSource = GetComponent<AudioSource>();
        owner = this;
    }
    public void InputSystemActive(bool b)
    {
        input.enabled = b;
    }
    protected void Init()
    {
        /* 소유자 지정 */
            moveController.SetOwner(this);
            attackController.SetOwner(this);
            healthController.SetOwner(this);
            skillController.SetOwner(this);
            interactController.SetOwner(this);
            uiController.SetOwner(this);
            stateController.SetOwner(this);
            statController.SetOwner(this);

        int playerID = 0;

        switch(playerClassData.classType)
        {
            case ClassType.Excalibur:
                playerID = 1000000;
                break;
            case ClassType.Warrior:
                playerID = 1000002;
                break;
            case ClassType.Knight:
                playerID = 1000001;
                break;
            case ClassType.Wizard:
                playerID = 1000003;
                break;
            case ClassType.Archer:
                playerID = 1000004;
                break;
        }

        if(playerID != 0)
        {
            fieldData = CsvParser.Instance.PlayerFieldDic[playerID];
        }

        if (photonView.IsMine)
        {
            //   Inventory.Instance.Player = this;
            if (!PhotonNetwork.LocalPlayer.GetProperty<bool>(DefinePropertyKey.OUTGAME))
            {
                visionController.CircleVision(false);
                virtualCamera.gameObject.SetActive(true);
                CopyPos[] miniCam = FindObjectsOfType<CopyPos>();
                foreach (CopyPos c in miniCam)
                {
                    c.target = transform;
                }

                /*if (id != 0)
                {
                    // TODO...
                    // 추후에 파이어 베이스 인증으로 인해 인증용 아이디로 변경하기
                    if (CsvParser.Instance.PlayerDic.ContainsKey(id))
                    {
                        FirebaseManager.Instance.Link.InitData(CsvParser.Instance.PlayerDic[id], $"{photonView.name} {Random.Range(0, 10)}");
                    }
                }
                // playerStatData = CsvParser.Instance.PlayerDic[id];*/
            }
            else
            {
                virtualCamera.gameObject.SetActive(false);
                
            }
        }
        else
        {
            if (input != null)
            {
                input.enabled = false;
            }

        }

        isSetting = true;
    }
    protected void Start()
    {
        Init();
    }
    /// <summary>
    /// 모든 컨트롤러 정지함수.
    /// </summary>
    public void AllControllerStop()
    {
        Debug.Log("all stop");
        moveController.enabled = false;
        attackController.AttackPoint.enabled = false;
        attackController.enabled = false;
        //skillController.enabled = false;
        interactController.enabled = false;
        uiController.enabled = false;
        if (stateController.CurState.Contain(PlayerState.Dead))
        {
           
            healthController.enabled = false;
            stateController.enabled = false;
        }

    }
    /// <summary>
    /// 수면에 빠지는 루틴 ui를 빼고 전부 false
    /// </summary>
    public void InSleep()
    {
        Debug.Log("InSleep");
        moveController.enabled = false;
        attackController.enabled = false;
        skillController.enabled = false;
        healthController.enabled = false;
        interactController.enabled = false;
        uiController.enabled = false;

    }
    public void MoveSetter(bool value)
    {
        moveController.enabled = value;
    }
    public void AllControllerStart()
    {
        if (!PhotonNetwork.LocalPlayer.GetProperty<bool>(DefinePropertyKey.GAMEOVER))
        {
            Debug.Log("all start");
            moveController.enabled = true;
            attackController.enabled = true;
            attackController.AttackPoint.enabled = true;
            skillController.enabled = true;
            healthController.enabled = true;
            interactController.enabled = true;
            uiController.enabled = true;
            stateController.enabled = true;
        }
    }


    #region Event
    /// <summary>
    /// HitEvent에 반응할 메소드
    /// 에디터 상에서 할당 필요함(HealthController의 OnHit이벤트)
    /// </summary>
    public void HitEvent()
    {

        if (useFoodItem != null)
        {
            StopCoroutine(useFoodItem);
            useFoodItem = null;
            Inventory.Instance.canUseFood = true;
        }
        StartCoroutine(HitRoutine());
    }

    /// <summary>
    /// DieEvent에 반응할 메소드
    /// 에디터 상에서 할당 필요함(HealthController의 OnDie이벤트)
    /// </summary>
    public void DieEvent()
    {
        Die();

        if (photonView.IsMine)
        {

            if (!CanRevive())
            {
                if (StateController.CurState.Contain(PlayerState.Dead))
                {
                    GameManager.Ins.DieEvent();
                }
                else if (StateController.CurState.Contain(PlayerState.Groggy))
                {
                    GameManager.Ins.Message("GROGGY");
                }
            }

        }

    }
    #endregion
    public bool CanRevive()
    {
        foreach (CommonSkillData curSkill in SkillController.CurCommonSkill)
        {
            if (curSkill is CommonSkill_Pendant revive)
            {
                if (!revive.IsUse)
                {

                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                continue;
            }
        }
        return false;
    }
    public void Die()
    {
        Debug.Log($"현재 팀 게임 여부 {PhotonNetwork.CurrentRoom.GetProperty<bool>(DefinePropertyKey.TEAMGAME)}");
        if (PhotonNetwork.CurrentRoom.GetProperty<bool>(DefinePropertyKey.TEAMGAME))
        {
            Debug.Log("Groggy");
            StateController.StateChange(PlayerState.Groggy, 0, 0, true, true);
        }
        else
        {
            Debug.Log("Die");
            StateController.StateChange(PlayerState.Dead, 0, 0, true, true);
        }
        photonView.RPC("PlaySound", RpcTarget.All, fieldData.deadSoundID);
        playerAnim.SetTrigger("Death");
        AllControllerStop();
    }
    #region Move
    private void OnMove(InputValue value)
    {
        inputKey?.Invoke();
        if (stateController.trapResistStart)
        {
            stateController.t -= stateController.t * 0.1f;
        }

        if (!moveController.enabled)
        {
            return;
        }
        if (stateController.CurState.Contain(PlayerState.Puzzle))
        {
            Vector2 moveDir = value.Get<Vector2>();
            moveController.SetMoveDir(moveDir);
            return;
        }
        if (stateController.CurState.Contain(PlayerState.Interact | PlayerState.Maintain | PlayerState.Dead|PlayerState.Chat))
        {
            return;
        }
        if (photonView.IsMine && moveController.enabled)
        {
            Vector2 moveDir = value.Get<Vector2>();
            moveController.Move(moveDir);
            if (moveDir.magnitude > 0)
            {
                if (SoundManager.instance.sfxLoopRoutine != null)
                {
                    photonView.RPC("PlaySoundLoop", RpcTarget.All, fieldData.moveSoundID, false);
                }

                photonView.RPC("PlaySoundLoop", RpcTarget.All, fieldData.moveSoundID, true);
                owner.StateController.StateChange(PlayerState.Move, 0, 0, true, false);
                owner.StateController.StateChange(PlayerState.Idle, 0, 0, false, false);
            }
            else
            {
                moveController.MovingStop();
                photonView.RPC("PlaySoundLoop", RpcTarget.All, fieldData.moveSoundID, false);
                owner.StateController.StateChange(PlayerState.Idle, 0, 0, true, false);
                owner.StateController.StateChange(PlayerState.Move, 0, 0, false, false); // 상태 변경
            }
        }
    }
    #endregion

    #region Attack
    // 오버라이딩 한 곳에도 반드시 [PunRPC] 적어줘야 함
    [PunRPC]
    public void Attack()
    { 
        playerAnim.SetTrigger("Attack");
        attackController.AttackPoint.WeaponAnim("Attack");
        if (owner.PlayerClassData.classType.Equals(ClassType.Warrior))  // 전사 직업의 공격일 경우
        {
            // 무기 오브젝트 비활성화 하기
            if (attackController.AtkEffect != null)
            {
                attackController.AtkEffect.SetTrigger("SkillOn");
                attackController.AtkEffect.SetBool("SkillStop", false);
            }
        }
        attackController.Attack();
    }

    private void OnAttack(InputValue value)
    {
       // Debug.Log("OnClick");
        if (GameManager.Ins.buttonDown)
        {
            Debug.Log("Button => attackCancle");
            return;
        }
        inputKey?.Invoke();
        if (!attackController.enabled)
        {
            return;
        }

        if (stateController.CurState.Contain(PlayerState.Interact | PlayerState.AttackStart | PlayerState.Maintain | PlayerState.Dead | PlayerState.Chat))
        {
            return;
        }

        if (photonView.IsMine)
        {
            photonView.RPC("Attack", RpcTarget.All);
            _rigid.velocity = Vector2.zero;
        }
    }
    #endregion


    #region Skill

    protected virtual void OnCommonSkill1(InputValue value)
    {
        if (!photonView.IsMine || stateController.CurState.Contain(PlayerState.Silence | PlayerState.SkillStart | PlayerState.AttackStart | PlayerState.Maintain | PlayerState.Chat))
        {
            return;
        }
        photonView.RPC("CommonSkill1", RpcTarget.All);
    }

    protected virtual void OnCommonSkill2(InputValue value)
    {
        if (!photonView.IsMine || stateController.CurState.Contain(PlayerState.Silence | PlayerState.SkillStart | PlayerState.AttackStart | PlayerState.Maintain | PlayerState.Chat))
        {
            return;
        }
        photonView.RPC("CommonSkill2", RpcTarget.All);
    }

    private void OnGeneralSkill1(InputValue value)
    {
        if (!photonView.IsMine || stateController.CurState.Contain(PlayerState.Silence | PlayerState.SkillStart | PlayerState.AttackStart | PlayerState.Maintain | PlayerState.Chat))
        {
            return;
        }
        photonView.RPC("GeneralSkill1", RpcTarget.All);
    }

    private void OnGeneralSkill2(InputValue value)
    {
        if (!photonView.IsMine || stateController.CurState.Contain(PlayerState.Silence | PlayerState.SkillStart | PlayerState.AttackStart | PlayerState.Maintain | PlayerState.Chat))
        {
            return;
        }
        photonView.RPC("GeneralSkill2", RpcTarget.All);
    }

    private void OnSuperSkill(InputValue value)
    {
        if (!photonView.IsMine || stateController.CurState.Contain(PlayerState.Silence | PlayerState.SkillStart | PlayerState.AttackStart | PlayerState.Maintain | PlayerState.Chat))
        {
            return;
        }
        photonView.RPC("SuperSkill", RpcTarget.All);
    }

    /// <summary>
    /// 트리거 애니메이션 동기화 메소드
    /// </summary>
    /// <param name="trigger"></param>
    public void SetAnimator(string trigger)
    {
        photonView.RPC("SetAnimator_Trigger", RpcTarget.All, trigger);
    }

    [PunRPC]
    protected void SetAnimator_Trigger(string trigger)
    {
        playerAnim.SetTrigger(trigger);
    }

    /// <summary>
    /// bool 변수 애니메이션 동기화 메소드
    /// </summary>
    /// <param name="trigger"></param>
    public void SetAnimator(string name, bool boolValue)
    {
        photonView.RPC("SetAnimator_Bool", RpcTarget.All, name, boolValue);
    }

    [PunRPC]
    protected void SetAnimator_Bool(string name, bool boolValue)
    {
        playerAnim.SetBool(name, boolValue);
    }

    /// <summary>
    /// float 변수 애니메이션 동기화 메소드
    /// </summary>
    /// <param name="trigger"></param>
    public void SetAnimator(string name, float floatValue)
    {
        photonView.RPC("SetAnimator_Float", RpcTarget.All, name, floatValue);
    }

    [PunRPC]
    protected void SetAnimator_Float(string name, float floatValue)
    {
        playerAnim.SetFloat(name, floatValue);
    }

    #endregion


    #region Interact
    private void OnInteract(InputValue value)
    {
        interactController.Interact(this);
    }
    #endregion


    #region OpenInventory
    private void OnOpenInventory(InputValue value)
    {
        if (GameManager.Ins.onChat)
        {
            return;
        }
        if (!photonView.IsMine)
        {
            return;
        }

        uiController.OpenInventoryUI();

        if (!Inventory.Instance.isOpened)   // 인벤토리가 처음 열리는 상황이라면
        {
            Inventory.Instance.isOpened = true;
            StartCoroutine(TurnOffVerticalLayout());
        }
    }

    /// <summary>
    /// 개발자: 이예린 / 인벤토리의 VerticalLayoutGroup를 0.1초 후 꺼주는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator TurnOffVerticalLayout()
    {
        yield return new WaitForSeconds(0.1f);
        Inventory.Instance.VerticalLayout.enabled = false;
    }
    #endregion


    #region OpenSetting
    private void OnOpenSetting(InputValue value)
    {
        uiController.OpenSettingUI();
    }
    #endregion


    #region Coroutine
    /// <summary>
    /// 플레이어 피격 루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator HitRoutine()
    {
        if(StateController.CurState.Contain(PlayerState.SuperArmor))
        {   // 슈퍼아머는 피격 루틴이 없음.
            yield break;
        }

        stateController.StateChange(PlayerState.Hit, 0, 1, true, true);
        moveController.Move(Vector2.zero);
        // attackController.enabled = false;
        // moveController.enabled = false;
        yield return null;
    }
    #endregion


    #region Item Use

    #region Hp Potion
    /// <summary>
    /// 개발자: 이예린 / 회복 포션 아이템 사용 시 발생하는 효과 구현한 코루틴 실행
    /// </summary>
    /// <param name="duration">지속 시간</param>
    /// <param name="healPerSec">초당 증가량</param>
    public void UseHpPotion(int duration, float healPerSec)
    {
        StartCoroutine(HpPotionDuration(duration, healPerSec));
    }

    /// <summary>
    /// 개발자: 이예린 / 회복 포션 아이템 사용 시 발생하는 효과 구현한 코루틴
    /// </summary>
    /// <param name="duration">지속 시간</param>
    /// <param name="healPerSec">초당 증가량</param>
    /// <returns></returns>
    IEnumerator HpPotionDuration(int duration, float healPerSec)
    {
        Inventory.Instance.canUseHpPotion = false;
        int countTime = 0;

        while (countTime++ < duration)
        {
            float hp = healthController.Hp;
            Debug.Log($"time: {countTime}");
            hp += healPerSec;
            if (hp > healthController.MaxHp)
            {
                healthController.Hp = healthController.MaxHp;
                break;
            }
            healthController.Hp = hp;
            yield return new WaitForSeconds(1f);
        }

        if (countTime < duration)
        {
            while (countTime++ < duration)
            {
                Debug.Log($"time: {countTime}");
                yield return new WaitForSeconds(1f);
            }
        }
        Inventory.Instance.canUseHpPotion = true;
    }



    #endregion

    #region Food
    /// <summary>
    /// 개발자: 이예린/ 식량 아이템 사용 시 발생하는 castinfg 코루틴 실행
    /// </summary>
    /// <param name="casting">캐스팅 시간</param>
    /// <param name="heal">회복량</param>
    public void UseFood(Slot slot, int casting, float heal)
    {
        useFoodItem = StartCoroutine(FoodCasting(slot, casting, heal));
    }

    /// <summary>
    /// 개발자: 이예린/ 식량 아이템 사용 시 발생하는 castinfg 코루틴
    /// </summary>
    /// <param name="casting">캐스팅 시간</param>
    /// <param name="heal">회복량</param>
    /// <returns></returns>
    IEnumerator FoodCasting(Slot slot, int casting, float heal)
    {
        usingFoodSlot = slot;
        Inventory.Instance.canUseFood = false;
        // moveController.enabled = false;
        // attackController.enabled = false;
        float hp = healthController.Hp;

        foodItemBar.StartFoodItmeRoutine(casting, slot.Item.ItemImage);
        yield return new WaitForSeconds(casting);

        Debug.Log($"--------------0 {hp}, {healthController.MaxHp}");
        hp += healthController.MaxHp * heal;
        Debug.Log($"--------------1 {hp}, {healthController.MaxHp}");
        if (hp > healthController.MaxHp)
        {
            hp = healthController.MaxHp;
        }

        healthController.Hp = hp;
        Debug.Log($"--------------2 {hp}, {healthController.MaxHp}");
        // moveController.enabled = true;
        // attackController.enabled = true;
        useFoodItem = null;
        usingFoodSlot = null;
        Inventory.Instance.canUseFood = true;

        if (slot.Type.Equals(SlotType.Bag))
        {
            slot.AfterUseItem();
        }
        else if (slot.Type.Equals(SlotType.Quick))
        {
            (slot as QuickSlot).AfterUseItem();
        }
    }
    #endregion

    #region Buff potion
    /// <summary>
    /// 개발자: 이예린 / 버프 포션 아이템 사용 시 발생하는 효과 구현한 코루틴 실행
    /// </summary>
    /// <param name="duration">지속 시간</param>
    /// <param name="statUp">능력치 증가량</param>
    /// <param name="stat">증가할 능력</param>
    public void UseBuffPotion(int duration, float statUp, IncrementalStat stat)
    {
        StartCoroutine(BuffPotionDuration(duration, statUp, stat));
    }

    /// <summary>
    /// 개발자: 이예린 / 버프 포션 아이템 사용 시 발생하는 효과 구현한 코루틴
    /// </summary>
    /// <param name="duration">지속 시간</param>
    /// <param name="statUp">능력치 증가량</param>
    /// <param name="stat">증가할 능력</param>
    /// <returns></returns>
    IEnumerator BuffPotionDuration(int duration, float statUp, IncrementalStat stat)
    {
        Inventory.Instance.canUseBuffPotion = false;
        switch (stat)
        {
            case IncrementalStat.Offensive:
                attackController.AtkDamage += statUp;
                yield return new WaitForSeconds(duration);
                attackController.AtkDamage -= statUp;
                break;
            case IncrementalStat.Defense:
                healthController.Defense += statUp;
                yield return new WaitForSeconds(duration);
                healthController.Defense -= statUp;
                break;
            case IncrementalStat.AttackSpeed:
                attackController.AtkSpeed += statUp;
                yield return new WaitForSeconds(duration);
                attackController.AtkSpeed -= statUp;
                break;
        }
        Inventory.Instance.canUseBuffPotion = true;
    }
    #endregion

    #endregion

    /// <summary>
    /// 플레이어의 카메라를 이동시키는 메서드, 죽어있는 상태가 아닐 때 타겟이 플레이어를 가리키지 않으면 다른 플레이어에게 어디를 보고 있는지 RPC로 알림
    /// </summary>
    /// <param name="target"></param>
    public void CameraMove(Transform target)
    {
        if (!owner.StateController.CurState.Contain(PlayerState.Dead))
        {
            if (target != owner.transform)
            {
                photonView.RPC("CamTargeting", RpcTarget.All, (Vector2)target.position);
            }
            else
            {
                Debug.Log("else Cam Move");
                photonView.RPC("CamTargeting", RpcTarget.All, Vector2.zero);
            }


        }

        if (owner.photonView.IsMine)
        {
            owner.virtualCamera.Follow = target;
        }
    }
    public void CameraMove(Vector2 pos)
    {
        if (owner.photonView.IsMine)
        {
            owner.virtualCamera.Follow = null;
            owner.virtualCamera.transform.position = new Vector3(pos.x, pos.y, -10);
            Debug.Log("Came Vector2 Move");
        }
    }
    [PunRPC]
    public void CamTargeting(Vector2 targetPos)
    {
        camMoveEvent?.Invoke(targetPos);
        Debug.Log(targetPos != Vector2.zero ? $"Targeting Vec : {targetPos}" : "OtherTargeting Unlook");
        targetCamOther = targetPos != Vector2.zero;
        if (targetCamOther)
        {
            this.targetPos = targetPos;
        }
    }

    /// <summary>
    /// 상태가 변경됐을경우 호출되는 메서드
    /// </summary>
    public void AddStateEvent(PlayerState state)
    {
        onAddStateEvent?.Invoke(this,state);
        if(state == PlayerState.Lobby)
        {
            return;
        }
        if (useFoodItem != null)
        {
            StopCoroutine(useFoodItem);
            foodItemBar.StopFoodItmeRoutine();
            usingFoodSlot.canDrag = true;
            if (usingFoodSlot.Type.Equals(SlotType.Quick))
            {
                Inventory.Instance.QuickItems[usingFoodSlot as QuickSlot].canDrag = true;
            }

            useFoodItem = null;
            usingFoodSlot = null;
            Inventory.Instance.canUseFood = true;
        }
        useFoodItem = null;
        if(Inventory.Instance != null)
        {
            Inventory.Instance.canUseFood = true;
        }

        if (state == PlayerState.Sleep)
        {
            SkillCut();
        }

    }
    public void SkillCut()
    {
        var skillData = owner?.SkillController?.playerSkillDataIns;

        if (skillData == null)
        {
            return;
        }

        switch (skillData)
        {
            case WarriorSkill warriorSkill:
                if (warriorSkill != null) // 추가적인 null 체크
                {
                    warriorSkill.StopSkill();
                }
                break;
        }
    }
   public int FactorCalculate(float dmgFactor)
    {
      return (int)(dmgFactor * AttackController.AtkDamage);
    }

    [PunRPC]
    void RPC_EntryConnect(int id)
    {
        //   PlayerController ins = PhotonView.Find(viewID).GetComponent<PlayerController>();
        this.id = id;
        StartCoroutine(FindWaitRoom());
    }
    IEnumerator FindWaitRoom()
    {
        inWaitRoom = FindObjectOfType<WaitRoomPanel>();
        yield return new WaitUntil(() =>inWaitRoom != null);
        yield return new WaitUntil(() => inWaitRoom.playerList != null);
        inWaitRoom.playerInstances.Add(this);
        
        foreach (var player in inWaitRoom.playerList)
        {
            player.SetPlayer(player.Player);
            
            if (player.playerNum == ID)
            {
                Debug.Log($"{player} : {ID}");
                GameManager.Ins.Init();
                player.controller = this;
                StateController.MaterialSet(AttackController.ownerMaterial);
                
                player.Player.SetProperty(DefinePropertyKey.LOADCOMPLETE, true);
                break;
            }
        }
    }
    [PunRPC]
    public void SetHpBarColor(float r, float g, float b, float a)
    {
        Color color = new Color(r, g, b, a);
        HealthController.hpBarUI.HPBarGauge.color = color;
    }

    [PunRPC]
    public void PlaySound(int id)
    {
        SoundManager.instance.PlaySFX(id, audioSource);
    }

    [PunRPC]
    public void PlaySoundLoop(int id, bool value)
    {
        if (value)
        {
            SoundManager.instance.PlaySFXLoop(id, audioSource, 0.5f);
        }
        else
        {
            SoundManager.instance.StopSFXLoop();
        }
    }
}