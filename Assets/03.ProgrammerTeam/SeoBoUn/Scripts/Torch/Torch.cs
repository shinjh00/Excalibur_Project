using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 개발자 : 서보운
/// <br/> 횃불 스크립트
/// <br/> Tap키를 눌러 켜고, 끄는 토글형 요소
/// <br/> 켜게 되면 시야 범위에 메리트가 있지만 디메리트 존재
/// </summary>
public class Torch : MonoBehaviour
{
    [SerializeField] TorchData curTorchData;
    [SerializeField] TorchData activeTorchData;
    [SerializeField] TorchData disableTorchData;

    public TorchData CurTorchData { get { return curTorchData; } }

    public Action TorchChange;

    private void Start()
    {
        // TODO.. active, disable 데이터 받아오기 -> 현재는 에디터 할당 중

        curTorchData.torchState = false;
        SetTorchData();
    }

    private void OnTorch(InputValue value)
    {   // 켜고 끌 땐 반대로 실행
        curTorchData.torchState = !curTorchData.torchState;

        SetTorchData();
    }

    /// <summary>
    /// 횃불 데이터 설정
    /// </summary>
    private void SetTorchData()
    {
        if (curTorchData.torchState)
        {   // 켜졌을 때
            curTorchData = activeTorchData;
        }
        else
        {   // 꺼졌을 때
            curTorchData = disableTorchData;
        }

        TorchChange?.Invoke();
    }

    /// <summary>
    /// 엑스칼리버 유저용 메소드. 항상 토치가 켜져 있어야 함.
    /// </summary>
    public void FixTorchData()
    {
        curTorchData = activeTorchData;
    }
}

[Serializable]
public struct TorchData
{
    public bool torchState;         // 켜짐, 꺼짐        
    public int fovAngle;            // 켜질 때의 시야각
    public int fovShortDis;         // 켜질 때의 짧은(내부) 거리
    public int fovLongDis;          // 켜질 때의 긴(외부) 거리
    public int brighteningRange;    // 밝아짐 범위
}