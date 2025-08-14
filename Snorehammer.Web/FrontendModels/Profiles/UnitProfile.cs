using Snorehammer.Web.Extensions;

namespace Snorehammer.Web.FrontendModels.Profiles
{
    public class UnitProfile : ICloneable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public UnitType Type { get; set; } = UnitType.Infantry;
        public List<AttackProfile> Attacks { get; set; } = new List<AttackProfile>();
        public int Toughness { get; set; }
        public int Wounds { get; set; }
        public int MinimumSave { get; set; }
        public int InvulnerableSave { get; set; }
        public bool HasInvulnerableSave { get; set; } = false;
        public int ModelCount { get; set; }
        public bool ArmorReroll { get; set; }
        public bool Reroll1Save { get; set; }
        public bool FeelNoPain { get; set; } = false;
        public bool Minus1Hit { get; set; } = false;
        public bool Minus1Wound { get; set; } = false;
        public bool Stealth { get; set; } = false;
        public bool ChargeBonus { get; set; } = false;
        public bool FightsFirst { get; set; } = false;
        public bool Overwatch { get; set; } = false;
        public int FeelNoPainTarget { get; set; } = 6;
        public bool HasCover { get; set; } = false;
        public bool MinusOneToWoundAgainstStronger { get; set; } = false;
        public bool TakesHalfDamage { get; set; } = false;
        public bool Psyker { get; set; } = false;
        public object Clone()
        {
            UnitProfile res = (UnitProfile)this.MemberwiseClone();
            if (res.Attacks.Any())
            {
                res.Attacks = (List<AttackProfile>)this.Attacks.Clone();
            }
            return res;
        }
    }
}
