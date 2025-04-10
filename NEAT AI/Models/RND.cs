namespace NEAT_AI.Models {
    public static class RND {
        private static Random _random = new Random();

        public static int Next(int minValue, int maxValue) {
            return _random.Next(minValue, maxValue);
        }

        public static int Next(int maxValue) {
            return _random.Next(0, maxValue);
        }

        public static double NextDouble() {
            return _random.NextDouble();
        }
    }
}
