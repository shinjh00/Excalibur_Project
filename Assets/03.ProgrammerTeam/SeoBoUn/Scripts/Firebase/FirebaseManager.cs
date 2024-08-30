using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 파이어베이스 매니저 스크립트
/// </summary>
public class FirebaseManager : MonoBehaviour
{
    private static FirebaseManager instance;
    public static FirebaseManager Instance { get { return instance; } }

    // FirebaseApp 변수 -> Firebase 연동에 성공했을 때 Firebase의 어플리케이션을 가져올 수 있음
    private static FirebaseApp app;
    public static FirebaseApp App { get { return app; } }

    // FirebaseAuth -> 인증에 관련된 클래스
    private static FirebaseDatabase db;
    public static FirebaseDatabase DB { get { return db; } }

    [SerializeField] FirebaseLink link;
    public FirebaseLink Link { get { return link; } }

    [SerializeField] FirebaseUpLoadData upLoadData;
    public FirebaseUpLoadData UpLoadData { get { return upLoadData; } }

    [SerializeField] InventorySaveData invenData;
    [SerializeField] UploadStoreData storeData;
    [SerializeField] UploadStoreData downloadStoreData;
    [SerializeField] UploadStorageData storageData;
    [SerializeField] LoginIDs loginIDs;
    public LoginIDs LoginIDs { get { return loginIDs; } }
    public InventorySaveData InvenData { get { return invenData; } set { invenData = value; } }

    public string curPlayer;
    public ClassType curClass;
    public int curGold = 0;


    [SerializeField] string jsonData;

    public const int PLAYERDATATABLEID = 2000000;
    public const int PLAYERDATATABLEID_END = 10000000;


    private void Awake()
    {
        loginIDs = new LoginIDs();
        loginIDs.loginIDs = new List<string>();
        loginIDs.IDs = new List<string>();
        loginIDs.passwords = new List<string>();
        invenData = new InventorySaveData();

        CreateInstance();
        CheckFirebase();

    }

    private void CreateInstance()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void CheckFirebase()
    {   
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                app = FirebaseApp.DefaultInstance;
                db = FirebaseDatabase.DefaultInstance;

                // 성공시의 작업
                Debug.Log("Firebase check success");

                // PlayerDataTable temp = new PlayerDataTable();
                // upLoadData.DownLoadStructData<PlayerDataTable>(temp, table);

                GetLoginList();
                // StartCoroutine(InventoryUploadRoutine());
                DB.GetReference("LoginIds")
                    .ValueChanged += ChangeLoginList;
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.

                // 실패시의 작업
                app = null;
                db = null;
            }
        });
    }
    
    private void GetLoginList()
    {
        db.GetReference("LoginIds")
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if(task.IsCanceled)
                {
                    return;
                }
                else if(task.IsFaulted)
                {
                    return;
                }

                DataSnapshot snapShot = task.Result;

                if(snapShot.Exists)
                {
                    string jsonData = snapShot.GetRawJsonValue();

                    loginIDs = JsonUtility.FromJson<LoginIDs>(jsonData);
                }

            });
    }

    private void ChangeLoginList(object sendor, ValueChangedEventArgs args)
    {
        db.GetReference("LoginIds")
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
                    string jsonData = snapShot.GetRawJsonValue();

                    loginIDs = JsonUtility.FromJson<LoginIDs>(jsonData);
                }

            });
    }

    public void UpLoadLoginList()
    {
        string jsonData = JsonUtility.ToJson(loginIDs);

        db.GetReference("LoginIds")
            .SetRawJsonValueAsync(jsonData)
            .ContinueWithOnMainThread(task =>
            {
                if(task.IsCanceled)
                {
                    return;
                }
                else if(task.IsFaulted)
                {
                    return;
                }
            });
    }

    [ContextMenu("Upload")]
    public void UpLoadInventoryData()
    {
        Debug.Log("UpLoadInvenData");
        invenData = Inventory.Instance.GetUploadInventoryDatas();

        db.GetReference("PlayerDataTable")
            .Child(curPlayer)
            .Child("characterData")
            .Child(curClass.ToString())
            .Child("detailData")
            .Child("inventoryData")
            .RemoveValueAsync();

        string json = JsonUtility.ToJson(invenData);

        db.GetReference("PlayerDataTable")
            .Child(curPlayer)
            .Child("characterData")
            .Child(curClass.ToString())
            .Child("detailData")
            .Child("inventoryData")
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

    public void UpLoadStore_StorageData()
    {   // 창고와 상점 데이터 업로드
        storeData = Inventory.Instance.Store.GetUploadStoreDatas();
        storageData = Inventory.Instance.Storage.GetUploadStoreDatas();

        db.GetReference("PlayerDataTable")
            .Child(curPlayer)
            .Child("StorageData")
            .RemoveValueAsync();

       //db.GetReference("PlayerDataTable")
       //    .Child(curPlayer)
       //    .Child("StoreData")
       //    .RemoveValueAsync();

        string json = JsonUtility.ToJson(storeData);

       // db.GetReference("PlayerDataTable")
       //     .Child(curPlayer)
       //     .Child("StoreData")
       //     .SetRawJsonValueAsync(json)
       //     .ContinueWithOnMainThread(task =>
       //     {
       //         if (task.IsFaulted)
       //         {
       //             return;
       //         }
       //         else if (task.IsCanceled)
       //         {
       //             return;
       //         }
       //     });
       // 
        json = JsonUtility.ToJson(storageData);

        db.GetReference("PlayerDataTable")
            .Child(curPlayer)
            .Child("StorageData")
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


    [ContextMenu("Download")]
    public void DownLoadStore_StorageData()
    {
        db.GetReference("PlayerDataTable")
            .Child(curPlayer)
            .GetValueAsync()
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

                DataSnapshot snapShot = task.Result;

                if (snapShot.Exists)
                {
                    IDictionary goldData = (IDictionary)snapShot.Value;
                    curGold = Convert.ToInt32(goldData["havingGold"]);
                }
            });

        // db.GetReference("PlayerDataTable")
        //     .Child(curPlayer)
        //     .Child("StoreData")
        //     .GetValueAsync()
        //     .ContinueWithOnMainThread(task =>
        //     {
        //         if (task.IsFaulted)
        //         {
        //             return;
        //         }
        //         else if (task.IsCanceled)
        //         {
        //             return;
        //         }
        // 
        //         DataSnapshot snapShot = task.Result;
        // 
        //         if (snapShot.Exists)
        //         {
        //             jsonData = snapShot.GetRawJsonValue();
        //             downloadStoreData = JsonUtility.FromJson<UploadStoreData>(jsonData);
        //         }
        //     });

        db.GetReference("PlayerDataTable")
            .Child(curPlayer)
            .Child("StorageData")
            .GetValueAsync()
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

                DataSnapshot snapShot = task.Result;

                if (snapShot.Exists)
                {
                    jsonData = snapShot.GetRawJsonValue();
                }
            });
    }

    public void UploadCurGold()
    {
        db.GetReference("PlayerDataTable")
            .Child(curPlayer)
            .Child("havingGold")
            .SetValueAsync(curGold)
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

    private void OnDestroy()
    {
        UpLoadInventoryData();
    }
}

[Serializable]
public struct LoginIDs
{
    public List<string> loginIDs;
    public List<string> passwords;
    public List<string> IDs;
}
