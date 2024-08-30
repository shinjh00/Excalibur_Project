using Photon.Pun;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필 / 트랩을 정의하는 가상부모클래스
/// </summary>
public abstract class Trap : MonoBehaviourPun
{
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected LayerMask trapLayer;
    [SerializeField] protected LayerMask wall;
    [SerializeField] protected LayerMask monster;

    [SerializeField] protected float coolTime;
    [SerializeField] protected float range;
    [SerializeField] protected float damage;
    [SerializeField] protected float knockBack;
    [SerializeField] protected int activeSound;

    [SerializeField] protected bool coolChecker = false;
    [SerializeField] protected SpriteRenderer sr;
    protected Animator anim;
    protected bool isActivate;
    public bool IsActivate { get { return isActivate; } set { isActivate = value; Activate(value); } }
    protected PlayerController targetPlayer;
    protected int trapId;


    [SerializeField] protected AudioSource audioSource;
    public int TrapId { get { return trapId; } set { trapId = value; photonView.RPC("SetTrap",RpcTarget.All,value); } }

    protected virtual void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        monster = LayerMask.GetMask("Monster");
        playerLayer = LayerMask.GetMask("Player");
        trapLayer = LayerMask.GetMask("Trap");
        wall = LayerMask.GetMask("Wall");
        audioSource = GetComponent<AudioSource>();

    }
    protected abstract void Init();

    /// <summary>
    /// 함정이 발동하는 메서드
    /// </summary>
    /// <param name="collider"></param>
    protected abstract void Activate(bool IsActivated);
    /// <summary>
    /// 함정을 만드는 메서드 
    /// </summary>
    /// <param name="pos"></param>
 //   public abstract void CreateTrap(Vector2 pos);
    [PunRPC]
    public abstract void SetActive(bool active,int ActivePlayerViewId);

 
}
