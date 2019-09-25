using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpawnManagerScript : MonoBehaviour
{
    public Tilemap tilemap;
    [SerializeField]
    private GameObject unit;
    private int count = 0;
    Vector3Int start,end;
    // Start is called before the first frame update
    void Start()
    {
        getStartPos();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && count < 1)
        {
            Vector3 startPos = tilemap.CellToWorld(start);
            GameObject unitObj = Instantiate(unit, startPos, Quaternion.identity) as GameObject;
            unitObj.transform.parent = this.transform;
            Unit01ParentScript script = unitObj.GetComponent<Unit01ParentScript>();
            // script.tilemap = tilemap;
            script.startPos = start;
            script.endPos = end;
            count += 1;
        }
    }

    private void getStartPos()
    {
        tilemap.CompressBounds();
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        for (int row = 0; row < bounds.size.x; row++) {
            for (int col = 0; col < bounds.size.y; col++) {
                TileBase tile = allTiles[row + col * bounds.size.x];
                if (tile != null) {
                    if (tile.name == "startTile")
                    {
                        start = new Vector3Int(tilemap.origin.x + row, tilemap.origin.y + col, 0);
                    } else if (tile.name == "endTile")
                    {
                        end = new Vector3Int(tilemap.origin.x + row, tilemap.origin.y + col, 0);
                    }
                }
            }
        }
    }
}
