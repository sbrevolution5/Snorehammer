using Snorehammer.Web.FrontendModels.Profiles;
using System.Text;

namespace Snorehammer.Web.Services
{
    public class SummaryService
    {
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
            if (attack.Lethal)
            {
                if (!first)
                {
                    sb.Append(" | ");
                }
                sb.Append($"Lethal Hits");
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
    }
}
