namespace Snorehammer.Web.FrontendModels
{
    public class MultiFightSimulation
    {
        public AttackProfile AttackProfile { get; set; }
        public UnitProfile Defender { get; set; }
        public List<FightSimulation> FightSimulations { get; set; } = new List<FightSimulation>();
        public int Rounds { get; set; }
        public int AttackNumber { get; set; } = 0;
        public int DamageNumber { get; set; } = 0;
        public int ModelsDestroyed { get; set; } = 0;
        public int WoundsInflicted { get; set; } = 0;
        public int ArmorSavesFailed { get; set; } = 0;
        public int AttacksHit { get; set; } = 0;
        public int FeelNoPainMade { get; set; } = 0;
        public void SetSimNumber(int number)
        {
            for (int i = 0; i < number; i++)
            {
                FightSimulations.Add(new FightSimulation()
                {
                    AttackProfile = AttackProfile,
                    Defender = Defender,
                });
            }
        }
    }
}
