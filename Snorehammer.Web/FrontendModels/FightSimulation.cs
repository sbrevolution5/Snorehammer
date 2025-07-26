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
        public int ModdedWoundTarget = 0;
        public int ArmorSave { get; set; } = 0;
        public int AttackNumber { get; set; } = 0;
        public int DamageNumber { get; set; } = 0;
        public int BlastBonus = 0;
        public bool CoverIgnored { get; set; } = false;
        public string WinnerMessage { get; set; } = "";
        public int ModelsDestroyed { get; set; } = 0;
        public int WoundsInflicted { get; set; } = 0;
        public int ArmorSavesFailed { get; set; } = 0;
        public int AttacksHit { get; set; } = 0;
        public int FeelNoPainMade { get; set; } = 0;
        public bool UnitEntirelyDestroyed { get; set; } = false;
        public bool HalfOrLess { get; set; } = false;
        public bool UnitDamaged { get; set; } = false;
        public bool LostAModel { get; set; } = false;
        public void Reset() {
            AttackDice = new List<Dice>();
            ArmorDice = new List<Dice>();
            ToHitDice = new List<Dice>();
            StrengthDice = new List<Dice>();
            WoundDice = new List<Dice>();
            FeelNoPainDice = new List<Dice>();
        }

    }
}
