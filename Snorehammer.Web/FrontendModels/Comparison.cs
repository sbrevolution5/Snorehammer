namespace Snorehammer.Web.FrontendModels
{
    public class Comparison
    {
        public bool ComparingAttackers { get; set; }
        public bool ComparingDefenders { get; set; }
        public MultiFightSimulation Simulation1 { get; set; }
        public MultiFightSimulation AlternateAttackSimulation { get; set; }
        public MultiFightSimulation AlternateDefenseSimulation { get; set; }
    }
}
