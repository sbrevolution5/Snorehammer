namespace Snorehammer.Web.FrontendModels
{
    public class Comparison
    {
        public bool ComparingAttackers { get; set; }
        public bool ComparingDefenders { get; set; }
        public AttackProfile Attacker1 { get; set; }
        public AttackProfile Attacker2 { get; set; }
        public UnitProfile Defender1 { get; set; }
        public UnitProfile Defender2 { get; set; }
        public FightSimulation Simulation1 { get; set; }
        public FightSimulation AlternateAttackSimulation { get; set; }
        public FightSimulation AlternateDefenseSimulation { get; set; }
    }
}
