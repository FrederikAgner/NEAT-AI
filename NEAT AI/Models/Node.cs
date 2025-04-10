namespace NEAT_AI.Models;

public class Node {
    public int NodeID { get; set; }
    public NodeTypeEnum NodeType { get; set; }
    public int NodeLayer { get; set; }
    public float SumInput { get; set; }
    public float SumOutput { get; set; }

    public Node() { }

    public Node(int NodeID, NodeTypeEnum NodeType, int NodeLayer, float SumInput, float SumOutput) {
        this.NodeID = NodeID;
        this.NodeType = NodeType;
        this.NodeLayer = NodeLayer;
        this.SumInput = SumInput;
        this.SumOutput = SumOutput;
    }

    public override string ToString() {
        return $"<b>Type:</b> {NodeType}, Layer: {NodeLayer}, Input: {SumInput}, Output: {SumOutput}";
    }
}

public enum NodeTypeEnum {
    Hidden = 0,
    Input = 1,
    Output = 2,
    Bias = 3
}
