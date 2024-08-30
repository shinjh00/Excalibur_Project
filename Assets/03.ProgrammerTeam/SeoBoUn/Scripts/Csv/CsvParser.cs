using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// Csv를 읽어오는 메소드
/// </summary>
public class CsvParser : MonoBehaviour
{
    // TODO... Csv or Data 매니저로 변경
    private static CsvParser instance;
    public static CsvParser Instance { get { return instance; } }
    public ItemDataList ItemList { get { return itemDataList; } }
    public List<ItemData> itemDatas = new List<ItemData>();

    public bool initComplete;

    [SerializeField] string[] data;
    [SerializeField] string[] elements;
    [SerializeField] ItemDataList itemDataList;

    Dictionary<int, DungeonTableData> dungeonTableData;
    Dictionary<int, RoomData> roomDataDic;
    Dictionary<int, ResourceData> resourceDic;
    Dictionary<int, PlayerStatData> playerDic;
    Dictionary<int, PlayerFieldData> playerFieldDic;
    Dictionary<int, MonsterData> monsterDic;
    Dictionary<int, MonsterSpawnData> monsterSpawnDic;
    Dictionary<int, ConsumablesStatData> consumableItemDic;
    Dictionary<int, WearableStatData> wearableItemDic;
    Dictionary<int, CashStatData> cashItemDic;
    Dictionary<int, BoxData> boxDic;
    Dictionary<int, BoxSpawnData> boxSpawnDic;
    Dictionary<int, BoxDropTable> boxDropTableDic;
    Dictionary<int, LevelData> levelDic;
    Dictionary<float, float> delayDic;
    Dictionary<int, TextTableData> textDic;
    Dictionary<int, TextEffectData> textEffectDic;
    Dictionary<int, TextBundleData> textBundleDic;
    Dictionary<int, SkillInfoData> skillInfoDic;
    Dictionary<int, SkillEffectData> skillEffectDic;
    Dictionary<int, MonsterSpawnBundle> monsterSpawnBunddleDic;
    Dictionary<int, BoxBundleData> boxSpawnBundleDic;
    Dictionary<int, TrapBundleData> trapBundleDic;
    Dictionary<int, TrapStruct> trapDic;
    Dictionary<int, TrapSpawnStruct> trapSpawnDic;
    Dictionary<int, PuzzleData> puzzleDic;
    Dictionary<int, OXQuizList> oxQuizDic;
    Dictionary<int, ExcaliburData> excaliburDic;

    public Dictionary<int, DungeonTableData> DungeonTableData { get { return dungeonTableData; } }
    public Dictionary<int, RoomData> RoomDataDic { get { return roomDataDic; } }
    public Dictionary<int, ResourceData> ResourceDic { get { return resourceDic; } }
    public Dictionary<int, PlayerStatData> PlayerDic { get { return playerDic; } }
    public Dictionary<int, MonsterData> MonsterDic { get { return monsterDic; } }
    public Dictionary<int, MonsterSpawnData> MonsterSpawnDic { get { return monsterSpawnDic; } }
    public Dictionary<int, ConsumablesStatData> ConsumableItemDic { get { return consumableItemDic; } }
    public Dictionary<int, WearableStatData> WearableItemDic { get { return wearableItemDic; } }
    public Dictionary<int, CashStatData> CashItemDic { get { return cashItemDic; } }
    public Dictionary<int, BoxData> BoxDic { get { return boxDic; } }
    public Dictionary<int, BoxSpawnData> BoxSpawnDic { get { return boxSpawnDic; } }
    public Dictionary<int, BoxDropTable> BoxDropTableDic { get { return boxDropTableDic; } }
    public Dictionary<int, LevelData> LevelDic { get { return levelDic; } }
    public Dictionary<float, float> DelayDic { get { return delayDic; } }
    public Dictionary<int, TextTableData> TextDic { get { return textDic; } }
    public Dictionary<int, TextEffectData> TextEffectDic { get { return textEffectDic; } }
    public Dictionary<int, TextBundleData> TextBundleDic { get { return textBundleDic; } }
    public Dictionary<int, SkillInfoData> SkillDic { get { return skillInfoDic; } }
    public Dictionary<int, SkillEffectData> SkillEffectDic { get { return skillEffectDic; } }
    public Dictionary<int, MonsterSpawnBundle> MonsterSpawnBunddleDic { get { return monsterSpawnBunddleDic; } }
    public Dictionary<int, BoxBundleData> BoxSpawnBundleDic { get { return boxSpawnBundleDic; } }
    public Dictionary<int, TrapBundleData> TrapSpawnBundleDic { get { return trapBundleDic; } }
    public Dictionary<int, TrapStruct> TrapDic { get { return trapDic; } }
    public Dictionary<int, TrapSpawnStruct> TrapSpawnDic { get { return trapSpawnDic; } }
    public Dictionary<int, PuzzleData> PuzzleDic { get { return puzzleDic; } }
    public Dictionary<int, OXQuizList> OXQuizList { get { return oxQuizDic; } }
    public Dictionary<int, ExcaliburData> ExcaliburDic { get { return excaliburDic; } }
    public Dictionary<int, PlayerFieldData> PlayerFieldDic { get { return playerFieldDic; } }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    public void Init()
    {
        StartCoroutine(ReadRoutine());
    }
    IEnumerator ReadRoutine()
    {
        dungeonTableData = new Dictionary<int, DungeonTableData>();
        roomDataDic = new Dictionary<int, RoomData>();
        resourceDic = new Dictionary<int, ResourceData>();
        playerDic = new Dictionary<int, PlayerStatData>();
        playerFieldDic = new Dictionary<int, PlayerFieldData>();
        monsterDic = new Dictionary<int, MonsterData>();
        monsterSpawnDic = new Dictionary<int, MonsterSpawnData>();
        consumableItemDic = new Dictionary<int, ConsumablesStatData>();
        wearableItemDic = new Dictionary<int, WearableStatData>();
        cashItemDic = new Dictionary<int, CashStatData>();
        boxDic = new Dictionary<int, BoxData>();
        levelDic = new Dictionary<int, LevelData>();
        boxSpawnDic = new Dictionary<int, BoxSpawnData>();
        boxDropTableDic = new Dictionary<int, BoxDropTable>();
        delayDic = new Dictionary<float, float>();
        textDic = new Dictionary<int, TextTableData>();
        textEffectDic = new Dictionary<int, TextEffectData>();
        textBundleDic = new Dictionary<int, TextBundleData>();
        skillInfoDic = new Dictionary<int, SkillInfoData>();
        skillEffectDic = new Dictionary<int, SkillEffectData>();
        monsterSpawnBunddleDic = new Dictionary<int, MonsterSpawnBundle>();
        boxSpawnBundleDic = new Dictionary<int, BoxBundleData>();
        trapBundleDic = new Dictionary<int, TrapBundleData>();
        trapDic = new Dictionary<int, TrapStruct>();
        trapSpawnDic = new Dictionary<int, TrapSpawnStruct>();
        puzzleDic = new Dictionary<int, PuzzleData>();
        oxQuizDic = new Dictionary<int, OXQuizList>();
        excaliburDic = new Dictionary<int, ExcaliburData>();

        ReadCsv(dungeonTableData, "DungeonTableData");

        ReadCsv(monsterSpawnBunddleDic, "DungeonData/MonsterBundleData");
        ReadCsv(boxSpawnBundleDic, "DungeonData/BoxBundleData");
        ReadCsv(trapBundleDic, "DungeonData/TrapBundleData");

        ReadCsv(roomDataDic, "DungeonData/RoomTableData");
        ReadCsv(resourceDic, "ver.4.0.0_ResourceTable");
        ReadCsv(playerDic, "PlayerData");
        ReadCsv(playerFieldDic, "PlayerData/PlayerFieldData");

        ReadCsv(monsterDic, "MonsterData/MonsterData");
        ReadCsv(monsterSpawnDic, "MonsterData/MonsterSpawnData");

        ReadCsv(consumableItemDic, "ItemData/ver.4.0.0._Consumable Table");
        ReadCsv(wearableItemDic, "ItemData/ver.4.0.0._Wearable Table");
        ReadCsv(cashItemDic, "ItemData/ver.4.0.0._Cash Table");

        ReadCsv(trapDic, "TrapData/TrapData");
        ReadCsv(trapSpawnDic, "TrapData/TrapSpawnData");

        ReadCsv(puzzleDic, "PuzzleData/PuzzleData");
        ReadCsv(oxQuizDic, "PuzzleData/PuzzleOXData");

        ReadCsv(levelDic, "LevelData/KnightLevelData");
        ReadCsv(levelDic, "LevelData/WarriorLevelData");
        ReadCsv(levelDic, "LevelData/WizardLevelData");
        ReadCsv(levelDic, "LevelData/ArcherLevelData");
        ReadCsv(levelDic, "LevelData/ExcaliburLevelData");

        ReadCsv(skillInfoDic, "SkillData/SkillData");
        ReadCsv(skillEffectDic, "SkillData/SkillDetailData");

        ReadCsv(boxDropTableDic, "BoxData/DropTableData");
        ReadCsv(boxDic, "BoxData/BoxData");
        ReadCsv(boxSpawnDic, "BoxData/BoxSpawnTableData");
        ReadCsv(delayDic, "DelayData");

        ReadCsv(textDic, "TextData/TextData");
        ReadCsv(textEffectDic, "TextData/TextEffectData");
        ReadCsv(textBundleDic, "TextData/TextBundleData");

        ReadCsv(excaliburDic, "ExcaliburData/ExcaliburData");


        yield return new WaitUntil(() => SpawnItemList.Instance != null);

        foreach (ConsumablesStatData data in consumableItemDic.Values)
        {
            data.CreateScriptableObject();
        }

        foreach (WearableStatData data in wearableItemDic.Values)
        {
            data.CreateScriptableObject();
        }

        foreach (CashStatData data in cashItemDic.Values)
        {
            data.CreateScriptableObject();
        }

        itemDataList.AddItem();
        initComplete = true;
    }

    /// <summary>
    /// Csv를 읽어오는 메소드
    /// </summary>
    /// <typeparam name="T">구조체 타입</typeparam>
    /// <param name="instanceDic">실제 구조체</param>
    /// <param name="filePath">파일명</param>
    public void ReadCsv<T>(Dictionary<int, T> instanceDic, string filePath) where T : struct, ICsvReadable
    {
        if(instanceDic == null)
        {
            instanceDic = new Dictionary<int, T>();
        }

        TextAsset csvData = Resources.Load<TextAsset>($"CsvFile/{filePath}");

        data = csvData.text.Split(new char[] { '\n' });

        int count = data.Length;

        for(int i = 2; i < count; i++)
        {
            if (data[i] == "")
                continue;

            T curData = new T();

            elements = data[i].Split(new char[] { ',' });

            curData.CsvRead(elements);

            if (int.Parse(elements[0]) == -9999)
            {   // 예외값은 읽지 말자
                return;
            }

            instanceDic.Add(int.Parse(elements[0]), curData);
        }

    }
    public void ReadCsv(Dictionary<float, float> instanceDic, string filePath)
    {
        if (instanceDic == null)
        {
            instanceDic = new Dictionary<float,float>();
        }

        TextAsset csvData = Resources.Load<TextAsset>($"CsvFile/{filePath}");

        data = csvData.text.Split(new char[] { '\n' });

        int count = data.Length;

        for (int i = 2; i < count; i++)
        {
            if (data[i] == "")
                continue;

            elements = data[i].Split(new char[] { ',' });

            instanceDic.Add(float.Parse(elements[0]), float.Parse(elements[1]));
        }
    }

    /// <summary>
    /// 리소스 경로 반환 메소드
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public string returnPath(int id)
    {
        if(id == -9999)
        {
            return null;
        }

        if (ResourceDic.ContainsKey(id))
        {
            string name = ResourceDic[id].name;
            string route = ResourceDic[id].route;
            route = route.TrimEnd();

            //paths.Add($"{route}/{name}");

            return $"{route}/{name}";
        }
        else
        {
            return null;
        }
    }
}
