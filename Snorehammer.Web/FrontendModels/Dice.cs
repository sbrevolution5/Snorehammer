namespace Snorehammer.Web.FrontendModels
{
    public class Dice
    {
        public Dice(int max)
        {
            Target = 1;
            Result = new Random().Next(1,max);
            Success = Target <= Result;
        }
        public int Target { get; set; }
        public int Sides { get; set; } = 6;
        public bool Success { get; set; }
        public int Result { get; set; }
    }
}
