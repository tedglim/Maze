using System.Linq;
using System.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public enum TileType {PATH, GRASS, WATER, START, END, EDGE}

public class AStarScript : MonoBehaviour
{
    private const int smallDist = 10;
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
    private Vector3Int startPos, endPos;
    private Node current;
    private HashSet<Node> openList, closedList;
    private Stack<Vector3Int> path;
    // private List<Vector3Int> unwalkableTiles = new List<Vector3Int>();
    private Dictionary<Vector3Int, Node> allNodes = new Dictionary<Vector3Int, Node>();
    // Start is called before the first frame update
    void Start()
    {
        // startPos = new Vector3Int(-3, 4, 0);
        // endPos = new Vector3Int(3, -6, 0);
        getStartEndPos();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(mainCam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, mask);
            if (hit.collider != null)
            {
                Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int clickPos = tilemap.WorldToCell(mouseWorldPos);
                print("mouse clicked");
                ChangeTile(clickPos);
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            print("pressed space");
            Algorithm();
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
                        startPos = new Vector3Int(tilemap.origin.x + row, tilemap.origin.y + col, 0);
                    } else if (tile.name == "endTile")
                    {
                        endPos = new Vector3Int(tilemap.origin.x + row, tilemap.origin.y + col, 0);
                    }
                } else {
                    Debug.Log("row:" + row + " col:" + col + " tile: (null)");
                }
            }
        }
    }

    private void Initialize()
    {
        //add start node
        current = GetNode(startPos);
        print("this is start node: ");
        print(current.Position);
        openList = new HashSet<Node>();
        closedList = new HashSet<Node>();
        openList.Add(current);
    }

    private Node GetNode(Vector3Int position)
    {
        if (allNodes.ContainsKey(position))
        {
            return allNodes[position];
        } else
        {
            Node node = new Node(position);
            allNodes.Add(position, node);
            return node;
        }
    }

    private void Algorithm()
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
        }
        
        // CalcValues(current, )
        AStarDebug.MyInstance.CreateTiles(openList, closedList, allNodes, startPos, endPos, path);
    }

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

    private List<Node> FindNeighbors(Vector3Int parentPosition)
    {
        List<Node> neighbors = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int neighborPos = new Vector3Int(parentPosition.x - x, parentPosition.y -y, parentPosition.z);
                if (y!=0 || x !=0)
                {
                    bool isUnWalkable = isUnwalkable(neighborPos);
                    if (neighborPos != startPos && tilemap.GetTile(neighborPos) && !isUnWalkable)
                    {
                        //if it's not an edge tile, and it's also not a grass tile
                        Node neighbor = GetNode(neighborPos);
                        neighbors.Add(neighbor);
                    }
                }
            }
        }
        return neighbors;
    }

    private void ExamineNeighbors(List<Node> neighbors, Node current)
    {
        for(int i = 0; i < neighbors.Count; i++)
        {
            Node neighbor = neighbors[i];
            int gScore = DetermineGScore(neighbors[i].Position, current.Position);
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
            // openList.Add(neighbors[i]);
            // CalcValues(current, neighbors[i], gScore);
        }
    }

    private void CalcValues(Node parent, Node neighbor, int cost)
    {
        neighbor.Parent = parent;
        neighbor.G = parent.G + cost;
        neighbor.H = (Math.Abs(neighbor.Position.x - endPos.x) + Math.Abs(neighbor.Position.y - endPos.y)) * smallDist;
        neighbor.F = neighbor.G + neighbor.H;
    }

    private int DetermineGScore(Vector3Int neighbor, Vector3Int current)
    {
        int gScore = 0;
        int x = current.x - neighbor.x;
        int y = current.y - neighbor.y;

        if (Math.Abs(x-y) % 2 == 1)
        {
            gScore = 10;
        } else {
            gScore = 14;
        }
        return gScore;
    }

//need to change original value of node
    private void UpdateCurrentTile(ref Node current)
    {
        openList.Remove(current);
        closedList.Add(current);

        if (openList.Count > 0)
        {
            current = openList.OrderBy(x => x.F).First();
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
        print("hello");
        Tile tileToChange = (Tile)tilemap.GetTile(clickPos);
        // if(tileType == TileType.WATER)
        // {
        //     print("water");
        // } else if (tileType == TileType.GRASS)
        // {
        //     print("Grass");
        // } else if (tileType == TileType.EDGE)
        // {
        //     print("edge");
        // } else 
        // {
        //     print("this never triggered");
        // }

        // if (tileToChange == TileType.GRASS)
        // {
        //     print("grass");
        // }
        string nameOfTileToChange = (tileToChange.sprite.name);
        print(nameOfTileToChange);
        if (nameOfTileToChange != "endTile" && nameOfTileToChange != "startTile" && nameOfTileToChange != "obstacleTile01" && initTileType != -1)
        {
            tilemap.SetTile(clickPos, tiles[(int)tileType]);
        } else {
            print("Cannot place tile here.");
        }
    }

    private bool isUnwalkable(Vector3Int position)
    {
        Tile currentTile = (Tile)tilemap.GetTile(position);
        string tileName = (currentTile.sprite.name);
        if (tileName == "obstacleTile01" || tileName == "grassTile")
        {
            return true;
        }
        return false;
    
        //get a tile  from the position, 
        //check if it's an unwalkable tile
        //if it's unwalkable (edge/grass), return true
        //else false
        // return false;
    }
}
