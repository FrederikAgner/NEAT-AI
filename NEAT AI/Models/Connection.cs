namespace NEAT_AI.Models;

public class Connection {
    public int InnovationID { get; set; }
    public int InNodeID { get; set; }
    public int OutNodeID { get; set; }
    public float ConnWeight { get; set; }
    public bool Enabled { get; set; }
    public bool IsRecurrent { get; set; }

    public Connection(int InnovationID, int InNodeID, int OutNodeID, float ConnWeight, bool Enabled, bool IsRecurrent) {
        this.InnovationID = InnovationID;
        this.InNodeID = InNodeID;
        this.OutNodeID = OutNodeID;
        this.ConnWeight = ConnWeight;
        this.Enabled = Enabled;
        this.IsRecurrent = IsRecurrent;
    }

    public override string ToString() {
        return $"InnID: {InnovationID}, Weight: {ConnWeight}, InNode: {InNodeID}, OutNode: {OutNodeID}, Enabled: {Enabled}, Recurrent: {IsRecurrent}";
    }
}
