using UnityEngine;

public class Tent : MonoBehaviour
{
    [SerializeField] LayerMask player;
    [SerializeField] MaintenanceController maintenanceController;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (player.Contain(collision.gameObject.layer))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null && player.photonView.IsMine)
            {
                CharacterSelectUI(true);
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
                CharacterSelectUI(false);
            }
        }
    }


    void CharacterSelectUI(bool b)
    {
        maintenanceController.gameObject.SetActive(b);
    }
}
