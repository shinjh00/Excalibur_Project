using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필 // 퀴즈퍼즐 클래스
/// </summary>
public class Puzzle_5 : Puzzle, IPunObservable
{
    [SerializeField] Canvas canvas;
    [SerializeField] TMP_Text text;
    [SerializeField] TMP_Text countDown;

    [SerializeField] PuzzleFire[] fires;
    [SerializeField] GameObject puzzleLight;

    [SerializeField] Collider2D[] cols;
    [SerializeField] BoxCollider2D area;
    [SerializeField] QuizButton oButton;
    [SerializeField] QuizButton xButton;
    [SerializeField] QuizStartButton startButton;
    [SerializeField] float quizTimeCount = 10.0f;
    [SerializeField] int quizCount = 5;
    [SerializeField] float clearCorrectRate = 0.8f;

    int quizIndex;
    int currentQuizIndex;
   public bool isStartQuiz = false;
    bool isEndQuiz = false;
    PlayerController player;

    Dictionary<PlayerController, int> playerList = new Dictionary<PlayerController, int>();    //플레이어와 해당 플레이어의 점수
    Dictionary<int, OXQuizList> quizDic;                                                       //퀴즈번호와 해당 번호에 해당하는 퀴즈구조

    List<PlayerController> players = new List<PlayerController>();                          
    protected override void Start()
    {
        base.Start();
        quizDic = CsvParser.Instance.OXQuizList;
        area.enabled = false;
        startButton.puzzle = this;
        startButton.Init();
        fires = GetComponentsInChildren<PuzzleFire>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isEndQuiz)
        {
            return;
        }
        if (playerLayer.Contain(collision.gameObject.layer))
        {
           // if (isStartQuiz)
          //  {
                PlayerController player = collision.GetComponent<PlayerController>();

                if (photonView.IsMine)
                {
                  if (!playerList.ContainsKey(player))
                  {
                      playerList.Add(player, 0);
                  }
                }
                if (player.photonView.IsMine && isStartQuiz)
                {

                    EnterQuizGame(player);
                }

          //  }
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isEndQuiz)
        {
            return;
        }
        if (playerLayer.Contain(collision.gameObject.layer))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if(photonView.IsMine)
            {
                if (playerList.ContainsKey(player))
                {
                    playerList.Remove(player);              //지역에서 벗어날 경우 리스트에서 제거
                }
            }

           
            if (player.photonView.IsMine)
            {
                ExitQuizGame();
            }
        }

    }

    [PunRPC]

    public void StartQuiz()
    {
        
        startButton.isStart = true;
        text.text = "Let's Start Quiz";
        area.enabled = true;
        if (PhotonNetwork.IsMasterClient)
        {
            int idx = quizDic.GetRandomKey();
            photonView.RPC("LoadQuiz", RpcTarget.All, idx);
        }

    }
    [PunRPC]
    void LoadQuiz(int index)
    {
        foreach (PlayerController player in playerList.Keys)
        {
            Debug.Log($"PuzzleMember : {player}");
            if (player.photonView.IsMine)
            {
                EnterQuizGame(player);
            }   
        }
        //퀴즈 인덱스 로드
        if (currentQuizIndex < quizCount)
        {
            Debug.Log($"cur : {currentQuizIndex}, quizCount {quizCount}");
            isStartQuiz = true;
            StartCoroutine(Countdown(index));
        }
        else
        {
            EndQuiz();

        }
    }
    /// <summary>
    /// 퀴즈 지역에 진입시 
    /// </summary>
    void EnterQuizGame(PlayerController player)
    {
        this.player = player;
        player.CameraMove(transform);
        puzzleLight.SetActive(true);
        canvas.enabled = true;
    }
    IEnumerator Countdown(int index)
    {
        yield return new WaitForSeconds(3);
        text.text = quizDic[index].quizText;
        fires[currentQuizIndex].Animaion("Clear", true);
        fires[currentQuizIndex].circleLight.SetActive(false);
        double startTime = PhotonNetwork.Time;
        int quizEndTime = (int)(startTime + quizTimeCount);

        while (PhotonNetwork.Time < quizEndTime)
        {
            int timeLeft = quizEndTime - (int)PhotonNetwork.Time;
            countDown.text = (Mathf.Max(0, timeLeft)).ToString();
            yield return null;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            currentQuizIndex++;
        }
        countDown.text = "0";
        CheckAnswer(quizDic[index].quizAnswer);
    }

    /// <summary>
    /// 정답인지 체크하는 로직
    /// </summary>
    /// <param name="answer"></param>
    public void CheckAnswer(bool answer)
    {


        if (answer)
        {
            text.text = "The Answer is O";
            oButton.GetPlayersInList();
            players = oButton.SetPlayers(PlayerState.UnAttackable, true);
            foreach (PlayerController p in players)
            {
                playerList[p] += 1;

            }


        }
        else
        {
            text.text = "The Answer is X";
            xButton.GetPlayersInList();
            players = xButton.SetPlayers(PlayerState.UnAttackable, true);
            foreach (PlayerController p in players)
            {
                playerList[p] += 1;

            }

        }

        AttackWrongAnswer();
        foreach (PlayerController p in players)
        {
            p.StateController.StateChange(PlayerState.UnAttackable, 0, 0, false, false);
        }
        oButton.players.Clear();
        xButton.players.Clear();
        players.Clear();
    }
    /// <summary>
    /// 오답자에게 공격을 가하는 메서드
    /// </summary>
    void AttackWrongAnswer()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(area.bounds.center, area.bounds.size, 0, playerLayer);
        foreach (Collider2D col in colliders)
        {
            PlayerController player = col.GetComponent<PlayerController>();
            IDamageable dmg = player.GetComponent<IDamageable>();
            IStateable stateable = player.GetComponent<IStateable>();
            if (player != null)
            {
                if(player.StateController.CurState.Contain(PlayerState.UnAttackable))
                {
                    if (player.photonView.IsMine)
                    {
                        SoundManager.instance.PlaySFX(1650085, audioSource);
                    }
  
                    continue;
                }
                else
                {
                    if (player.photonView.IsMine)
                    {
                        SoundManager.instance.PlaySFX(1650086, audioSource);
                    }
                    if (photonView.IsMine)
                    {

                        int dmgValue = (int)(player.HealthController.MaxHp * 0.1f);
                        dmg.TakeDamage(dmgValue, DamageType.Fixed);
                    }
                    Debug.Log($"PlayerMaxHp = {player.HealthController.MaxHp}");
                    stateable.StateChange(PlayerState.Knockback, 0.5f, 0.3f, true, false);
                }


            }
        }
        isStartQuiz = false;
        if (photonView.IsMine)
        {
            int idx = quizDic.GetRandomKey();
            photonView.RPC("IndexSend", RpcTarget.All, idx);
        }

    }
    IEnumerator QuizSetUpRoutine(int idx)
    {
        yield return new WaitForSeconds(2f);
        if (currentQuizIndex < quizCount)
        {
            ExitQuizGame();
            text.text = "";
            countDown.text = "";
            startButton.idx = idx;
        }
        else
        {
            if (photonView.IsMine)
            {
                photonView.RPC("LoadQuiz", RpcTarget.All, idx);
            }

        }


    }
    [PunRPC]
    public void IndexSend(int idx)
    {
        StartCoroutine(QuizSetUpRoutine(idx));  
    }
    /// <summary>
    /// 퀴즈게임이 끝났을 경우 불러오는 메서드
    /// </summary>

    void EndQuiz()
    {
        isEndQuiz = true;
        int count = 0;
        float correctRate = 0;
        Collider2D[] colliders = Physics2D.OverlapBoxAll(area.bounds.center, area.bounds.size, 0, playerLayer);

        foreach (Collider2D col in colliders)
        {
            PlayerController p = col.GetComponent<PlayerController>();
            count = Mathf.Max(count, playerList[p]);

        }

        correctRate = (float)count / quizCount;
        if (correctRate >= clearCorrectRate)
        {
            text.text = $"GAME CLEAR RATE : {correctRate}";
            if (PhotonNetwork.IsMasterClient)
            {
                SpawnBox();
            }
            SoundManager.instance.PlaySFX(1650084, audioSource);
        }
        else
        {
            text.text = $"GAME OVER RATE : {correctRate}";
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.InstantiateRoomObject("6.Prefab/Monster/Monster_Mimic_Middle", transform.position, Quaternion.identity).GetComponent<BaseMonster>();
            }

        }
        Destroy(countDown.gameObject);
        StartCoroutine(GameOver());
    }


    /// <summary>
    /// 퀴즈를 나갔을 경우 나오는 메서드
    /// </summary>
    void ExitQuizGame()
    {
        startButton.col.enabled = true;
        if(player != null)
        {
            player.CameraMove(player.transform);
            puzzleLight.SetActive(false);
            canvas.enabled = false;
        }
    }
    /// <summary>
    /// 게임이 완전히 끝났을 경우 진행하는 루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator GameOver()
    {

        yield return new WaitForSeconds(3);
        ExitQuizGame();
        Destroy(text.gameObject);
        Destroy(startButton.gameObject);
        foreach (PuzzleFire f in fires)
        {
            Destroy(f.gameObject);
        }

    }
    /// <summary>
    /// 포톤 변수 직렬화
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="info"></param>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {

            stream.SendNext(quizIndex);
            stream.SendNext(currentQuizIndex);
            stream.SendNext(isStartQuiz);
            stream.SendNext(isEndQuiz);
            stream.SendNext(playerList.Count);
            foreach (var pair in playerList)
            {
                stream.SendNext(pair.Key.photonView.ViewID);
                stream.SendNext(pair.Value);
            }
        }
        else
        {
            quizIndex = (int)stream.ReceiveNext();
            currentQuizIndex = (int)stream.ReceiveNext();
            isStartQuiz = (bool)stream.ReceiveNext();
            isEndQuiz = (bool)stream.ReceiveNext();
            int playerCount = (int)stream.ReceiveNext();
            playerList.Clear();
            for (int i = 0; i < playerCount; i++)
            {
                int viewID = (int)stream.ReceiveNext();
                int score = (int)stream.ReceiveNext();
                PlayerController player = PhotonView.Find(viewID).GetComponent<PlayerController>();
                playerList[player] = score;
            }
        }
    }
}
