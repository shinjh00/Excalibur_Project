using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Puzzle_4 : Puzzle
{
    [SerializeField] Puzzle4_StartButton startButton;
    [SerializeField] Puzzle4_Button[] buttons;               //3x3 을 이루는 9칸짜리 이동 가능한 타일
    [SerializeField] Puzzle4_Button_1[] edgeButtons;            // 외곽
    [SerializeField] public GameObject checker;              // 움직일 오브젝트
    [SerializeField] GameObject puzzleLight;
    [SerializeField] public bool isStart;
    [SerializeField] int startNr;
    protected override void Start()
    {
        base.Start();
        startButton.puzzle = this;
        buttons = GetComponentsInChildren<Puzzle4_Button>();
        edgeButtons = GetComponentsInChildren<Puzzle4_Button_1>();

        foreach (Puzzle4_Button button in buttons)
        {
            button.Init();
        }

    }


    public void GameStart(PlayerController player)
    {
        player.StateController.StateChange(PlayerState.Interact, 0, 0, true, false);
        player.StateController.StateChange(PlayerState.Puzzle, 0, 0, true, false);           //퍼즐 상태로 변경
        puzzleLight.SetActive(true);
        int r = Random.Range(0, buttons.Length);
        photonView.RPC("TileSerializer", RpcTarget.All, r);
        player.transform.position = startButton.transform.position;             //플레이어 포지션 재위치
        player.MoveController.onMove += MoveChecker;
        //photonView.RPC("Puzzle_4_Start", RpcTarget.All);

    }

    public void MoveChecker()

    {
        if (startButton.player.MoveController.MoveDir == Vector2.zero)
        {
            return;
        }
        Vector2 moveDir = startButton.player.MoveController.MoveDir * 3f;        //움직이는 방향
        Vector3 newCheckerPosition = checker.transform.position + new Vector3(moveDir.x, moveDir.y, 0);

        if (Vector3.Distance(newCheckerPosition, startButton.transform.position) < 0.1f)
        {
            checker.transform.position = startButton.transform.position;
            Exit();
            return;
        }
        // 새로운 위치가 타일 그리드 내에 있는지 확인
        foreach (Puzzle4_Button button in buttons)
        {
            if (Vector3.Distance(newCheckerPosition, button.transform.position) < 0.1f)
            {
                SoundManager.instance.PlaySFX(1650082, audioSource);
                photonView.RPC("UpdateCheckerPosition", RpcTarget.All, button.transform.position);
                break;
            }
        }

        foreach (Puzzle4_Button_1 edgeB in edgeButtons)
        {
            if (Vector3.Distance(newCheckerPosition, edgeB.transform.position) < 0.1f)
            {
                photonView.RPC("UpdateCheckerEdge", RpcTarget.All, edgeB.transform.position);
                break;
            }
        }

    }

    [PunRPC]
    public void TileSerializer(int r)
    {
        startNr = r;
        buttons[r].IsCheck = true;
        isStart = true;
        Debug.Log("isStart true");
    }
    [PunRPC]
    public void ExitPuzzle()
    {
        Debug.Log("IsStart False");
        isStart = false;
    }
    [PunRPC]
    public void UpdateCheckerPosition(Vector3 newPosition)
    {
        checker.transform.position = newPosition;                             // 다른 클라이언트에서 체커 위치 업데이트
        CheckTiles(newPosition);
        CheckComplete();
    }
    [PunRPC]
    public void UpdateCheckerEdge(Vector3 newPos)
    {
        checker.transform.position = newPos;
    }
    void CheckTiles(Vector3 currentButtonPos)
    {
        Vector3[] dirs = new Vector3[]
        {
            Vector3.zero,
            Vector3.up,    // 위쪽
            Vector3.down,  // 아래쪽
            Vector3.left,  // 왼쪽
            Vector3.right  // 오른쪽
        };

        foreach (Vector3 dir in dirs)
        {
            Vector3 checkPosition = currentButtonPos + dir * 3f;
            foreach (Puzzle4_Button button in buttons)
            {
                if (Vector3.Distance(checkPosition, button.transform.position) < 0.1f)
                {
                    button.IsCheck = !button.IsCheck;
                    break;
                }
            }
        }
    }
    void CheckComplete()
    {
        foreach (Puzzle4_Button button in buttons)
        {
            if (!button.IsCheck)
            {
                return;
            }
        }
        if (photonView.IsMine)
        {
            SpawnBox();
        }
        SoundManager.instance.PlaySFX(1650084, audioSource);

        if (startButton.player != null && startButton.player.photonView.IsMine)
        {
            Exit();
        }
        Destroy(startButton.gameObject);
        Destroy(checker.gameObject);



    }
     public void Exit()                                                                         //이쪽 수정해야함 아마 player를 못받아오는듯?
    {
        photonView.RPC("ExitPuzzle", RpcTarget.All);
        startButton.player.HealthController.OnHit.RemoveListener(Exit);
        startButton.player.MoveController.onMove -= MoveChecker;
        startButton.player.CameraMove(startButton.player.transform);
        puzzleLight.SetActive(false);
        startButton.ExitPuzzle();
        startButton.player = null;

    }

}
