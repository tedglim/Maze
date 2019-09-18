using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStarDebug : MonoBehaviour
{

    private static AStarDebug instance;

    public static AStarDebug MyInstance
    {
        get
        {
            if(instance == null)
            {
                instance = FindObjectOfType<AStarDebug>();
            }
            return instance;
        }
    }

    [SerializeField]
    private Grid grid;

    [SerializeField]
    private Tilemap tilemap;

    [SerializeField]
    private Tile tile;

    [SerializeField]
    private Color openColor, closedColor, pathColor, currentColor, startColor, goalColor;

    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private GameObject debugTextPrefab;

    private List<GameObject> debugObject = new List<GameObject>();


    public void CreateTiles(HashSet<Node> openList, HashSet<Node> closedList, Dictionary<Vector3Int, Node> allNodes, Vector3Int start, Vector3Int goal, Stack<Vector3Int> path = null)
    {
        print("thisis oG start position: ");
        print(start);
        
        // print(startColor);
        foreach (Node node in openList) 
        {
            print("thisis recorded node position");
            print(node.Position);
            ColorTile(node.Position, openColor);
            
        }
        foreach (Node node in closedList)
        {
            ColorTile(node.Position, closedColor);
        }
        
        if (path != null)
        {
            foreach(Vector3Int pos in path)
            {
                if (pos != start && pos != goal)
                {
                    ColorTile(pos, pathColor);
                }
            }
        }

        // tilemap.SetTile(start, tile);
        // tilemap.SetTile(start, tile);
        ColorTile(start, startColor);
        ColorTile(goal, goalColor);

        foreach(KeyValuePair<Vector3Int, Node> node in allNodes)
        {
            if (node.Value.Parent != null)
            {
                GameObject go = Instantiate(debugTextPrefab, canvas.transform);
                go.transform.position = grid.CellToWorld(node.Key);
                debugObject.Add(go);
                GenerateDebugText(node.Value, go.GetComponent<DebugTextScript>());
            }
        }
    }

    private void GenerateDebugText(Node node, DebugTextScript debugText)
    {
        debugText.P.text = $"P:{node.Position.x},{node.Position.y}";
        debugText.F.text = $"F:{node.F}";
        debugText.G.text = $"G:{node.G}";
        debugText.H.text = $"H:{node.H}";

        if (node.Parent.Position.x >= node.Position.x || node.Parent.Position.y != node.Position.y)
        {
            if (node.Parent.Position.x < node.Position.x && node.Parent.Position.y > node.Position.y)
            {
                debugText.MyArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 135));
            }
            else if (node.Parent.Position.x < node.Position.x && node.Parent.Position.y < node.Position.y)
            {
                debugText.MyArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 225));
            }
            else if (node.Parent.Position.x > node.Position.x && node.Parent.Position.y == node.Position.y)
            {
                debugText.MyArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            }
            else if (node.Parent.Position.x > node.Position.x && node.Parent.Position.y > node.Position.y)
            {
                debugText.MyArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 45));
            }
            else if (node.Parent.Position.x > node.Position.x && node.Parent.Position.y < node.Position.y)
            {
                debugText.MyArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, -45));
            }
            else if (node.Parent.Position.x == node.Position.x && node.Parent.Position.y > node.Position.y)
            {
                debugText.MyArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
            }
            else if (node.Parent.Position.x == node.Position.x && node.Parent.Position.y < node.Position.y)
            {
                debugText.MyArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 270));
            }
        }
        else
        {
            debugText.MyArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 180));
        }
    }

    public void ColorTile(Vector3Int position, Color color)
    {
        // print(position);
        tilemap.SetTile(position, tile);
        tilemap.SetTileFlags(position, TileFlags.None);
        // print(tile);
        tilemap.SetColor(position, color);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
