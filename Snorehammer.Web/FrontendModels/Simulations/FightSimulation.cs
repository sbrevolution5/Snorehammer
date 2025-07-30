using Snorehammer.Web.FrontendModels.Profiles;
using Snorehammer.Web.FrontendModels.Stats;

namespace Snorehammer.Web.FrontendModels.Simulations
{
    public class FightSimulation : ISimulationForStats
    {
        public UnitProfile Attacker { get; set; }
        public UnitProfile Defender { get; set; }
        public string WinnerMessage { get; set; } = "";
        public List<WeaponSimulation> WeaponSimulations { get; set; } = new List<WeaponSimulation>();
        public FightStats Stats { get; set; } = new FightStats();
        public void Reset() {
            Stats.UnitEntirelyDestroyed = false;
            Stats.LessThanHalf = false;
            Stats.UnitDamaged = false;
            Stats.LostAModel = false;
        }

    }
}
