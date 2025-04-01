namespace Snorehammer.Web.FrontendModels
{
    public class Dice
    {
        public Dice(int target)
        {
            Target = target;
            Result = new Random().Next(1,6);
        }
        public int Target { get; set; }
        public int Sides { get; set; } = 6;
        public bool Success { get { return Target >= Result; } }
        public int Result { get; set; }
    }
}
