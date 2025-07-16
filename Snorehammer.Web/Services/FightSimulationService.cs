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
            _random = new Random(Guid.NewGuid().GetHashCode());
        }
        public void RollAttackDice(FightSimulation sim)
        {
            sim.AttackDice = new List<Dice>();
            for (int i = 0; i < sim.AttackProfile.VariableDiceNumber; i++)
            {
                //0 because target doesn't matter here
                sim.AttackDice.Add(new Dice(7, _random,sim.AttackProfile.VariableDiceSides));
            }
        }
        public void RollToHit(FightSimulation sim)
        {
            sim.ToHitDice = new List<Dice>();
            sim.BlastBonus = sim.Defender.ModelCount / 5;
            sim.AttackNumber = sim.AttackProfile.Attacks;
            if (sim.AttackDice.Count != 0)
            {
                sim.AttackNumber = sim.AttackDice.Sum(d => d.Result) + sim.AttackProfile.VariableDiceConstant;
                if (sim.AttackProfile.Blast)
                {
                    sim.AttackNumber += sim.BlastBonus;
                }
            }
            if (sim.AttackProfile.Torrent)
            {
                for (int i = 0; i < sim.AttackNumber; i++)
                {
                    sim.ToHitDice.Add(new Dice(true));
                }
                //torrent attacks don't count as criticals, incase there are lethal hits or sustained involved
                sim.ToHitDice.ForEach(d => d.Critical = false);
                return;
            }
            for (int i = 0; i < sim.AttackNumber; i++)
            {
                sim.ToHitDice.Add(new Dice(sim.AttackProfile.Skill, _random));
            }
            if (sim.AttackProfile.RerollHit)
            {

                var failed = sim.ToHitDice.Where(d => !d.Success);
                sim.ToHitDice = sim.ToHitDice.Where(d => d.Success).ToList();
                foreach (var die in failed)
                {
                    die.Reroll(_random);
                    sim.ToHitDice.Add(die);
                }
            }
            else if (sim.AttackProfile.Reroll1Hit)
            {
                var failed = sim.ToHitDice.Where(d => d.Result == 1);
                sim.ToHitDice = sim.ToHitDice.Where(d => d.Result >= 1).ToList();
                foreach (var die in failed)
                {
                    die.Reroll(_random);
                    sim.ToHitDice.Add(die);
                }
            }
        }
        public void RollStrengthStep(FightSimulation sim)
        {
            var targetValue = DetermineWoundTarget(sim.Defender.Toughness, sim.AttackProfile.Strength);
            if (sim.AttackProfile.Sustained)
            {
                for (int i = 0; i < sim.ToHitDice.Where(d => d.Critical).Count(); i++)
                {
                    sim.StrengthDice.Add(new Dice(targetValue, _random));
                }
            }
            if (sim.AttackProfile.Lethal)
            {

                for (int i = 0; i < sim.ToHitDice.Where(d => d.Critical).Count(); i++)
                {
                    //skips rolling and sets result to a 7
                    sim.StrengthDice.Add(new Dice(true));
                }
                for (int i = 0; i < sim.ToHitDice.Where(d => d.Success && !d.Critical).Count(); i++)
                {
                    sim.StrengthDice.Add(new Dice(targetValue, _random));
                }
            }
            for (int i = 0; i < sim.ToHitDice.Where(d => d.Success).Count(); i++)
            {
                sim.StrengthDice.Add(new Dice(targetValue, _random));
            }
            if (sim.AttackProfile.RerollWound)
            {
                var failed = sim.StrengthDice.Where(d => !d.Success);
                sim.StrengthDice = sim.StrengthDice.Where(d => d.Success).ToList();
                foreach (var die in failed)
                {
                    die.Reroll(_random);
                    sim.StrengthDice.Add(die);
                }
            }
        }
        public void RollArmorSaves(FightSimulation sim)
        {

            var targetValue = DetermineArmorSave(sim);
            if (sim.AttackProfile.Devastating)
            {
                for (int i = 0; i < sim.StrengthDice.Where(d => d.Critical).Count(); i++)
                {
                    //skips rolling and sets result to an automatic failure
                    sim.ArmorDice.Add(new Dice(false));
                }
                for (int i = 0; i < sim.StrengthDice.Where(d => d.Success && !d.Critical).Count(); i++)
                {
                    sim.ArmorDice.Add(new Dice(targetValue, _random));
                }
            }
            else
            {

                for (int i = 0; i < sim.StrengthDice.Where(d => d.Success).Count(); i++)
                {
                    sim.ArmorDice.Add(new Dice(targetValue, _random));
                }
            }
            if (sim.AttackProfile.RerollWound)
            {
                var failed = sim.ArmorDice.Where(d => !d.Success);
                sim.ArmorDice = sim.ArmorDice.Where(d => d.Success).ToList();
                foreach (var die in failed)
                {
                    die.Reroll(_random);
                    sim.ArmorDice.Add(die);
                }
            }

        }

        public int DetermineArmorSave(FightSimulation sim)
        {
            int moddedSave = sim.Defender.MinimumSave + sim.AttackProfile.ArmorPenetration;

            if (sim.Defender.HasCover)
            {
                if ((sim.Defender.MinimumSave > 4 && sim.AttackProfile.ArmorPenetration == 0) || sim.AttackProfile.ArmorPenetration > 0)
                {
                    moddedSave--;
                }
                else
                {
                    sim.CoverIgnored = true;
                }

            }
            if (moddedSave < 2)
            {
                moddedSave = 2;
            }
            if (moddedSave < sim.Defender.InvulnerableSave || sim.Defender.InvulnerableSave == 0)
            {
                if (moddedSave < 3 && sim.Defender.MinimumSave <= 3)
                {
                    sim.CoverIgnored = true;
                    sim.ArmorSave = sim.Defender.MinimumSave;
                    return sim.Defender.MinimumSave;
                }
                sim.ArmorSave = moddedSave;
                return moddedSave;
            }
            if (sim.Defender.HasCover)
            {
                sim.CoverIgnored = true;
            }
            sim.ArmorSave = sim.Defender.InvulnerableSave;
            return sim.Defender.InvulnerableSave;
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
        public void RollFeelNoPain(FightSimulation sim)
        {
            if (!sim.Defender.FeelNoPain)
            {
                throw new InvalidOperationException("Defender has no Feel no pain save, and attempted to roll one");
            }
            for (int i = 0; i < sim.ArmorDice.Where(d => !d.Success).Count() * sim.AttackProfile.Damage; i++)
            {
                sim.FeelNoPainDice.Add(new Dice(sim.Defender.FeelNoPainTarget, _random));
            }

        }
        public string GenerateWinnerMessage(FightSimulation sim)
        {
            var res = new StringBuilder();
            var successful = sim.ArmorDice.Where(d => !d.Success).Count();
            res.Append($"{successful} out of {sim.AttackProfile.Attacks} attacks broke through armor.\n");
            int inflictedWounds = successful * sim.AttackProfile.Damage;
            if (sim.Defender.FeelNoPain && inflictedWounds != 0)
            {
                var fnpBlockedWounds = sim.FeelNoPainDice.Where(d => d.Success).Count();
                if (fnpBlockedWounds == inflictedWounds)
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

                int totalWounds = sim.Defender.Wounds * sim.Defender.ModelCount;
                var destroyedModels = inflictedWounds / sim.Defender.Wounds;
                if (destroyedModels >= sim.Defender.ModelCount)
                {
                    res.Append("The entire unit was destroyed.\n");
                    return res.ToString();
                }
                if (sim.Defender.ModelCount > 1)
                {
                    res.Append($"{destroyedModels} out of {sim.Defender.ModelCount} models were destroyed.\n");
                    int woundRemainder = inflictedWounds % sim.Defender.Wounds;
                    if (woundRemainder != 0)
                    {
                        res.Append($"A remaining model was inflicted {woundRemainder} wounds, leaving it with {sim.Defender.Wounds - woundRemainder} remaining.\n");
                    }
                    return res.ToString();
                }
                res.Append($"The model has {sim.Defender.Wounds - inflictedWounds} wound(s) remaining.\n");
            }
            return res.ToString();
        }
    }
}
