using Snorehammer.Web.FrontendModels.Stats;

namespace Snorehammer.Web.Services
{
    public class GraphCreationService
    {
        public List<BarGraphStat> MakeBarGraphStats(List<int> values)
        {
            var res = new List<BarGraphStat>();
            var possibleResults = values.Distinct();
            foreach (var v in possibleResults)
            {
                res.Add(new BarGraphStat()
                {
                    Category = v,
                    Value = values.Where(i=>i == v).Count()
                });
            }
            res = res.OrderBy(s => s.Category).ToList();
            return res;
        }
        public List<PieGraphStat> MakePieGraphStats(float value,int total)
        {
            var res = new List<PieGraphStat>
            {
                new PieGraphStat()
                {
                    Category = "Success",
                    Value = value
                },
                new PieGraphStat()
                {
                    Category = "Failure",
                    Value = (float)(total - value)
                }
            };
            return res;
        }
    }
}
