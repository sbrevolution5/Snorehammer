using Snorehammer.Web.FrontendModels.Profiles;
using Snorehammer.Web.FrontendModels.Stats;

namespace Snorehammer.Web.FrontendModels.Simulations
{
    public class WeaponSimulation : ISimulationForStats
    {
        public WeaponSimulation(AttackProfile weapon, UnitProfile defender,int id)
        {
            Stats = new FightStats();
            Weapon = weapon;
            Defender = defender;
            Id = id;
        }
        public int Id { get; set; }
        public AttackProfile Weapon { get; set; }
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
        public FightStats Stats { get; set; } = new FightStats();
    }
}
