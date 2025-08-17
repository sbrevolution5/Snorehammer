using Microsoft.JSInterop;
using Snorehammer.Web.FrontendModels;
using Snorehammer.Web.FrontendModels.Profiles;
using Snorehammer.Web.FrontendModels.Simulations;

namespace Snorehammer.Web.Services
{
    public class LoadComparisonService
    {
        public Comparison SetupDemo1()
        {
            var comparison = new Comparison();
            comparison.Simulation1 = new MultiFightSimulation()
            {

                Defender = new UnitProfile()
                {
                    InvulnerableSave = 0,
                    MinimumSave = 3,
                    Toughness = 4,
                    Wounds = 2,
                    ModelCount = 10,
                    Name = "Intercessors"
                },
                Attacker = new UnitProfile()
                {
                    Attacks = new List<AttackProfile>()
                {
                    new AttackProfile()
                    {
                        Name = "Termagant spinefists",
                        WeaponsInUnit = 10,
                        ArmorPenetration = 0,
                        Attacks = 2,
                        Damage = 1,
                        Skill = 4,
                        Strength = 3,
                        VariableAttackDiceConstant = 1,
                        VariableAttackDiceNumber = 2,
                        VariableDamageDiceConstant = 1,
                        VariableDamageDiceNumber = 1,
                        RerollWound = true
                    }
                },
                    InvulnerableSave = 0,
                    MinimumSave = 5,
                    Toughness = 3,
                    Wounds = 1,
                    ModelCount = 10,
                    Name = "Termagants"
                }
            };
            comparison.AlternateAttackSimulation = new MultiFightSimulation()
            {
                Defender = (UnitProfile)comparison.Simulation1.Defender.Clone(),
                Attacker = (UnitProfile)comparison.Simulation1.Attacker.Clone()
            };
            comparison.AlternateDefenseSimulation = new MultiFightSimulation()
            {
                Defender = (UnitProfile)comparison.Simulation1.Defender.Clone(),
                Attacker = (UnitProfile)comparison.Simulation1.Attacker.Clone()
            };
            return comparison;
        }
        public Comparison SetupDemo2()
        {
            var comparison = new Comparison();
            comparison.Simulation1 = new MultiFightSimulation()
            {

                Defender = new UnitProfile()
                {
                    InvulnerableSave = 4,
                    MinimumSave = 2,
                    Toughness = 5,
                    Wounds = 2,
                    ModelCount = 5,
                    Name = "Terminators"
                },
                Attacker = new UnitProfile()
                {
                    Attacks = new List<AttackProfile>()
                {
                    new AttackProfile()
                    {
                        Name = "Soulshatter lascannon",
                        WeaponsInUnit = 2,
                        ArmorPenetration = 3,
                        Attacks = 2,
                        Damage = 1,
                        Skill = 3,
                        Strength = 12,
                        VariableAttackDiceConstant = 1,
                        VariableAttackDiceNumber = 2,
                        VariableDamageDiceConstant = 1,
                        VariableDamageDiceNumber = 1,
                        VariableAttackDiceSides =6,
                        IsVariableDamage = true,
                        IsVariableAttacks = true
                    },
                    new AttackProfile()
                    {
                        Name = "Twin heavy bolter",
                        WeaponsInUnit = 1,
                        ArmorPenetration = 1,
                        Attacks = 3,
                        Damage = 2,
                        Skill = 3,
                        Strength = 5,
                        VariableAttackDiceConstant = 1,
                        VariableAttackDiceNumber = 2,
                        VariableDamageDiceConstant = 1,
                        VariableDamageDiceNumber = 1
                    },
                    new AttackProfile()
                    {
                        Name = "Combi-bolter",
                        WeaponsInUnit = 1,
                        ArmorPenetration = 0,
                        Attacks = 4,
                        Damage = 1,
                        Skill = 3,
                        Strength = 4,
                        VariableAttackDiceConstant = 1,
                        VariableAttackDiceNumber = 2,
                        VariableDamageDiceConstant = 1,
                        VariableDamageDiceNumber = 1
                    },
                    new AttackProfile()
                    {
                        Name= "Havoc Launcher",
                        IsVariableAttacks=true,
                        Blast=true,
                        WeaponsInUnit=1,
                        Attacks=1,
                        Skill=3,
                        Strength=5,
                        ArmorPenetration = 0,
                        VariableAttackDiceConstant = 1,
                        VariableAttackDiceNumber=1,
                        VariableAttackDiceSides =6,
                        Damage=1
                    }
                },
                    InvulnerableSave = 0,
                    MinimumSave = 2,
                    Toughness = 12,
                    Wounds = 16,
                    ModelCount = 1,
                    Name = "Chaos Land Raider",
                    Type = UnitType.Vehicle
                }
            };
            comparison.AlternateAttackSimulation = new MultiFightSimulation()
            {
                Defender = (UnitProfile)comparison.Simulation1.Defender.Clone(),
                Attacker = (UnitProfile)comparison.Simulation1.Attacker.Clone()
            };
            comparison.AlternateDefenseSimulation = new MultiFightSimulation()
            {
                Defender = (UnitProfile)comparison.Simulation1.Defender.Clone(),
                Attacker = (UnitProfile)comparison.Simulation1.Attacker.Clone()
            };
            return comparison;


        }
        public Comparison SetupDemo3()
        {
            var comparison = new Comparison()
            {
                ComparingAttackers = true,
                MeleeFightBack = true
            };
            comparison.Simulation1 = new MultiFightSimulation()
            {

                Defender = new UnitProfile()
                {
                    InvulnerableSave = 5,
                    MinimumSave = 5,
                    Toughness = 4,
                    Wounds = 2,
                    ModelCount = 10,
                    Name = "Genestealers",
                    Attacks = new List<AttackProfile>()
                    {
                        new AttackProfile()
                        {
                            Name = "Genestealer Claws And Talons",
                            WeaponsInUnit = 10,
                            ArmorPenetration = 2,
                            Attacks = 4,
                            Damage = 1,
                            Skill = 2,
                            Strength = 4,
                            VariableAttackDiceConstant = 1,
                            VariableAttackDiceNumber = 2,
                            VariableDamageDiceConstant = 1,
                            VariableDamageDiceNumber = 1,
                            Reroll1Hit = true,
                            Melee=true
                        },
                    }
                },
                Attacker = new UnitProfile()
                {
                    Attacks = new List<AttackProfile>()
                    {
                        new AttackProfile()
                        {
                            Name = "Astartes chainsword",
                            WeaponsInUnit = 9,
                            ArmorPenetration = 1,
                            Attacks = 4,
                            Damage = 1,
                            Skill = 3,
                            Strength = 4,
                            VariableAttackDiceConstant = 1,
                            VariableAttackDiceNumber = 2,
                            VariableDamageDiceConstant = 1,
                            VariableDamageDiceNumber = 1,
                            Melee=true
                        },
                        new AttackProfile()
                        {
                            Name = "Power weapon",
                            WeaponsInUnit = 1,
                            ArmorPenetration = 2,
                            Attacks = 4,
                            Damage = 1,
                            Skill = 3,
                            Strength = 5,
                            VariableAttackDiceConstant = 1,
                            VariableAttackDiceNumber = 2,
                            VariableDamageDiceConstant = 1,
                            VariableDamageDiceNumber = 1,
                            Melee=true

                        }
                    },
                    InvulnerableSave = 0,
                    MinimumSave = 3,
                    Toughness = 4,
                    Wounds = 2,
                    ModelCount = 10,
                    Name = "Blood Claws"
                }
            };
            comparison.AlternateAttackSimulation = new MultiFightSimulation()
            {
                Defender = (UnitProfile)comparison.Simulation1.Defender.Clone(),
                Attacker = new UnitProfile()
                {
                    Attacks = new List<AttackProfile>()
                {
                    new AttackProfile()
                    {
                        Name = "Close combat weapon",
                        WeaponsInUnit = 9,
                        ArmorPenetration = 0,
                        Attacks = 3,
                        Damage = 1,
                        Skill = 3,
                        Strength = 4,
                        VariableAttackDiceConstant = 1,
                        VariableAttackDiceNumber = 2,
                        VariableDamageDiceConstant = 1,
                        VariableDamageDiceNumber = 1,
                        Melee=true
                    },
                    new AttackProfile()
                    {
                        Name = "Power fist",
                        WeaponsInUnit = 1,
                        ArmorPenetration = 2,
                        Attacks = 3,
                        Damage = 2,
                        Skill = 3,
                        Strength = 8,
                        VariableAttackDiceConstant = 1,
                        VariableAttackDiceNumber = 2,
                        VariableDamageDiceConstant = 1,
                        VariableDamageDiceNumber = 1,
                        Melee=true

                    }
                },
                    InvulnerableSave = 0,
                    MinimumSave = 3,
                    Toughness = 4,
                    Wounds = 2,
                    ModelCount = 10,
                    Name = "Intercessors"
                }
            };
            comparison.AlternateDefenseSimulation = new MultiFightSimulation()
            {
                Defender = (UnitProfile)comparison.Simulation1.Defender.Clone(),
                Attacker = (UnitProfile)comparison.Simulation1.Attacker.Clone()
            };
            return comparison;
        }
    }
}
