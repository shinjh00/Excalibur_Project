using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 키 입력에 대한 정보를 가지고있고, 특정한 기능에 대응하는 키를 관리하는 매니저 클래스
/// </summary>
public class KeyManager : MonoBehaviour
{
    /*private Dictionary<UserAction, KeyCode> _bindingDict;
    public Dictionary<UserAction, KeyCode> BindingKey => _bindingDict;


    // 생성자
    public KeyManager(bool initalize = true)
    {
        _bindingDict = new Dictionary<UserAction, KeyCode>();

        if (initalize)
        {
            ResetAll();
        }
    }

    // 새로운 바인딩 적용
    public void ApplyNewBindings(InputBinding newBinding)
    {
        _bindingDict = new Dictionary<UserAction, KeyCode>(newBinding._bindingDict);
    }

    // 바인딩 지정 메소드 : allowOverlap 매개변수를 통해 중복 바인딩 허용여부를 결정한다.
    public void Bind(in UserAction action, in KeyCode code, bool allowOverlap = false)
    {
        if (!allowOverlap && _bindingDict.ContainsValue(code))
        {
            var copy = new Dictionary<UserAction, KeyCode>(_bindingDict);

            foreach (var pair in copy)
            {
                if (pair.Value.Equals(code))
                {
                    _bindingDict[pair.Key] = KeyCode.None;
                }
            }
        }
        _bindingDict[action] = code;
    }

    // 초기 바인딩셋 지정 메소드
    public void ResetAll()
    {
        Bind(UserAction.Attack, KeyCode.Mouse0);

        Bind(UserAction.MoveForward, KeyCode.W);
        Bind(UserAction.MoveBackward, KeyCode.S);
        Bind(UserAction.MoveLeft, KeyCode.A);
        Bind(UserAction.MoveRight, KeyCode.D);

        Bind(UserAction.Run, KeyCode.LeftControl);
        Bind(UserAction.Jump, KeyCode.Space);

        Bind(UserAction.UI_Inventory, KeyCode.I);
        Bind(UserAction.UI_Status, KeyCode.P);
        Bind(UserAction.UI_Skill, KeyCode.K);
    }*/


    /*private static string mOptionDataFileName = "/KeyData.json"; //키 데이터 파일 이름
    private static string mFilePath;

    private Dictionary<string, KeyCode> mKeyDictionary;

    void Awake()
    {
        mKeyDictionary = new Dictionary<string, KeyCode>();
        mFilePath = Application.persistentDataPath + mOptionDataFileName;

        LoadOptionData();
    }

    private void LoadOptionData()
    {
        // 저장된 게임이 있다면
        if (File.Exists(mFilePath))
        {
            string fromJsonData = File.ReadAllText(mFilePath);

            List<KeyData> keyList = JsonConvert.DeserializeObject<List<KeyData>>(fromJsonData);

            foreach (var data in keyList)
            {
                mKeyDictionary.Add(data.keyName, data.keyCode);
            }
        }

        // 저장된 게임이 없다면
        else
        {
            Debug.Log(GetType() + " 파일이 없음");

            ResetOptionData();
        }
    }

    /// <summary>
    /// 프로젝트마다 별도로 해당 게임의 컨셉에 맞게 키를 설정한다.
    /// 스크립트에서 지정한 키로 재설정된다.
    /// </summary>
    private void ResetOptionData()
    {
        mKeyDictionary.Clear();

        //씬 내에서 사용할 키 데이터들//
        mKeyDictionary.Add("Inventory", KeyCode.I); //아이템 인벤토리
        mKeyDictionary.Add("Equipment", KeyCode.O); //장비 인벤토리
        mKeyDictionary.Add("Stat", KeyCode.P); //스탯
        mKeyDictionary.Add("Skill", KeyCode.K); //스킬
        mKeyDictionary.Add("Quest", KeyCode.Q); //퀘스트

        mKeyDictionary.Add("ItemQuickSlot0", KeyCode.Alpha1); //아이템 퀵슬롯 1번
        mKeyDictionary.Add("ItemQuickSlot1", KeyCode.Alpha2); //아이템 퀵슬롯 2번
        mKeyDictionary.Add("ItemQuickSlot2", KeyCode.Alpha3); //아이템 퀵슬롯 3번
        mKeyDictionary.Add("ItemQuickSlot3", KeyCode.Alpha4); //아이템 퀵슬롯 4번
        mKeyDictionary.Add("ItemQuickSlot4", KeyCode.Alpha5); //아이템 퀵슬롯 5번

        mKeyDictionary.Add("SkillQuickSlot0", KeyCode.Z); //스킬 퀵슬롯 1번
        mKeyDictionary.Add("SkillQuickSlot1", KeyCode.X); //스킬 퀵슬롯 2번
        mKeyDictionary.Add("SkillQuickSlot2", KeyCode.C); //스킬 퀵슬롯 3번
        mKeyDictionary.Add("SkillQuickSlot3", KeyCode.V); //스킬 퀵슬롯 4번
        mKeyDictionary.Add("SkillQuickSlot4", KeyCode.B); //스킬 퀵슬롯 5번  

        Debug.Log(GetType() + " 초기화");

        SaveOptionData();
    }

    public void SaveOptionData()
    {
        //딕셔너리에 있는 키 데이터들을 오브젝트 리스트를 이용하여 태그를 만들어서 직렬화시킨다.
        //리스트를 사용하지 않고 딕셔너리만 직렬화하면 태그가 없기에 사용할 수 없다. 오브젝트 형태(KeyData)로 만들고, Object type의 json 파일로 만들었다.
        //https://www.geeksforgeeks.org/json-data-types/#:~:text=JSON%20(JavaScript%20Object%20Notation)%20is,easy%20to%20understand%20and%20generate.

        //KeyData를 오브젝트로 담을 리스트
        List<KeyData> keys = new List<KeyData>();

        //모든 딕셔너리에 있는 키 값을 리스트에 넣어준다.
        foreach (KeyValuePair<string, KeyCode> keyName in mKeyDictionary)
        {
            keys.Add(new KeyData(keyName.Key, keyName.Value));
        }

        //List<KeyData>를 SeriaizeObject를 하면 Object type json이 나온다.
        string jsonData = JsonConvert.SerializeObject(keys);

        //파일로 쓰기
        FileStream fileStream = new FileStream(mFilePath, FileMode.Create);
        byte[] data = Encoding.UTF8.GetBytes(jsonData);
        fileStream.Write(data, 0, data.Length);
        fileStream.Close();

        Debug.Log(GetType() + " 파일 쓰기");
    }

    /// <summary>
    /// 키 이름을 기반으로 해당 키에 등록된 KeyCode를 리턴한다.
    /// </summary>
    /// <param name="keyName"></param>
    /// <returns></returns>
    public KeyCode GetKeyCode(string keyName)
    {
        return mKeyDictionary[keyName];
    }

    /// <summary>
    /// 해당 키에서 자기 자신을 제외한 키가 등록되어있는경우를 방지하고, 특정한 키 설정을 방지하기위해 키를 체크한다.
    /// </summary>
    /// <returns>할당 가능한 키인가?</returns>
    public bool CheckKey(KeyCode key, KeyCode currentKey)
    {
        //예외1. 현재 할당된 키에 같은 키로 설정하도록 한 경우는 허용으로 리턴한다.
        if (currentKey == key) { return true; }

        //1차 키 검사. 
        //키는 아래의 키만 허용한다.
        if
        (
            key >= KeyCode.A && key <= KeyCode.Z || //97 ~ 122   A~Z
            key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9 || //48 ~ 57    알파 0~9
            key == KeyCode.Quote || //39         
            key == KeyCode.Comma || //44
            key == KeyCode.Period || //46
            key == KeyCode.Slash || //47
            key == KeyCode.Semicolon || //59
            key == KeyCode.LeftBracket || //91
            key == KeyCode.RightBracket || //93
            key == KeyCode.Minus || //45
            key == KeyCode.Equals || //61
            key == KeyCode.BackQuote //96
        ) { }
        else { return false; }

        //2차 키 검사. 
        //1차 키 검사를 포함한 키 중 다음 조건문 키는 설정할 수 없다.
        if
        (
            //이동 키 WASD
            key == KeyCode.W ||
            key == KeyCode.A ||
            key == KeyCode.S ||
            key == KeyCode.D
        ) { return false; }

        //3차 키 검사.
        //현재 설정된 키들 중 이미 할당된 키가 있는경우는 설정할 수 없다.
        foreach (KeyValuePair<string, KeyCode> keyPair in mKeyDictionary)
        {
            if (key == keyPair.Value)
            {
                return false;
            }
        }

        //모든 키 검사를 통과하면 해당 키는 설정이 가능한 키.
        return true;
    }

    /// <summary>
    /// keyName에 해당하는 키를 KeyCode인 key로 변경시킨다.
    /// </summary>
    /// <param name="keyCode">새로 설정하는 키의 코드값(enum)</param>
    /// <param name="keyName">설정된 키(keyCode)를 keyName에 할당한다</param>
    public void AssignKey(KeyCode keyCode, string keyName)
    {
        //딕셔너리 
        mKeyDictionary[keyName] = keyCode;

        //키 파일을 로컬에 저장
        SaveOptionData();
    }*/
}
