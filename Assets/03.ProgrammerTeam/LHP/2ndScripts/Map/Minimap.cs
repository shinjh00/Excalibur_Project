using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자 : 이형필 / 게임 내 미니맵 오브젝트를 참조하기 위한 클래스
/// </summary>
public class Minimap : MonoBehaviour
{
    [SerializeField]public Image minimap;
    [SerializeField] public RawImage camImage;
}
