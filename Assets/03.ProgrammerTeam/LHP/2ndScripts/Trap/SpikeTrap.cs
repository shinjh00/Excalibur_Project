using Photon.Pun;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필 / 가시함정을 구현하는 클래스
/// </summary>
public class SpikeTrap : Trap
{

    [SerializeField] float knockBackSpeed = 1f;

    [PunRPC]
    protected override void Init()
    {
        base.Start();
   //     range = CsvParser.Instance.TrapDic[1202000].range;
        BoxCollider2D thisCol = GetComponent<BoxCollider2D>();
        thisCol.size = new Vector2(range, range);
    }
    protected override void Activate(bool isActivated)
    {




    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (playerLayer.Contain(collision.gameObject.layer))
        {
            targetPlayer = collision.GetComponent<PlayerController>();
            if (targetPlayer!=null && targetPlayer.photonView.IsMine)
            {
               
                    IDamageable dmgable = targetPlayer.GetComponent<IDamageable>();
                    IStateable stateable = targetPlayer.GetComponent<IStateable>();
                    PlayerHealthController PHC = targetPlayer.GetComponent<PlayerHealthController>();
                    float dmg = PHC.MaxHp * damage;
                    if (dmgable != null)
                    {
                        dmgable.TakeDamage(dmg, DamageType.Fixed);
                        dmgable.TakeKnockBack(transform.position, knockBack, knockBackSpeed);
                    }
                    if (stateable != null)
                    {
                        stateable.StateChange(PlayerState.Knockback, 0.5f, 0.3f, true, false);
                    }

                

            }
            targetPlayer = null;
        }
    }
  /*  public override void CreateTrap(Vector2 pos)
    {
        Collider2D[] colliders;
        Vector2 changedPos = pos;
        int r = Random.Range(randomMin, randomMax + 1);
        for (int i = 1; i < r; i++)
        {
            int inf = 0;
            do
            {
                inf++;

                EDir dir = (EDir)Random.Range(0, 5);
                switch (dir)
                {
                    case EDir.UP:
                        changedPos += new Vector2(0, 1);
                        break;
                    case EDir.DOWN:
                        changedPos += new Vector2(0, -1);
                        break;
                    case EDir.LEFT:
                        changedPos += new Vector2(1, 0);
                        break;
                    case EDir.RIGHT:
                        changedPos += new Vector2(-1, 0);
                        break;
                }
                colliders = Physics2D.OverlapCircleAll(changedPos, 0.1f, trapLayer | wall);
            }
            while (colliders.Length != 0 && inf < 8);
            PhotonNetwork.InstantiateRoomObject("6.Prefab/Trap/SpikeTrap", changedPos, Quaternion.identity);



        }

    }*/

    [PunRPC]
    public override void SetActive(bool active, int viewID )
    {
            
        if (active)
        {
            if (viewID != -1 || viewID != 0)
            {
                targetPlayer = GameManager.Ins.FindPlayer(viewID);
                GameManager.Ins.Message($"Find : {viewID}");
            }
        }
        IsActivate = active;
    }
    [PunRPC]
    void SetTrap(int trapID)
    {
        range = CsvParser.Instance.TrapDic[trapID].range;
        coolTime = CsvParser.Instance.TrapDic[trapID].coolTime;
        damage = CsvParser.Instance.TrapDic[trapID].perDamage;
        knockBack = CsvParser.Instance.TrapDic[trapID].knockBackDis;
        activeSound = CsvParser.Instance.TrapDic[trapID].trapSound;
        Init();
    }

}
