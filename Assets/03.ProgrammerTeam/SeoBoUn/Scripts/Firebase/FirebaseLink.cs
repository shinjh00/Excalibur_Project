using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// <br/>파이어 베이스 링크 연동용
/// </summary>
public class FirebaseLink : MonoBehaviour
{
    // 실시간 연동용 코드
    [SerializeField] PlayerStatData playerClassData;
    Dictionary<int, PlayerDataTable> playerDataTable = new Dictionary<int, PlayerDataTable>();
    
    // 현재 플레이 중인 몬스터 데이터ID? 
    [SerializeField] List<int> monsterID = new List<int>();
    [SerializeField] List<int> boxID = new List<int>();

    string nickname;

    public string Nickname { get { return nickname; } }

    public Dictionary<int, PlayerDataTable> PlayerData { get { return playerDataTable; } }

    /// <summary>
    /// 타켓 설정. 목표하는 대상의 데이터 전달 및 관찰
    /// </summary>
    /// <param name="target">목표하는 대상</param>
    public void InitData(PlayerStatData target, string name)
    {
        playerClassData = target;
        nickname = name;
        SetData(playerClassData);
    }

    /// <summary>
    /// 초기 데이터 세팅
    /// </summary>
    /// <param name="curData">타겟 데이터</param>
    private void SetData(PlayerStatData curData)
    {
        string json = JsonUtility.ToJson(curData);
        FirebaseManager.DB
            .GetReference("CurPlayerStatData")
            .Child(nickname)
            .SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    return;
                }
                else if (task.IsFaulted)
                {
                    return;
                }
            });
    }

    /// <summary>
    /// Hp 변동시 DB 연동함수
    /// Update에서 올리는 것만 피할 수 있도록(횟수의 문제)
    /// 부담된다면 DB변동에 대하여 생각해 볼 것.
    /// (특히 누군가가 hp를 회복하는 상황과, 동시에 공격하는 상황에서 DB가 꼬일 수 있음)
    /// 
    /// 특정 시간에 변경할 수 있도록 설정하면 좋음
    /// </summary>
    /// <param name="hp">변화한 hp</param>
    public void ChagneHp(float hp)
    {
        FirebaseManager.DB
            .GetReference("CurPlayerStatData")
            .Child(nickname)
            .Child("hp")
            .SetValueAsync(hp)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    return;
                }
                else if (task.IsFaulted)
                {
                    return;
                }
            });
    }

    /// <summary>
    /// 파이어 베이스 플레이 데이터 저장용 일반화 메소드(구조체)
    /// <br/> 구조체 데이터를 파이어 베이스에 플레이 데이터로 올리기 위한 메소드
    /// <br/> 반드시 IFireBaseReadable 인터페이스를 포함하고 있어야 함.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="structType"></param>
    /// <returns></returns>
    public int SetStructData<T>(T structType) where T : struct, IFireBaseReadable
    {
        PlayDataID curTarget = structType.FireBaseString().firebaseID;
        List<int> curList = FindType(curTarget);

        int curID = curList.Count;
        curList.Add(curID);

        string json = JsonUtility.ToJson(structType);
        FirebaseManager.DB
            .GetReference("PlayData")
            .Child(structType.FireBaseString().name)
            .Child(curID.ToString())
            .SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    return;
                }
                else if (task.IsFaulted)
                {
                    return;
                }
            });

        return curID;
    }

    private List<int> FindType(PlayDataID type)
    {
        switch(type)
        {
            case PlayDataID.Player:
                return null;
            case PlayDataID.Monster:
                return monsterID;
            case PlayDataID.Box:
                return boxID;
            case PlayDataID.Map:
                return null;
        }

        return null;
    }

    /// <summary>
    /// 몬스터 체력 변동 메소드
    /// </summary>
    /// <param name="hp"></param>
    /// <param name="id"></param>
    public void ChangeMonsterHp(float hp, int id)
    {   // 1. 몬스터 플레이 데이터
        // 2. ID 부여받기
        FirebaseManager.DB
            .GetReference("PlayData")
            .Child("MonsterData")
            .Child(id.ToString())
            .Child("hp")
            .SetValueAsync(hp)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    return;
                }
                else if (task.IsFaulted)
                {
                    return;
                }
            });
    }

    public void LoginUser(string curID)
    {
        FirebaseManager.DB.GetReference("PlayerDataTable")
            .Child(curID)
            .Child("lastLoginTime")
            .SetValueAsync(DateTime.Now.ToString())
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    return;
                }
                else if (task.IsFaulted)
                {
                    return;
                }
            });

        FirebaseManager.DB.GetReference("PlayerDataTable")
            .Child(curID)
            .Child("nickname")
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    return;
                }
                else if (task.IsFaulted)
                {
                    return;
                }

                DataSnapshot snapShot = task.Result;

                if(snapShot.Exists)
                {
                    nickname = JsonUtility.FromJson<string>(snapShot.GetRawJsonValue());
                }
            });
    }
}

/// <summary>
/// 파이어 베이스에 데이터를 올릴 때 구분용 ID(리스트 찾기용)
/// <br/> Player, Monster, Box, Map
/// </summary>
public enum PlayDataID
{
    Player,         // 0
    Monster,        // 1
    Box,            // 2
    Map,            // 3
    None
}