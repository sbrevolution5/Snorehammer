namespace Snorehammer.Web.FrontendModels.Profiles
{
    public class AttackProfile :ICloneable
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int WeaponsInUnit { get; set; }
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
        public bool Minus1Hit { get; set; } = false;
        public bool Plus1Wound { get; set; } = false;
        public bool Lance { get; set; } = false;
        public int VariableAttackDiceNumber { get; set; }
        public int VariableAttackDiceSides { get; set; } = 6;
        public int VariableAttackDiceConstant { get; set; }
        public bool IsVariableDamage { get; set; } = false;
        public int VariableDamageDiceNumber { get; set; }
        public int VariableDamageDiceSides { get; set; } = 6;
        public int VariableDamageDiceConstant { get; set; }
        public bool Melta { get; set; } = false;
        public int MeltaDamage { get; set; } = 1;
        public bool Melee { get; set; } = false;
        public bool BigGuns { get; set; } = false;
        public bool AntiInfantry { get; set; } = false;
        public int AntiInfantryValue { get; set; } = 2;
        public bool AntiMounted { get; set; } = false;
        public int AntiMountedValue { get; set; } = 2;
        public bool AntiBeast { get; set; } = false;
        public int AntiBeastValue { get; set; } = 2;
        public bool AntiSwarm { get; set; } = false;
        public int AntiSwarmValue { get; set; } = 2;
        public bool AntiVehicle { get; set; } = false;
        public int AntiVehicleValue { get; set; } = 2;
        public bool AntiMonster { get; set; } = false;
        public int AntiMonsterValue { get; set; } = 2;
        public bool RapidFire { get; set; } = false;
        public int RapidFireBonus { get; set; } = 1;
        //Overwatch should only be set based on the unit class
        public bool Overwatch { get; set; } = false;
        public object Clone()
        {
            return this.MemberwiseClone();
        }

    }
}
