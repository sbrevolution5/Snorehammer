namespace Snorehammer.Web.FrontendModels.Utility
{
    public class RowToPropertyCorrelation
    {
        public string RowName { get; set; }
        public string PropertyName { get; set; }
        public RowToPropertyCorrelation(string rowName, string propertyName)
        {
            RowName = rowName;
            PropertyName = propertyName;
        }
    }
}
