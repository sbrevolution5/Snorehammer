using Snorehammer.Web.FrontendModels.Profiles;
using Snorehammer.Web.FrontendModels.Stats;

namespace Snorehammer.Web.FrontendModels.Simulations
{
    public class MultiFightSimulation
    {
        public UnitProfile Attacker { get; set; }
        public UnitProfile Defender { get; set; }
        public List<FightSimulation> FightSimulations { get; set; } = new List<FightSimulation>();
        public int Rounds { get; set; }
        public MultiFightStats Stats { get; set; } = new MultiFightStats();
        public void SetSimNumber(int number)
        {
            Rounds = number;
            FightSimulations = new List<FightSimulation>();
            for (int i = 0; i < number; i++)
            {
                FightSimulations.Add(new FightSimulation()
                {
                    Attacker = Attacker,
                    Defender = Defender,
                });
            }
        }




    }
}
