namespace Snorehammer.Web.FrontendModels
{
    public class FightSimulation
    {
        public List<Dice> AttackDice { get; set; } = new List<Dice>();
        public List<Dice> StrengthDice { get; set; } = new List<Dice>();
        public List<Dice> ArmorDice { get; set; } = new List<Dice>();
        public List<Dice> WoundDice { get; set; } = new List<Dice>();
        public List<Dice> FeelNoPainDice { get; set; } = new List<Dice>();
        public string WinnerMessage { get; set; } = "";
    }
}
