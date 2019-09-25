using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public enum TileType {PATH, GRASS, WATER, START, END, EDGE}

public class AStarScript : MonoBehaviour
{
    [SerializeField]
    private Camera mainCam;
    [SerializeField]
    private Tilemap tilemap;
    [SerializeField]
    private LayerMask mask;

    private bool hasStartEnd;
    private Vector3Int startPos, endPos;
    private bool[] startEndTileFlags = new bool[2];
    
    [SerializeField]
    private Tile[] tiles;
    private int currTileType = -1;
    private HashSet<Vector3Int> changedTiles = new HashSet<Vector3Int>();
    private TileType tileType;
    private TileButton prevButton;

    private Node current;
    private HashSet<Node> openList, closedList;
    private Dictionary<Vector3Int, Node> allGridNodes = new Dictionary<Vector3Int, Node>();
    private const int cardinalDistance = 10;
    private const int costToTurn = 5;
    private int count = 0;
    private Stack<Vector3Int> path;
    private Dictionary<Vector3Int, String> initMapState = new Dictionary<Vector3Int, String>();
    [SerializeField]
    private GameObject unit;

    //Detect Start and End tiles at Start of Scene.
    void Start()
    {
        hasStartEndPos();
    }

    //Get Start and End tile positions in tilemap
    private void hasStartEndPos()
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
                        startPos = new Vector3Int(tilemap.origin.x + row, tilemap.origin.y + col, 0);
                        startEndTileFlags[0] = true;
                    } else if (tile.name == "endTile")
                    {
                        endPos = new Vector3Int(tilemap.origin.x + row, tilemap.origin.y + col, 0);
                        startEndTileFlags[1] = true;
                    }
                }
                initMapState.Add(new Vector3Int(tilemap.origin.x + row, tilemap.origin.y + col, 0), tile.name);
            }
        }

        hasStartEnd = true;
        foreach (bool flag in startEndTileFlags)
        {
            if (!flag) {
                hasStartEnd = false;
                print("Map doesn't have start/end tile");
            }
        }
    }

    //Get the cell position of mouseclick in tilemap and change the tile if applicable
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

    //Change tiles on tilemap and record locations
    private void ChangeTile(Vector3Int clickPos)
    {
        Tile tileToChange = (Tile)tilemap.GetTile(clickPos);
        string nameOfTileToChange = (tileToChange.sprite.name);
        if (nameOfTileToChange != "endTile" && nameOfTileToChange != "startTile" && nameOfTileToChange != "obstacleTile01" && currTileType != -1)
        {
            tilemap.SetTile(clickPos, tiles[(int)tileType]);
        } else {
            print("Cannot place tile here.");
        }
    }

    //Select tile type to change tile to.
    public void ChangeTileType(TileButton button)
    {
        Color buttonColor = button.GetComponent<Image>().color;
        if (buttonColor == Color.white)
        {
            button.GetComponent<Image>().color = Color.red;
            tileType = button.myTileType;
            currTileType = (int)tileType;
            if (prevButton != null)
            {
                prevButton.GetComponent<Image>().color = Color.white;
            }
            prevButton = button;
        }
    }

    /* Button functions */
    //Quick Pathfind
    private void CreateQuickPath(bool step)
    {
        if (current == null)
        {
            Initialize();
        }

        while (openList.Count > 0 && path == null)
        {
            List<Node> neighbors = FindNeighbors(current.Position);
            ExamineNeighbors(neighbors, current);
            UpdateCurrentTile(ref current);
            path = GeneratePath(current);
            if(step)
            {
                AStarDebug.MyInstance.Show();
                break;
            }
        }

        AStarDebug.MyInstance.CreateTiles(openList, closedList, allGridNodes, startPos, endPos, path);

        if (path != null)
        {
            foreach (Vector3Int position in path)
            {
                if (position != endPos)
                {
                    tilemap.SetTile(position, tiles[0]);
                }
            }
            // return path;
        }
        
    }

    //Initializes open/closed list to setup pathfinding
    private void Initialize()
    {
        current = GetNode(startPos);
        openList = new HashSet<Node>();
        closedList = new HashSet<Node>();
        openList.Add(current);
    }

    //Get Node associated with a position
    private Node GetNode(Vector3Int position)
    {
        if (!allGridNodes.ContainsKey(position))
        {
            Node node = new Node(position);
            allGridNodes.Add(position, node);
            return node;
        } else
        {
            return allGridNodes[position];
        }
    }

    //Look around tile in cardinal directions; if applicable, add to pool of neighbors to consider. (no diagonals)
    private List<Node> FindNeighbors(Vector3Int parentPosition)
    {
        List<Node> neighbors = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int neighborPos = new Vector3Int(parentPosition.x - x, parentPosition.y - y, parentPosition.z);
                if ((y!=0 && x == 0) || (x != 0 && y == 0))
                {
                    bool isUnWalkable = isUnwalkable(neighborPos);
                    if (neighborPos != startPos && tilemap.GetTile(neighborPos) && !isUnWalkable)
                    {
                        Node neighbor = GetNode(neighborPos);
                        if (x != 0)
                        {
                            neighbor.Direction = "x";
                        } else if (y != 0)
                        {
                            neighbor.Direction = "y";
                        }
                        neighbors.Add(neighbor);
                    }
                }
            }
        }
        return neighbors;
    }

    //Checks whether tile is walkable
    private bool isUnwalkable(Vector3Int position)
    {
        Tile currentTile = (Tile)tilemap.GetTile(position);
        string tileName = (currentTile.sprite.name);
        if(tileName != "obstacleTile01" && tileName != "grassTile")
        {
            return false;
        }
        return true;
    }

    //This takes a list of neighbor nodes and calculates costs of traveling to each neighbor
    //adds to openlist or recalculates costs if neighbor hasn't been seen/has already been seen respectively.
    private void ExamineNeighbors(List<Node> neighbors, Node current)
    {
        for(int i = 0; i < neighbors.Count; i++)
        {
            Node neighbor = neighbors[i];
            int gScore = cardinalDistance;
            if (openList.Contains(neighbor))
            {
                if (current.G + gScore < neighbor.G)
                {
                    CalcValues(current, neighbor, gScore);
                }
            } else if (!closedList.Contains(neighbor))
            {
                CalcValues(current, neighbor, gScore);
                openList.Add(neighbor);
            }
        }
        count += 1;
    }

    //calculates travel cost scores. Heuristic looks at how close current position is to end position and minimizes turns.
    private void CalcValues(Node parent, Node neighbor, int cost)
    {
        neighbor.Parent = parent;
        neighbor.G = parent.G + cost;
        neighbor.Rank = count;
        if (neighbor.Direction == neighbor.Parent.Direction)
        {
            neighbor.H = (Math.Abs(neighbor.Position.x - endPos.x) + Math.Abs(neighbor.Position.y - endPos.y)) * cardinalDistance;
        } else 
        {
            neighbor.H = (Math.Abs(neighbor.Position.x - endPos.x) + Math.Abs(neighbor.Position.y - endPos.y)) * cardinalDistance + costToTurn;
        }
        neighbor.F = neighbor.G + neighbor.H;
    }

    //after deciding which node to move to, updates the current tile path
    private void UpdateCurrentTile(ref Node current)
    {
        openList.Remove(current);
        closedList.Add(current);

        if (openList.Count > 0)
        {
            current = openList.OrderBy(x => x.F).ThenByDescending(x => x.Rank).First();
        }
    }

    //If there's a path from start to end, Make the path.
    private Stack<Vector3Int> GeneratePath(Node current)
    {
        if (current.Position == endPos)
        {
            Stack<Vector3Int> finalPath = new Stack<Vector3Int>();

            while(current.Position != startPos)
            {
                finalPath.Push(current.Position);
                current = current.Parent;
            }
            return finalPath;
        }
        return null;
    }

    //Resets path and debug back to original.
    public void Reset()
    {
        AStarDebug.MyInstance.Reset(allGridNodes);
        AStarDebug.MyInstance.ShowHide();
        foreach(KeyValuePair<Vector3Int, String> pair in initMapState)
        {
            if (pair.Value == "pathTile")
            {
                tilemap.SetTile(pair.Key, tiles[0]);
            } else if (pair.Value == "grassTile")
            {
                tilemap.SetTile(pair.Key, tiles[1]);
            } else if (pair.Value == "obstacleTile01")
            {
                tilemap.SetTile(pair.Key, tiles[5]);
            }
        }

        tilemap.SetTile(startPos, tiles[3]);
        tilemap.SetTile(endPos, tiles[4]);

        allGridNodes.Clear();
        path = null;
        current = null;
    }
}
