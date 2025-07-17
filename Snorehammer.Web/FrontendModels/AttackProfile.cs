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
        public int SustainAmount { get; set; } = 1;
        public bool Lethal { get; set; } = false;
        public bool RerollHit { get; set; } = false;
        public bool RerollWound { get; set; } = false;
        public bool RerollDamage { get; set; } = false;
        public bool Reroll1Hit { get; set; } = false;
        public bool Reroll1Wound { get; set; } = false;
        public bool Reroll1Damage { get; set; } = false;
        public bool Torrent { get; set; } = false;
        public bool Blast { get; set; } = false;
        public bool IsVariableAttacks { get; set; } = false;
        public bool Plus1Hit { get; set; } = false;
        public bool Plus1Wound { get; set; } = false;
        public int VariableAttackDiceNumber { get; set; }
        public int VariableAttackDiceSides { get; set; } = 6;
        public int VariableAttackDiceConstant { get; set; }
        public bool IsVariableDamage { get; set; } = false;
        public int VariableDamageDiceNumber { get; set; }
        public int VariableDamageDiceSides { get; set; } = 6;
        public int VariableDamageDiceConstant { get; set; }
        public bool Melta { get; set; } = false;
        public int MeltaDamage { get; set; } = 1;

    }
}
