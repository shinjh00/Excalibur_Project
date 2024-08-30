using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainInformation : MonoBehaviour
{
    [Tooltip("BackButton (에디터 할당 필요)")]
    [SerializeField] Button backButton;

    GameObject mainPanelBlockImage;

    [SerializeField] Image image;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] TMP_Text pageTxt;

    [SerializeField] Sprite[] imageList;
    

    [Header("textList는 3줄까지만")]
    [TextArea]
    [SerializeField] string[] textList;

    [SerializeField] Button leftArrowButton;
    [SerializeField] Button rightArrowButton;

    int pageNum;
    bool isReverse = false;
    CircularQueue<Sprite> queueList;

    private void Awake()
    {


        mainPanelBlockImage = GetComponentInParent<MainPanel>().MainPanelBlockImage;
        queueList = new CircularQueue<Sprite>(imageList.Length);
        foreach (Sprite sprite in imageList)
        {
            if (sprite != null)
            {
                queueList.Enqueue(sprite);
            }
        }
    }

    private void Start()
    {
        pageNum = 0;
        ApplyText();

        leftArrowButton.onClick.AddListener(ClickLeftArrow);
        rightArrowButton.onClick.AddListener(ClickRightArrow);
        ApplyImage();
        backButton.onClick.AddListener(BackButton);
    }

    void ApplyImage()
    {
        image.sprite = queueList.Dequeue();
        queueList.Enqueue(image.sprite);
    }
    private void ApplyText()
    {
        text.text = textList[pageNum];
        pageTxt.text = $"{pageNum + 1} / {queueList.Count}";
    }

    private void ClickLeftArrow()
    {
        Reverse(false);
        ApplyImage();
        if (pageNum >0)
        {
            pageNum--;
        }
        else
        {
            pageNum = queueList.Count - 1;
        }
        ApplyText();
    }

    private void ClickRightArrow()
    {
        Reverse(true);
        ApplyImage();
        if (pageNum < queueList.Count -1 )
        {
            pageNum++;
        }
        else
        {
            pageNum = 0;
        }
        ApplyText();
    }

    private void BackButton()
    {
        gameObject.SetActive(false);
        mainPanelBlockImage.SetActive(false);
        pageNum = 0;
        ApplyText();
    }
    void Reverse(bool b)
    {
        if (isReverse == b)
        {
            queueList.Reverse();
            isReverse = !b;
            ApplyImage();
        }

    }
}
