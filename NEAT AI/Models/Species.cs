namespace NEAT_AI.Models;

public class Species {
    public int SpeciesID { get; set; }
    public List<Brain> Members { get; set; } = new();
    public Brain Representative { get; set; }
    public int AllowedOffspring { get; set; }
    public float AvgFitness { get; set; }
    public float AvgAdjFitness { get; set; }
    public float TotalFitness { get; set; }
    public int GensSinceLastImprovement { get; set; } = 0;

    public Species(int SpeciesID, Brain Representative) {
        this.SpeciesID = SpeciesID;
        this.Representative = Representative;
        Members.Add(Representative);
    }

    public override string ToString() {
        return $"Members: {Members.Count}, AvgFitness: {AvgFitness}, AllowedOffspring: {AllowedOffspring}";
    }

    public void AdjustFitness() {
        int speciesSize = Members.Count;
        AvgFitness = Members.Average(b => b.Fitness);
        TotalFitness = Members.Sum(b => b.Fitness);

        foreach (var brain in Members) {
            brain.AdjustedFitness = brain.Fitness / speciesSize;
        }
        AvgAdjFitness = Members.Average(b => b.AdjustedFitness);
    }

    public void CalculateOffspring(float GlobalAdjAvg, int PopulationSize) {
        //zz | Er måske ikke rigtig. Skal muligvis kigges på senere.
        //AllowedOffspring = (int)Math.Round((AvgAdjFitness / GlobalAdjAvg) * Members.Count);
        //AllowedOffspring = (int)Math.Min(Math.Round(AvgAdjFitness / GlobalAdjAvg * Members.Count, 0, MidpointRounding.ToEven), PopulationSize);

        /// Adjustled Fitness
        //float avgAdjFitness = Members.Sum(b => b.AdjustedFitness);
        //AllowedOffspring = (int)Math.Floor((avgAdjFitness / GlobalAdjAvg) * PopulationSize);

        /// Average Fitness
        AllowedOffspring = (int)Math.Round((AvgFitness / GlobalAdjAvg) * PopulationSize);
    }

    public List<Brain> SelectSpecies() {
        List<Brain> selectedParent = new();
        float totalFitness = Members.Sum(m => m.Fitness);
        float sums = 0;

        Random RND = new();
        var random1 = (float)(RND.NextDouble() * totalFitness);
        Brain parent1 = new();
        foreach (var item in Members) {
            sums += item.Fitness;
            if (sums > random1) {
                parent1 = item;
                selectedParent.Add(parent1);
                break;
            }
        }

        //rollAgain:
        sums = 0;
        var random2 = (float)(RND.NextDouble() * totalFitness);
        Brain parent2 = new();
        foreach (var item in Members) {
            sums += item.Fitness;
            if (sums > random2) {
                //if (item == parent1) goto rollAgain;
                parent2 = item;
                selectedParent.Add(parent2);
                break;
            }
        }

        return selectedParent;
    }
}
