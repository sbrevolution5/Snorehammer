﻿using Polly.Timeout;
using Snorehammer.Web.FrontendModels;
using Snorehammer.Web.FrontendModels.Profiles;
using Snorehammer.Web.FrontendModels.Simulations;
using Snorehammer.Web.FrontendModels.Stats;
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
                sim.WeaponSimulations.Clear();
                var i = 0;
                foreach (var weapon in sim.Attacker.Attacks)
                {
                    sim.WeaponSimulations.Add(new WeaponSimulation((AttackProfile)weapon.Clone(), (UnitProfile)sim.Defender.Clone(), i));
                    i++;
                }
                SimulateFight(sim);
            }
            multiSim.Stats.SetAverages(multiSim.FightSimulations);
            //should call set averages on a list of each weapon fight
            multiSim.Stats.PerWeaponStats.Clear();
            List<List<WeaponSimulation>> listPerWeapon = new List<List<WeaponSimulation>>();
            foreach (var weapon in multiSim.Attacker.Attacks)
            {
                //need to combine the fights per weapon into a list with only that weapon.  
                var singleWeaponList = multiSim.FightSimulations.SelectMany(f => f.WeaponSimulations.Where(w => w.Weapon.Id == weapon.Id));
                //then add a per weapon stats object for each weapon
                var multiStats = new MultiFightStats();
                //then run set averages on each list and add to overall simulation
                multiStats.SetAverages(singleWeaponList);
                multiSim.Stats.PerWeaponStats.Add(multiStats);
            }
        }
        public void SimulateFight(FightSimulation sim)
        {
            sim.Reset();
            foreach (var weapon in sim.WeaponSimulations)
            {
                if (weapon.Weapon.IsVariableAttacks)
                {
                    RollAttackDice(weapon);
                }
                DetermineHitTarget(weapon);
                RollToHit(weapon);
                RollStrengthStep(weapon);
                RollArmorSaves(weapon);

                if (sim.Attacker.Attacks[0].IsVariableDamage)
                {
                    RollDamageDice(weapon);
                }
                if (sim.Defender.FeelNoPain)
                {
                    RollFeelNoPain(weapon);
                }
            }
            DealDamage(sim);
            sim.WinnerMessage = GenerateWinnerMessage(sim);
        }
        public void RollAttackDice(WeaponSimulation sim)
        {
            sim.AttackDice = new List<Dice>();
            for (int j = 0; j < sim.Weapon.WeaponsInUnit; j++)
            {
                for (int i = 0; i < sim.Weapon.VariableAttackDiceNumber; i++)
                {
                    sim.AttackDice.Add(new Dice(0, _random, sim.Weapon.VariableAttackDiceSides));
                }
            }
        }
        public void DetermineHitTarget(WeaponSimulation sim)
        {
            sim.HitTarget = sim.Weapon.Skill;
            if (sim.Weapon.Plus1Hit)
            {
                sim.HitTarget--;
            }
            if (sim.Defender.Stealth || sim.Defender.Minus1Hit || sim.Weapon.Minus1Hit || sim.Weapon.BigGuns)
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
        public void RollToHit(WeaponSimulation sim)
        {
            sim.ToHitDice = new List<Dice>();
            if (sim.Weapon.Blast && !sim.Weapon.Melee)
            {
                sim.BlastBonus = sim.Defender.ModelCount / 5;
            }
            sim.Stats.AttackNumber = sim.Weapon.Attacks * sim.Weapon.WeaponsInUnit;
            if (sim.AttackDice.Count != 0)
            {
                //currently doesn't account for "per model" just a flat equation, so 1d6 +1 with 10 models must be written as 10d6 + 10
                sim.Stats.AttackNumber = sim.AttackDice.Sum(d => d.Result) + (sim.Weapon.VariableAttackDiceConstant * sim.Weapon.WeaponsInUnit);
                if (sim.Weapon.Blast && !sim.Weapon.Melee)
                {
                    sim.Stats.AttackNumber += sim.BlastBonus * sim.Weapon.WeaponsInUnit;
                }
            }
            if (sim.Weapon.Torrent && !sim.Weapon.Melee)
            {
                for (int j = 0; j < sim.Weapon.WeaponsInUnit; j++)
                {
                    for (int i = 0; i < sim.Stats.AttackNumber; i++)
                    {
                        sim.ToHitDice.Add(new Dice(true));
                    }
                }
                //torrent attacks don't count as criticals, incase there are lethal hits or sustained involved
                sim.ToHitDice.ForEach(d => d.Critical = false);
                return;
            }
            for (int i = 0; i < sim.Stats.AttackNumber; i++)
            {
                sim.ToHitDice.Add(new Dice(sim.HitTarget, _random));
            }
            if (sim.Weapon.RerollHit)
            {

                var failed = sim.ToHitDice.Where(d => !d.Success);
                sim.ToHitDice = sim.ToHitDice.Where(d => d.Success).ToList();
                foreach (var die in failed)
                {
                    die.Reroll(_random);
                    sim.ToHitDice.Add(die);
                }
            }
            else if (sim.Weapon.Reroll1Hit)
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
        public void DetermineWoundTarget(WeaponSimulation sim)
        {
            var strength = sim.Weapon.Strength;
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
        public void DetermineModdedWoundTarget(WeaponSimulation sim)
        {
            DetermineWoundTarget(sim);
            sim.ModdedWoundTarget = sim.WoundTarget;
            if (sim.Weapon.Plus1Wound || (sim.Weapon.Lance && sim.Weapon.Melee))
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
        public void RollStrengthStep(WeaponSimulation sim)
        {
            DetermineModdedWoundTarget(sim);
            var targetValue = sim.ModdedWoundTarget;
            if (sim.Weapon.Sustained)
            {
                for (int i = 0; i < sim.ToHitDice.Where(d => d.Critical).Count(); i++)
                {
                    for (int j = 0; j < sim.Weapon.SustainAmount; j++)
                    {
                        sim.StrengthDice.Add(new Dice(targetValue, _random));
                    }
                }
            }
            if (sim.Weapon.Lethal)
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
            if (sim.Weapon.RerollWound)
            {
                var failed = sim.StrengthDice.Where(d => !d.Success);
                sim.StrengthDice = sim.StrengthDice.Where(d => d.Success).ToList();
                foreach (var die in failed)
                {
                    die.Reroll(_random);
                    sim.StrengthDice.Add(die);
                }
            }
            else if (sim.Weapon.Reroll1Wound)
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
        public void RollArmorSaves(WeaponSimulation sim)
        {

            var targetValue = DetermineArmorSave(sim);
            if (sim.Weapon.Devastating)
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
            if (!sim.Weapon.IsVariableDamage)
            {

                int failedsaves = sim.ArmorDice.Where(d => !d.Success).Count();
                sim.Stats.DamageNumber = failedsaves * sim.Weapon.Damage;
                if (sim.Weapon.Melta && !sim.Weapon.Melee)
                {
                    sim.Stats.DamageNumber += failedsaves * sim.Weapon.MeltaDamage;
                }
            }
            sim.Stats.ArmorSavesFailed = sim.ArmorDice.Where(d => !d.Success).Count();
            sim.Stats.WoundsSuccessful = sim.ArmorDice.Count();
        }

        public int DetermineArmorSave(WeaponSimulation sim)
        {
            int moddedSave = sim.Defender.MinimumSave + sim.Weapon.ArmorPenetration;

            if (sim.Defender.HasCover)
            {
                if ((sim.Defender.MinimumSave >= 4 && sim.Weapon.ArmorPenetration == 0) || sim.Weapon.ArmorPenetration > 0)
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
        public void RollDamageDice(WeaponSimulation sim)
        {
            sim.WoundDice = new List<Dice>();
            sim.Stats.ArmorSavesFailed = sim.ArmorDice.Where(d => !d.Success).Count();
            for (int i = 0; i < sim.Stats.ArmorSavesFailed; i++)
            {
                for (int j = 0; j < sim.Weapon.VariableDamageDiceNumber; j++)
                {
                    sim.WoundDice.Add(new Dice(0, _random, sim.Weapon.VariableDamageDiceSides));
                }
            }
            sim.Stats.DamageNumber = sim.WoundDice.Sum(d => d.Result) + sim.Weapon.VariableDamageDiceConstant * sim.Stats.ArmorSavesFailed;
            if (sim.Weapon.Melta && !sim.Weapon.Melee)
            {
                sim.Stats.DamageNumber += sim.Weapon.MeltaDamage * sim.Stats.ArmorSavesFailed;
            }
            sim.Stats.PreFNPDamage = sim.Stats.DamageNumber;

        }
        public void RollFeelNoPain(WeaponSimulation sim)
        {
            if (!sim.Defender.FeelNoPain)
            {
                throw new InvalidOperationException("Defender has no Feel no pain save, and attempted to roll one");
            }
            for (int i = 0; i < sim.Stats.DamageNumber; i++)
            {
                sim.FeelNoPainDice.Add(new Dice(sim.Defender.FeelNoPainTarget, _random));
            }
            sim.Stats.FeelNoPainMade = sim.FeelNoPainDice.Where(d => d.Success).Count();
            sim.Stats.WoundsInflicted = sim.Stats.DamageNumber;
            sim.Stats.DamageNumber -= sim.Stats.FeelNoPainMade;
        }
        public void DealDamage(FightSimulation sim)
        {
            //sets baseline model wounds, which get changed after damage is dealt
            sim.Stats.SingleModelRemainingWounds = sim.Defender.Wounds;
            foreach (var weaponSim in sim.WeaponSimulations)
            {
                //weaponsim has current remaining wounds
                weaponSim.Stats.SingleModelRemainingWounds = sim.Stats.SingleModelRemainingWounds;
                DealDamageFromWeapon(sim, weaponSim);
                //if we ended up with a damaged model, the overall damage needs to be updated on the simulation
                sim.Stats.SingleModelRemainingWounds = weaponSim.Stats.SingleModelRemainingWounds;
            }
            CompileStatsFromWeapons(sim);
        }

        private void DealDamageFromWeapon(FightSimulation sim, WeaponSimulation weaponSim)
        {
            if (weaponSim.Stats.DamageNumber >= 1)
            {
                weaponSim.Stats.UnitDamaged = true;
                int AttacksApplied = 0;
                var DamageDiceCopy = new List<Dice>();
                if (weaponSim.Weapon.IsVariableDamage)
                {
                    DamageDiceCopy.AddRange(weaponSim.WoundDice);
                }
                int fnpUnused = weaponSim.Stats.FeelNoPainMade;
                //loops through models
                while (weaponSim.Stats.ModelsDestroyed < weaponSim.Defender.ModelCount && AttacksApplied < weaponSim.Stats.ArmorSavesFailed)
                {
                    //loops through damage on individual model
                    while (weaponSim.Stats.SingleModelRemainingWounds > 0 && AttacksApplied < weaponSim.Stats.ArmorSavesFailed)
                    {
                        int fnpBlock = 0;
                        AttacksApplied++;
                        //if we have any unused feelnopains absorb damage with them.
                        var weaponDamage = 0;
                        if (!weaponSim.Weapon.IsVariableDamage)
                        {
                            weaponDamage = weaponSim.Weapon.Damage;
                        }
                        else
                        {
                            weaponDamage = DamageDiceCopy.First().Result + weaponSim.Weapon.VariableDamageDiceConstant;
                            DamageDiceCopy.Remove(DamageDiceCopy.First());
                        }
                        if (weaponSim.Weapon.Melta && !weaponSim.Weapon.Melee)
                        {
                            weaponDamage += weaponSim.Weapon.MeltaDamage;
                        }
                        if (fnpUnused > 0)
                        {
                            //set fnpBlock to either the damage done, or the remaining fnp available
                            if (fnpUnused < weaponDamage)
                            {
                                fnpBlock = fnpUnused;
                            }
                            else
                            {
                                fnpBlock = weaponDamage;
                            }
                            fnpUnused -= fnpBlock;
                        }
                        weaponSim.Stats.SingleModelRemainingWounds -= weaponDamage - fnpBlock;
                        if (weaponSim.Stats.SingleModelRemainingWounds <= 0)
                        {
                            weaponSim.Stats.ModelsDestroyed++;
                        }
                    }
                    if (AttacksApplied != weaponSim.Stats.ArmorSavesFailed)
                    {
                        weaponSim.Stats.SingleModelRemainingWounds = weaponSim.Defender.Wounds;
                    }

                }
            }
            SetDamageStatsAfterWound(weaponSim);
        }

        private void SetDamageStatsAfterWound(WeaponSimulation weaponSim)
        {
            if (weaponSim.Stats.ModelsDestroyed >= weaponSim.Defender.ModelCount)
            {
                weaponSim.Stats.UnitEntirelyDestroyed = true;
            }
            if (weaponSim.Defender.ModelCount > 1)
            {
                if (weaponSim.Stats.ModelsDestroyed > 0)
                {
                    weaponSim.Stats.LostAModel = true;
                }
                if (weaponSim.Stats.ModelsDestroyed > weaponSim.Defender.ModelCount / 2)
                {
                    weaponSim.Stats.LessThanHalf = true;
                }
            }
            else if (weaponSim.Defender.Wounds - weaponSim.Stats.DamageNumber < weaponSim.Defender.Wounds / 2)
            {
                weaponSim.Stats.LessThanHalf = true;
            }
            weaponSim.Stats.ColumnName = weaponSim.Weapon.Name;
        }

        private void CompileStatsFromWeapons(FightSimulation sim)
        {
            sim.Stats.AttackNumber = sim.WeaponSimulations.Select(s => s.Stats.AttackNumber).Sum();
            sim.Stats.DamageNumber = sim.WeaponSimulations.Select(s => s.Stats.DamageNumber).Sum();
            sim.Stats.ModelsDestroyed = sim.WeaponSimulations.Select(s => s.Stats.ModelsDestroyed).Sum();
            sim.Stats.PreFNPDamage = sim.WeaponSimulations.Select(s => s.Stats.PreFNPDamage).Sum();
            sim.Stats.FeelNoPainMade = sim.WeaponSimulations.Select(s => s.Stats.FeelNoPainMade).Sum();
            sim.Stats.ArmorSavesFailed = sim.WeaponSimulations.Select(s => s.Stats.ArmorSavesFailed).Sum();
            sim.Stats.AttacksHit = sim.WeaponSimulations.Select(s => s.Stats.AttacksHit).Sum();
            sim.Stats.WoundsSuccessful = sim.WeaponSimulations.Select(s => s.Stats.WoundsSuccessful).Sum();
            sim.Stats.WoundsInflicted = sim.WeaponSimulations.Select(s => s.Stats.WoundsInflicted).Sum();
            sim.Stats.LostAModel = sim.WeaponSimulations.Where(s => s.Stats.LostAModel).Any();
            sim.Stats.UnitDamaged = sim.WeaponSimulations.Where(s => s.Stats.UnitDamaged).Any();
            sim.Stats.UnitEntirelyDestroyed = sim.WeaponSimulations.Where(s => s.Stats.UnitEntirelyDestroyed).Any();
        }
        public void ValidateResult(FightSimulation sim)
        {
            if(sim.Stats.ArmorSavesFailed > sim.Stats.AttackNumber)
            {
                throw new InvalidProgramException("There are more failed armor saves than attacks");
            }
            if(sim.Stats.LostAModel && sim.Stats.ModelsDestroyed == 0)
            {
                throw new InvalidProgramException("Lost a model is true but 0 models were destroyed");
            }
        }
        public string GenerateWinnerMessage(FightSimulation sim)
        {
            ValidateResult(sim);
            var res = new StringBuilder();

            res.Append($"{sim.Stats.ArmorSavesFailed} out of {sim.Stats.AttackNumber} attacks broke through armor.\n");
            if (sim.Defender.FeelNoPain && sim.Stats.PreFNPDamage != 0)
            {
                if (sim.Stats.FeelNoPainMade == sim.Stats.PreFNPDamage)
                {
                    res.Append("All wounds blocked by feel no pain. \n");
                    return res.ToString();
                }
                res.Append($"{sim.Stats.FeelNoPainMade} of {sim.Stats.PreFNPDamage} wounds blocked by Feel No Pain. \n");
            }
            if (sim.Stats.WoundsInflicted > 0)
            {
                res.Append($"{sim.Stats.WoundsInflicted} wounds inflicted to defender.\n");
                if (sim.Stats.ModelsDestroyed >= sim.Defender.ModelCount)
                {
                    res.Append("The entire unit was destroyed.\n");
                    return res.ToString();
                }
                if (sim.Defender.ModelCount > 1)
                {
                    res.Append($"{sim.Stats.ModelsDestroyed} out of {sim.Defender.ModelCount} models were destroyed.\n");

                    if (sim.Stats.SingleModelRemainingWounds > 0 && sim.Stats.SingleModelRemainingWounds != sim.Defender.Wounds)
                    {
                        res.Append($"A remaining model was inflicted {sim.Defender.Wounds - sim.Stats.SingleModelRemainingWounds} wounds, leaving it with {sim.Stats.SingleModelRemainingWounds} remaining.\n");
                    }
                    return res.ToString();
                }
                res.Append($"The model has {sim.Defender.Wounds - sim.Stats.SingleModelRemainingWounds} wound(s) remaining.\n");
            }
            return res.ToString();
        }
    }
}
