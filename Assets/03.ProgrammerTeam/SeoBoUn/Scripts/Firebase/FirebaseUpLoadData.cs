using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 파이어베이스에 데이터를 업로드하기 위한 스크립트
/// </summary>
public class FirebaseUpLoadData : MonoBehaviour
{
    [SerializeField] string jsonData;

    string[] jsonDatas;
    [SerializeField] List<string> sbDatas; 
    [SerializeField] int id;
    
    //[SerializeField] MonsterData monsterData;
    [SerializeField] List<MonsterData> monsterList;

    const int PLAYERID = 100000;
    const int MONSTERID = 300000;
    const int BOXID = 400000;
    const int BOXDROPTABLEID = 403000;

    /// <summary>
    /// 사용 예시(몬스터 데이터 읽어오기)
    /// </summary>
    [ContextMenu("ReadMonsterData")]
    public void DownLoadMonsterData()
    {
        // 임시로 사용할 구조체 타입 만들어 두고,
        MonsterData temp = new MonsterData();
        // 구조체, 데이터를 넣을 리스트를 넣어주면 저장 완.
        DownLoadStructData(temp, monsterList);
    }

    /// <summary>
    /// 파이어 베이스 일반화 메소드(파이어 베이스에 게임 데이터 구조체 데이터 올리기)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="structType"></param>
    public void UpLoadStructData<T>(T structType) where T : struct, IFireBaseReadable
    {
        FirebaseReadData readData = structType.FireBaseString();

        string json = JsonUtility.ToJson(structType);
        FirebaseManager.DB
            .GetReference(readData.name)
            .Child($"{readData.id}")
            .SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    return;
                }
                else if (task.IsCanceled)
                {
                    return;
                }
            });
    }

    /// <summary>
    /// 파이어 베이스 일반화 메소드(파이어 베이스에 있는 구조체 값 읽어오기)
    /// <br/> 단, 마지막 데이터가 문자열인 경우에 가능
    /// </summary>
    /// <typeparam name="T">구조체(단, 인터페이스 IFireBaseReadble) 필수</typeparam>
    /// <param name="structType"></param>
    /// <param name="targetList">여기에 담을 예정</param>
    public void DownLoadStructData_String<T>(T structType, List<T> targetList) where T : struct, IFireBaseReadable
    {
        FirebaseReadData readData = structType.FireBaseString();
        sbDatas = new List<string>();
        FirebaseManager.DB
            .GetReference(readData.name)
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

                if (snapShot.Exists)
                {
                    jsonData = snapShot.GetRawJsonValue();
                    // 1. {"300001" : {jsonData}, 
                    // }, 로 분리. { 앞에 있는 내용 전부 제거 jsonData 저장
                    jsonDatas = jsonData.Split("},");

                    StringBuilder stringBuilder = new StringBuilder();
                    for (int i = 0; i < jsonDatas.Length; i++)
                    {
                        stringBuilder.Clear();
                        int index = jsonDatas[i].IndexOf("{", 2);
                        stringBuilder.Append(jsonDatas[i].Substring(index, jsonDatas[i].Length - index - 1));
                        stringBuilder.Append('"');
                        if (i != jsonDatas.Length - 1)
                        {
                            stringBuilder.Append("}");
                        }
                        else
                        {
                            stringBuilder.Remove(stringBuilder.Length - 1, 1);
                        }

                        sbDatas.Add(stringBuilder.ToString());

                        T structData = JsonUtility.FromJson<T>(sbDatas[i]);
                        targetList.Add(structData);
                    }
                }
            });
    }
    /// <summary>
    /// 파이어 베이스 일반화 메소드(파이어 베이스에 있는 구조체 값 읽어오기)
    /// </summary>
    /// <typeparam name="T">구조체(단, 인터페이스 IFireBaseReadble) 필수</typeparam>
    /// <param name="structType"></param>
    /// <param name="targetList">여기에 담을 예정</param>
    public void DownLoadStructData<T>(T structType, List<T> targetList) where T : struct, IFireBaseReadable
    {
        T structData = new T();
        FirebaseReadData readData = structType.FireBaseString();
        sbDatas = new List<string>();
        FirebaseManager.DB
            .GetReference(readData.name)
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

                if (snapShot.Exists)
                {
                    jsonData = snapShot.GetRawJsonValue();
                    // 1. {"300001" : {jsonData}, 
                    // }, 로 분리. { 앞에 있는 내용 전부 제거 jsonData 저장
                    jsonDatas = jsonData.Split("},");

                    StringBuilder stringBuilder = new StringBuilder();

                    for (int i = 0; i < jsonDatas.Length; i++)
                    {
                        stringBuilder.Clear();
                        int index = jsonDatas[i].IndexOf("{", 2);
                        
                        stringBuilder.Append(jsonDatas[i].Substring(index, jsonDatas[i].Length - index));
                        if (i != jsonDatas.Length - 1)
                        {
                            stringBuilder.Append("}");
                        }
                        else
                        {
                            stringBuilder.Remove(stringBuilder.Length - 1, 1);
                        }

                        sbDatas.Add(stringBuilder.ToString());

                        structData = JsonUtility.FromJson<T>(sbDatas[i]);

                        targetList.Add(structData);
                    }
                }
            });
    }

    public void FirstUpLoadData(int id)
    {   // 처음 생성하는 유저 인벤토리 업로드 테스트 용도
        InventorySaveData temp = new InventorySaveData();
       // temp.itemID = new List<int>(); temp.itemID.Add(1);
       // temp.posX = new List<int>(); temp.posX.Add(1);
       // temp.posY = new List<int>(); temp.posY.Add(1);
       // temp.slot = new List<WearablesType>(); temp.slot.Add(WearablesType.Null);
       // temp.quickSlot_Number = new List<int>(); temp.quickSlot_Number.Add(1);

        for (int i = 0; i < (int)ClassType.Excalibur; i++)
        {
            string json = JsonUtility.ToJson(temp);

            FirebaseManager.DB
                .GetReference("PlayerDataTable")
                .Child(id.ToString())
                .Child("characterData")
                .Child(((ClassType)i).ToString())
                .Child("detailData")
                .Child("inventoryData")
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

        // for(int i = 0; i < (int)ClassType.Excalibur; i++)
        // {
        //     FirebaseManager.DB
        //         .GetReference("PlayerDataTable")
        //         .Child(id.ToString())
        //         .Child("characterData")
        //         .Child(((ClassType)i).ToString())
        //         .Child("detailData")
        //         .Child("inventoryData")
        //         .RemoveValueAsync();
        // }
    }
}
