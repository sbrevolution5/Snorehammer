using Polly.Timeout;
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
        public void SimulateAllFights(MultiFightSimulation multiSim, bool meleeFightBack = false)
        {
            foreach (var sim in multiSim.FightSimulations)
            {
                sim.WeaponSimulations.Clear();
                var i = 0;
                foreach (var weapon in sim.Attacker.Attacks)
                {
                    weapon.Id = i;
                    sim.WeaponSimulations.Add(
                        new WeaponSimulation((AttackProfile)weapon.Clone(), (UnitProfile)sim.Defender.Clone(), i));
                    i++;
                }
                if (meleeFightBack)
                {
                    sim.HasFightBack = true;
                    sim.FightBackSimulation = new FightSimulation(sim.Defender, sim.Attacker);
                }
                SimulateFight(sim, meleeFightBack);
            }
            CreateStats(multiSim, meleeFightBack);
        }

        private static void CreateStats(MultiFightSimulation multiSim,bool fightBack)
        {
            multiSim.Stats.SetAverages(multiSim.FightSimulations);
            multiSim.Stats.PerWeaponStats.Clear();
            foreach (var weapon in multiSim.Attacker.Attacks)
            {
                weapon.UnitName = multiSim.Attacker.Name;
                //need to combine the fights per weapon into a list with only that weapon.  
                var singleWeaponList = multiSim.FightSimulations.SelectMany(f => f.WeaponSimulations.Where(w => w.Id == weapon.Id)).ToList();
                //then add a per weapon stats object for each weapon
                var multiStats = new MultiFightStats();
                //then run set averages on each list and add to overall simulation
                multiStats.SetAverages(singleWeaponList);
                multiSim.Stats.PerWeaponStats.Add(multiStats);
            }
            if (fightBack)
            {
                multiSim.FightBackStats.SetAverages(multiSim.FightSimulations.Select(f => f.FightBackSimulation));
                multiSim.FightBackStats.PerWeaponStats.Clear();
                multiSim.FightBackStats.ColumnName = $"{multiSim.Defender.Name} vs {multiSim.Attacker.Name}";
                List<List<WeaponSimulation>> fblistPerWeapon = new List<List<WeaponSimulation>>();
                foreach (var weapon in multiSim.Defender.Attacks)
                {
                    weapon.UnitName = multiSim.Defender.Name;
                    //need to combine the fights per weapon into a list with only that weapon.  
                    var singleWeaponList = multiSim.FightSimulations.SelectMany(f => f.FightBackSimulation.WeaponSimulations.Where(w => w.Id == weapon.Id)).ToList();
                    //then add a per weapon stats object for each weapon
                    var multiStats = new MultiFightStats();
                    //then run set averages on each list and add to overall simulation
                    multiStats.SetAverages(singleWeaponList);
                    multiSim.FightBackStats.PerWeaponStats.Add(multiStats);
                }
            }
        }

        private void DetermineWeaponsRemaining(FightSimulation sim)
        {
            sim.FightBackSimulation.Attacker.Attacks.ForEach(a => a.WeaponsRemaining = a.WeaponsInUnit);
            //order by descending number in unit
            var atkList = sim.FightBackSimulation.Attacker.Attacks.Where(a => a.Melee).OrderByDescending(a => a.WeaponsInUnit);
            //start removing weapons remaining from most common, unless that weapon is empty
            var mostCommon = atkList.First(a => a.WeaponsRemaining > 0);
            for (int i = 0; i < sim.Stats.ModelsDestroyed; i++)
            {
                var currentWeapon = sim.FightBackSimulation.Attacker.Attacks.Where(a => a.Name == mostCommon.Name).First();
                currentWeapon.WeaponsRemaining--;
                if (mostCommon.WeaponsRemaining == 0)
                {
                    mostCommon = atkList.First(a => a.WeaponsRemaining > 0);
                }
            }
        }

        public void SimulateFight(FightSimulation sim, bool fightBack)
        {
            sim.Reset();
            foreach (var weapon in sim.WeaponSimulations)
            {
                if (sim.Attacker.Overwatch)
                {
                    weapon.Weapon.Overwatch = true;
                }
                SimulateFightWithWeapon(weapon);
            }
            DealDamage(sim);
            if (fightBack)
            {
                SetupFightback(sim);
                foreach (var fbweapon in sim.FightBackSimulation.WeaponSimulations)
                {
                    //copy from defender's weapons remaining, which were set in the determine method, 
                    SimulateFightWithWeapon(fbweapon);
                }
                DealDamage(sim.FightBackSimulation);
            }
            sim.WinnerMessage = GenerateWinnerMessage(sim);
        }

        private void SetupFightback(FightSimulation sim)
        {
            DetermineWeaponsRemaining(sim);
            var i = 0;
            foreach (var weapon in sim.Defender.Attacks.Where(a => a.Melee))
            {
                var weaponSim = new WeaponSimulation((AttackProfile)weapon.Clone(), (UnitProfile)sim.Attacker.Clone(), i, true,weapon.WeaponsRemaining);
                sim.FightBackSimulation.WeaponSimulations.Add(weaponSim);
                i++;
            }
            sim.FightBackSimulation.RemainingAttackingModels = sim.Defender.ModelCount - sim.Stats.ModelsDestroyed;
        }

        private void SimulateFightWithWeapon(WeaponSimulation weapon)
        {

            if (weapon.Weapon.IsVariableAttacks)
            {
                RollAttackDice(weapon);
            }
            DetermineHitTarget(weapon);
            RollToHit(weapon);
            RollStrengthStep(weapon);
            RollArmorSaves(weapon);

            if (weapon.Weapon.IsVariableDamage)
            {
                RollDamageDice(weapon);
            }
            if (weapon.Defender.FeelNoPain)
            {
                RollFeelNoPain(weapon);
            }
        }

        public void RollAttackDice(WeaponSimulation sim)
        {
            sim.AttackDice = new List<Dice>();
            for (int j = 0; j < sim.Weapon.WeaponsRemaining; j++)
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
            if (sim.Weapon.Heavy)
            {
                sim.HitTarget--;
            }
            if ((sim.Defender.Stealth && !sim.Weapon.Melee))
            {
                sim.HitTarget++;
            }
            if(sim.Defender.Minus1Hit)
            {
                sim.HitTarget++;
            }
            if(sim.Weapon.Minus1Hit)
            {
                sim.HitTarget++;
            }
            if(sim.Weapon.BigGuns && !sim.Weapon.Melee)
            {
                sim.HitTarget++;
            }
            if (sim.HitTarget < 2)
            {
                sim.HitTarget = 2;
            }
            if (sim.HitTarget > 6 || sim.Weapon.Overwatch)
            {
                sim.HitTarget = 6;
            }
            if (sim.HitTarget > sim.Weapon.Skill && sim.HitTarget - sim.Weapon.Skill != 1)
            {
                sim.HitTarget = sim.Weapon.Skill + 1;
            }
            if (sim.HitTarget < sim.Weapon.Skill && sim.Weapon.Skill - sim.HitTarget != 1)
            {
                sim.HitTarget = sim.Weapon.Skill - 1;
            }
        }
        public void RollToHit(WeaponSimulation sim)
        {
            sim.ToHitDice = new List<Dice>();
            if (sim.Weapon.Blast && !sim.Weapon.Melee)
            {
                sim.BlastBonus = sim.Defender.ModelCount / 5;
            }
            sim.Stats.AttackNumber = sim.Weapon.Attacks * sim.WeaponsRemaining;

            if (sim.AttackDice.Count != 0)
            {
                sim.Stats.AttackNumber = sim.AttackDice.Sum(d => d.Result) + (sim.Weapon.VariableAttackDiceConstant * sim.Weapon.WeaponsRemaining);
                if (sim.Weapon.Blast && !sim.Weapon.Melee)
                {
                    sim.Stats.AttackNumber += sim.BlastBonus * sim.Weapon.WeaponsRemaining;
                }
            }
            if (sim.Weapon.RapidFire && !sim.Weapon.Melee)
            {
                sim.Stats.AttackNumber += sim.Weapon.RapidFireBonus * sim.Weapon.WeaponsRemaining;
            }
            if (sim.Weapon.Torrent && !sim.Weapon.Melee)
            {
                for (int j = 0; j < sim.Weapon.WeaponsRemaining; j++)
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
                RerollDice(sim.ToHitDice);
            }
            else if (sim.Weapon.Reroll1Hit)
            {
                Reroll1s(sim.ToHitDice);
            }

        }

        private void Reroll1s(List<Dice> dice)
        {
            var failed = dice.Where(d => d.Result == 1);
            dice = dice.Where(d => d.Result >= 1).ToList();
            foreach (var die in failed)
            {
                die.Reroll(_random);
                dice.Add(die);
            }
        }
        private void RerollDice(List<Dice> dice)
        {
            var failed = dice.Where(d => !d.Success);
            dice = dice.Where(d => d.Success).ToList();
            foreach (var die in failed)
            {
                die.Reroll(_random);
                dice.Add(die);
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
                else
                {
                    sim.WoundTarget = 5;
                }
            }
            else
            {
                if (strength >= toughness * 2)
                {
                    sim.WoundTarget = 2;
                }
                else
                {
                    sim.WoundTarget = 3;
                }
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
                sim.ModdedWoundTarget--;
            }
            if (sim.Defender.Stealth || sim.Defender.Minus1Hit)
            {
                sim.ModdedWoundTarget++;
            }
            if (sim.ModdedWoundTarget < 2)
            {
                sim.ModdedWoundTarget = 2;
            }
            if (sim.ModdedWoundTarget > 6)
            {
                sim.ModdedWoundTarget = 6;
            }
            DetermineAntiWoundNumber(sim);
        }

        private void DetermineAntiWoundNumber(WeaponSimulation sim)
        {
            if (sim.Defender.Type == UnitType.Infantry && sim.Weapon.AntiInfantry && sim.ModdedWoundTarget > sim.Weapon.AntiInfantryValue)
            {
                sim.ModdedWoundTarget = sim.Weapon.AntiInfantryValue;
            }
            else if (sim.Defender.Type == UnitType.Monster && sim.Weapon.AntiMonster && sim.ModdedWoundTarget > sim.Weapon.AntiMonsterValue)
            {
                sim.ModdedWoundTarget = sim.Weapon.AntiMonsterValue;
            }
            else if (sim.Defender.Type == UnitType.Vehicle && sim.Weapon.AntiVehicle && sim.ModdedWoundTarget > sim.Weapon.AntiVehicleValue)
            {
                sim.ModdedWoundTarget = sim.Weapon.AntiVehicleValue;
            }
            else if (sim.Defender.Type == UnitType.Swarm && sim.Weapon.AntiSwarm && sim.ModdedWoundTarget > sim.Weapon.AntiSwarmValue)
            {
                sim.ModdedWoundTarget = sim.Weapon.AntiSwarmValue;
            }
            else if (sim.Defender.Type == UnitType.Beast && sim.Weapon.AntiBeast && sim.ModdedWoundTarget > sim.Weapon.AntiBeastValue)
            {
                sim.ModdedWoundTarget = sim.Weapon.AntiBeastValue;
            }
            else if (sim.Defender.Type == UnitType.Mounted && sim.Weapon.AntiMounted && sim.ModdedWoundTarget > sim.Weapon.AntiMountedValue)
            {
                sim.ModdedWoundTarget = sim.Weapon.AntiMountedValue;
            }
            else if (sim.Defender.Psyker && sim.Weapon.AntiPsyker && sim.ModdedWoundTarget > sim.Weapon.AntiPsykerValue)
            {
                sim.ModdedWoundTarget = sim.Weapon.AntiPsykerValue;
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
                RerollDice(sim.StrengthDice);
            }
            else if (sim.Weapon.Reroll1Wound)
            {
                Reroll1s(sim.StrengthDice);
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
                RerollDice(sim.ArmorDice);
            }
            else if (sim.Defender.Reroll1Save)
            {
                Reroll1s(sim.ArmorDice);
            }
            if (!sim.Weapon.IsVariableDamage)
            {

                int failedsaves = sim.ArmorDice.Where(d => !d.Success).Count();
                sim.Stats.DamageNumber = failedsaves * sim.Weapon.Damage;
                if (sim.Weapon.Melta && !sim.Weapon.Melee)
                {
                    sim.Stats.DamageNumber += failedsaves * sim.Weapon.MeltaDamage;
                }
                sim.Stats.WoundsInflicted = sim.Stats.DamageNumber;
            }
            sim.Stats.ArmorSavesFailed = sim.ArmorDice.Where(d => !d.Success).Count();
            sim.Stats.WoundsSuccessful = sim.ArmorDice.Count();
        }

        public int DetermineArmorSave(WeaponSimulation sim)
        {
            int moddedSave = sim.Defender.MinimumSave + sim.Weapon.ArmorPenetration;
            if (!sim.Defender.HasInvulnerableSave)
            {
                sim.Defender.InvulnerableSave = 0;
            }
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
            sim.Stats.WoundsInflicted = sim.Stats.DamageNumber;
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
            //sets baseline model wounds, which get changed after damage is 
            //This means we are before fightback, set wounds normally
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
                while (sim.Stats.ModelsDestroyed < sim.Defender.ModelCount && AttacksApplied < weaponSim.Stats.ArmorSavesFailed)
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
                        if (weaponSim.Defender.TakesHalfDamage)
                        {

                            var floatWeaponDamage = (float)weaponDamage;
                            floatWeaponDamage = floatWeaponDamage / 2;
                            weaponDamage = (int)Math.Ceiling(floatWeaponDamage);

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
                            sim.Stats.ModelsDestroyed++;
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
            weaponSim.Stats.ColumnName = weaponSim.Weapon.Name;
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
            sim.Stats.AttackNumber = sim.WeaponSimulations.Select(w => w.Stats.AttackNumber).Sum();
            sim.Stats.DamageNumber = sim.WeaponSimulations.Select(w => w.Stats.DamageNumber).Sum();
            sim.Stats.ModelsDestroyed = sim.WeaponSimulations.Select(w => w.Stats.ModelsDestroyed).Sum();
            sim.Stats.PreFNPDamage = sim.WeaponSimulations.Select(w => w.Stats.PreFNPDamage).Sum();
            sim.Stats.FeelNoPainMade = sim.WeaponSimulations.Select(w => w.Stats.FeelNoPainMade).Sum();
            sim.Stats.ArmorSavesFailed = sim.WeaponSimulations.Select(w => w.Stats.ArmorSavesFailed).Sum();
            sim.Stats.AttacksHit = sim.WeaponSimulations.Select(w => w.Stats.AttacksHit).Sum();
            sim.Stats.WoundsSuccessful = sim.WeaponSimulations.Select(w => w.Stats.WoundsSuccessful).Sum();
            sim.Stats.WoundsInflicted = sim.WeaponSimulations.Select(w => w.Stats.WoundsInflicted).Sum();
            sim.Stats.LostAModel = sim.WeaponSimulations.Where(w => w.Stats.LostAModel).Any();
            sim.Stats.UnitDamaged = sim.WeaponSimulations.Where(w => w.Stats.UnitDamaged).Any();
            sim.Stats.UnitEntirelyDestroyed = sim.WeaponSimulations.Where(w => w.Stats.UnitEntirelyDestroyed).Any();
        }
        public void ValidateResult(FightSimulation sim)
        {
            if (sim.Stats.ArmorSavesFailed > sim.Stats.AttackNumber)
            {
                throw new InvalidProgramException("There are more failed armor saves than attacks");
            }
            if (sim.Stats.LostAModel && sim.Stats.ModelsDestroyed == 0)
            {
                throw new InvalidProgramException("Lost a model is true but 0 models were destroyed");
            }
        }
        public string GenerateWinnerMessage(FightSimulation sim, bool fightsBack = false)
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
                if (sim.Defender.TakesHalfDamage)
                {
                    res.Append("Damage was halved.\n");
                }
                if (sim.Stats.ModelsDestroyed >= sim.Defender.ModelCount)
                {
                    res.Append("The entire unit was destroyed");
                    if (sim.HasFightBack)
                    {
                        res.Append(" and therefore couldn't fight back.");
                    }
                    res.Append(".\n");
                    return res.ToString();
                }
                if (sim.Defender.ModelCount > 1)
                {
                    res.Append($"{sim.Stats.ModelsDestroyed} out of {sim.Defender.ModelCount} models were destroyed.\n");

                    if (sim.Stats.SingleModelRemainingWounds > 0 && sim.Stats.SingleModelRemainingWounds != sim.Defender.Wounds)
                    {
                        res.Append($"A remaining model was inflicted {sim.Defender.Wounds - sim.Stats.SingleModelRemainingWounds} wounds, leaving it with {sim.Stats.SingleModelRemainingWounds} remaining.\n");
                    }
                }
                else
                {
                    res.Append($"The model has {sim.Defender.Wounds - sim.Stats.SingleModelRemainingWounds} wound(s) remaining.\n");
                }
            }
            if (sim.HasFightBack)
            {
                res.Append($"Then Defender fought back with {sim.FightBackSimulation.RemainingAttackingModels} remaining models.\n");
                //run this method again, but fightback variable is false.
                res.Append(GenerateWinnerMessage(sim.FightBackSimulation));
            }
            return res.ToString();
        }
    }
}
