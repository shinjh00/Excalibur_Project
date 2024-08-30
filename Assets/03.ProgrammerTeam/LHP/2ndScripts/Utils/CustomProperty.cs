using Photon.Realtime;
using System;
using System.Threading.Tasks;
using UnityEngine;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
/// <summary>
/// 개발자 : 이형필 / 포톤을 활용한 커스텀 프로퍼티를 정의하는 클래스
/// </summary>
public static class DefinePropertyKey
{

    public static string READY = "Ready";
    public static string LOAD = "Load";

    public static string RED = "Red";
    public static string BLUE = "Blue";

    public static string PASSWORD = "Password";

    public static string TEAMGAME = "TeamGame";
    public static string OUTGAME = "OutGame";
    public static string LOBBYREADY = "LobbyReady";
    public static string HOST = "host";
    public static string ROOMNAME = "RoomName";
    public static string GAMEON = "GameOn";

    public static string LOADCOMPLETE = "LoadComplete";

    public static string BLUESCORE = "BlueScore";
    public static string REDSCORE = "RedScore";

    public static string USERID = "UserId";

    public static string VICTORY = "Victory";
    public static string GAMEOVER = "GameOver";

    public static string CHARACTERCLASS = "CharacterClass";
    public static string COMMONSKILL1 = "CommomSkill1";
    public static string COMMONSKILL2 = "CommonSkill2";

    public static string SELECTECOMPLETE = "SelectComplete";

    public static string ROOMID = "RoomID";

    public static string STARTTIME = "StartTime";

}
/// <summary>
/// 개발자 : 이형필  / 커스텀 프로퍼티를 세팅하거나 얻어오는 클래스
/// </summary>
public static class CustomProperty
{
    /// <summary>
    /// 해당 플레이어의 커스텀프로퍼티를 가져옴
    /// </summary>
    /// <typeparam name="T">프로퍼티의 데이터타입</typeparam>
    /// <param name="player">해당 플레이어</param>
    /// <param name="key">가져올 프로퍼티 이름</param>
    /// <returns></returns>
    public static T GetProperty<T>(this Player player, string key)
    {
        PhotonHashtable property = player.CustomProperties;
        //key값이 있으면 해당 해시테이블에서 값을 가져와 T타입으로 변환해서 반환
        //없을 경우 해당 타입의 디폴트 값을 반환
        return property.ContainsKey(key) ? (T)property[key] : default(T);
    }
    /// <summary>
    /// 해당 플레이어의 커스텀프로퍼티를 설정함
    /// </summary>
    /// <typeparam name="T">프로퍼티 데이터타입</typeparam>
    /// <param name="player">해당 플레이어</param>
    /// <param name="str">프로퍼티 이름</param>
    /// <param name="value">세팅해줄 값</param>
    public static void SetProperty<T>(this Player player, string str, T value)
    {

        PhotonHashtable property = new PhotonHashtable();
        if (property.ContainsKey(str) == false)
            //해당 프로퍼티가 포함되어있지 않을 경우 해시테이블을 생성
            property = new PhotonHashtable { { str, value } };
        else
            //포함되어있을 경우 값을 변경
            property[str] = value;
        //프로퍼티를 저장
        player.SetCustomProperties(property);
    }
    /// <summary>
    /// 해당 룸의 커스텀 프로퍼티를 삭제함
    /// </summary>
    /// <param name="room">해당 룸</param>
    /// <param name="key">삭제할 프로퍼티 이름</param>
    public static void RemoveProperty(this Player player, string key)
    {
        PhotonHashtable property = player.CustomProperties;

        if (property.ContainsKey(key))
        {
            property.Remove(key);
            player.SetCustomProperties(property);
        }
    }
    /// <summary>
    /// 해당 룸의 커스텀프로퍼티를 가져옴
    /// </summary>
    /// <typeparam name="T">프로퍼티 데이터타입</typeparam>
    /// <param name="room">해당 룸</param>
    /// <param name="key">가져올 프로퍼티 이름</param>
    /// <returns></returns>
    public static T GetProperty<T>(this Photon.Realtime.Room room, string key)
    {
        PhotonHashtable property = room.CustomProperties;
        return property.ContainsKey(key) ? (T)property[key] : default(T);
    }
    /// <summary>
    /// 해당 룸의 커스텀프로퍼티를 설정함
    /// </summary>
    /// <typeparam name="T">프로퍼티 데이터 타입</typeparam>
    /// <param name="room">해당 룸</param>
    /// <param name="str">프로퍼티 이름</param>
    /// <param name="value">세팅할 값</param>
    public static void SetProperty<T>(this Photon.Realtime.Room room, string str, T value)
    {
        PhotonHashtable property = new PhotonHashtable();
        if (property.ContainsKey(str) == false)
            /*PhotonHashtable*/
            property = new PhotonHashtable { { str, value } };
        else
            property[str] = value;
        // property.Add(str, value);
        room.SetCustomProperties(property);
    }

    /// <summary>
    /// 커스텀프로퍼티의 비동기 흐름을 제어하는 메서드
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="player"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task SetPropertyAsync<T>(this Player player, string key, T value)
    {
        var property = new PhotonHashtable();
        var tcs = new TaskCompletionSource<bool>();

        if (!property.ContainsKey(key))
        {
            property = new PhotonHashtable { { key, value } };
        }
        else
        {
            property[key] = value;
        }
        PropertyTaskCompletionSourceStore.Store(player, key, tcs);
        try
        {
            bool result = player.SetCustomProperties(property, null, null);
            
        }
        catch (Exception ex)
        {
            tcs.SetException(ex);
        }

        await tcs.Task;
    }

}