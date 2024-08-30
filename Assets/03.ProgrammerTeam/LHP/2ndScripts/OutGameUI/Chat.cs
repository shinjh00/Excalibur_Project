using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using WebSocketSharp;

public class Chat : MonoBehaviourPun
{
    public enum ChatType { ALL, TARGET, TEAM, NEW, MY, END };
    [SerializeField] Button chatResetButton;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TMP_Text text;
    [SerializeField] RectTransform content;
    [SerializeField] ScrollRect chatPanel;
    [SerializeField] TMP_Text chatTextPrefab;
    [SerializeField] ChatType chatTarget;
    [SerializeField] float bubblingTime;
    [SerializeField] Vector2 validChatRange;
    [SerializeField] LayerMask playerLayer;
    Player currentMessageTarget;
    Coroutine chatRoutine;
    Coroutine chatBubbleRoutine;
    Collider2D[] targets;


    private void Awake()
    {
        targets = new Collider2D[PhotonNetwork.PlayerList.Length];
        playerLayer = LayerMask.GetMask("Player");
        if (chatResetButton != null)
            chatResetButton.onClick.AddListener(RemoveEntry);
    }

    private void Update()
    {
        if (!GameManager.Ins.onChat && text.text != "")
        {
            inputField.DeactivateInputField();
            inputField.text = "";
            text.text = "";
        }

    }
    string RemoveUnderlineTags(string input)
    {
        // <u>와 </u> 태그를 제거
        return input.Replace("<u>", "").Replace("</u>", "");
    }

    private void OnEnable()
    {
        chatTarget = ChatType.ALL;
        //합류 메시지 전송

        // 포톤 서버에 접속하지 않아도 RPC가 실행되며 오류가 남.
        // photonView.RPC("AddMessage", RpcTarget.All, $"{PhotonNetwork.LocalPlayer.NickName}이 입장하셨습니다.", ChatType.NEW);
    }
    private void OnDisable()
    {
        RemoveEntry();  //모든 대화목록 삭제(대화 객체 파괴)
    }

    public void LeftPlayer(Player otherPlayer)
    {
        //현재 귓속말 상대가 방을 나갔다면
        if (currentMessageTarget != null && currentMessageTarget.ActorNumber == otherPlayer.ActorNumber)
        {
            //타겟을 비우고 채팅 타입을 전체채팅으로 변경한다.
            currentMessageTarget = null;
            chatTarget = ChatType.ALL;
        }
        AddMessage($"{otherPlayer.NickName}이 퇴장하였습니다.", Chat.ChatType.NEW, PhotonNetwork.LocalPlayer);
    }
    public bool isTest(Player player)
    {
        if (player == currentMessageTarget)
            return true;
        else
            return false;
    }
    public void SendTarget(Player target)
    {
        //타겟이 비어있다면 덮어쓴다.
        if (currentMessageTarget == null)
            currentMessageTarget = target;
        else //타겟이 현재 설정되어있는 타겟과 동일하다면 비운다.
            currentMessageTarget = currentMessageTarget.ActorNumber == target.ActorNumber ? null : target;
        //현재 타겟이 비워저있으면 전체채팅으로 있다면 귓속말로 설정한다.
        chatTarget = currentMessageTarget == null ? ChatType.ALL : ChatType.TARGET;
    }
    void SendChat()
    {
        string ChatContent = RemoveUnderlineTags(text.text);
        Debug.Log($"Send : {ChatContent}");
        if(string.IsNullOrEmpty(ChatContent))               //확인해야함 비어있는데도 입력됨
        {
            Debug.Log("아무것도 입력 안함");
            return;

        }
        else
        {
            Debug.Log(ChatContent);
            SendMessageToTarget(ChatContent);
        }
    }

    void OnChat(InputValue value)
    {
        if (!PhotonNetwork.LocalPlayer.GetProperty<bool>(DefinePropertyKey.OUTGAME))
        {
            chatRoutine = StartCoroutine(ChatViewRoutine());
            // inputField.gameObject.SetActive(false);
        }
        if (GameManager.Ins.m_Player != null)
        {
            if (!GameManager.Ins.onChat)
            {

                Debug.Log("OnChat");
                GameManager.Ins.onChat = true;
                inputField.ActivateInputField();
                GameManager.Ins.m_Player.StateController.StateChange(PlayerState.Chat, 0, 0, true, false);

            }
            else
            {
                SendChat();
                Debug.Log("OffChat");
                GameManager.Ins.onChat = false;
                inputField.DeactivateInputField();
                GameManager.Ins.m_Player.StateController.StateChange(PlayerState.Chat, 0, 0, false, false);


            }
        }
    }
    [PunRPC]
    public void AddMessage(string message, ChatType chatType, Player player)
    {
        
        PlayerController playerController = player.GetPlayerController();
        Debug.Log($"Player : {player} PlayerController Collect : {playerController}");

        TMP_Text newMessage = Instantiate(chatTextPrefab);

        switch (chatType) //타입에 따라 폰트 색상 변경
        {
            case ChatType.ALL:
                newMessage.color = Color.white;
                newMessage.outlineWidth = 0.15f;
                newMessage.outlineColor = Color.black;
                break;
            case ChatType.TARGET:
                newMessage.color = Color.red;
                break;
            case ChatType.TEAM:
                newMessage.color = Color.blue;
                break;
            case ChatType.MY:
                newMessage.color = Color.green;
                break;
            case ChatType.NEW:
                newMessage.color = Color.yellow;
                newMessage.fontStyle |= FontStyles.Bold; //폰트를 두껍게 설정
                break;
        }


            string chatPanelMessage = $"{player.NickName}({player.GetProperty<ClassType>(DefinePropertyKey.CHARACTERCLASS)}) : {message}";
        if (chatType == ChatType.NEW)
        { chatPanelMessage = $"{message}"; }


            newMessage.text = chatPanelMessage;  //내용 입력
        newMessage.transform.SetParent(content); //객체를 콘텐츠의 자식으로 설정

        RectTransform rect = newMessage.transform as RectTransform;
        if (rect != null) // 스케일을 1로 변경
            rect.localScale = Vector3.one;

        chatPanel.verticalScrollbar.value = 0; //스크롤 뷰를 맨 밑으로 설정

        message = message.FormatMessage();
        if(chatType == ChatType.ALL || chatType == ChatType.MY)
        {
            if (chatBubbleRoutine != null)
            {
                StopCoroutine(chatBubbleRoutine);
            }
            if (playerController.chatBubbleRoutine != null)
            {
                StopCoroutine(playerController.chatBubbleRoutine);
                Debug.Log("StopFadeOut");
            }
            chatBubbleRoutine = StartCoroutine(ChatBubbling(message, playerController));

        }

    }

    IEnumerator ChatViewRoutine()
    {
        if (chatRoutine != null)
            StopCoroutine(chatRoutine);
        chatPanel.gameObject.SetActive(true);
        yield return new WaitForSeconds(10f);
        chatPanel.gameObject.SetActive(false);

    }

    void SendMessageToTarget(string chat)
    {

        targets = Physics2D.OverlapBoxAll(GameManager.Ins.m_Player.transform.position, validChatRange, 0, playerLayer);
        PhotonView targetPv = null;



        switch (chatTarget)
        {
            case ChatType.ALL:
                for (int i = 0; i < targets.Length; i++)
                {

                    targetPv = targets[i].GetComponent<PhotonView>();
                    if (targetPv.Owner == PhotonNetwork.LocalPlayer)
                    {
                        continue;
                    }
                    photonView.RPC("AddMessage", targetPv.Owner, chat, ChatType.ALL, PhotonNetwork.LocalPlayer);

                }

                break;
            case ChatType.TARGET:
                photonView.RPC("AddMessage", currentMessageTarget, $"[Whisper]  {chat}", ChatType.TARGET, PhotonNetwork.LocalPlayer);

                break;
            case ChatType.TEAM:
                SendTeam(chat);
                break;
        }


        if (chatTarget != ChatType.TARGET)
            AddMessage(chat, ChatType.MY, PhotonNetwork.LocalPlayer);
        else
            AddMessage($"[MY] {chat}", ChatType.TARGET, PhotonNetwork.LocalPlayer);

    }
    void SendTeam(string chat)
    {
        foreach (Player team in PhotonNetwork.PlayerList)
        {
            if (team.GetProperty<bool>(DefinePropertyKey.RED) == PhotonNetwork.LocalPlayer.GetProperty<bool>(DefinePropertyKey.RED) ||
               team.GetProperty<bool>(DefinePropertyKey.BLUE) == PhotonNetwork.LocalPlayer.GetProperty<bool>(DefinePropertyKey.BLUE))
            {
                photonView.RPC("AddMessage", team, chat, ChatType.TEAM);
            }
        }
    }

    void RemoveEntry()
    {
        for (int i = 0; i < content.childCount; i++)
            Destroy(content.GetChild(i).gameObject);
    }

    IEnumerator ChatBubbling(string s, PlayerController player)
    {
        Debug.Log($"bubble : {player}");

        player.chatBubble.SetText(s,player.photonView.Owner);
        player.chatBubble.gameObject.SetActive(true);
        yield return new WaitForSeconds(bubblingTime);

        player.chatBubbleRoutine = StartCoroutine(player.chatBubble.FadeOutTextAndImage());
    }

}


public enum ChatColor { }
