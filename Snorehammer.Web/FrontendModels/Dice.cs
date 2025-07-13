namespace Snorehammer.Web.FrontendModels
{
    public class Dice
    {
        public Dice (int target, Random random)
        {
            Target = target;
            Result = random.Next(1, Sides);
        }
        public int Target { get; set; }
        public int Sides { get; set; } = 6;
        public bool Success { get { return Target <= Result; } }
        public int Result { get; set; }
    }
}
