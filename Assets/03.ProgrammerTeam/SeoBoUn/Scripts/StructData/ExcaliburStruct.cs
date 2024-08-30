using System;
using System.Collections.Generic;

/// <summary>
/// 엑스칼리버 세팅 데이터 필드
/// </summary>
[Serializable]
public struct ExcaliburData : ICsvReadable, IFireBaseReadable
{
    public int id;                      // id
    public string name;                 // 이름
    public int lockedImageID;           // 잠김 이미지ID
    public int activeImageID;           // 활성 이미지ID
    public int attackBuff;              // 공격력 상승량
    public int defenceBuff;             // 방어력 상승량
    public int excaliburActiveTime;     // 엑스칼리버 활성화 시간
    public int buffTime;                // 버프 시간
    public int equipTime;               // 변신 시간
    public int equipEffectID;           // 변신 이펙트ID
    public int equipSoundID;            // 변신 사운드ID

    public List<int> cirPSafeZoneSize;  // 자기장 페이즈 안전 구역 사이즈
    public List<int> cirPSustaionTime;  // 자기장 페이즈 유지 시간
    public List<int> cirPSwitchingTime; // 자기장 페이즈 전환 시간
    public List<float> cirPDamage;      // 자기장 페이즈 데미지

    public void CsvRead(string[] elements)
    {
        int index = 0;

        id = int.Parse(elements[index++]);
        name = elements[index++];
        lockedImageID = int.Parse(elements[index++]);
        activeImageID = int.Parse(elements[index++]);
        attackBuff = int.Parse(elements[index++]);
        defenceBuff = int.Parse(elements[index++]);
        excaliburActiveTime = int.Parse(elements[index++]);
        buffTime = int.Parse(elements[index++]);
        equipTime = int.Parse(elements[index++]);
        equipEffectID = int.Parse(elements[index++]);
        equipSoundID = int.Parse(elements[index++]);

        cirPSafeZoneSize = new List<int>();
        cirPSustaionTime = new List<int>();
        cirPSwitchingTime = new List<int>();
        cirPDamage = new List<float>();

        for(int i = 0; i < 5; i++)
        {
            cirPSafeZoneSize.Add(int.Parse(elements[index++]));
            cirPSustaionTime.Add(int.Parse(elements[index++]));
            cirPSwitchingTime.Add(int.Parse(elements[index++]));
            cirPDamage.Add(float.Parse(elements[index++]));
        }
    }

    public FirebaseReadData FireBaseString()
    {
        return new FirebaseReadData("ExcaliburData", id, PlayDataID.None);
    }
}

