using Snorehammer.Web.FrontendModels;
using Snorehammer.Web.FrontendModels.Simulations;
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
        public async Task ResetDice(MultiFightSimulation multiSim)
        {
            _random = new Random(Guid.NewGuid().GetHashCode());
            foreach (var sim in multiSim.FightSimulations)
            {
                sim.Reset();
            }
            await Task.Yield();
        }
        public void SimulateAllFights(MultiFightSimulation multiSim)
        {
            foreach (var sim in multiSim.FightSimulations)
            {
                SimulateFight(sim);
            }
            multiSim.Stats.SetAverages(multiSim.FightSimulations);
        }
        public void SimulateFight(FightSimulation sim)
        {
            sim.Reset();
            if (sim.Attacker.Attacks[0].IsVariableAttacks)
            {
                RollAttackDice(sim);
            }
            DetermineHitTarget(sim);
            RollToHit(sim);
            RollStrengthStep(sim);
            RollArmorSaves(sim);

            if (sim.Attacker.Attacks[0].IsVariableDamage)
            {
                RollDamageDice(sim);
            }
            if (sim.Defender.FeelNoPain)
            {
                RollFeelNoPain(sim);
            }
            sim.WinnerMessage = GenerateWinnerMessage(sim);
        }
        public void RollAttackDice(FightSimulation sim)
        {
            sim.AttackDice = new List<Dice>();
            for (int j = 0; j < sim.Attacker.Attacks[0].WeaponsInUnit; j++)
            {
                for (int i = 0; i < sim.Attacker.Attacks[0].VariableAttackDiceNumber; i++)
                {
                    sim.AttackDice.Add(new Dice(0, _random, sim.Attacker.Attacks[0].VariableAttackDiceSides));
                }
            }
        }
        public void DetermineHitTarget(FightSimulation sim)
        {
            sim.HitTarget = sim.Attacker.Attacks[0].Skill;
            if (sim.Attacker.Attacks[0].Plus1Hit)
            {
                sim.HitTarget--;
            }
            if (sim.Defender.Stealth || sim.Defender.Minus1Hit || sim.Attacker.Attacks[0].Minus1Hit || sim.Attacker.Attacks[0].BigGuns)
            {
                sim.HitTarget++;
            }
            if (sim.HitTarget < 2)
            {
                sim.HitTarget = 2;
            }
            if (sim.HitTarget > 6)
            {
                sim.HitTarget = 6;
            }
        }
        public void RollToHit(FightSimulation sim)
        {
            sim.ToHitDice = new List<Dice>();
            if (sim.Attacker.Attacks[0].Blast && !sim.Attacker.Attacks[0].Melee)
            {
                sim.BlastBonus = sim.Defender.ModelCount / 5;
            }
            sim.AttackNumber = sim.Attacker.Attacks[0].Attacks;
            if (sim.AttackDice.Count != 0)
            {
                //currently doesn't account for "per model" just a flat equation, so 1d6 +1 with 10 models must be written as 10d6 + 10
                sim.AttackNumber = sim.AttackDice.Sum(d => d.Result) + sim.Attacker.Attacks[0].VariableAttackDiceConstant;
                if (sim.Attacker.Attacks[0].Blast && !sim.Attacker.Attacks[0].Melee)
                {
                    sim.AttackNumber += sim.BlastBonus * sim.Attacker.Attacks[0].WeaponsInUnit;
                }
            }
            if (sim.Attacker.Attacks[0].Torrent && !sim.Attacker.Attacks[0].Melee)
            {
                for (int j = 0; j < sim.Attacker.Attacks[0].WeaponsInUnit; j++)
                {
                    for (int i = 0; i < sim.AttackNumber; i++)
                    {
                        sim.ToHitDice.Add(new Dice(true));
                    }
                }
                //torrent attacks don't count as criticals, incase there are lethal hits or sustained involved
                sim.ToHitDice.ForEach(d => d.Critical = false);
                return;
            }
            for (int j = 0; j < sim.Attacker.Attacks[0].WeaponsInUnit; j++)
            {
                for (int i = 0; i < sim.AttackNumber; i++)
                {
                    sim.ToHitDice.Add(new Dice(sim.HitTarget, _random));
                }
            }
            if (sim.Attacker.Attacks[0].RerollHit)
            {

                var failed = sim.ToHitDice.Where(d => !d.Success);
                sim.ToHitDice = sim.ToHitDice.Where(d => d.Success).ToList();
                foreach (var die in failed)
                {
                    die.Reroll(_random);
                    sim.ToHitDice.Add(die);
                }
            }
            else if (sim.Attacker.Attacks[0].Reroll1Hit)
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
        public void DetermineWoundTarget(FightSimulation sim)
        {
            var strength = sim.Attacker.Attacks[0].Strength;
            var toughness = sim.Defender.Toughness;
            if (toughness == strength)
            {
                sim.WoundTarget = 4;
            }
            else if (toughness > strength)
            {

                if (toughness >= strength * 2)
                {
                    sim.WoundTarget = 6;
                }
                sim.WoundTarget = 5;
            }
            else
            {
                if (strength >= toughness * 2)
                {
                    sim.WoundTarget = 2;
                }
                sim.WoundTarget = 3;
                if (sim.Defender.MinusOneToWoundAgainstStronger)
                {
                    sim.WoundTarget++;
                }
            }
        }
        public void DetermineModdedWoundTarget(FightSimulation sim)
        {
            DetermineWoundTarget(sim);
            sim.ModdedWoundTarget = sim.WoundTarget;
            if (sim.Attacker.Attacks[0].Plus1Wound || (sim.Attacker.Attacks[0].Lance && sim.Attacker.Attacks[0].Melee))
            {
                sim.WoundTarget--;
            }
            if (sim.Defender.Stealth || sim.Defender.Minus1Hit)
            {
                sim.WoundTarget++;
            }
            if (sim.WoundTarget < 2)
            {
                sim.WoundTarget = 2;
            }
            if (sim.WoundTarget > 6)
            {
                sim.WoundTarget = 6;
            }
        }
        public void RollStrengthStep(FightSimulation sim)
        {
            DetermineModdedWoundTarget(sim);
            var targetValue = sim.ModdedWoundTarget;
            if (sim.Attacker.Attacks[0].Sustained)
            {
                for (int i = 0; i < sim.ToHitDice.Where(d => d.Critical).Count(); i++)
                {
                    for (int j = 0; j < sim.Attacker.Attacks[0].SustainAmount; j++)
                    {
                        sim.StrengthDice.Add(new Dice(targetValue, _random));
                    }
                }
            }
            if (sim.Attacker.Attacks[0].Lethal)
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
            if (sim.Attacker.Attacks[0].RerollWound)
            {
                var failed = sim.StrengthDice.Where(d => !d.Success);
                sim.StrengthDice = sim.StrengthDice.Where(d => d.Success).ToList();
                foreach (var die in failed)
                {
                    die.Reroll(_random);
                    sim.StrengthDice.Add(die);
                }
            }
            else if (sim.Attacker.Attacks[0].Reroll1Wound)
            {
                var failed = sim.StrengthDice.Where(d => d.Result == 1);
                sim.StrengthDice = sim.StrengthDice.Where(d => d.Result >= 1).ToList();
                foreach (var die in failed)
                {
                    die.Reroll(_random);
                    sim.StrengthDice.Add(die);
                }
            }
            sim.Stats.AttacksHit = sim.StrengthDice.Count();
        }
        public void RollArmorSaves(FightSimulation sim)
        {

            var targetValue = DetermineArmorSave(sim);
            if (sim.Attacker.Attacks[0].Devastating)
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
            if (sim.Defender.ArmorReroll)
            {
                var failed = sim.ArmorDice.Where(d => !d.Success);
                sim.ArmorDice = sim.ArmorDice.Where(d => d.Success).ToList();
                foreach (var die in failed)
                {
                    die.Reroll(_random);
                    sim.ArmorDice.Add(die);
                }
            }
            else if (sim.Defender.Reroll1Save)
            {
                var failed = sim.ArmorDice.Where(d => d.Result == 1);
                sim.ArmorDice = sim.ArmorDice.Where(d => d.Result >= 1).ToList();
                foreach (var die in failed)
                {
                    die.Reroll(_random);
                    sim.ArmorDice.Add(die);
                }
            }
            if (!sim.Attacker.Attacks[0].IsVariableDamage)
            {

                int failedsaves = sim.ArmorDice.Where(d => !d.Success).Count();
                sim.DamageNumber = failedsaves * sim.Attacker.Attacks[0].Damage;
                if (sim.Attacker.Attacks[0].Melta && !sim.Attacker.Attacks[0].Melee)
                {
                    sim.DamageNumber += failedsaves * sim.Attacker.Attacks[0].MeltaDamage;
                }
            }
            sim.Stats.WoundsInflicted = sim.ArmorDice.Count();
        }

        public int DetermineArmorSave(FightSimulation sim)
        {
            int moddedSave = sim.Defender.MinimumSave + sim.Attacker.Attacks[0].ArmorPenetration;

            if (sim.Defender.HasCover)
            {
                if ((sim.Defender.MinimumSave > 4 && sim.Attacker.Attacks[0].ArmorPenetration == 0) || sim.Attacker.Attacks[0].ArmorPenetration > 0)
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
        public void RollDamageDice(FightSimulation sim)
        {
            sim.WoundDice = new List<Dice>();
            sim.Stats.ArmorSavesFailed = sim.ArmorDice.Where(d => !d.Success).Count();
            for (int i = 0; i < sim.Stats.ArmorSavesFailed; i++)
            {
                for (int j = 0; j < sim.Attacker.Attacks[0].VariableDamageDiceNumber; j++)
                {
                    sim.WoundDice.Add(new Dice(0, _random, sim.Attacker.Attacks[0].VariableDamageDiceSides));
                }
            }
            sim.DamageNumber = sim.WoundDice.Sum(d => d.Result) + sim.Attacker.Attacks[0].VariableDamageDiceConstant * sim.Stats.ArmorSavesFailed;
            if (sim.Attacker.Attacks[0].Melta && !sim.Attacker.Attacks[0].Melee)
            {
                sim.DamageNumber += sim.Attacker.Attacks[0].MeltaDamage * sim.Stats.ArmorSavesFailed;
            }
        }
        public void RollFeelNoPain(FightSimulation sim)
        {
            if (!sim.Defender.FeelNoPain)
            {
                throw new InvalidOperationException("Defender has no Feel no pain save, and attempted to roll one");
            }
            for (int i = 0; i < sim.DamageNumber; i++)
            {
                sim.FeelNoPainDice.Add(new Dice(sim.Defender.FeelNoPainTarget, _random));
            }
            sim.Stats.FeelNoPainMade = sim.FeelNoPainDice.Where(d => d.Success).Count();

        }
        public string GenerateWinnerMessage(FightSimulation sim)
        {
            var res = new StringBuilder();
            sim.Stats.ArmorSavesFailed = sim.ArmorDice.Where(d => !d.Success).Count();

            res.Append($"{sim.Stats.ArmorSavesFailed} out of {sim.Attacker.Attacks[0].Attacks} attacks broke through armor.\n");
            int inflictedWounds = sim.DamageNumber;
            var fnpBlockedWounds = sim.FeelNoPainDice.Where(d => d.Success).Count();
            if (sim.Defender.FeelNoPain && inflictedWounds != 0)
            {
                if (fnpBlockedWounds == inflictedWounds)
                {
                    res.Append("All wounds blocked by feel no pain. \n");
                    return res.ToString();
                }
                res.Append($"{fnpBlockedWounds} of {inflictedWounds} wounds blocked by Feel No Pain. \n");
                inflictedWounds -= fnpBlockedWounds;
            }
            sim.Stats.WoundsInflicted = inflictedWounds;
            if (inflictedWounds > 0)
            {
                sim.Stats.UnitDamaged = true;
                res.Append($"{inflictedWounds} wounds inflicted to defender.\n");
                int AttacksApplied = 0;
                var DamageDiceCopy = new List<Dice>();
                if (sim.Attacker.Attacks[0].IsVariableDamage)
                {
                    DamageDiceCopy.AddRange(sim.WoundDice);
                }
                int fnpUnused = fnpBlockedWounds;
                int singleModelRemainingWounds = sim.Defender.Wounds;
                while (sim.Stats.ModelsDestroyed < sim.Defender.ModelCount && AttacksApplied < sim.Stats.ArmorSavesFailed)
                {
                    if (!sim.Attacker.Attacks[0].IsVariableDamage)
                    {
                        singleModelRemainingWounds = sim.Defender.Wounds;
                        while (singleModelRemainingWounds > 0 && AttacksApplied < sim.Stats.ArmorSavesFailed)
                        {
                            int fnpBlock = 0;
                            AttacksApplied++;
                            //if we have any unused feelnopains absorb damage with them.
                            if (fnpUnused > 0)
                            {
                                //set fnpBlock to either the damage done, or the remaining fnp available
                                if (fnpUnused < singleModelRemainingWounds)
                                {
                                    fnpBlock = fnpUnused;
                                }
                                else
                                {
                                    fnpBlock = sim.Attacker.Attacks[0].Damage;
                                }
                                fnpUnused -= fnpBlock;
                            }
                            singleModelRemainingWounds -= sim.Attacker.Attacks[0].Damage - fnpBlock;
                            if (singleModelRemainingWounds <= 0)
                            {
                                sim.Stats.ModelsDestroyed++;
                            }
                        }
                    }
                    else
                    {
                        //uses variable damage stats
                        singleModelRemainingWounds = sim.Defender.Wounds;
                        while (singleModelRemainingWounds > 0 && AttacksApplied < sim.Stats.ArmorSavesFailed)
                        {
                            int fnpBlock = 0;
                            AttacksApplied++;
                            //if we have any unused feelnopains absorb damage with them.
                            if (fnpUnused > 0)
                            {
                                //set fnpBlock to either the damage done, or the remaining fnp available
                                if (fnpUnused < singleModelRemainingWounds)
                                {
                                    fnpBlock = fnpUnused;
                                }
                                else
                                {
                                    fnpBlock = DamageDiceCopy.First().Result + sim.Attacker.Attacks[0].VariableDamageDiceConstant;
                                    if (sim.Attacker.Attacks[0].Melta && !sim.Attacker.Attacks[0].Melee)
                                    {
                                        fnpBlock += sim.Attacker.Attacks[0].MeltaDamage;
                                    }
                                }
                                fnpUnused -= fnpBlock;

                            }
                            singleModelRemainingWounds -= DamageDiceCopy.First().Result + sim.Attacker.Attacks[0].VariableDamageDiceConstant - fnpBlock;
                            if (sim.Attacker.Attacks[0].Melta && !sim.Attacker.Attacks[0].Melee)
                            {
                                singleModelRemainingWounds -= sim.Attacker.Attacks[0].MeltaDamage;
                            }
                            if (singleModelRemainingWounds <= 0)
                            {
                                sim.Stats.ModelsDestroyed++;
                            }
                            DamageDiceCopy.Remove(DamageDiceCopy.First());
                        }
                    }

                }
                if (sim.Stats.ModelsDestroyed >= sim.Defender.ModelCount)
                {
                    sim.Stats.UnitEntirelyDestroyed = true;
                    res.Append("The entire unit was destroyed.\n");
                    return res.ToString();
                }
                if (sim.Defender.ModelCount > 1)
                {
                    if (sim.Stats.ModelsDestroyed > 0)
                    {
                        sim.Stats.LostAModel = true;
                    }
                    if (sim.Stats.ModelsDestroyed > sim.Defender.ModelCount / 2)
                    {
                        sim.Stats.LessThanHalf = true;
                    }
                    res.Append($"{sim.Stats.ModelsDestroyed} out of {sim.Defender.ModelCount} models were destroyed.\n");

                    if (singleModelRemainingWounds >= 0)
                    {
                        res.Append($"A remaining model was inflicted {sim.Defender.Wounds - singleModelRemainingWounds} wounds, leaving it with {singleModelRemainingWounds} remaining.\n");
                    }
                    return res.ToString();
                }
                if (sim.Defender.Wounds - inflictedWounds < sim.Defender.Wounds / 2)
                {
                    sim.Stats.LessThanHalf = true;
                }
                res.Append($"The model has {sim.Defender.Wounds - inflictedWounds} wound(s) remaining.\n");
            }
            return res.ToString();
        }
    }
}
