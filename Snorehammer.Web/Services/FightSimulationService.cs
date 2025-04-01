using Snorehammer.Web.FrontendModels;

namespace Snorehammer.Web.Services
{
    public class FightSimulationService
    {
        public FightSimulationService() { }
        public List<Dice> SimulateToHitRoll(AttackProfile attack)
        {
            var res = new List<Dice>();
            for (int i = 0; i < attack.Attacks; i++)
            {
                res.Add(new Dice(6));
            }
            return res;
        }
        public List<Dice> RollStrengthStep(UnitProfile defender, AttackProfile attack, List<Dice> DicePool)
        {
            var roller = new Random();
            var res = new List<Dice>();
            var targetValue = DetermineWoundTarget(defender.Toughness, attack.Strength);
            res.AddRange(DicePool);
            foreach (var die in DicePool)
            {
                die.Target = targetValue;
                die.Result = roller.Next(die.Sides);
            }
            return res;
        }

        private int DetermineWoundTarget(int toughness, int strength)
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
