using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Data;
using System;
using System.Diagnostics;

namespace NEAT_AI.Models;

public class Brain() {
    public List<Node> Nodes { get; set; } = new();
    public List<Connection> Connections { get; set; } = new();
    public float Fitness { get; set; } = 0;
    public float AdjustedFitness { get; set; } = 0; // Species Size Divided by Fitness
    public int SpeciesID { get; set; } = 0;

    public override string ToString() {
        return $"Nodes: {Nodes.Count}, Conns: {Connections.Count}, Fitness: {Fitness}, Species: {SpeciesID}";
    }

    public Brain Clone() {
        var json = JsonSerializer.Serialize(this);
        return JsonSerializer.Deserialize<Brain>(json);
    }

    public void Initialize(int InputNodes, int OutputNodes, int HiddenNodes, float procentConn) {
        int nodeID = 1;

        for (int i = 0; i < InputNodes - 1; i++) {
            Nodes.Add(new Node(nodeID++, NodeTypeEnum.Input, 1, 0, 0));
        }
        Nodes.Add(new Node(nodeID++, NodeTypeEnum.Bias, 1, 0, 1));

        for (int i = 0; i < HiddenNodes; i++) {
            Nodes.Add(new Node(nodeID++, NodeTypeEnum.Hidden, 2, 0, 0));
        }

        for (int i = 0; i < OutputNodes; i++) {
            Nodes.Add(new Node(nodeID++, NodeTypeEnum.Output, 3, 0, 0));
        }

        var inputList = Nodes.Where(x => x.NodeType == NodeTypeEnum.Input || x.NodeType == NodeTypeEnum.Bias).ToList();
        var outputList = Nodes.Where(x => x.NodeType == NodeTypeEnum.Output).ToList();
        var hiddenList = Nodes.Where(x => x.NodeType == NodeTypeEnum.Hidden).ToList();

        //Connections
        Random random = new Random();
        if (hiddenList.Count > 0) {
            // Input -> Hidden
            foreach (var hidden in hiddenList) {
                foreach (var input in inputList) {
                    if (random.NextDouble() <= procentConn) {
                        float weight = (float)(random.NextDouble() * 40 - 20);
                        Connections.Add(new Connection(input.NodeID * 1000 + hidden.NodeID, input.NodeID, hidden.NodeID, weight, true, false));
                    }
                }
            }
            // Hidden -> Output
            foreach (var output in outputList) {
                foreach (var hidden in hiddenList) {
                    if (random.NextDouble() <= procentConn) {
                        float weight = (float)(random.NextDouble() * 40 - 20);
                        Connections.Add(new Connection(hidden.NodeID * 1000 + output.NodeID, hidden.NodeID, output.NodeID, weight, true, false));
                    }
                }
            }
        }
        else {
            // Input -> Output
            foreach (var output in outputList) {
                foreach (var input in inputList) {
                    if (random.NextDouble() <= procentConn) {
                        float weight = (float)(random.NextDouble() * 40 - 20);
                        Connections.Add(new Connection(input.NodeID * 1000 + output.NodeID, input.NodeID, output.NodeID, weight, true, false));
                    }
                }
            }
        }
    }

    public void AddNode() {
        // Select a connection and disable it
        // Add 1 node to the arrNode()
        // Add 2 connections to arrConnection()
        // Get the layers right

        Random RND = new();
        if (RND.NextDouble() <= 0.1) {
            Connection rndConn = Connections[RND.Next(0, Connections.Count)];
            rndConn.Enabled = false;

            int nodeID = Nodes.Max(n => n.NodeID) + 1;
            Node newNode = new(nodeID, NodeTypeEnum.Hidden, 0, 0, 0);
            Nodes.Add(newNode);

            Connections.Add(new Connection(rndConn.InNodeID * 1000 + nodeID, rndConn.InNodeID, nodeID, rndConn.ConnWeight, true, false));
            Connections.Add(new Connection(nodeID * 1000 + rndConn.OutNodeID, nodeID, rndConn.OutNodeID, (float)(RND.NextDouble() * 2 - 1), true, false));

            RecalculateLayers();
        }
    }

    public void RecalculateLayers() {
        foreach (var node in Nodes)
            node.NodeLayer = -1;

        foreach (var node in Nodes.Where(n => n.NodeType == NodeTypeEnum.Input || n.NodeType == NodeTypeEnum.Bias))
            node.NodeLayer = 1;

        bool changed;
        do {
            changed = false;

            foreach (var node in Nodes) {
                if (node.NodeLayer != -1)
                    continue;

                //Find alle enabled Conns som går ind i denne Node
                var inputConns = Connections.Where(c => c.OutNodeID == node.NodeID && c.Enabled);
                //Find alle Nodes som går ind i denne Node
                var inputNodes = inputConns.Select(c => Nodes.First(n => n.NodeID == c.InNodeID));

                //Checker om alle indadgående Nodes har et Layer nr.
                if (inputNodes.All(n => n.NodeLayer > 0)) {
                    int newLayer = inputNodes.Max(n => n.NodeLayer) + 1;
                    if (node.NodeLayer != newLayer) {
                        node.NodeLayer = newLayer;
                        changed = true;
                    }
                }
            }
        } while (changed);
    }

    //private void DepthFirstSearch(Node node, List<Node> visited) {
    //    visited.Add(node);
    //    foreach (var conn in Connections.Where(c => c.InNodeID == node.NodeID && c.Enabled)) {
    //        Node nextNode = Nodes.FirstOrDefault(n => n.NodeID == conn.OutNodeID);
    //        if (nextNode != null && !visited.Contains(nextNode)) {
    //            DepthFirstSearch(nextNode, visited);
    //        }
    //    }
    //}

    public void AddConnection() {
        // 5% Chance of Connection being added
        // 20 Attemps made to find valid node pair
        // 25% Chance disabled connection being reactivated

        Random RND = new();
        if (RND.NextDouble() <= 0.05) {
            int attempts = 0;
            bool found = false;
            while (attempts < 20 && !found) {
                var inNode = Nodes[RND.Next(0, Nodes.Count - 1)];
                var outNode = Nodes[RND.Next(0, Nodes.Count - 1)];

                bool sameLayer = inNode.NodeLayer == outNode.NodeLayer;
                bool recurrentLayer = inNode.NodeLayer > outNode.NodeLayer;
                bool selfConnection = inNode.NodeID == outNode.NodeID;
                var existingConn = Connections.FirstOrDefault(c => (c.InNodeID == inNode.NodeID && c.OutNodeID == outNode.NodeID) || (c.InNodeID == outNode.NodeID && c.OutNodeID == inNode.NodeID));

                if (!selfConnection && !sameLayer && existingConn == null && !recurrentLayer) {
                    float weight = (float)(RND.NextDouble() * 40 - 20);
                    Connections.Add(new Connection(inNode.NodeID * 1000 + outNode.NodeID, inNode.NodeID, outNode.NodeID, weight, true, false));
                    found = true;
                }

                //var disabledConn = Connections.FirstOrDefault(c => !c.Enabled);
                if (existingConn != null && !existingConn.Enabled && RND.NextDouble() <= 0.25) {
                    existingConn.Enabled = true;
                }
                attempts++;
            }
        }
    }

    private readonly static float _chanceOfMutation = 0.5f;
    private readonly static float _chanceOfPlusMinusChange = 0.9f;
    private readonly static float _chanceOfNewRandomVal = 0.1f;

    public void Mutate() {
        Random RND = new();

        if (RND.NextDouble() <= _chanceOfMutation) {
            foreach (var conn in Connections) {
                if (RND.NextDouble() <= _chanceOfPlusMinusChange) {
                    //float changeFactor = 1 + ((float)(RND.NextDouble() * 0.4 - 0.2));
                    float changeFactor = (float)(conn.ConnWeight * (RND.NextDouble() * 0.4 - 0.2));
                    conn.ConnWeight *= changeFactor;
                }
                else {
                    conn.ConnWeight = (float)(RND.NextDouble() * 40 - 20);
                }

                conn.ConnWeight = Math.Clamp(conn.ConnWeight, -10f, 10f);
            }
        }
    }

    public void LoadInputs(float[] inputs) {
        int i = 0;
        foreach (var node in Nodes) {
            if (node.NodeLayer != 1) continue;
            node.SumInput = inputs[i];
            node.SumOutput = inputs[i++];
        }
    }

    public void RunTheNetwork() {
        int maxLayer = Nodes.Max(n => n.NodeLayer);

        for (int layer = 2; layer <= maxLayer; layer++) {
            foreach (var node in Nodes) {
                if (node.NodeLayer != layer) continue;

                node.SumInput = 0;
                foreach (var conn in Connections.Where(c => c.OutNodeID == node.NodeID && c.Enabled)) {
                    Node inputNode = Nodes.FirstOrDefault(x => x.NodeID == conn.InNodeID);
                    if (inputNode != null) {
                        node.SumInput += inputNode.SumOutput * conn.ConnWeight;
                    }
                }
                node.SumOutput = (float)(1 / (1 + Math.Exp(-node.SumInput)));
            }
        }
    }

    public float GetOutput() {
        var outputNode = Nodes.FirstOrDefault(x => x.NodeType == NodeTypeEnum.Output);
        return outputNode.SumOutput;
        //return outputNode != null ? outputNode.SumOutput : 0;
    }

    private readonly static float _c1 = 1.0f;
    private readonly static float _c2 = 1.0f;
    private readonly static float _c3 = 0.4f;

    public float ComparisonCheck(Brain b2) {
        List<Connection> b1Conns = Connections;
        List<Connection> b2Conns = b2.Connections;

        int excessGenes = 0;
        int disjointGenes = 0;
        float weightDifference = 0;
        int matchingGenes = 0;

        int maxInnovation1 = b1Conns.Count > 0 ? b1Conns.Max(c => c.InnovationID) : 0;
        int maxInnovation2 = b2Conns.Count > 0 ? b2Conns.Max(c => c.InnovationID) : 0;

        //int maxInnovation = Math.Max(maxInnovation1, maxInnovation2);

        var allIDs = new HashSet<int>(b1Conns.Select(c => c.InnovationID).Union(b2Conns.Select(c => c.InnovationID)));
        //for (int i = 0; i < maxInnovation; i++) {
        foreach (int id in allIDs) {
            var conn1 = b1Conns.FirstOrDefault(c => c.InnovationID == id);
            var conn2 = b2Conns.FirstOrDefault(c => c.InnovationID == id);

            if (conn1 != null && conn2 != null) {
                matchingGenes++;
                weightDifference += Math.Abs(conn1.ConnWeight - conn2.ConnWeight);
            }
            else if (conn1 != null && conn2 == null) {
                if (id > maxInnovation2) excessGenes++;
                else disjointGenes++;
            }
            else if (conn2 != null && conn1 == null) {
                if (id > maxInnovation1) excessGenes++;
                else disjointGenes++;
            }
        }

        //float avgWeightDifference = weightDifference / matchingGenes;
        float avgWeightDifference = matchingGenes > 0 ? weightDifference / matchingGenes : 0;
        return (_c1 * excessGenes) + (_c2 * disjointGenes) + (_c3 * avgWeightDifference);
    }
}