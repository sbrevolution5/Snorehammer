namespace Snorehammer.Web.FrontendModels
{
    public class Dice
    {
        public Dice(int max)
        {
            Result = new Random().Next(1,max); 
        }
        public int Target { get; set; }
        public int Sides { get; set; } = 6;
        public bool Success { get; set; }
        public int Result { get; set; }
    }
}
