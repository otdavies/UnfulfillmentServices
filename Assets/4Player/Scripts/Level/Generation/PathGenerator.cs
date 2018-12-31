using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PathGenerator : MonoBehaviour
{
    public GameObject[] tiles;

    private static readonly float HexigonWidthLong = 40;
    private static readonly float HexigonWidthShort = HexigonWidthLong * 0.5f * Mathf.Sqrt(3f);

    private int modelAngleFix = -60;

    private readonly Dictionary<int, Hexigon.Layout> layoutRequirements = new Dictionary<int, Hexigon.Layout>();


    private void Start ()
    {
        StartCoroutine(GenerateTree());
    }

    public bool WaitTillSpace()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }

    public IEnumerator GenerateTree()
    {
        // Variables that change per tile
        int tileDepth = 0;

        Stack<Hexigon> currentHexigonDepth = new Stack<Hexigon>();
        Stack<Hexigon> nextHexigonDepth = new Stack<Hexigon>();

        Hexigon root = new Hexigon(Hexigon.Layouts[0], PositionToHash(Vector3.zero), Vector3.zero, tileDepth);
        currentHexigonDepth.Push(root);
        root = ApplyLayoutConstraints(root);
        SpawnHexigonWorldObject(root);
        tileDepth++;

        while (tileDepth < 6)
        {
            Hexigon cur = currentHexigonDepth.Pop();
            for (int i = 0; i < cur.layout.connections.Length; i++)
            {
                Hexigon.ConnectorType connectionType = cur.layout.connections[i];
                if (!CheckLegalOutput(i, connectionType)) continue;

                Vector3 worldPosition = GenerateHexigonWorldPosition(cur, i);
                int hash = PositionToHash(worldPosition, true);
                Debug.Assert(cur.Hash != hash, "Current hash is equal to new tile hash!");
                Hexigon next = new Hexigon(ChooseLegalLayout(hash, worldPosition), hash, worldPosition, tileDepth);
                next = ApplyLayoutConstraints(next);

                SpawnHexigonWorldObject(next);
                nextHexigonDepth.Push(next);
                yield return new WaitForEndOfFrame();
            }

            // Swap stacks
            if (currentHexigonDepth.Count < 1)
            {
                Stack<Hexigon> swapReference = currentHexigonDepth;
                currentHexigonDepth = nextHexigonDepth;
                nextHexigonDepth = swapReference;
                tileDepth++;
            }
        }
    }

    protected Hexigon ApplyLayoutConstraints(Hexigon hexigon)
    {
        hexigon.layout.isFringe = false;
        layoutRequirements[hexigon.Hash] = hexigon.layout;

        // Surrounding tiles need rules
        for (int i = 0; i < hexigon.layout.connections.Length; i++)
        {
            Hexigon.ConnectorType connectionType = hexigon.layout.connections[i];
            if (!CheckLegalOutput(i, connectionType)) continue;

            Vector3 worldPosition = GenerateHexigonWorldPosition(hexigon, i);
            int hash = PositionToHash(worldPosition);

            if (!layoutRequirements.ContainsKey(hash))
            {
                layoutRequirements[hash] = new Hexigon.Layout(-1, new[] {0, 0, 0, 0, 0, 0});
            }

            Hexigon.Layout result = layoutRequirements[hash];
            if (result.isFringe)
            {
                result.AddConnection((i + 3) % 6, Hexigon.ConnectorType.INPUT);
                layoutRequirements[hash] = result;
            }

            Debug.DrawLine(hexigon.WorldPosition, worldPosition, Color.red, 100);
            Debug.DrawRay(worldPosition, Vector3.up, Color.green, 100);
            Debug.DrawRay(worldPosition, Vector3.up * 50, Color.white, 5);
        }

        return hexigon;
    }

    protected bool CheckLegalOutput(int direction, Hexigon.ConnectorType type)
    {
        return type == Hexigon.ConnectorType.OUTPUT;
    }

    protected Hexigon.Layout ChooseLegalLayout(int hash, Vector3 worldPosition)
    {
        Hexigon.Layout target = layoutRequirements[hash];
        List<Hexigon.Layout> layouts = target.FindMatchingLayouts();
        for (int i = 0; i < layouts.Count; i++)
        {
            Hexigon.Layout layout = layouts[i];
            foreach (Hexigon.ConnectorType connection in layout.connections)
            {
                if (connection == Hexigon.ConnectorType.OUTPUT)
                {
                    int h = PositionToHash(worldPosition, (int) connection);
                    if (layoutRequirements.ContainsKey(h))
                    {
                        if (!layoutRequirements[h].isFringe)
                        {
                            layouts.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }

        return layouts[Random.Range(0, layouts.Count)];
    }

    protected Vector3 GenerateHexigonWorldPosition(Vector3 position, int direction)
    {
        return position + Quaternion.AngleAxis((60 * direction) + modelAngleFix, Vector3.up) * Vector3.forward * HexigonWidthShort;
    }

    protected Vector3 GenerateHexigonWorldPosition(Hexigon hexigon, int direction)
    {
        return hexigon.WorldPosition + Quaternion.AngleAxis((60 * direction) + modelAngleFix, Vector3.up) * Vector3.forward * HexigonWidthShort;
    }

    protected void SpawnHexigonWorldObject(Hexigon hexigon)
    {
        Debug.DrawRay(hexigon.WorldPosition, Vector3.up * 50, Color.yellow, 5);
        if (hexigon.layout.primitiveLayoutType < 0)
        {
            Debug.LogError("Tile type not found");
            return;
        }
        GameObject hexi = Instantiate(tiles[hexigon.layout.primitiveLayoutType], hexigon.WorldPosition + Vector3.up * hexigon.Depth * HexigonWidthLong * 0.1f, Quaternion.AngleAxis((60 * -hexigon.layout.rotationOffset) + 30, Vector3.up));
    }

    protected int PositionToHash(Vector3 position, bool print=false)
    {
        Vector2Int result = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        int hash = result.GetHashCode();

        if (print) Debug.Log(hash + ", " + result + ", " + position);
        return hash;
    }

    protected int PositionToHash(Hexigon parentHexigon, int direction)
    {
        Vector3 position = GenerateHexigonWorldPosition(parentHexigon, direction);
        return new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z)).GetHashCode();
    }

    protected int PositionToHash(Vector3 p, int direction)
    {
        Vector3 position = GenerateHexigonWorldPosition(p, direction);
        return new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z)).GetHashCode();
    }
}
