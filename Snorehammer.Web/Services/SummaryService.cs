using Snorehammer.Web.FrontendModels.Profiles;
using System.Text;

namespace Snorehammer.Web.Services
{
    public class SummaryService
    {
        public string HitSummary(UnitProfile unit)
        {
            var sb = new StringBuilder();
            var first = true;
            if (unit.Stealth)
            {
                sb.Append("Stealth");
                first = false;
            }
            if (unit.Minus1Hit)
            {
                if (!first)
                {
                    sb.Append(" | ");
                }
                sb.Append("-1 to hit");
            }
            var res = sb.ToString();
            if (string.IsNullOrEmpty(res))
            {
                return "no modifiers";
            }
            return res;
        }
        public string HitSummary(AttackProfile attack)
        {
            var sb = new StringBuilder();
            var first = true;
            if (attack.RerollHit)
            {
                sb.Append("Reroll all hits");
                first = false;
            }
            if (attack.Plus1Hit)
            {
                if (!first)
                {
                    sb.Append(" | ");
                }
                sb.Append("+1 to hit");
                first = false;
            }
            if (attack.Minus1Hit)
            {
                if (!first)
                {
                    sb.Append(" | ");
                }
                sb.Append("-1 to hit");
                first = false;
            }

            if (attack.Torrent && !attack.Melee)
            {
                if (!first)
                {
                    sb.Append(" | ");
                }
                sb.Append("Torrent");
                first = false;
            }
            if (attack.Reroll1Hit)
            {
                if (!first)
                {
                    sb.Append(" | ");
                }
                sb.Append("Reroll 1s to hit");
                first = false;
            }
            if (attack.BigGuns)
            {
                if (!first)
                {
                    sb.Append(" | ");
                }
                sb.Append("Big Guns (-1)");
                first = false;
            }
            if (attack.Sustained)
            {
                if (!first)
                {
                    sb.Append(" | ");
                }
                sb.Append($"Sustained {attack.SustainAmount}");
                first = false;
            }
            if (attack.Blast)
            {
                if (!first)
                {
                    sb.Append(" | ");
                }
                sb.Append("Blast");
                first = false;
            }
            if (attack.Lethal)
            {
                if (!first)
                {
                    sb.Append(" | ");
                }
                sb.Append("Lethal Hits");
                first = false;
            }
            if (attack.RapidFire)
            {
                if (!first)
                {
                    sb.Append(" | ");
                }
                sb.Append($"Rapid Fire {attack.RapidFireBonus}");
            }
            var res = sb.ToString();
            if (string.IsNullOrEmpty(res))
            {
                return "no modifiers";
            }
            return res;
        }
        public string WoundSummary(UnitProfile unit)
        {
            var sb = new StringBuilder();
            var first = true;
            if (unit.Minus1Wound)
            {
                sb.Append("-1 to wound");
                first = false;
            }
            if (unit.MinusOneToWoundAgainstStronger)
            {
                if (!first)
                {
                    sb.Append(" | ");
                }
                sb.Append("-1 to Wound when Strength > toughness");
            }
            var res = sb.ToString();
            if (string.IsNullOrEmpty(res))
            {
                return "no modifiers";
            }
            return res;
        }
        public string WoundSummary(AttackProfile attack)
        {
            var sb = new StringBuilder();
            var first = true;
            if (attack.RerollWound)
            {
                sb.Append("Twin-Linked/Reroll all wounds");
                first = false;
            }
            if (attack.Plus1Wound)
            {
                if (!first)
                {
                    sb.Append(" | ");
                }
                sb.Append("+1 to Wound");
                first = false;
            }

            if (attack.Lance && attack.Melee)
            {
                if (!first)
                {
                    sb.Append(" | ");
                }
                sb.Append("Lance");
                first = false;
            }
            if (attack.Devastating)
            {
                if (!first)
                {
                    sb.Append(" | ");
                }
                sb.Append("Devastating Wounds");
                first = false;
            }
            if (attack.Reroll1Wound)
            {
                if (!first)
                {
                    sb.Append(" | ");
                }
                sb.Append("Reroll 1s to Wound");
            }
            var res = sb.ToString();
            if (string.IsNullOrEmpty(res))
            {
                return "no modifiers";
            }
            return res;
        }
        public string ArmorSummary(UnitProfile unit)
        {
            var sb = new StringBuilder();
            var first = true;
            if (unit.ArmorReroll)
            {
                sb.Append("Reroll Armor Saves");
                first = false;
            }
            if (unit.Reroll1Save)
            {
                if (!first)
                {
                    sb.Append(" | ");
                }
                sb.Append("Reroll 1s to Save");
                first = false;
            }
            if (unit.HasCover)
            {
                if (!first)
                {
                    sb.Append(" | ");
                }
                sb.Append("Cover");
                first = false;
            }
            var res = sb.ToString();
            if (string.IsNullOrEmpty(res))
            {
                return "no modifiers";
            }
            return res;
        }
        public string DamageSummary(AttackProfile attack)
        {
            var sb = new StringBuilder();
            var first = true;
            if (attack.RerollDamage)
            {
                sb.Append("Reroll all damage");
                first = false;
            }
            if (attack.Reroll1Damage)
            {
                if (!first)
                {
                    sb.Append(" | ");
                }
                sb.Append("Reroll 1s for damage");
                first = false;
            }

            if (attack.Melta)
            {
                if (!first)
                {
                    sb.Append(" | ");
                }
                sb.Append($"Melta {attack.MeltaDamage}");
            }
            var res = sb.ToString();
            if (string.IsNullOrEmpty(res))
            {
                return "no modifiers";
            }
            return res;
        }
        public string AntiSummary(AttackProfile attack)
        {
            var sb = new StringBuilder();
            if (attack.AntiBeast)
            {
                sb.Append($"Anti-Beast {attack.AntiBeastValue}  ");
            }
            if (attack.AntiInfantry)
            {
                sb.Append($"Anti-Infantry {attack.AntiInfantryValue}  ");
            }
            if (attack.AntiMonster)
            {
                sb.Append($"Anti-Monster {attack.AntiMonsterValue}  ");
            }
            if (attack.AntiVehicle)
            {
                sb.Append($"Anti-Vehicle {attack.AntiVehicleValue}  ");
            }
            if (attack.AntiMounted)
            {
                sb.Append($"Anti-Mounted {attack.AntiMountedValue}  ");
            }
            if (attack.AntiSwarm)
            {
                sb.Append($"Anti-Swarm {attack.AntiSwarmValue}  ");
            }
            var res = sb.ToString();
            if (string.IsNullOrEmpty(res))
            {
                return "no Anti-";
            }
            return res;
        }
        public string AbilitySummary(AttackProfile attack)
        {
            var sb = new StringBuilder();
            var hit = HitSummary(attack);
            if (hit != "no modifiers")
            {
                sb.AppendLine(hit);
            }
            var wound = WoundSummary(attack);
            if (wound != "no modifiers")
            {
                sb.AppendLine(wound);
            }
            var damage = DamageSummary(attack);
            if (damage != "no modifiers")
            {
                sb.AppendLine(damage);
            }
            var anti = AntiSummary(attack);
            if (anti != "no Anti-")
            {
                sb.AppendLine(anti);
            }
            return sb.ToString();
        }
        public string AbilitySummary(UnitProfile unit)
        {
            var sb = new StringBuilder();
            var hit = HitSummary(unit);
            if (hit != "no modifiers")
            {
                sb.AppendLine(hit);
            }
            var wound = WoundSummary(unit);
            if (wound != "no modifiers")
            {
                sb.AppendLine(wound);
            }
            var damage = ArmorSummary(unit);
            if (damage != "no modifiers")
            {
                sb.AppendLine(damage);
            }
            if (unit.FightsFirst)
            {
                sb.AppendLine("Fights First");
            }
            if (unit.FeelNoPain)
            {
                sb.AppendLine($"Feel No Pain {unit.FeelNoPainTarget}+");
            }
            if (unit.Overwatch)
            {
                sb.AppendLine("Overwatch mode");
            }
            if (unit.TakesHalfDamage)
            {
                sb.AppendLine("Takes Half Damage");
            }
            sb.AppendLine();
            return sb.ToString();
        }
        public string AttackSummary(AttackProfile attack)
        {
            var sb = new StringBuilder();
            if (attack.IsVariableAttacks)
            {
                sb.Append($"{attack.VariableAttackDiceNumber}d{attack.VariableAttackDiceSides} + {attack.VariableAttackDiceConstant}");
            }
            else
            {
                sb.Append($"{attack.Attacks}");
            }
            sb.Append($" | {attack.Strength}");
            if (attack.ArmorPenetration > 0)
            {
                sb.Append($" | -{attack.ArmorPenetration}");
            }
            else
            {
                sb.Append($" | 0");
            }
            if (attack.IsVariableDamage)
            {
                sb.Append($" | {attack.VariableDamageDiceNumber}d{attack.VariableDamageDiceSides} + {attack.VariableDamageDiceConstant}");
            }
            else
            {
                sb.Append($" | {attack.Damage}");
            }
            return sb.ToString();
        }
    }
}
