using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
/// <summary>
/// 개발자 : 이형필 / 전장의 안개를 제거 클래스
/// </summary>
public class FowClear : MonoBehaviour

{

    [SerializeField] float fowValue;
    [SerializeField] float rad;
    [SerializeField] LayerMask fowLayer;
    Coroutine fowRoutine;
    [SerializeField] Collider2D[] fowCols;
    [SerializeField] TileBase fowTile;
    Tilemap fowMap;
    Dictionary<Vector3Int, bool> visitedTiles = new Dictionary<Vector3Int, bool>(); // 방문한 타일 기록
    private void Start()
    {
        if (!PhotonNetwork.LocalPlayer.GetProperty<bool>(DefinePropertyKey.OUTGAME))
        {
            fowMap = GameObject.Find("fow")?.GetComponent<Tilemap>();
            fowRoutine = StartCoroutine(FowRoutine());
        }

    }
    /// <summary>
    /// 전장의 안개 루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator FowRoutine()
    {
        while (true)
        {
            // 현재 위치 기준으로 반경 내의 타일들을 검사
            Vector3Int playerCellPosition = GetCellPositionFromWorld(transform.position);
            List<Vector3Int> nearbyCells = GetNearbyCellPositions(playerCellPosition, rad);

            foreach (Vector3Int cellPosition in nearbyCells)
            {// 타일맵에서 해당 위치의 타일을 찾아서 처리

                TileBase tileBase = fowMap.GetTile(cellPosition);
                if (tileBase != null && tileBase is Tile tile)
                {
                    tile.color = Color.clear;
                    fowMap.RefreshTile(cellPosition);
                }
                if (!visitedTiles.ContainsKey(cellPosition))
                {
                    visitedTiles[cellPosition] = true; // 방문한 타일로 기록
                }
            }

            // 방문하지 않은 타일의 알파값을 설정
            foreach (var entry in visitedTiles)
            {
                if (!nearbyCells.Contains(entry.Key))
                {
                    SetTileAlpha(entry.Key, 0.5f);
                }
            }

            yield return new WaitForSeconds(fowValue);
        }
    }

    /// <summary>
    /// 타일 알파값 설정
    /// </summary>
    /// <param name="cellPosition"></param>
    /// <param name="alpha"></param>
    void SetTileAlpha(Vector3Int cellPosition, float alpha)
    {

        TileBase tileBase = fowMap.GetTile(cellPosition);
        if (tileBase != null && tileBase is Tile tile)
        {
            Color tileColor = tile.color;
            tileColor.a = alpha;
            tile.color = tileColor;
            fowMap.RefreshTile(cellPosition);
        }

    }

    /// <summary>
    /// 월드 좌표를 셀 위치로 변환하는 메서드
    /// </summary>
    /// <param name="worldPosition"></param>
    /// <returns></returns>
    Vector3Int GetCellPositionFromWorld(Vector3 worldPosition)
    {
        Tilemap tilemap = GetTilemapAtWorldPosition(worldPosition);
        if (tilemap != null)
        {
            Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
            return cellPosition;
        }
        return Vector3Int.zero;
    }

    /// <summary>
    ///  주어진 위치 주변의 모든 셀 위치(Vector3Int)를 반환하는 메서드
    /// </summary>
    /// <param name="centerCellPosition"></param>
    /// <param name="radius">시야 반지름</param>
    /// <returns></returns>
    List<Vector3Int> GetNearbyCellPositions(Vector3Int centerCellPosition, float radius)
    {
        List<Vector3Int> nearbyCells = new List<Vector3Int>();



        BoundsInt bounds = fowMap.cellBounds;
        for (int x = bounds.xMin; x <= bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y <= bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, centerCellPosition.z);
                if (Vector3Int.Distance(centerCellPosition, cellPosition) <= radius)
                {
                    nearbyCells.Add(cellPosition);
                }
            }
        }


        return nearbyCells;
    }

    /// <summary>
    /// 월드좌표에 해당하는 타일맵을 반환하는 메서드
    /// </summary>
    /// <param name="worldPosition"></param>
    /// <returns></returns>
    Tilemap GetTilemapAtWorldPosition(Vector3 worldPosition)
    {
        Collider2D collider = Physics2D.OverlapPoint(worldPosition, fowLayer);
        if (collider != null)
        {
            return collider.GetComponent<Tilemap>();
        }
        return null;
    }

}
