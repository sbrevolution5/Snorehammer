using Snorehammer.Web.FrontendModels.Profiles;
using Snorehammer.Web.FrontendModels.Stats;

namespace Snorehammer.Web.FrontendModels.Simulations
{
    public class FightSimulation
    {
        public UnitProfile Attacker { get; set; }
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
        public List<WeaponSimulation> WeaponSimulations { get; set; } = new List<WeaponSimulation>();
        public FightStats Stats { get; set; } = new FightStats();
        public List<FightStats> WeaponStats { get; set; } = new List<FightStats>();
        public void Reset() {
            AttackDice = new List<Dice>();
            ArmorDice = new List<Dice>();
            ToHitDice = new List<Dice>();
            StrengthDice = new List<Dice>();
            WoundDice = new List<Dice>();
            FeelNoPainDice = new List<Dice>();
            Stats.UnitEntirelyDestroyed = false;
            Stats.LessThanHalf = false;
            Stats.UnitDamaged = false;
            Stats.LostAModel = false;
        }

    }
}
