using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자: 이예린/ 대기창 상점 열고 닫기 관련 기능
/// </summary>
public class Shop : MonoBehaviour
{
    [SerializeField] LayerMask player;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (player.Contain(collision.gameObject.layer))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null && player.photonView.IsMine)
            {
                Inventory.Instance.OpenOutGameInvenUI();
            }

        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (player.Contain(collision.gameObject.layer))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null && player.photonView.IsMine)
            {
                Inventory.Instance.CloseOutGameInvenUI();
            }
        }
    }
}
