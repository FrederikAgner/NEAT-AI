using NEAT_AI.Models;
using Newtonsoft.Json;
using System;

namespace NEAT_AI;

public class Program {
    public static List<Brain> Networks = new();
    public static List<Brain> TempNetworks = new();
    public static List<Species> SpeciesList = new();
    public static int Generation = 1;
    public static float ComputedThreshold = 4.0f;
    public static int TargetSpecies = 5;
    public static bool Elitism = true;

    private readonly static int _populationSize = 50;
    private readonly static int _inputNodes = 3;
    private readonly static int _hiddenNodes = 1;
    private readonly static int _outputNodes = 1;
    private readonly static float _procentConn = 1.0f;

    private static void Main(string[] args) {
        RunNeatAI();

        // Console Writing
        var sortedList = Networks.OrderBy(x => x.Fitness).ToList();
        foreach (var item in sortedList) {
            Console.WriteLine($"Fitness: {item.Fitness} - SpeciesID: {item.SpeciesID} - Nodes: {item.Nodes.Count} - Connections: {item.Connections.Count}");
        }
        Console.WriteLine();
        Console.WriteLine($"Count: {sortedList.Count}");
        Console.WriteLine($"Generations: {Generation}");
        Console.WriteLine($"Species: {SpeciesList.Count}");
        if (Networks.Any(n => n.Fitness >= 3.999)) Console.WriteLine("Found Perfect Network!!");

        Brain best = Networks.Where(x => x.Fitness == Networks.Max(y => y.Fitness)).FirstOrDefault();
        string myjson = JsonConvert.SerializeObject(best, Formatting.Indented);
        var path = Path.Combine("C:\\Temp", "BestNetwork.json");
        File.WriteAllText(path, myjson);
        Console.ReadLine();
    }

    public static void RunNeatAI() {
        PopulateNetwork(_populationSize);

        //while (true) {
        while (Generation < 10000) {
            if (Networks.Max(n => n.Fitness >= 3.999)) {
                break;
            }

            TestNetwork();
            NextGeneration();

            if (SpeciesList.Count > TargetSpecies) ComputedThreshold += 0.1f;
            else if (SpeciesList.Count < TargetSpecies) ComputedThreshold = Math.Max(1.0f, ComputedThreshold - 0.1f);

            Generation++;

            if (Generation % 1000 == 0) {
                Console.WriteLine($"Best: {Networks.Max(n => n.Fitness)}");                
            }
        }
    }

    private static void PopulateNetwork(int population) {
        Networks.Clear();
        for (int i = 0; i < population; i++) {
            Brain newBrain = new();
            newBrain.Initialize(_inputNodes, _outputNodes, _hiddenNodes, _procentConn);
            Networks.Add(newBrain);
        }
    }

    private static void TestNetwork() {
        foreach (var brain in Networks) {
            brain.LoadInputs([0, 0, 1]);
            brain.RunTheNetwork();
            brain.Fitness = 1 - brain.GetOutput();
            //brain.Fitness += 1 - Math.Abs(4 - brain.GetOutput());

            brain.LoadInputs([0, 1, 1]);
            brain.RunTheNetwork();
            brain.Fitness += brain.GetOutput();

            brain.LoadInputs([1, 0, 1]);
            brain.RunTheNetwork();
            brain.Fitness += brain.GetOutput();

            brain.LoadInputs([1, 1, 1]);
            brain.RunTheNetwork();
            brain.Fitness += 1 - brain.GetOutput();
        }
    }

    private static void NextGeneration() {
        Speciate();
        AdjustAllFitness();

        float GlobalAdjAvg = SpeciesList.Average(s => s.AvgAdjFitness);
        float GlobalFitness = SpeciesList.Sum(s => s.AvgFitness);
        foreach (var species in SpeciesList) {
            species.CalculateOffspring(GlobalFitness, _populationSize);
        }

        var totalOffspring = SpeciesList.Sum(s => s.AllowedOffspring);

        TempNetworks = Networks.ToList();
        PopulateNetwork(_populationSize);
        GenerateOffspring();

        foreach (var brain in Networks) {
            if (brain == Networks[0] && Elitism) continue;

            brain.Mutate();
            brain.AddNode();
            brain.AddConnection();
        }
    }

    private static void Speciate() {
        //SpeciesList.Clear();
        SpeciesList.ForEach(sl => sl.Members.Clear());

        foreach (var brain in Networks) {
            bool foundSpecies = false;

            foreach (var species in SpeciesList) {
                float distance = brain.ComparisonCheck(species.Representative);

                if (distance < ComputedThreshold) {
                    brain.SpeciesID = species.SpeciesID;
                    species.Members.Add(brain);
                    foundSpecies = true;
                    break;
                }
            }

            if (!foundSpecies) {
                Species newSpecies = new(SpeciesList.Count + 1, brain);
                SpeciesList.Add(newSpecies);
                brain.SpeciesID = newSpecies.SpeciesID;
            }
        }

        SpeciesList.RemoveAll(species => species.Members.Count == 0);        
    }

    private static void AdjustAllFitness() {
        foreach (var species in SpeciesList) {
            species.AdjustFitness();
        }
    }

    private static void GenerateOffspring() {
        int networkIndex = 0;
        List<Species> speciesToRemove = new();
        foreach (var species in SpeciesList) {
            if (species.GensSinceLastImprovement >= 15) {
                species.AllowedOffspring = 0;
                speciesToRemove.Add(species);
                continue;
            }

            if (species.AllowedOffspring > 0) {
                for (int i = 0; i < species.AllowedOffspring; i++) {
                    var parents = species.SelectSpecies();

                    var parent1 = parents[0];
                    var parent2 = parents[1];

                    Brain fittestParent;
                    if (parent1.Fitness > parent2.Fitness) fittestParent = parent1;
                    else if (parent2.Fitness > parent1.Fitness) fittestParent = parent2;
                    else fittestParent = parents[RND.Next(parents.Count - 1)];

                    Brain newOffspring = fittestParent.Clone();

                    var parent1IDs = parent1.Connections.Select(c => c.InnovationID).ToHashSet();
                    var parent2IDs = parent2.Connections.Select(c => c.InnovationID).ToHashSet();
                    var sharedIDs = parent1IDs.Intersect(parent2IDs);

                    foreach (var id in sharedIDs) {
                        var conn1 = parent1.Connections.First(c => c.InnovationID == id);
                        var conn2 = parent2.Connections.First(c => c.InnovationID == id);

                        Connection selectedConn;
                        if (RND.Next(2) == 0) selectedConn = conn1;
                        else selectedConn = conn2;

                        var index = newOffspring.Connections.FindIndex(c => c.InnovationID == id);
                        if (index != -1) newOffspring.Connections[index].ConnWeight = selectedConn.ConnWeight;
                    }

                    if (networkIndex < Networks.Count) {
                        Networks[networkIndex++] = newOffspring;
                    }
                }
            }
        }

        speciesToRemove.ForEach(x => SpeciesList.Remove(x));

        if (Elitism) {
            Brain bestTest = TempNetworks.Where(x => x.Fitness == TempNetworks.Max(y => y.Fitness)).FirstOrDefault();
            Networks[0] = bestTest;
        }
    }

    private static void ToggleElitism() {
        Elitism = !Elitism;
    }
}