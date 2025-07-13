namespace Snorehammer.Web.FrontendModels
{
    public class AttackProfile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Attacks { get; set; }
        public int Skill { get; set; }
        public int Strength { get; set; }
        public int ArmorPenetration { get; set; }
        public int Damage { get; set; }
        public bool Devastating { get; set; } = false;
        public bool Sustained { get; set; } = false;
        public bool Lethal { get; set; } = false;
        public bool RerollHit { get; set; } = false;
        public bool RerollWound { get; set; } = false;
        public bool RerollDamage { get; set; } = false;
    }
}
