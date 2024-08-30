using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum GridType
    {
        HallWay = -2,
        None = -1,
        MainRoom,
        ExcaliburRoom = 1,
        UpperRoom = 2,
        Obs = 3,
        PuzzleRoom = 4
    }

    public enum CardType
    {
        Summon = 0,
        Magic,
        None
    }

    public enum ActorID
    {
        Unknown = 0,
        GoldenGoblin,
        Witch,
        Subordinate,
        Boss
    }

    public enum BuffType
    {
        Atk,
        Spd,
        Def
    }

    public enum SFXSoundType
    {
        Paper,
        Coin,
        Throw,
        Fragile,
        Place,
    }

    public enum BgmType
    {
        Main,
        Game,
        NPC1
    }

    public enum ButtonSoundType
    {
        ClickButton,
        ShowButton
    }

    public enum DialogSoundType
    {
        LongWrite,
        MediumWrite,
        ShortWrite,
        SelectChange,
        SelectChoose
    }

    public enum EffectSoundType
    {
        Hit,
        Fire,
        Coin
    }
    public enum RoomType
    {
        PlayerSpawnRoom,
        ExcaliburRoom,
        BeginningRoom,
        LateRoom,
        EndRoom,
        PuzzleRoom
       
    }
    public enum NpcSoundType
    {
        Goblin = 0,
        Witch,
        Subordinate,
        Boss,
        Merchant
    }

    public enum AtkType
    {
        Slash = 0,
        Bow,
        Ice,
        Thunder,
        Blood
    }
}