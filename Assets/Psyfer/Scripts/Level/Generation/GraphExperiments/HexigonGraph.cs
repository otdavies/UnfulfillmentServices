using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using Random = UnityEngine.Random;

public class HexigonGraph : MonoBehaviour
{

    public int generationDepth = 10;
    public int width = 3;
    public int start = 10;

    private void Start ()
    {
        GenerateGraph();
    }

    private void GenerateGraph()
    {
        int depth = 0;
        NodeLayer current = null;
        while (depth < generationDepth)
        {
            current = new NodeLayer(current, width, depth);

            if (current.prev != null)
            {

                for (int target = 0; target < width; target++)
                {
                    List<int> legal = current.FindPossibleEdges(target);
                    if(legal.Count > 0) current.ConnectNodes(target, legal[Random.Range(0, legal.Count)]);
                }
            
                //current.TrimOverlappingEdges();
                //current.TrimDoubleDiagonals();

                for (int i = 0; i < current.prev.nodes.Length; i++)
                {
                    Node node = current.prev.nodes[i];
                    node.Active = node.isConnected;
                }
            }
            else
            {
                current.SetNodeActive(start);
            }


            for (int i = 0; i < current.nodes.Length; i++)
            {
                current.nodes[i].Show();
            }

            current.Show();
            depth++;
        }
    }
}

public class NodeLayer
{
    public NodeLayer prev;
    public Node[] nodes;

    public NodeLayer(NodeLayer prev, int width, int depth)
    {
        this.prev = prev;

        // Initialize nodes
        nodes = new Node[width];
        for (int i = 0; i < width; i++)
        {
            nodes[i] = new Node(prev, i, depth);
        }
    }

    public void ConnectNodes(int connectingFrom, int connectingTo)
    {
        Node from = nodes[connectingFrom];
        int localSpaceConnectingTo = from.ToLocalEdgeSpace(connectingTo);

        from.edge[localSpaceConnectingTo] = true;

        SetNodeActive(connectingFrom);
        SetNodeConnected(connectingTo);
    }

    public List<int> FindPossibleEdges(int self)
    {
        List<int> edges = new List<int>(3);
        for (int i = 0; i < prev.nodes.Length; i++)
        {
            if (Math.Abs(i - self) < 2 && prev.nodes[i].Active)
            {
                edges.Add(i);
            }
        }

        return edges;
    }

    public void SetNodeActive(int val)
    {
        nodes[val].Active = true;
    }

    public void SetNodeConnected(int val)
    {
        prev.nodes[val].isConnected = true;
    }

    public void TrimOverlappingEdges()
    {
        for (int i = 1; i < nodes.Length - 1; i++)
        {
            bool leftCross = nodes[i].edge[0] && nodes[i - 1].edge[2];
            bool rightCross = nodes[i].edge[2] && nodes[i + 1].edge[0];
            if(leftCross) nodes[i].edge[0] = false;
            if(rightCross) nodes[i].edge[2] = false;
        }
    }

    public void TrimDoubleDiagonals()
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            if ((nodes[i].edge[0] || nodes[i].edge[2]) && prev.nodes[i].Active) nodes[i].Active = false;
        }
    }

    public void Show()
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            Debug.DrawRay(Vector3.forward * nodes[i].depth + Vector3.right * i, Vector3.up * 0.1f, nodes[i].Active ? Color.green : Color.red, 10000);
        }
    }
}

public class Node
{
    public NodeLayer prev;

    public bool[] edge;
    public int depth;
    public int weight;
    public int position;
    public bool isConnected;

    private bool active;
    public bool Active
    {
        get { return active; }
        set
        {
            active = value;
            if (active == false)
            {
                for (int index = 0; index < edge.Length; index++)
                {
                    edge[index] = false;
                }
            }
        }
    }


    public Node(NodeLayer prev, int position, int depth)
    {
        this.prev = prev;
        this.position = position;
        this.depth = depth;

        this.active = false;
        this.isConnected = false;
        this.edge = new bool[]{false, false, false};
        this.weight = 0;
    }

    public bool BoundsCheck(int index)
    {
        return Math.Abs(index - position) < 2 && index >= 0 && index < prev.nodes.Length;
    }

    public int ToLocalEdgeSpace(int p)
    {
        return (p - position) + 1;
    }

    public int ToGlobalEdgeSpace(int index)
    {
        return index + (position - 1);
    }

    public void Show()
    {
        if (prev != null)
        {
            for (int index = 0; index < edge.Length; index++)
            {
                if (edge[index])
                {
                    Debug.DrawLine(Vector3.forward * depth + Vector3.right * position, Vector3.forward * prev.nodes[index].depth + Vector3.right * prev.nodes[ToGlobalEdgeSpace(index)].position, Color.cyan, 10000);
                }
            }
        }
    }
}
