using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Puzzle : MonoBehaviourPun
{
    [SerializeField] public LayerMask bothLayer;
    [SerializeField] public LayerMask playerLayer;
    [SerializeField] protected BoxRank itemRank;
    [SerializeField] public AudioSource audioSource;

    protected virtual void Start()
    {
       audioSource = GetComponent<AudioSource>();
        bothLayer = LayerMask.GetMask("Monster") | LayerMask.GetMask("Player");
        playerLayer = LayerMask.GetMask("Player");
    }


    protected virtual void SpawnBox()
    {

        BoxSpawner box = PhotonNetwork.InstantiateRoomObject("ItemBox/BoxSpawner", transform.position, Quaternion.identity).GetComponent<BoxSpawner>();
        //box.SetBoxRank(itemRank);
        box.SpawnBox(BoxRank.Legend, (int)BoxRank.Legend);
    }

}
