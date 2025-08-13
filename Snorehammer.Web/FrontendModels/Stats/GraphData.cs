using Microsoft.AspNetCore.Components;

namespace Snorehammer.Web.FrontendModels.Stats
{
    public class GraphData
    {
        public List<FightStats> dataA { get; set; }
        public List<FightStats> dataB { get; set; }
        public int TotalSims { get; set; }
        public MultiFightStats multiA { get; set; }
        public MultiFightStats? multiB { get; set; }
        public string titleA { get; set; }
        public string titleB { get; set; }
        public bool showFnp { get; set; } = false;
        public bool isMultiAttack { get; set; } = false;
    }
}
