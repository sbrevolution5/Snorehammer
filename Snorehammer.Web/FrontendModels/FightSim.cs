namespace Snorehammer.Web.FrontendModels
{
    public class FightSim
    {
        public int Id { get; set; }
        public List<int> AttackDice { get; set; }
        public List<int> StrengthDice { get; set; }
        public List<int> ArmorDice { get; set; }
    }
}
