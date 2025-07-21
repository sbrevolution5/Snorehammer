namespace Snorehammer.Web.FrontendModels
{
    public class Comparison
    {
        public AttackProfile Attacker1 { get; set; }
        public AttackProfile Attacker2 { get; set; }
        public UnitProfile Defender1 { get; set; }
        //public UnitProfile Defender2 { get; set; }
        public FightSimulation simulation1 { get; set; }
        public FightSimulation simulation2 { get; set; }
    }
}
