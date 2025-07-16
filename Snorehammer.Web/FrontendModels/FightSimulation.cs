namespace Snorehammer.Web.FrontendModels
{
    public class FightSimulation
    {
        public AttackProfile AttackProfile { get; set; }
        public UnitProfile Defender { get; set; }
        public List<Dice> AttackDice { get; set; } = new List<Dice>();
        public List<Dice> ToHitDice { get; set; } = new List<Dice>();
        public List<Dice> StrengthDice { get; set; } = new List<Dice>();
        public List<Dice> ArmorDice { get; set; } = new List<Dice>();
        public List<Dice> WoundDice { get; set; } = new List<Dice>();
        public List<Dice> FeelNoPainDice { get; set; } = new List<Dice>();
        public int HitTarget = 0;
        public int WoundTarget = 0;
        public int ArmorSave { get; set; } = 0;
        public int AttackNumber { get; set; } = 0;
        public int DamageNumber { get; set; } = 0;
        public int BlastBonus = 0;
        public bool CoverIgnored { get; set; } = false;
        public string WinnerMessage { get; set; } = "";
    }
}
