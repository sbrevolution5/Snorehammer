using Snorehammer.Web.FrontendModels;

namespace Snorehammer.Web.Services
{
    public class FightSimulationService
    {
        private readonly Random _random;
        public FightSimulationService()
        {
            _random = new Random();
        }
        public List<Dice> SimulateToHitRoll(AttackProfile attack)
        {
            var res = new List<Dice>();
            for (int i = 0; i < attack.Attacks; i++)
            {
                res.Add(new Dice(attack.Skill,_random));
            }
            return res;
        }
        public List<Dice> RollStrengthStep(UnitProfile defender, AttackProfile attack, List<Dice> dicePool)
        {
            var res = new List<Dice>();
            var targetValue = DetermineWoundTarget(defender.Toughness, attack.Strength);
            for (int i = 0; i < dicePool.Where(d => d.Success).Count(); i++)
            {
                res.Add(new Dice(targetValue,_random));
            }
            return res;
        }
        public List<Dice> RollArmorSaves(UnitProfile defender, AttackProfile attack, List<Dice> dicePool)
        {
            var res = new List<Dice>();
            var targetValue = DetermineArmorSave(defender, attack);
            var roller = new Random();
            for (int i = 0; i < dicePool.Where(d => d.Success).Count(); i++)
            {
                res.Add(new Dice(targetValue, _random));
            }
            return res;
        }

        public int DetermineArmorSave(UnitProfile defender, AttackProfile attack)
        {
            int moddedSave = defender.MinimumSave - attack.ArmorPenetration;
            if (moddedSave < defender.InvulnerableSave || defender.InvulnerableSave == 0)
            {
                return moddedSave;
            }
            return defender.InvulnerableSave;
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
