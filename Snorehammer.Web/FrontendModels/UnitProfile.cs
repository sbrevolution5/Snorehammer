namespace Snorehammer.Web.FrontendModels
{
    public class UnitProfile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<AttackProfile> Attacks { get; set; }
        public int Toughness { get; set; }
        public int Wounds { get; set; }
        public int MinimumSave { get; set; }
        public int InvulnerableSave { get; set; }
        public int ModelCount { get; set; }
        public bool ArmorReroll { get; set; }
    }
}
