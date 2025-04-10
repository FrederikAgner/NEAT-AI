using NEAT_AI.Models;

public  class Program {
    private static void Main(string[] args) {
        NEAT_AI.Program.RunNeatAI();

        Brain brain = NEAT_AI.Program.Networks.OrderByDescending(n => n.Fitness).FirstOrDefault();
        brain.LoadInputs([0, 0, 1]);
        brain.RunTheNetwork();
        Console.WriteLine(Math.Round(brain.GetOutput()));

        brain.LoadInputs([0, 1, 1]);
        brain.RunTheNetwork();
        Console.WriteLine(Math.Round(brain.GetOutput()));

        brain.LoadInputs([1, 0, 1]);
        brain.RunTheNetwork();
        Console.WriteLine(Math.Round(brain.GetOutput()));

        brain.LoadInputs([1, 1, 1]);
        brain.RunTheNetwork();
        Console.WriteLine(Math.Round(brain.GetOutput()));

        Console.ReadLine();
    }
}