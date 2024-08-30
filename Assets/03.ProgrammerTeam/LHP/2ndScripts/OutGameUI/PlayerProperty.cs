using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProperty : MonoBehaviour
{
    [SerializeField] Player player; //우클릭한 대상
    [SerializeField] Button getOut; //추방 버튼
    [SerializeField] Button windowClick;
    [SerializeField] TMP_Text whisperText;
    private void Awake()
    {
        windowClick.onClick.AddListener(Close);
        getOut.onClick.AddListener(GetOut); //추방 버튼에 추방 함수 연결
    }

 /*   private void Start()
    {
        chat ??= FindObjectOfType<Chat>();
    }
    public void SetChat(Chat _chat)
    {
        chat = _chat; //대화창 연결
    }*/
    public void SetPlayer(Player _player)
    {
        if (_player != null) //우클릭 객체가 비어있는지 확인
            player = _player; //있다면 대입
        else
            gameObject.SetActive(false); //없다면 오류기때문에 비활성화
    }

    void Close()
    {
        gameObject.SetActive(false);
    }

    void GetOut()
    {
        //추방시킨다.
        PhotonNetwork.CloseConnection(player);
        //임무를 마쳤기에 비활성화
        gameObject.SetActive(false);
    }
}
