namespace Snorehammer.Web.FrontendModels
{
    public class UnitProfile : ICloneable
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
        public bool Reroll1Save { get; set; }
        public bool FeelNoPain { get; set; } = false;
        public bool Minus1Hit { get; set; } = false;
        public bool Minus1Wound { get; set; } = false;
        public bool Stealth { get; set; } = false;
        public int FeelNoPainTarget { get; set; } = 6;
        public bool HasCover { get; set; } = false;
        public bool MinusOneToWoundAgainstStronger { get; set; } = false;
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
