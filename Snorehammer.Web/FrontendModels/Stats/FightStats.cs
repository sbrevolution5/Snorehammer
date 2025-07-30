namespace Snorehammer.Web.FrontendModels.Stats
{
    public class FightStats
    {
        public int AttackNumber { get; set; } = 0;
        public int DamageNumber { get; set; } = 0;
        public int PreFNPDamage { get; set; } = 0;
        public int SingleModelRemainingWounds { get; set; } = 0;
        public int ModelsDestroyed { get; set; } = 0;
        public int WoundsInflicted { get; set; } = 0;
        public int ArmorSavesFailed { get; set; } = 0;
        public int AttacksHit { get; set; } = 0;
        public int FeelNoPainMade { get; set; } = 0;
        public bool UnitEntirelyDestroyed { get; set; } = false;
        public bool LessThanHalf { get; set; } = false;
        public bool UnitDamaged { get; set; } = false;
        public bool LostAModel { get; set; } = false;
    }
}
