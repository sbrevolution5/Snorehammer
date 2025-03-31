using Snorehammer.Web.FrontendModels;

namespace Snorehammer.Web.Services
{
    public class FightSimulationService
    {
        public FightSimulationService() { }
        public void SimulateSimpleFight(UnitProfile defender, AttackProfile attack)
        {
            int dicePool = attack.Attacks;
            Random roller = new Random();
            List<int> dice = new List<int>();
            for (int i = 0; i < dicePool; i++)
            {
                dice.Add(roller.Next(1, 6));
            }
            var remaining =dice.Where(i => i >= attack.Skill);
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
        //public List<Dice> RollAttackStep(UnitProfile defender, AttackProfile attack, List<int> DicePool)
        //{
        //    var roller = new Random();
        //    var res = new List<int>();
        //    foreach (var die in DicePool)
        //    {
        //        if (die == 0)
        //        {
        //            res.Add(0);
        //        }
        //        else {
        //            res.Add(roller.Next(1, 6));
        //        }
        //    }
        //    return res;
        //}
        public List<int> RollStrengthStep(UnitProfile defender, AttackProfile attack, List<int> DicePool)
        {
            var roller = new Random();
            var res = new List<int>();
            foreach (var die in DicePool)
            {
                //check threshold
                if (die == 0)
                {
                    res.Add(0);
                }
                else {
                    res.Add(roller.Next(1, 6));
                }
            }
            return res;
        }
    }
}
