using Photon.Pun;
using UnityEngine;
using UnityEngine.UIElements;
/// <summary>
/// 마우스로 플레이어가 타겟팅하는 방향을 정의하는 클래스
/// </summary>
public class AttackPoint : MonoBehaviour
{
    [SerializeField] protected VisionController playerLight;
    [SerializeField] protected float smoothingFactor;
    [Tooltip("무기 애니메이션(아쳐 보우용)")]
    [SerializeField] Animator weaponAnim;

    [SerializeField] protected PlayerAttackController attackController;
    [SerializeField] protected AttackEffect atteckEffect;
    protected PhotonView playerPv;
    protected Camera _camera;
    protected Vector3 mouseDir;
    protected float cosRange;
    [SerializeField] protected float range;
    [SerializeField] protected double smfRange = 0.0001;

    protected bool isAttacking;

    [SerializeField] Animator effectAnim;

    #region Property
    public Vector3 MouseDir { get { return mouseDir; } set { mouseDir = value; } }
    public bool IsAttacking { get { return isAttacking; } set { isAttacking = value; } }
    public AttackEffect Effect { get { return atteckEffect; } }
    //public Vector3 MousePos;
    #endregion

    protected virtual void Awake()
    {
        _camera = Camera.main;
        attackController = GetComponentInParent<PlayerAttackController>();
        cosRange = Mathf.Cos(90 * Mathf.Deg2Rad);
        playerPv = GetComponentInParent<PhotonView>();

        if (!playerPv.IsMine)
        {
            playerLight.CircleVision(false);
            playerLight.FovVision(false);
        }
    }

    protected virtual void Update()
    {
        //   MousePos = _camera.ScreenToWorldPoint(Input.mousePosition);

        if (playerPv.IsMine)
        {
            if(isAttacking)
            {
                return;
            }

            float distanceToMouse = Vector3.Distance(transform.position, _camera.ScreenToWorldPoint(Input.mousePosition));

            if (distanceToMouse > range)
            {

                Vector3 targetDir = (_camera.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
                targetDir.z = 0;
                mouseDir = Vector3.Lerp(mouseDir, targetDir, Time.deltaTime * smoothingFactor); // smoothingFactor는 부드럽게 하는 정도를 조절

                float dis = Vector3.Distance(targetDir, mouseDir);

                if (dis > smfRange)
                {
                    smoothingFactor = 20f;
                }
                else
                    smoothingFactor = 1f;
                transform.up = mouseDir;
       //         attackController.SetMousePoiot(mouseDir);
                RotateCornLight();
            }

        }
    }

    /// <summary>
    /// 개발자 : 서보운
    /// 마우스 포인터의 위치 체크 메소드
    /// </summary>
    public bool CheckFlip()
    {
        Vector2 dirToTarget = mouseDir.normalized;

        if (Vector2.Dot(Vector3.right, dirToTarget) > cosRange)
        {
            return false;
        }
        else
        {
            return true;
        }

    }

    protected void RotateCornLight()
    {
        playerLight.transform.up = mouseDir.normalized;
    }


    private void OnDrawGizmos()
    {
        /*if (mouseDir == Vector3.zero)
            return;

        Vector3 targetDir = mouseDir.normalized;
        Vector3 leftDir = Quaternion.AngleAxis(attackController.AtkAngle * 0.5f, Vector3.forward) * targetDir;
        Vector3 rightDir = Quaternion.AngleAxis(attackController.AtkAngle * 0.5f, (-1) * Vector3.forward) * targetDir;
        Debug.DrawRay(transform.position, targetDir * attackController.AtkRange, Color.blue);
        Debug.DrawRay(transform.position, leftDir * attackController.AtkRange, Color.blue);
        Debug.DrawRay(transform.position, rightDir * attackController.AtkRange, Color.blue);
        */
    }

    /// <summary>
    /// 개발자 : 서보운
    /// 보우용 웨폰 애니메이션 재생
    /// </summary>
    public void StartWeaponAnim()
    {
        if (weaponAnim == null)
        {
            return;
        }
        weaponAnim.SetTrigger("Attack");
    }
    public void WeaponAnim(string str)
    {
        if(weaponAnim == null)
        {
            return;
        }
        weaponAnim.SetTrigger(str);
    }
    public void EffectAnim(string str)
    {
        if(effectAnim == null)
        {
            return;
        }
        effectAnim.SetTrigger(str);
    }
    public void Attack()
    {
        attackController.CheckDamgeable();
    }

}