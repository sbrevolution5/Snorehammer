using Microsoft.Extensions.Primitives;
using Snorehammer.Web.FrontendModels;
using System.Text;

namespace Snorehammer.Web.Services
{
    public class FightSimulationService
    {
        private readonly Random _random;
        public FightSimulationService()
        {
            _random = new Random();
        }
        public List<Dice> SimulateToHitRoll(AttackProfile attack)
        {
            var res = new List<Dice>();
            for (int i = 0; i < attack.Attacks; i++)
            {
                res.Add(new Dice(attack.Skill, _random));
            }
            return res;
        }
        public List<Dice> RollStrengthStep(UnitProfile defender, AttackProfile attack, List<Dice> dicePool)
        {
            var res = new List<Dice>();
            var targetValue = DetermineWoundTarget(defender.Toughness, attack.Strength);
            for (int i = 0; i < dicePool.Where(d => d.Success).Count(); i++)
            {
                res.Add(new Dice(targetValue, _random));
            }
            return res;
        }
        public List<Dice> RollArmorSaves(UnitProfile defender, AttackProfile attack, List<Dice> dicePool)
        {
            var res = new List<Dice>();
            var targetValue = DetermineArmorSave(defender, attack);
            var roller = new Random();
            for (int i = 0; i < dicePool.Where(d => d.Success).Count(); i++)
            {
                res.Add(new Dice(targetValue, _random));
            }
            return res;
        }

        public int DetermineArmorSave(UnitProfile defender, AttackProfile attack)
        {
            int moddedSave = defender.MinimumSave - attack.ArmorPenetration;
            if (moddedSave < defender.InvulnerableSave || defender.InvulnerableSave == 0)
            {
                return moddedSave;
            }
            return defender.InvulnerableSave;
        }

        public int DetermineWoundTarget(int toughness, int strength)
        {
            if (toughness == strength)
            {
                return 4;
            }
            else if (toughness > strength)
            {

                if (toughness >= strength * 2)
                {
                    return 6;
                }
                return 5;
            }
            else
            {
                if (strength >= toughness * 2)
                {
                    return 2;
                }
                return 3;
            }
        }
        public string GenerateWinnerMessage(UnitProfile defender, AttackProfile attack, List<Dice> armorSaves)
        {
            var res = new StringBuilder();
            var successful = armorSaves.Where(d => !d.Success).Count();
            res.Append($"{successful} out of {attack.Attacks} attacks broke through armor.");
            int inflictedWounds = successful * attack.Damage;
            if (inflictedWounds > 0)
            {
                res.Append($"{inflictedWounds} wounds inflicted to defender.");

                int totalWounds = defender.Wounds * defender.ModelCount;
                var destroyedModels = inflictedWounds / defender.Wounds;
                if (destroyedModels >= defender.ModelCount)
                {
                    res.Append("The entire unit was destroyed");
                    return res.ToString();
                }
                if (defender.ModelCount > 1)
                {
                    res.Append($"{destroyedModels} out of {defender.ModelCount} models were destroyed");
                    int woundRemainder = inflictedWounds % defender.Wounds;
                    if (woundRemainder != 0)
                    {
                        res.Append($"A remaining model was inflicted {woundRemainder} wounds, leaving it with {defender.Wounds - woundRemainder} remaining");
                    }
                    return res.ToString();
                }
                res.Append($"The model has {defender.Wounds - inflictedWounds} wound(s) remaining");
            }
            return res.ToString();
        }

        public void SimulateSimpleFight(UnitProfile defender, AttackProfile attack)
        {
            int dicePool = attack.Attacks;
            Random roller = new Random();
            List<int> dice = new List<int>();
            for (int i = 0; i < dicePool; i++)
            {
                dice.Add(roller.Next(1, 6));
            }
            var remaining = dice.Where(i => i >= attack.Skill);
            Console.WriteLine("%s attacks connected", remaining.Count());
            for (int i = 0; i < remaining.Count(); i++)
            {
                //remaining number of dice get rolled, need result based on strength vs tough.

            }
            for (int i = 0; i < dicePool; i++)
            {
                //Check AP
            }
            // subtract wounds
            //determine killer
        }

    }
}
