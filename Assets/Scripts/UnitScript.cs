using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitScript : MonoBehaviour
{
    public Vector3Int startPos, endPos;
    Node current;
    private HashSet<Node> openList, closedList;
    private Dictionary<Vector3Int, Node> allGridNodes = new Dictionary<Vector3Int, Node>();
    private Stack<Vector3Int> path;
    [SerializeField]
    private Tilemap tilemap;
    private int cardinalDistance = 10;
    private int count = 0;
    private int costToTurn = 5;
    private Vector3Int ah;
    private Vector3 intermediary;
    private float offsetX = 0.5f;
    private float offsetY = 0.5f;
    private float speed = .1f;
    private float t = 1.0f;
    Stack<Vector3Int> pathway;
    private bool isMoving;



    // Start is called before the first frame update
    void Start()
    {
        tilemap = this.transform.parent.GetComponent<SpawnManagerScript>().tilemap;
        print("unit start: " + startPos);
        print("unit end: " + endPos);
        pathway = setUpPath();
        // pathway.Pop();

        // transform.Translate(Vector3.left * speed * Time.deltaTime);


        
        // print(ah);
        // print(intermediary);
        // print(this.transform.position);
        // this.transform.position = Vector3.MoveTowards(this.transform.position, intermediary, .1f * Time.deltaTime);
        // if(distance <= 0f)
        // {
        //     if(path.Count > 0)
        //     {
        //         print("continued");
        //         ah = pathway.Pop();
        //         intermediary = new Vector3(ah.x, ah.y, ah.z);
        //     } else
        //     {
        //         path = null;
        //     }
        // }
    }

    void Update()
    {
        if (!isMoving)
        {
            moveSprite();
        }
    }

    private void moveSprite()
    {
        if (t <= 0.0f)
        {
            isMoving = true;
            StartCoroutine(goGo());
            // isMoving = false;
        } else
        {
            t -= Time.deltaTime;
        }
    }

    IEnumerator goGo()
    {
        // print("fucik");
        if (pathway.Count > 0)
        {
            ah = pathway.Pop();
            intermediary = tilemap.CellToWorld(ah);
            intermediary = new Vector3(intermediary.x + offsetX, intermediary.y + offsetY, intermediary.z);
            print(ah);
            print(intermediary);
            this.transform.position = Vector3.MoveTowards(this.transform.position, intermediary, 1.0f);
        }

        yield return new WaitForSeconds(0.1f);
        t = 0.1f;
        isMoving =false;
    }
        // if(t > 5.0f)
        // {
        //     t = 0;
        // } else 
        // {
        //     print("this is dumb");
        //     t += Time.deltaTime;
            
        // }
        // ah = pathway.Pop();
        // intermediary = new Vector3(ah.x + offsetX, ah.y - offsetY, ah.z);
        // this.transform.position = Vector3.MoveTowards(this.transform.position, intermediary, speed * Time.deltaTime);

        // while (pathway.Count > 0)
        // {
            // ah = pathway.Pop();
            // intermediary = new Vector3(ah.x + offsetX, ah.y - offsetY, ah.z);
        //     float distance = Vector3.Distance(intermediary, this.transform.position);
        //     print(distance);
        //     while (distance > 0)
        //     {
                
        //         this.transform.position = Vector3.MoveTowards(this.transform.position, intermediary, 1.0f * Time.deltaTime);
        //         distance = Vector3.Distance(intermediary, this.transform.position);
        //     }
        // }
        // }

    // }

    private Stack<Vector3Int> setUpPath()
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
        if (path != null)
        {
            return path;
        }
        return null;
    }

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
}
