using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

public class SpawnManagerScript : MonoBehaviour
{
    // [SerializeField]
    public Tilemap tilemap;
    [SerializeField]
    private GameObject unit;
    private int count;
    private float offsetX = 1.0f;
    private float offsetY = .5f;
    private Vector3Int startPos, endPos;

    // Start is called before the first frame update
    void Start()
    {
        count = 0;
    }

    // Update is called once per frame
    void Update()
    {
        SpawnUnit();
    }

    private void SpawnUnit ()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (count > 0)
            {
                return;
            }
            print("hey");
            getStartEndPos();
            Vector3 place = tilemap.CellToWorld(startPos);
            place = new Vector3(place.x + .35f, place.y + .5f, startPos.z);
            GameObject thing = Instantiate(unit, place, Quaternion.identity) as GameObject;
            thing.transform.parent = this.transform;
            UnitScript uScript = thing.GetComponent<UnitScript>();
            uScript.startPos = startPos;
            uScript.endPos = endPos;
            count += 1;
        }
    }

    private void getStartEndPos()
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
                        print("row: " + row);
                        print("col: " + col);
                        startPos = new Vector3Int(tilemap.origin.x + row, tilemap.origin.y + col, 0);
                    }
                    else if (tile.name == "endTile")
                    {
                        print("row: " + row);
                        print("col: " + col);
                        endPos = new Vector3Int(tilemap.origin.x + row, tilemap.origin.y + col, 0);
                    }
                }
            }
        }
    }

}
