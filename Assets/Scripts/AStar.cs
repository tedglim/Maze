using System.Net.Mime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

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

    private TileButton prevButton;
    private int initTileType = -1;
    private Vector3Int startPos;
    private Vector3Int endPos;
    
    void Start()
    {
        tilemap.CompressBounds();
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        for (int x = 0; x < bounds.size.x; x++) {
            for (int y = 0; y < bounds.size.y; y++) {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null) {
                    Debug.Log("x:" + x + " y:" + y + " tile:" + tile.name);
                    if (tile.name == "startTile")
                    {
                        startPos = new Vector3Int(x, y, 0);
                    } else if (tile.name == "endTile")
                    {
                        endPos = new Vector3Int(x,y, 0);
                    }
                } else {
                    Debug.Log("x:" + x + " y:" + y + " tile: (null)");
                }
            }
        }  
        print(tilemap.cellBounds);
        print(startPos);
        print(endPos);
        // print(tilemap.)
    }
    
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
        Color buttonColor = button.GetComponent<Image>().color;
        if (buttonColor == Color.white)
        {
            button.GetComponent<Image>().color = Color.red;
            tileType = button.myTileType;
            initTileType = (int)tileType;
            if (prevButton != null)
            {
                prevButton.GetComponent<Image>().color = Color.white;
            }
            prevButton = button;
        }
    }

    private void ChangeTile(Vector3Int clickPos)
    {
        Tile tileToChange = (Tile)tilemap.GetTile(clickPos);
        String nameOfTileToChange = (tileToChange.sprite.name);
        // if(tileType == TileType.START) {
        //     print("start tile");
        // }
        if (nameOfTileToChange != "endTile" && nameOfTileToChange != "startTile" && nameOfTileToChange != "Water" && initTileType != -1)
        {
            tilemap.SetTile(clickPos, tiles[(int)tileType]);
        } else {
            print("Cannot place tile here.");
        }
    }
}
