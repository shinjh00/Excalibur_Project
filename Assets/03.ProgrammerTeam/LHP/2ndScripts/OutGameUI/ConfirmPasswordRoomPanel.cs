using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConfirmPasswordRoomPanel : MonoBehaviour
{
    [SerializeField] public Button closeButton;
    [SerializeField] public Button confirmButton;
    [SerializeField] public TMP_InputField passwordInputField;
    RoomEntry roomEntry;


    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
    }

    public void Init(RoomEntry entry)
    {
        roomEntry = entry;
        confirmButton.onClick.AddListener(roomEntry.OnConfirmPassword);
    }
    public void Close()
    {
        gameObject.SetActive(false); 
        if (roomEntry != null)
        {
            // Remove the listener from the confirm button
            roomEntry.RemoveConfirmPasswordListener(roomEntry.OnConfirmPassword);
        }
    }

}
