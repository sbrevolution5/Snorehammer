namespace Snorehammer.Web.FrontendModels
{
    public class Dice
    {
        public Dice (int target, Random random, int criticalThreshold = 6)
        {
            Target = target;
            CriticalThreshold = criticalThreshold;
            Result = random.Next(1, Sides);
            Success = Target <= Result;
            Critical = CriticalThreshold == Result;
        }
        public Dice(bool success)
        {
            Success = success;
            Critical = success;
            Result = Sides + 1;
        }
        public int Target { get; set; }
        public int Sides { get; set; } = 6;
        public bool Success { get; set; }
        public int Result { get; set; }
        public bool Critical { get; set; }
        public int CriticalThreshold { get; set; }
    }
}
