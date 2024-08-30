using System;
using System.Collections.Generic;

/// <summary>
/// 캐릭터 필드 설계 데이터
/// </summary>
[Serializable]
public struct PlayerFieldData : ICsvReadable
{
    public int id;              // id
    public string className;    // 직업 이름
    public int price;           // 가격
    public int classImage;      // 직업 이미지
    public int idleAnimID;      // 대기 애니메이션ID
    public int moveAnimID;      // 이동 애니메이션ID
    public int attackAnimID;    // 피격 애니메이션ID
    public int deadAnimID;      // 사망 애니메이션ID
    public int moveSoundID;     // 이동 소리ID
    public int attackSoundID;   // 피격 소리ID
    public int deadSoundID;     // 사망 소리ID

    public void CsvRead(string[] elements)
    {
        int index = 0;

        id = int.Parse(elements[index++]);
        className = elements[index++];
        price = int.Parse(elements[index++]);
        classImage = int.Parse(elements[index++]);
        idleAnimID = int.Parse(elements[index++]);
        moveAnimID = int.Parse(elements[index++]);
        attackAnimID = int.Parse(elements[index++]);
        deadAnimID = int.Parse(elements[index++]);
        moveSoundID = int.Parse(elements[index++]);
        attackSoundID = int.Parse(elements[index++]);
        deadSoundID = int.Parse(elements[index++]);
    }
}

/// <summary>
/// 플레이어 세이브 데이터테이블
/// </summary>
[Serializable]
public struct PlayerDataTable : IFireBaseReadable
{
    public int ID;                      // 유저 구분용 ID (80으로 시작하는 중복없는 6자리)
    public string LoginID;              // 로그인용 ID
    public string password;             // 로그인용 password(최소 8자리, 최대 16자리 / 숫자, 영문, 특수문자 필수)
    public string nickname;             // 닉네임(한글 8자, 영어 16자)
    public int hashValue;               // 중복 닉네임에 대한 처리값
    public int imageID;                 // 프로필 이미지에 대한 ID값
    public string introduce;            // defalut : 잘 부탁드립니다.
    public int level;                   // 플레이어 레벨
    public int exp;                     // 플레이어 현재 경험치  
    public int maxExp;                  // 다음 레벨을 위한 요구 경험치
    public int playedSession;           // 플레이한 세션
    public int recentCharacter;         // 가장 최근 캐릭터
    public int winningGames;            // 승리 횟수
    public string createdTime;          // 계정 생성 시간
    public string lastLoginTime;        // 최근 접속 시간
    public string lastStoreRefreshTime; // 최근 상점 갱신 시각
    public SettingSaveData settingData;
    public Character characterData;
    public UploadStorageData uploadStorageData;
    public UploadStoreData uploadStoreData;
    public int havingGold;              // 소지 골드량

    public void SetNewPlayer(string loginID, string password)
    {
        this.ID = UnityEngine.Random.Range(FirebaseManager.PLAYERDATATABLEID, FirebaseManager.PLAYERDATATABLEID_END);
        this.LoginID = loginID;
        this.nickname = $"플레이어{UnityEngine.Random.Range(1000, 9999)}";
        this.password = password;
        this.createdTime = DateTime.Now.ToString();
        this.lastLoginTime = DateTime.Now.ToString();
        this.introduce = "잘 부탁드립니다";
        this.level = 1;

        settingData = new SettingSaveData();
        characterData = new Character();
        uploadStorageData = new UploadStorageData();
        uploadStoreData = new UploadStoreData();
    }

    public FirebaseReadData FireBaseString()
    {
        return new FirebaseReadData("PlayerDataTable", ID, 0);
    }

}

/// <summary>
/// 세팅 데이터
/// <br/>language, resolution, mouseControl, wholeSound, off, effsound, off
/// </summary>
[Serializable]
public struct SettingSaveData
{
    public int language;
    public int resolution;
    public int mouseControl;
    public int wholeSound;
    public int wholeSoundOff;
    public int effectSound;
    public int effectSoundOff;
}

/// <summary>
/// 인벤토리 세이브 데이터 구조체(아이템 하나를 위해 사용)
/// <br/> itemID, posX, posY, slot(WearablesType), quickSlot_Number
/// </summary>
[Serializable]
public struct InventorySaveData
{
    public List<int> itemID;
    public List<int> posX;
    public List<int> posY;
    public List<WearablesType> slot;
    public List<int> quickSlot_Number;

    public void SetData(int itemID, int posX, int posY, WearablesType slot, int quickSlot_Number)
    {
        if(this.itemID == null)
        {
            this.itemID = new List<int>();
        }
        if (this.posX == null)
        {
            this.posX = new List<int>();
        }
        if (this.posY == null)
        {
            this.posY = new List<int>();
        }
        if (this.slot == null)
        {
            this.slot = new List<WearablesType>();
        }
        if (this.quickSlot_Number == null)
        {
            this.quickSlot_Number = new List<int>();
        }

        this.itemID.Add(itemID);
        this.posX.Add(posX);
        this.posY.Add(posY);
        this.slot.Add(slot);
        this.quickSlot_Number.Add(quickSlot_Number);
    }
}

[Serializable]
public struct Character
{
    public ClassType userClassType;
    public CharacterDetail detailData;
}

[Serializable]
public struct CharacterDetail
{
    public InventorySaveData inventorySaveData;
    public int skill1ID;
}