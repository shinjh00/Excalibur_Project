using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운 
/// <br/>오브젝트를 그룹화하기 위한 메소드 
/// </summary>
public class ObjectGrouping : MonoBehaviour
{
    private static ObjectGrouping instance;

    [Tooltip("에디터상의 빈 오브젝트로 생성 후 할당 필요(위치는 0, 0, 0)")]
    [SerializeField] Transform monsterGroup;
    [Tooltip("에디터상의 빈 오브젝트로 생성 후 할당 필요(위치는 0, 0, 0)")]
    [SerializeField] Transform boxGroup;
    [Tooltip("에디터상의 빈 오브젝트로 생성 후 할당 필요(위치는 0, 0, 0)")]
    [SerializeField] Transform monsterSpawnerGroup;
    [Tooltip("에디터상의 빈 오브젝트로 생성 후 할당 필요(위치는 0, 0, 0)")]
    [SerializeField] Transform roomsGroup;
    [Tooltip("에디터상의 빈 오브젝트로 생성 후 할당 필요(위치는 0, 0, 0)")]
    [SerializeField] Transform trapGroup;

    public static ObjectGrouping Instance { get { return instance; } }
    public Transform MonsterGroup { get { return monsterGroup; } }
    public Transform BoxGroup { get { return boxGroup; } }
    public Transform MonsterSpawnerGroup { get { return monsterSpawnerGroup; } }
    public Transform RoomsGroup { get { return roomsGroup; } }
    public Transform TrapGroup { get { return trapGroup; } }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

}
