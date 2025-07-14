using Microsoft.Extensions.Primitives;
using Snorehammer.Web.FrontendModels;
using System.Text;

namespace Snorehammer.Web.Services
{
    public class FightSimulationService
    {
        private Random _random;
        public FightSimulationService()
        {
            _random = new Random();
        }
        public List<Dice> SimulateToHitRoll(AttackProfile attack)
        {
            _random = new Random(Guid.NewGuid().GetHashCode());
            var res = new List<Dice>();
            for (int i = 0; i < attack.Attacks; i++)
            {
                res.Add(new Dice(attack.Skill, _random));
            }
            if (attack.RerollHit) {
                
                var failed = res.Where(d => !d.Success);
                res = res.Where(d=> d.Success).ToList();
                foreach (var die in failed)
                {
                    die.Reroll(_random);
                    res.Add(die);
                }
            }
            return res;
        }
        public List<Dice> RollStrengthStep(UnitProfile defender, AttackProfile attack, List<Dice> dicePool)
        {
            var res = new List<Dice>();
            var targetValue = DetermineWoundTarget(defender.Toughness, attack.Strength);
            if (attack.Sustained)
            {
                for (int i = 0; i < dicePool.Where(d => d.Critical).Count(); i++)
                {
                    res.Add(new Dice(targetValue, _random));
                }
            }
            if (attack.Lethal)
            {

                for (int i = 0; i < dicePool.Where(d => d.Critical).Count(); i++)
                {
                    //skips rolling and sets result to a 7
                    res.Add(new Dice(true));
                }
                for (int i = 0; i < dicePool.Where(d => d.Success && !d.Critical).Count(); i++)
                {
                    res.Add(new Dice(targetValue, _random));
                }
                return res;
            }
            for (int i = 0; i < dicePool.Where(d => d.Success).Count(); i++)
            {
                res.Add(new Dice(targetValue, _random));
            }
            if (attack.RerollWound)
            {
                var failed = res.Where(d => !d.Success);
                res = res.Where(d => d.Success).ToList();
                foreach (var die in failed)
                {
                    die.Reroll(_random);
                    res.Add(die);
                }
            }
            return res;
        }
        public List<Dice> RollArmorSaves(UnitProfile defender, AttackProfile attack, List<Dice> dicePool)
        {
            var res = new List<Dice>();
            var targetValue = DetermineArmorSave(defender, attack);
            if (attack.Devastating)
            {
                for (int i = 0; i < dicePool.Where(d => d.Critical).Count(); i++)
                {
                    //skips rolling and sets result to a 7
                    res.Add(new Dice(true));
                }
                for (int i = 0; i < dicePool.Where(d => d.Success && !d.Critical).Count(); i++)
                {
                    res.Add(new Dice(targetValue, _random));
                }
                if (attack.RerollWound)
                {
                    var failed = res.Where(d => !d.Success);
                    res = res.Where(d => d.Success).ToList();
                    foreach (var die in failed)
                    {
                        die.Reroll(_random);
                        res.Add(die);
                    }
                }
                return res;
            }
            for (int i = 0; i < dicePool.Where(d => d.Success).Count(); i++)
            {
                res.Add(new Dice(targetValue, _random));
            }
            if (attack.RerollWound)
            {
                var failed = res.Where(d => !d.Success);
                res = res.Where(d => d.Success).ToList();
                foreach (var die in failed)
                {
                    die.Reroll(_random);
                    res.Add(die);
                }
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
        public List<Dice> RollFeelNoPain(UnitProfile defender, AttackProfile attack, FightSimulation sim)
        {
            if (!defender.FeelNoPain)
            {
                throw new InvalidOperationException("Defender has no Feel no pain save, and attempted to roll one");
            }
            var res = new List<Dice>();
            for (int i = 0; i < sim.ArmorDice.Where(d=>!d.Success).Count()*attack.Damage; i++)
            {
                res.Add(new Dice(defender.FeelNoPainTarget, _random));
            }
            return res;
        }
        public string GenerateWinnerMessage(UnitProfile defender, AttackProfile attack, FightSimulation sim)
        {
            var res = new StringBuilder();
            var successful = sim.ArmorDice.Where(d => !d.Success).Count();
            res.Append($"{successful} out of {attack.Attacks} attacks broke through armor.\n");
            int inflictedWounds = successful * attack.Damage;
            if (defender.FeelNoPain && inflictedWounds != 0)
            {
                var fnpBlockedWounds = sim.FeelNoPainDice.Where(d => d.Success ).Count();
                if(fnpBlockedWounds == inflictedWounds)
                {
                    res.Append("All wounds blocked by feel no pain. \n");
                    return res.ToString();
                }
                res.Append($"{fnpBlockedWounds} of {inflictedWounds} wounds blocked by Feel No Pain. \n");
                inflictedWounds -= fnpBlockedWounds;
            }
            if (inflictedWounds > 0)
            {
                res.Append($"{inflictedWounds} wounds inflicted to defender.\n");

                int totalWounds = defender.Wounds * defender.ModelCount;
                var destroyedModels = inflictedWounds / defender.Wounds;
                if (destroyedModels >= defender.ModelCount)
                {
                    res.Append("The entire unit was destroyed.\n");
                    return res.ToString();
                }
                if (defender.ModelCount > 1)
                {
                    res.Append($"{destroyedModels} out of {defender.ModelCount} models were destroyed.\n");
                    int woundRemainder = inflictedWounds % defender.Wounds;
                    if (woundRemainder != 0)
                    {
                        res.Append($"A remaining model was inflicted {woundRemainder} wounds, leaving it with {defender.Wounds - woundRemainder} remaining.\n");
                    }
                    return res.ToString();
                }
                res.Append($"The model has {defender.Wounds - inflictedWounds} wound(s) remaining.\n");
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
