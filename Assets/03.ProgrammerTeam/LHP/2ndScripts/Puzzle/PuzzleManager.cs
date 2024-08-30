using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public void PuzzleInit()
    {
        List<Vector2> posList = SecondMapGen.Ins.GetRoomPosAll(Define.RoomType.PuzzleRoom);

        Dictionary<int, PuzzleData> puzzleData = CsvParser.Instance.PuzzleDic;              //테이블 딕셔너리 캐싱
        foreach (Vector2 pos in posList)
        {

            int randomKey = puzzleData.GetRandomKey();
            PuzzleData selectedPuzzle = puzzleData[randomKey];

            Debug.Log($"Create : {pos} puzzle : {selectedPuzzle.id}");
            CreatePhotonObject.Create(selectedPuzzle.id, pos);

        }
    }


}
