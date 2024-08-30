using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
///  개발자: 이예린/ 자기장 관련 기능 구현 클래스
/// </summary>
public class MagneticField : MonoBehaviour
{
    public GameObject owner;
    public Vector2 excaliburPos;

    [SerializeField] float magneticFieldRadius;
    [SerializeField] float currentRadius;
    [SerializeField] List<float> phaseRadius = new List<float>();
    [SerializeField] List<float> phaseParticleRadius = new List<float>();
    [SerializeField] List<float> phaseDamage = new List<float>();

    [SerializeField] LayerMask playerLayer;

    [SerializeField] Transform magneticPos;

    Collider2D[] damageTargets = new Collider2D[20];
    Collider2D[] safeAreaTargets = new Collider2D[20];
    List<PlayerController> damageableList = new List<PlayerController>();

    int curentPhase = 0;

    public bool getExcalibur;
    [SerializeField] float checkTime = 0;

    [Header("UI")]
    [SerializeField] TMP_Text magneticFieldMessage;
    [SerializeField] string waitMessage;
    [SerializeField] string decayMessage;

    public Coroutine safeZoneShrinkers;

    public int CurentPhase => curentPhase;
    public Transform MagneticPos => magneticPos;

    #region Unity Event
    IEnumerator Start()
    {
        yield return new WaitUntil(() => GameManager.Ins != null);

        GameManager.Ins.magneticField = this;
        currentRadius = 400f;
        magneticPos.localScale = new Vector2(161f, 161f);
        excaliburPos = Vector2.zero;
        StartSafeZoneShrinkers();
    }

    private void Update()
    {
        if (owner != null)
        {
            magneticPos.position = owner.transform.position;
        }
    }
    #endregion

    /// <summary>
    /// 자기장 관련 기능 수행하는 코루틴 실행하는 메소드
    /// </summary>
    public void StartSafeZoneShrinkers()
    {
        if (safeZoneShrinkers != null)
        {
            StopCoroutine(safeZoneShrinkers);
        }

        safeZoneShrinkers = StartCoroutine(SafeZoneShrinkers());
    }

    /// <summary>
    /// 자기장 안전지역 크기 페이즈에 맞춰 줄여주는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator SafeZoneShrinkers()
    {
        if (curentPhase == 0)
        {
            int time = 0;
            float radiusShrinkAmount;
            float particleShrinkAmount;

            magneticFieldMessage.text = waitMessage;
            if (!getExcalibur)
            {
                while (checkTime < 300f)
                {
                    yield return new WaitForSeconds(1f);
                    inflictMagneticDamage();
                    checkTime++;
                }
                radiusShrinkAmount = (currentRadius - phaseRadius[curentPhase]) / 60f;
                particleShrinkAmount = (magneticPos.localScale.x - phaseParticleRadius[curentPhase]) / 60f;
            }
            else
            {
                radiusShrinkAmount = (currentRadius - phaseRadius[curentPhase]) / (60f + 300f - checkTime);
                particleShrinkAmount = (magneticPos.localScale.x - phaseParticleRadius[curentPhase]) / (60f + 300f - checkTime);
            }

            magneticFieldMessage.text = decayMessage;
            while (time < 60 + (300f - checkTime))
            {
                if (time < 3)
                {
                    magneticFieldMessage.color = Color.red;
                }
                yield return new WaitForSeconds(1f);
                if (owner != null)
                {
                    inflictMagneticDamage(owner.transform.position);
                }
                else
                {
                    inflictMagneticDamage();
                }
                if (time < 3)
                {
                    magneticFieldMessage.color = Color.white;
                }
                time++;
                currentRadius -= radiusShrinkAmount;
                magneticPos.localScale -= new Vector3(particleShrinkAmount, particleShrinkAmount);
            }
            curentPhase++;
        }
        else
        {
            checkTime = 0;

            magneticFieldMessage.text = waitMessage;
            while (checkTime < 90f)
            {
                yield return new WaitForSeconds(1f);
                if (owner != null)
                {
                    inflictMagneticDamage(owner.transform.position);
                }
                else
                {
                    inflictMagneticDamage();
                }
                checkTime++;
            }

            int time = 0;
            float colliderShrinkAmount = (currentRadius - phaseRadius[curentPhase]) / 45f;
            float particleShrinkAmount = (magneticPos.localScale.x - phaseParticleRadius[curentPhase]) / 45f;
            magneticFieldMessage.text = decayMessage;
            while (time < 45)
            {
                if (time < 3)
                {
                    magneticFieldMessage.color = Color.red;
                }
                yield return new WaitForSeconds(1f);
                if (owner != null)
                {
                    inflictMagneticDamage(owner.transform.position);
                }
                else
                {
                    inflictMagneticDamage();
                }
                if (time < 3)
                {
                    magneticFieldMessage.color = Color.white;
                }

                time++;
                currentRadius -= colliderShrinkAmount;
                magneticPos.localScale -= new Vector3(particleShrinkAmount, particleShrinkAmount);
            }
            curentPhase++;
        }

        if (curentPhase != 5)
        {
            safeZoneShrinkers = StartCoroutine(SafeZoneShrinkers());
            magneticFieldMessage.gameObject.SetActive(false);
        }
    }

    private void inflictMagneticDamage(Vector2 ownerPos)
    {
        int magneticFieldCount = Physics2D.OverlapCircleNonAlloc(ownerPos, magneticFieldRadius, damageTargets, playerLayer);
        int safeFieldCount = Physics2D.OverlapCircleNonAlloc(ownerPos, currentRadius, safeAreaTargets, playerLayer);

        for (int i = 0; i < magneticFieldCount; i++)
        {
            if (owner == null)
            {
                return;
            }

            if (damageTargets[i].gameObject == owner)
            {
                continue;
            }
                
            PlayerController playerController = damageTargets[i].GetComponent<PlayerController>();
            
            if (playerController != null)
            {
                bool checkSafeArea = false;
                for (int j = 0; j < safeFieldCount; j++)
                {
                    if (safeAreaTargets[j].gameObject.Equals(damageTargets[i].gameObject))
                    {
                        checkSafeArea = true;
                        break;
                    }
                }

                if (!checkSafeArea)
                {
                    damageableList.Add(playerController);
                }
            }
        }

        for (int i = 0; i < damageableList.Count; i++)
        {
            if (damageableList[i].HealthController.Hp == 0)
            {
                continue;
            }
            float damage = damageableList[i].HealthController.MaxHp * phaseDamage[curentPhase];
            damageableList[i].HealthController.Hp -= damage;
            damageableList[i].HealthController.photonView.RPC("StartSetDamageColorRed", RpcTarget.All);
            damageableList[i].HealthController.photonView.RPC("FloatingDamageRPC", RpcTarget.All, damage);

            if (damageableList[i].HealthController.Hp <= 0)
            {
                damageableList[i].HealthController.Hp = 0;
                damageableList[i].HealthController.photonView.RPC("PlayDieEvent", RpcTarget.All);
            }
        }

        damageableList.Clear();
    }

    private void inflictMagneticDamage()
    {
        int magneticFieldCount = Physics2D.OverlapCircleNonAlloc(excaliburPos, magneticFieldRadius, damageTargets, playerLayer);
        int safeFieldCount = Physics2D.OverlapCircleNonAlloc(excaliburPos, currentRadius, safeAreaTargets, playerLayer);

        for (int i = 0; i < magneticFieldCount; i++)
        {
            PlayerController playerController = damageTargets[i].GetComponent<PlayerController>();

            if (playerController != null)
            {
                bool checkSafeArea = false;
                for (int j = 0; j < safeFieldCount; j++)
                {
                    if (safeAreaTargets[j].gameObject.Equals(damageTargets[i].gameObject))
                    {
                        checkSafeArea = true;
                        break;
                    }
                }

                if (!checkSafeArea)
                {
                    damageableList.Add(playerController);
                }
            }
        }

        for (int i = 0; i < damageableList.Count; i++)
        {
            if (damageableList[i].HealthController.Hp == 0)
            {
                continue;
            }
            float damage = damageableList[i].HealthController.MaxHp * phaseDamage[curentPhase];
            damageableList[i].HealthController.Hp -= damage;
            damageableList[i].HealthController.photonView.RPC("StartSetDamageColorRed", RpcTarget.All);
            damageableList[i].HealthController.photonView.RPC("FloatingDamageRPC", RpcTarget.All, damage);

            if (damageableList[i].HealthController.Hp <= 0)
            {
                damageableList[i].HealthController.Hp = 0;
                damageableList[i].HealthController.photonView.RPC("PlayDieEvent", RpcTarget.All);
            }
        }

        damageableList.Clear();
    }

    IEnumerator MessageColorRed()
    {
        int count = 0;
        while(count++ < 3)
        {
            magneticFieldMessage.color = Color.red;
            yield return new WaitForSeconds(1f);
            magneticFieldMessage.color = Color.white;
        }
    }

    private void OnDrawGizmos()
    {
        Vector2 pos;
        if (owner != null)
        {
            pos = owner.transform.position;
        }
        else
        {
            pos = excaliburPos;
        }
        Gizmos.color = Color.blue;  // Gizmos의 색상을 블루로 설정
        Gizmos.DrawSphere(pos, currentRadius);

        Gizmos.color = Color.magenta;  // Gizmos의 색상을 마젠타로 설정
        Gizmos.DrawWireSphere(pos, magneticFieldRadius);
    }
}