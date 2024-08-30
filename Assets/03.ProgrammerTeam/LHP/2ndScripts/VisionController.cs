using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
/// <summary>
/// 개발자 : 이형필 / 캐릭터 시야를 조절하는 클래스
/// </summary>
public class VisionController : MonoBehaviourPun
{
    [SerializeField] Light2D circleVision;
    [SerializeField] Light2D fovVision;
    [Tooltip("외부에서 횃불을 켰을 때 보여지는 빛")]
    [SerializeField] Light2D circleLight;
    [Tooltip("횃불 스크립트 할당 필요")]
    [SerializeField] Torch torch;

    ClassType curClass;

    private void Start()
    {
        circleVision.overlapOperation = Light2D.OverlapOperation.AlphaBlend;
        torch.TorchChange += ActiveTorch;

        if (photonView.IsMine)
        {
            circleLight.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 클래스 설정 메소드. 엑스칼리버 유저용
    /// </summary>
    /// <param name="curClass"></param>
    public void SetClass(ClassType curClass)
    {
        this.curClass = curClass;

        ActiveTorch();
    }

    /// <summary>
    /// 토치를 켰는지 확인하기 위한 메소드
    /// </summary>
    /// <returns></returns>
    public bool IsTorch()
    {
        return torch.CurTorchData.torchState;
    }

    public void CircleVision(bool b)
    {
        circleVision.gameObject.SetActive(b);
    }

    public void FovVision(bool b)
    {
        fovVision.gameObject.SetActive(b);
    }

    /// <summary>
    /// 횃불이 켜고 끌 때 반응할 메소드(사용한 유저는 시야각이 넓어지고, 다른 유저는 해당 유저를 볼 수 있음)
    /// </summary>
    private void ActiveTorch()
    {
        if(curClass == ClassType.Excalibur)
        {
            torch.FixTorchData();
        }

        if (photonView.IsMine)
        {
            fovVision.pointLightInnerRadius = torch.CurTorchData.fovShortDis;   // 내부 반지름(짧은 거리)
            fovVision.pointLightOuterRadius = torch.CurTorchData.fovLongDis;    // 외부 반지름(긴 거리)
            fovVision.pointLightInnerAngle = torch.CurTorchData.fovAngle;       // 시야 각도
            fovVision.pointLightOuterAngle = torch.CurTorchData.fovAngle;       // 시야 각도

            photonView.RPC("ActiveTorch_Sync", RpcTarget.Others, torch.CurTorchData.torchState);
        }
    }

    [PunRPC]
    private void ActiveTorch_Sync(bool isActive)
    {
        circleLight.gameObject.SetActive(isActive);
    }
}
