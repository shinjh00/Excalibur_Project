using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    // 상호작용 인터페이스
    // 확정 :
    // 보류 : Box, NPC

    public void Interact(PlayerController player);
}
