using Snorehammer.Web.FrontendModels.Stats;

namespace Snorehammer.Web.Services
{
    public class GraphCreationService
    {
        public List<GraphStat> MakeGraphStats(List<int> values)
        {
            var res = new List<GraphStat>();
            var possibleResults = values.Distinct();
            foreach (var v in possibleResults)
            {
                res.Add(new GraphStat()
                {
                    Category = v,
                    Value = values.Where(i=>i == v).Count()
                });
            }
            res = res.OrderBy(s => s.Category).ToList();
            return res;
        }
    }
}
