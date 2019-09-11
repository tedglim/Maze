using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType {GRASS, PATH, WATER, START, END}

public class AStar : MonoBehaviour
{
    private TileType tileType;

    [SerializeField]
    private Tile[] tiles;

    [SerializeField]
    private Camera mainCam;

    [SerializeField]
    private LayerMask mask;

    [SerializeField]
    private Tilemap tilemap;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(mainCam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, mask);
        if (hit.collider != null)
        {
            Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int clickPos = tilemap.WorldToCell(mouseWorldPos);
            ChangeTile(clickPos);
        }
        
        }
    }

    public void ChangeTileType(TileButton button)
    {
        tileType = button.myTileType;
    }

    private void ChangeTile(Vector3Int clickPos)
    {
        tilemap.SetTile(clickPos, tiles[(int)tileType]);
    }

}
