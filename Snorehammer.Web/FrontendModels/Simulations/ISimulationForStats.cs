using Snorehammer.Web.FrontendModels.Stats;

namespace Snorehammer.Web.FrontendModels.Simulations
{
    public interface ISimulationForStats
    {
        public FightStats Stats { get; set; }
        public int AttackNumber { get; set; }
        public int DamageNumber { get; set; }
    }
}