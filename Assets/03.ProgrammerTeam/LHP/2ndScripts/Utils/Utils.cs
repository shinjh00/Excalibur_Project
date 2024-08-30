using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필 / 전역적으로 활용할 메서드를 할당하는 클래스
/// </summary>
public static class Utils
/// <summary>
/// 개발자 : 이형필 / 레이어가 포함되어있는지 확인
/// </summary>
{
    public static bool Contain(this LayerMask layerMask, int layer)
    {
        return ((1 << layer) & layerMask) != 0;
    }

    /// <summary>
    /// 개발자 : 이형필 / 선후딜 계산하는 메서드
    /// </summary>
    /// <param name="minDelay">선딜레이</param>
    /// <param name="maxDelay">후딜레이</param>
    /// <param name="atkSpeed">공격속도</param>
    /// <returns></returns>
    public static float CalculateDelay(float minDelay, float maxDelay, float atkSpeed)
    {
        float caldAtkSpeed = CsvParser.Instance.DelayDic.Keys.Where(key => key <= atkSpeed).DefaultIfEmpty(0.44f).Max(); // 만약 비어있으면 공속은 최소값반환(0.44) 키값보다 큰데 그 다음 키값보다 작으면 작은 키값 반환
        if (caldAtkSpeed > 1.82f)
        {
            caldAtkSpeed = 1.82f;
        }
        float delayReduceRate = CsvParser.Instance.DelayDic[caldAtkSpeed];

        float calDelay = maxDelay - (delayReduceRate * (maxDelay - minDelay));
       // Debug.Log($"return : {calDelay},  atkSpd : {atkSpeed}, caldAtkSpeed : {caldAtkSpeed}, reduce : {delayReduceRate}");

        return calDelay;
    }
    /// <summary>
    /// 개발자 : 이형필 / 현재 상태가 state를 포함하고 있는지
    /// </summary>
    /// <param name="curState">현 상태</param>
    /// <param name="state">검사할 상태</param>
    /// <returns></returns>
    public static bool Contain(this PlayerState curState, PlayerState state)
    {
        return (curState & state) != 0;
    }
    public static void AddDamagable(this Dictionary<IDamageable, bool> dic, List<IDamageable> damagableList)
    {
        foreach (var d in damagableList)
        {
            if (!dic.ContainsKey(d))
            {
                Debug.Log($"Add to {d}");
                dic.Add(d, true);
            }
        }
    }
    /// <summary>
    /// 개발자 : 이형필 / 전역적으로 타이머를 텍스트와 연동하여 타이머가 끝나면 함수가 실행될 수 있도록
    /// </summary>
    /// <param name="value"></param>
    /// <param name="text"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IEnumerator Timer(float value, TMP_Text text, Action action)                                                     //6
    {

        double loadTime = PhotonNetwork.Time;

        while (PhotonNetwork.Time - loadTime < value)
        {

            int remainTime = (int)(value - (PhotonNetwork.Time - loadTime));
            int minutes = Mathf.FloorToInt((remainTime % 3600) / 60);
            int seconds = Mathf.FloorToInt(remainTime % 60);

            text.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            yield return null;
        }
        action();
    }
    /// <summary>
    /// 개발자 : 이형필 / 채팅 메시지 포맷메서드
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static string FormatMessage(this string message)
    {
        // 기본 메시지 구성
        string baseMessage = $"{message}";

        // 최대 길이 39자를 초과할 경우 '...'으로 대체
        if (baseMessage.Length > 39)
        {
            baseMessage = baseMessage.Substring(0, 36) + "...";
        }

        // 20자를 넘으면 줄바꿈 처리
        string[] lines = SplitIntoLines(baseMessage, 20);

        // 줄바꿈된 메시지 조합
        string formattedMessage = string.Join("\n", lines);     // 배열 사이에 줄바꿈

        return formattedMessage;
    }
    /// <summary>
    /// 개발자 : 이형필 / 긴 문자열을 주어진 최대 줄 길이(maxLineLength)를 기준으로 잘라 여러 줄의 문자열 배열로 반환
    /// </summary>
    /// <param name="text"></param>
    /// <param name="maxLineLength"></param>
    /// <returns></returns>
    static string[] SplitIntoLines(string text, int maxLineLength)
    {
        var lines = new List<string>();
        for (int i = 0; i < text.Length; i += maxLineLength)
        {
            if (i + maxLineLength > text.Length)
            {
                lines.Add(text.Substring(i));
            }
            else
            {
                lines.Add(text.Substring(i, maxLineLength));
            }
        }
        return lines.ToArray();
    }
    /// <summary>
    /// 개발자 : 이형필 / 포톤 플레이어에서 플레이어컨트롤러 가져옴
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static PlayerController GetPlayerController(this Player player)
    {
        GameManager.Ins.playerDic.TryGetValue(player, out PlayerController playerController);
        return playerController;
    }
    /// <summary>
    /// 개발자 : 이형필 / 방에서 같은 플레이어 리스트로 반환 본인제외
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static List<Player> AllyPlayers(this Player player)
    {
        List<Player> allies = new List<Player>();

        bool isTeamGame = PhotonNetwork.CurrentRoom.GetProperty<bool>(DefinePropertyKey.TEAMGAME);
        bool isRedTeam = player.GetProperty<bool>(DefinePropertyKey.RED);

        if (isTeamGame)
        {
            foreach (var p in PhotonNetwork.PlayerList)
            {
                if ((isRedTeam && p.GetProperty<bool>(DefinePropertyKey.RED)) ||
                    (!isRedTeam && p.GetProperty<bool>(DefinePropertyKey.BLUE)))
                {
                    if (p == PhotonNetwork.LocalPlayer)
                        continue;
                    allies.Add(p);
                }
            }
        }

        return allies;
    }
    public static bool ArePlayersOnSameTeam(this List<Player> players)
    {
        if (players.Count == 0)
            return false;

        bool isTeamGame = PhotonNetwork.CurrentRoom.GetProperty<bool>(DefinePropertyKey.TEAMGAME);

        if (!isTeamGame)
            return false; // 팀 게임이 아닌 경우 모든 플레이어가 같은 팀인지 확인할 수 없음

        bool? firstPlayerTeam = players[0].GetProperty<bool>(DefinePropertyKey.RED);

        foreach (var player in players)
        {
            bool? playerTeam = player.GetProperty<bool>(DefinePropertyKey.RED);
            if (playerTeam != firstPlayerTeam)
                return false; // 한 명이라도 다른 팀이면 false 반환
        }

        return true; // 모두 같은 팀이면 true 반환
    }
    /// <summary>
    /// 개발자 : 이형필 / 로컬플레이어랑 같은 팀인지 확인
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static bool IsTeammate(this Player player)
    {
        bool isOutGame = player.GetProperty<bool>(DefinePropertyKey.OUTGAME);
        bool isTeamGame = PhotonNetwork.CurrentRoom.GetProperty<bool>(DefinePropertyKey.TEAMGAME);

        if (!isTeamGame || isOutGame)
        {
            return false; // 팀 게임이 아니면 false 반환
        }

        bool playerIsRed = player.GetProperty<bool>(DefinePropertyKey.RED);
        bool otherPlayerIsRed = PhotonNetwork.LocalPlayer.GetProperty<bool>(DefinePropertyKey.RED);

        return playerIsRed == otherPlayerIsRed;

    }
    /// <summary>
    /// 개발자 : 이형필 / 데이터테이블 딕셔너리에서 id 랜덤으로 가져오기
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    public static int GetRandomKey<T>(this Dictionary<int, T> data) where T : struct
    {
        List<int> id = new List<int>(data.Keys);
        int randomIndex = UnityEngine.Random.Range(0, data.Keys.Count);
        return id[randomIndex];
    }


}

/// <summary>
/// 개발자 : 이형필 / 커스텀프로퍼티 원활한 비동기 진행을 위한 tcs저장관련 클래스 
///  <br/>패널컨트롤러쪽에 플레이어 커스텀프로퍼티 오버라이드로 업데이트 될때 tcs 결과값을 true로 바꿔서 비동기 흐름을 제어함
/// </summary>
public static class PropertyTaskCompletionSourceStore
{
    static readonly Dictionary<(Player, string), TaskCompletionSource<bool>> _store = new Dictionary<(Player, string), TaskCompletionSource<bool>>();
   

    public static void Store(Player player, string key, TaskCompletionSource<bool> tcs)
    {
        _store[(player, key)] = tcs;
    }
    /// <summary>
    /// 플레이어 커스텀프로퍼티 검색
    /// </summary>
    /// <param name="player"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static TaskCompletionSource<bool> Retrieve(Player player, string key)
    {
        _store.TryGetValue((player, key), out var tcs);
        return tcs;
    }

    public static void Remove(Player player, string key)
    {
        _store.Remove((player, key));
    }
    public static IEnumerable<string> GetAllKeys(Player player)
    {

        return _store.Keys
            .Where(kvp => kvp.Item1 == player)
            .Select(kvp => kvp.Item2)
            .ToList();
    }
}

public static class CreatePhotonObject
{
    public static GameObject Create(int id, Vector2 pos)
    {
        GameObject instance = null;
        switch (id)
        {

            #region Trap
            case 1202000:
                instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Trap/SpikeTrap", pos, Quaternion.identity);
                break;
            case 1202001:
                instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Trap/SpiderTrap", pos, Quaternion.identity);
                break;
            case 1202002:
                instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Trap/CoffinTrap", pos, Quaternion.identity);
                break;
            case 1202003:
                instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Trap/BananaTrap", pos, Quaternion.identity);
                break;




            #endregion
            #region Puzzle
            case 1303000:
                instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Puzzle/Puzzle_1_1", pos, Quaternion.identity);
                break;
            case 1303001:
                instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Puzzle/Puzzle_1_2", pos, Quaternion.identity);
                break;
            case 1303002:
                instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Puzzle/Puzzle_1_3", pos, Quaternion.identity);
                break;
            case 1303003:
                instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Puzzle/Puzzle_1_4", pos, Quaternion.identity);
                break;
            case 1303004:
                instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Puzzle/Puzzle_1_5", pos, Quaternion.identity);
                break;
            case 1303005:
                instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Puzzle/Puzzle_2", pos, Quaternion.identity);
                break;
            case 1303006:
                instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Puzzle/Puzzle_3", pos, Quaternion.identity);
                break;
            case 1303007:
                instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Puzzle/Puzzle_4", pos, Quaternion.identity);
                break;
            case 1303008:
                instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Puzzle/Puzzle_5", pos, Quaternion.identity);
                break;
                #endregion
        }

        return instance;
    }

    // 전역적으로 사용할 수 있는 코루틴 재시작 메서드
    public static void RestartCoroutine(this MonoBehaviour monoBehaviour, ref Coroutine coroutineHandle, IEnumerator coroutine)
    {
        // 기존 코루틴이 실행 중이면 중지
        if (coroutineHandle != null)
        {
            monoBehaviour.StopCoroutine(coroutineHandle);
        }

        // 새로운 코루틴 시작 및 핸들 저장
        coroutineHandle = monoBehaviour.StartCoroutine(coroutine);
    }
}

public class CircularQueue<T>
{
    private T[] _queue;
    private int _size;
    private int _front;
    private int _rear;
    private int _count;

    public int Count { get { return _count; } }

    public CircularQueue(int size)
    {
        _size = size;
        _queue = new T[size];
        _front = 0;
        _rear = 0;
        _count = 0;
    }

    public bool IsEmpty => _count == 0;
    public bool IsFull => _count == _size;

    public void Enqueue(T item)
    {
        if (IsFull)
        {
            throw new InvalidOperationException("Queue is full");
        }
        _queue[_rear] = item;
        _rear = (_rear + 1) % _size;
        _count++;
    }

    public T Dequeue()
    {
        if (IsEmpty)
        {
            throw new InvalidOperationException("Queue is empty");
        }
        T item = _queue[_front];
        _queue[_front] = default(T); // Clear the slot
        _front = (_front + 1) % _size;
        _count--;
        return item;
    }

    public void Reverse()
    {
        Stack<T> stack = new Stack<T>();

        // 큐의 모든 요소를 꺼내서 스택에 넣음
        while (!IsEmpty)
        {
            stack.Push(Dequeue());
        }

        // 스택의 모든 요소를 큐에 다시 넣음
        while (stack.Count > 0)
        {
            Enqueue(stack.Pop());
        }
    }

    public void Clear()
    {
        _front = 0;
        _rear = 0;
        _count = 0;
        Array.Clear(_queue, 0, _size);
    }
}
