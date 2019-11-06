using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//        0
//        |
//        v
//       __  
// 5 -> /  \ <- 1
// 4 -> \__/ <- 2
//        ^
//        |
//        3

public struct Hexigon
{
    public enum ConnectorType
    {
        NONE = 0,
        INPUT = 1,
        OUTPUT = 2
    }

    public static Layout[] Layouts = 
    {
        new Layout(0, new[] {0, 0, 0, 2, 0, 0}), // Start tile
        new Layout(1, new[] {1, 0, 0, 2, 0, 0}), // I 
        new Layout(2, new[] {1, 2, 0, 0, 0, 0}), // V right
        new Layout(3, new[] {1, 0, 0, 0, 0, 2}), // V left
        new Layout(4, new[] {1, 0, 2, 0, 0, 0}), // U right
        new Layout(5, new[] {1, 0, 0, 0, 2, 0}), // U left
        new Layout(6, new[] {1, 0, 2, 0, 2, 0}), // 1-2 Y
        new Layout(7, new[] {0, 1, 0, 2, 0, 1}), // 2-1 Y
        new Layout(8, new[] {1, 0, 2, 2, 2, 0}), // 1-3 Ψ
        new Layout(9, new[] {1, 1, 0, 2, 0, 1}), // 3-1 Ψ
        new Layout(10, new[] {1, 0, 2, 0, 0, 1}), // 2-1 λ right
        new Layout(11, new[] {1, 1, 0, 0, 2, 0}), // 2-1 λ left
        new Layout(12, new[] {1, 1, 0, 2, 2, 0}), // 2-2 X
        new Layout(13, new[] {1, 1, 0, 0, 2, 2}), // 2-2 K

    };

    public struct Layout
    {
        public bool isFringe;
        public ConnectorType[] connections;
        public int rotationOffset;
        public int primitiveLayoutType;
        public int inputCount;
        public int outputCount;

        public Layout(int id, int[] connections)
        {
            isFringe = true;
            inputCount = 0;
            outputCount = 0;
            rotationOffset = 0;
            primitiveLayoutType = id;

            this.connections = new ConnectorType[6];
            for (int index = 0; index < connections.Length; index++)
            {
                this.connections[index] = (ConnectorType)connections[index];
                if (this.connections[index] == ConnectorType.INPUT) inputCount++;
                else if (this.connections[index] == ConnectorType.OUTPUT) outputCount++;
            }
        }

        public void Rotate(int amount)
        {
            ConnectorType[] rotatedConnections = new ConnectorType[6];
            rotationOffset += amount;
            for (int index = 0; index < connections.Length; index++)
            {
                int offset = index + amount;
                if (offset < 0) offset += 6;
                else offset %= 6;
                rotatedConnections[index] = this.connections[offset];
            }
            this.connections = rotatedConnections;
        }

        public void AddConnection(int position, ConnectorType type)
        {
            if (connections[position] != ConnectorType.NONE) return;

            connections[position] = type;
            if (type == ConnectorType.INPUT) this.inputCount++;
            else if (type == ConnectorType.OUTPUT) this.outputCount++;
        }

        public bool AreInputsEqual(Layout other)
        {
            int inputMatches = 0;
            for (int i = 0; i < 6; i++)
            {
                if (this.connections[i] != ConnectorType.INPUT) continue;
                if (this.connections[i] == other.connections[i]) inputMatches++;
            }
            return other.inputCount == inputMatches;
        }

        public List<Layout> FindMatchingLayouts()
        {
            Debug.Assert(inputCount != 0, "Input count is zero");
            List<Layout> layouts = new List<Layout>();

            for (int index = 1; index < Layouts.Length; index++)
            {
                Layout staticLayout = Layouts[index];
                if (staticLayout.inputCount != inputCount) continue;
                for (int i = 0; i < 6; i++)
                {
                    staticLayout.Rotate(1);
                    if (AreInputsEqual(staticLayout))
                    {
                        layouts.Add(staticLayout);
                    }
                }
            }

            return layouts;
        }

        public override string ToString()
        {
            string toPrint = "(";
            foreach (ConnectorType type in connections)
            {
                toPrint += (int) type + ", ";
            }
            return toPrint.TrimEnd(' ').TrimEnd(',') + ")";
        }
    }

    public int Hash { get; private set; }
    public int Depth { get; private set; }
    public Layout layout;
    public Vector3 WorldPosition { get; private set; }

    public Hexigon(Layout layout, int hash, Vector3 worldPosition, int depth) : this()
    {
        this.WorldPosition = worldPosition;
        this.Depth = depth;
        this.Hash = hash;
        this.layout = layout;
    }

}
