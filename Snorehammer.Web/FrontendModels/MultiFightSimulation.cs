namespace Snorehammer.Web.FrontendModels
{
    public class MultiFightSimulation
    {
        public UnitProfile Attacker { get; set; }
        public UnitProfile Defender { get; set; }
        public List<FightSimulation> FightSimulations { get; set; } = new List<FightSimulation>();
        public int Rounds { get; set; }
        public float AttackNumber { get; set; } = 0;
        public float DamageNumber { get; set; } = 0;
        public float ModelsDestroyed { get; set; } = 0;
        public float WoundsInflicted { get; set; } = 0;
        public float ArmorSavesFailed { get; set; } = 0;
        public float AttacksHit { get; set; } = 0;
        public float FeelNoPainMade { get; set; } = 0;
        public int UnitEntirelyDestroyed { get; set; } = 0;
        public int HalfOrLess { get; set; } = 0;
        public int UnitDamaged { get; set; } = 0;
        public int LostAModel { get; set; } = 0;
        public void SetSimNumber(int number)
        {
            Rounds = number;
            FightSimulations = new List<FightSimulation>();
            for (int i = 0; i < number; i++)
            {
                FightSimulations.Add(new FightSimulation()
                {
                    Attacker = Attacker,
                    Defender = Defender,
                });
            }
        }
        public void SetAverages()
        {
            AttackNumber = (float)FightSimulations.Select(f => f.AttackNumber).Average();
            DamageNumber = (float)FightSimulations.Select(f => f.DamageNumber).Average();
            ModelsDestroyed = (float)FightSimulations.Select(f => f.ModelsDestroyed).Average();
            WoundsInflicted = (float)FightSimulations.Select(f => f.WoundsInflicted).Average();
            ArmorSavesFailed = (float)FightSimulations.Select(f => f.ArmorSavesFailed).Average();
            AttacksHit = (float)FightSimulations.Select(f => f.AttacksHit).Average();
            FeelNoPainMade = (float)FightSimulations.Select(f => f.FeelNoPainMade).Average();
            UnitEntirelyDestroyed = FightSimulations.Where(f => f.UnitEntirelyDestroyed).Count();
            HalfOrLess = FightSimulations.Where(f => f.LessThanHalf).Count();
            UnitDamaged = FightSimulations.Where(f => f.UnitDamaged).Count();
            LostAModel = FightSimulations.Where(f => f.LostAModel).Count();
        }
    }
}
