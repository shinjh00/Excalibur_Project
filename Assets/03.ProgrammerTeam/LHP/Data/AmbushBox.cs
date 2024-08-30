using Photon.Pun;
using System.Collections;
using UnityEngine;

public class AmbushBox : MonoBehaviourPun, IPunObservable, IInteractable
{
    public PlayerController player;
    Rigidbody2D rb;
    [SerializeField] LayerMask playerLayer;
    [Tooltip("KeyImage 할당 필요")]
    [SerializeField] SpriteRenderer keyImageRender;
    bool isActive = false;
    Coroutine healRoutine;
    AmbushBoxHitPoint hitPoint;
    [SerializeField] SpriteRenderer sr;
    [SerializeField] Material ownerMat;

    private void Start()
    {
        playerLayer = LayerMask.GetMask("Player");
        sr = GetComponent<SpriteRenderer>();    
        hitPoint = GetComponentInChildren<AmbushBoxHitPoint>();
        hitPoint.owner = this;
    }

    public bool IsActive
    {
        get { return isActive; }
        set
        {
            isActive = value;
            UnAmbush(player, value);
        }
    }
    public void Unmetamorph()
    {
        photonView.RPC("UnmetamorphRPC", RpcTarget.All);
    }
    [PunRPC]
    public void Initialize(int playerViewID)
    {
        player = PhotonView.Find(playerViewID).GetComponent<PlayerController>();
        rb = player.GetComponent<Rigidbody2D>();
        IsActive = true;
        if (player.photonView.IsMine)
        {
            sr.material = ownerMat;
        }
    }
    [PunRPC]
    void UnmetamorphRPC()
    {
        IsActive = false;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(IsActive);
        }
        else
        {
            IsActive = (bool)stream.ReceiveNext();
        }
    }
    [PunRPC]
    public void Active()
    {
        gameObject.SetActive(true);
        transform.position = player.transform.position;
        IsActive = true;
        Debug.Log($"view ID : {photonView.ViewID} player : {player}");
    }
    public void UnAmbush(PlayerController controller, bool set)
    {
        if (set)
        {
            controller.MoveController.MovingStop();
            rb.bodyType = RigidbodyType2D.Kinematic;
            controller.StateController.PlayerSpriteColorChange(Color.clear);
            if (!controller.photonView.IsMine)
            {
                controller.StateController.BorderSetter(false);
            }

            controller.inputKey += Unmetamorph;
            if (controller.photonView.IsMine)
            {
                if (healRoutine != null)
                {
                    StopCoroutine(healRoutine);
                }
                healRoutine = StartCoroutine(HealRoutine());

            }
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            if (!controller.photonView.IsMine)
            {
                controller.StateController.BorderSetter(true);
            }
            controller.StateController.PlayerSpriteColorChange(Color.white);
            controller.StateController.StateChange(PlayerState.Metamorph, 0, 0, false, false);
            controller.SkillController.enabled = true;
            controller.inputKey -= Unmetamorph;
            controller.HealthController.OnChangeHp(controller.HealthController.Hp);
            if (controller.photonView.IsMine)
            {
                if (healRoutine != null)
                {
                    StopCoroutine(healRoutine);
                }
            }
            gameObject.SetActive(false);
        }

    }
    IEnumerator HealRoutine()
    {
        while (true)
        {
            Debug.Log("Heal up");
            player.HealthController.Hp += 3;
            yield return new WaitForSeconds(1);
        }

    }
    public void Interact(PlayerController player)
    {
        player.StateController.StateChange(PlayerState.Interact, 0, 0, false, false);
        Unmetamorph();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (playerLayer.Contain(collision.gameObject.layer))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player == this.player)
            {
                return;
            }
            if (player != null && player.photonView.IsMine)
            {
                keyImageRender.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (playerLayer.Contain(collision.gameObject.layer))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null && player.photonView.IsMine)
            {
                keyImageRender.gameObject.SetActive(false);
            }
        }
    }
}
