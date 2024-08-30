using Photon.Pun;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// <br/> 공용스킬 매복
/// <br/> 보물상자로 변신하여 체력을 회복
/// <br/> 변신하면 1초에 3씩 회복하며, 본인이 행동을 하거나 적 플레이어가 공격, 상호작용 시 변신이 해제
/// <br/> 변신은 무제한, 쿨타임 1분
/// </summary>
public class CommonSkill_Ambush : CommonSkillData
{
    [Tooltip("1초당 회복량")]
    public int healAmount;

    [Tooltip("보물상자 프리팹")]
    [SerializeField] AmbushBox ambushBoxPrefab;  // 생성할 때 사용할 프리팹

    AmbushBox activeAmbushBox;  // 생성된 박스를 저장할 필드

    public override void SetData()
    {
        skillInfo = CsvParser.Instance.SkillDic[id];

        skillInfo.ReadEffectData();
    }

    public override void Execute(PlayerController controller)
    {
        SoundManager.instance.PlaySFX(skillInfo.skEffect1_Data.mainSound, controller.audioSource);
        controller.StateController.StateChange(PlayerState.Metamorph, 0, 0, true, false);
        controller.SkillController.enabled = false;
        Ambush(controller);
    }

    /// <summary>
    /// 매복 루틴
    /// </summary>
    /// <param name="controller"></param>
    /// <returns></returns>
    public void Ambush(PlayerController controller)
    {
        Debug.Log(controller.name);

        if (controller.photonView.IsMine)
        {
            if (activeAmbushBox == null)
            {

                activeAmbushBox = PhotonNetwork.Instantiate("Prefabs/AmbushBox", controller.transform.position, Quaternion.identity).GetComponent<AmbushBox>();
                activeAmbushBox.photonView.RPC("Initialize", RpcTarget.All, controller.photonView.ViewID);
            }
            else
            {

                activeAmbushBox.gameObject.SetActive(true);
                activeAmbushBox.photonView.RPC("Active", RpcTarget.All);
            }
        }
        

    }
}
