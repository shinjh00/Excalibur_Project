using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UserAction
{
    Move_up,
    Move_down,
    Move_left,
    Move_right,
    Attack,
    CommonSkill1,
    CommonSkill2,
    GeneralSkill1,
    GeneralSkill2,
    SuperSkill,
    Interact,
    OpenInventory,
    OpenSetting
}

public class KeyData : MonoBehaviour
{
    //해당 키의 사용처(이름)
    public string keyName;

    //유니티에서 제공하는 KeyCode 값들
    public KeyCode keyCode; //json형태로 저장이 될 때는 KeyCode.I 가 아니라 106(숫자)로 저장이 된다. (enum)

    //KeyData 생성자
    public KeyData(string keyName, KeyCode keyCode)
    {
        this.keyName = keyName;
        this.keyCode = keyCode;
    }
}
