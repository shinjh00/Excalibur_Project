using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle_3 : Puzzle, IPunObservable
{
    [SerializeField] float timer = 3f;
    [Range(0, 1)][SerializeField] float fovRotateSpeed = 0.3f;
    Coroutine coroutine;
    Coroutine timerRoutine;
    Animator anim;
    SpriteRenderer spriteRenderer;
    public bool isDeactivated = false;
    bool isLookingFront = false;

    bool active;
    GolemEDir currentDir;
    GolemEDir front;

    Vector3 targetAng;
    Coroutine currentLerpCoroutine;
    [SerializeField] PuzzleGolemFov golemFov;
    [SerializeField] GameObject puzzleLight;
    [SerializeField] public Room room;
    [Range(0, 100)] public float damageMaxPer = 5;

    public Transform damagedTarget;
    [Tooltip("Interactor 할당 필요")]
    public PuzzleInteractor puzzleInteractor;
   [SerializeField] GameObject box;

    protected override void Start()
    {
        base.Start();

        
        
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (photonView.IsMine)
        {
            room = Physics2D.OverlapBox(transform.position, Vector2.one, 0, LayerMask.GetMask("Room")).GetComponent<Room>();
            currentDir = (GolemEDir)Random.Range(0, 4); // 0 : 상 ,1 : 우 , 2 : 하 , 3 : 좌
        }

        front = currentDir;
        UpdateAnimatorDirection(currentDir);
        puzzleInteractor.Init(this);
        golemFov.puzzle = this;
        golemFov.Init();
    }

    IEnumerator PuzzleRoutine()
    {

        while (!isDeactivated)
        {

            int pattern = 0;

            if (photonView.IsMine)
            {
                pattern = Random.Range(0, 3); // 0: 좌, 1: 우, 2: 정면
                if (isLookingFront)
                {
                    pattern = 3; // 이전 패턴이 정면이면 뒤를 봄
                    isLookingFront = false;
                }
                photonView.RPC("RotateGolem", RpcTarget.All, pattern);
            }
            yield return new WaitForSeconds(timer);

            Collider2D[] cols = Physics2D.OverlapBoxAll(transform.position, new Vector2(20, 20), 0, playerLayer);
            if (cols.Length == 0)
            {
                if (coroutine != null)
                {
                    Debug.Log("Stop");
                    StopCoroutine(coroutine);
                }
            }

        }
    }

    [PunRPC]
    void RotateGolem(int pattern)
    {
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
        }
        timerRoutine = StartCoroutine(TimerRoutine());

        switch (pattern)
        {
            case 0:
                currentDir = Direction.GetLeftDirection(front);
                break;
            case 1:
                currentDir = Direction.GetRightDirection(front);
                break;
            case 2:
                // 정면을 바라보고, 다음엔 반대 방향을 바라보도록 함
                currentDir = Direction.GetFrontDirection(front);
                isLookingFront = true;
                anim.SetBool("Back", true);
                break; ;
            case 3:
                currentDir = Direction.GetBackDirection(front);
                anim.SetBool("Back", false);
                break;
        }
        UpdateVisionDirection();
        UpdateAnimatorDirection(currentDir);
        front = currentDir;

    }
    IEnumerator TimerRoutine()
    {
        yield return new WaitForSeconds(3f);
        active = false;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isDeactivated)
            return;
        if (playerLayer.Contain(collision.gameObject.layer) && !active)
        {
            if (coroutine != null)
            {

                StopCoroutine(coroutine);

            }
            Debug.Log("puzzleActive");
            active = true;
            coroutine = StartCoroutine(PuzzleRoutine());
        }
    }
    [PunRPC]
    public void ClearGolem()
    {
        if (!isDeactivated)
        {
            SoundManager.instance.PlaySFX(1650067, audioSource);
            isDeactivated = true;
            StartCoroutine(DeactivateRoutine());
        }
    }

    IEnumerator DeactivateRoutine()
    {
        float duration = 2f; // 골렘이 어두워지는 시간
        float elapsedTime = 0f;
        Color initialColor = spriteRenderer.color;
        Color targetColor = Color.black;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(initialColor, targetColor, elapsedTime / duration);
            yield return null;
        }

        spriteRenderer.color = targetColor;

        box.SetActive(true);

        Destroy(golemFov.gameObject); // 기존 골렘 객체 파괴
        Destroy(puzzleLight);


    }
    void UpdateAnimatorDirection(GolemEDir direction)
    {
        SoundManager.instance.PlaySFX(1650083,audioSource);
        anim.SetInteger("Direction", (int)direction);
    }


    void UpdateVisionDirection()
    {
        switch (currentDir)
        {
            case GolemEDir.UP:
                targetAng = new Vector3(0, 0, 0);
                break;
            case GolemEDir.RIGHT:
                targetAng = new Vector3(0, 0, 270);
                break;
            case GolemEDir.DOWN:
                targetAng = new Vector3(0, 0, 180);
                break;
            case GolemEDir.LEFT:
                targetAng = new Vector3(0, 0, 90);
                break;
        }


        if (currentLerpCoroutine != null)
        {
            StopCoroutine(currentLerpCoroutine);
        }
        currentLerpCoroutine = StartCoroutine(LerpFov(targetAng, timer * fovRotateSpeed));
    }

    IEnumerator LerpFov(Vector3 target, float d)
    {
        Vector3 init = golemFov.transform.eulerAngles;
        float t = 0f;

        while (t < d)
        {
            golemFov.transform.eulerAngles = Vector3.Lerp(init, target, t / d);
            t += Time.deltaTime;
            yield return null;
        }

        // Ensure the final rotation is set to the target rotation
        golemFov.transform.eulerAngles = target;

    }



    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(front);
        }
        else
        {
            front = (GolemEDir)stream.ReceiveNext();
        }
    }
    [PunRPC]
    public void Teleport(Vector2 hallwayPos)
    {

        if (damagedTarget != null)
        {
            PlayerHealthController h = damagedTarget.GetComponent<PlayerHealthController>();
            h.knockBackCancle();
            damagedTarget.position = hallwayPos;
            IDamageable targetable = damagedTarget.GetComponent<IDamageable>();
            IStateable stateable = damagedTarget.GetComponent<IStateable>();
            targetable.TakeDamage(h.MaxHp * (damageMaxPer * 0.01f), DamageType.Fixed);
            stateable.StateChange(PlayerState.Knockback, 0.5f, 0.3f, true, false);
            damagedTarget = null;
        }
    }
    [PunRPC]
    public void GetHallWayPos(Vector2 pos)
    {
        golemFov.hallwayPos = pos;

    }
}


   





