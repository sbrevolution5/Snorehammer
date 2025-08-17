using Snorehammer.Web.FrontendModels.Profiles;
using Snorehammer.Web.FrontendModels.Stats;

namespace Snorehammer.Web.FrontendModels.Simulations
{
    public class FightSimulation : ISimulationForStats
    {
        public FightSimulation(UnitProfile attacker, UnitProfile defender)
        {
            Attacker= attacker;
            Defender= defender;
        }
        public UnitProfile Attacker { get; set; }
        public UnitProfile Defender { get; set; }
        public string WinnerMessage { get; set; } = "";
        public List<WeaponSimulation> WeaponSimulations { get; set; } = new List<WeaponSimulation>();
        public bool HasFightBack = false;
        public FightSimulation FightBackSimulation { get; set; }
        public int RemainingModels { get; set; }
        public FightStats Stats { get; set; } = new FightStats();
        public void Reset() {
            foreach (var sim in WeaponSimulations)
            {
                sim.ClearDiceLists();
            }
            if (FightBackSimulation is not null)
            {
                FightBackSimulation.Reset();
            }
            Stats.UnitEntirelyDestroyed = false;
            Stats.LessThanHalf = false;
            Stats.UnitDamaged = false;
            Stats.LostAModel = false;
        }

    }
}
