namespace Snorehammer.Web.FrontendModels.Stats
{
    public class DataGridStats
    {
        public List<MultiFightStats> List { get; set; } = new List<MultiFightStats>();
        public bool ShowFnp { get; set; } = false;
        public bool ShowMultiAttack { get; set; } = false;
        public bool MultiRound { get; set; } = false;
    }
}
