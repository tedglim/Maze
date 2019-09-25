using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Unit01ParentScript : MonoBehaviour
{
    private Stack<Vector3> pathAlgo, path2Pop;
    public Vector3Int startPos, endPos;
    public Tilemap tilemap;
    Node current;
    private HashSet<Node> openList, closedList;
    private Dictionary<Vector3Int, Node> allGridNodes = new Dictionary<Vector3Int, Node>();
    private const int cardinalDistance = 10;
    private const int costToTurn = 5;
    private int count = 0;
    private float speed = 3.0f;
    private Vector3 destination;

    // Start is called before the first frame update
    void Start()
    {
        tilemap = this.transform.parent.GetComponent<SpawnManagerScript>().tilemap;
        path2Pop = getPath();
        destination = path2Pop.Pop();
    }


    private Stack<Vector3> getPath()
    {
        if (current == null)
        {
            Initialize();
        }

        while (openList.Count > 0 && pathAlgo == null)
        {
            List<Node> neighbors = FindNeighbors(current.Position);
            ExamineNeighbors(neighbors, current);
            UpdateCurrentTile(ref current);
            pathAlgo = GeneratePath(current);

        }
        return pathAlgo;
    }

    private void Initialize()
    {
        current = GetNode(startPos);
        openList = new HashSet<Node>();
        closedList = new HashSet<Node>();
        openList.Add(current);
    }

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
    private Stack<Vector3> GeneratePath(Node current)
    {
        if (current.Position == endPos)
        {
            Stack<Vector3> finalPath = new Stack<Vector3>();

            while(current.Position != startPos)
            {
                Vector3 temp = tilemap.CellToWorld(current.Position);
                finalPath.Push(temp);
                current = current.Parent;
            }
            return finalPath;
        }
        return null;
    }

    // Update is called once per frame
    void Update()
    {
        float dist = Vector2.Distance(destination, transform.position);
        if(dist <= 0)
        {
            if(path2Pop.Count > 0)
            {
                destination = path2Pop.Pop();
            } else {
                Destroy(this.gameObject);
            }
        }
        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
    }
}
