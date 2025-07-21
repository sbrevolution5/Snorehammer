namespace Snorehammer.Web.FrontendModels
{
    public class Comparison
    {
        public bool ComparingAttackers { get; set; }
        public bool ComparingDefenders { get; set; }
        public FightSimulation Simulation1 { get; set; }
        public FightSimulation AlternateAttackSimulation { get; set; }
        public FightSimulation AlternateDefenseSimulation { get; set; }
    }
}
