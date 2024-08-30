using UnityEngine;
using UnityEngine.EventSystems;

public class ReadyButtonCatcher : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{



    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("button Down");
        GameManager.Ins.buttonDown = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameManager.Ins.buttonDown = false;
        Debug.Log("button Up");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        GameManager.Ins.buttonDown = false;
    }

}
