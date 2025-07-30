using Snorehammer.Web.FrontendModels.Stats;

namespace Snorehammer.Web.FrontendModels.Simulations
{
    public interface ISimulationForStats
    {
        public FightStats Stats { get; set; }
    }
}