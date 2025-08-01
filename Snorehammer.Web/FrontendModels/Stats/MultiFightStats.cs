﻿using Snorehammer.Web.FrontendModels.Simulations;

namespace Snorehammer.Web.FrontendModels.Stats
{
    public class MultiFightStats
    {
        public string ColumnName { get; set; } = "";
        public float AttackNumber { get; set; } = 0;
        public float DamageNumber { get; set; } = 0;
        public float ModelsDestroyed { get; set; } = 0;
        public float WoundsSuccessful { get; set; } = 0; 
        public float WoundsInflicted { get; set; } = 0;
        public float ArmorSavesFailed { get; set; } = 0;
        public float AttacksHit { get; set; } = 0;
        public float FeelNoPainMade { get; set; } = 0;
        public float UnitEntirelyDestroyed { get; set; } = 0;
        public float LessThanHalf { get; set; } = 0;
        public float UnitDamaged { get; set; } = 0;
        public float LostAModel { get; set; } = 0;
        public List<MultiFightStats> PerWeaponStats { get; set; } = new List<MultiFightStats>();
        public void SetAverages(IEnumerable<ISimulationForStats> fightSimulations)
        {
            AttackNumber = (float)fightSimulations.Select(f => f.Stats.AttackNumber).Average();
            DamageNumber = (float)fightSimulations.Select(f => f.Stats.DamageNumber).Average();
            ModelsDestroyed = (float)fightSimulations.Select(f => f.Stats.ModelsDestroyed).Average();
            WoundsInflicted = (float)fightSimulations.Select(f => f.Stats.WoundsInflicted).Average();
            ArmorSavesFailed = (float)fightSimulations.Select(f => f.Stats.ArmorSavesFailed).Average();
            AttacksHit = (float)fightSimulations.Select(f => f.Stats.AttacksHit).Average();
            FeelNoPainMade = (float)fightSimulations.Select(f => f.Stats.FeelNoPainMade).Average();
            WoundsSuccessful = (float)fightSimulations.Select(f => f.Stats.WoundsSuccessful).Average();
            UnitEntirelyDestroyed = fightSimulations.Where(f => f.Stats.UnitEntirelyDestroyed).Count();
            LessThanHalf = fightSimulations.Where(f => f.Stats.LessThanHalf).Count();
            UnitDamaged = fightSimulations.Where(f => f.Stats.UnitDamaged).Count();
            LostAModel = fightSimulations.Where(f => f.Stats.LostAModel).Count();
        }
    }
}
