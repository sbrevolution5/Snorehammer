using Snorehammer.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Snorehammer.Web.FrontendModels;
using NUnit.Framework;

namespace Snorehammer.Web.Services.Tests
{
    public class FightSimulationServiceTests
    {
        private FightSimulationService service;
        private AttackProfile attacker;
        private UnitProfile unitProfile;
        private List<Dice> diceList;
        private List<Dice> fnpDiceList;
        private FightSimulation sim;
        private Random random = A.Fake<Random>();
        private Random fnpRandom = A.Fake<Random>();
        [SetUp]
        public void Setup()
        {
            service = new FightSimulationService();
            attacker = new AttackProfile()
            {
                ArmorPenetration = 0,
                Attacks = 2,
                Damage = 1,
                Id = 1,
                Name = "bolt Rifle",
                Skill = 3,
                Strength = 4
            };
            unitProfile = new UnitProfile()
            {
                Id = 1,
                InvulnerableSave = 0,
                ModelCount = 10,
                Name = "Intercessors",
                MinimumSave = 3,
                Toughness = 4,
                Wounds = 2
            };
            diceList = new List<Dice>();
            sim = new FightSimulation();
            fnpDiceList = new List<Dice>();

        }
        [TearDown]
        public void cleanup()
        {
            sim = null;
            attacker = null;
            unitProfile = null;
            service = null;
            diceList = null;
        }
        [Test]
        public void WinnerMessageFNPBlocksNone()
        {
            //arrange
            A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
            unitProfile.FeelNoPain = true;
            A.CallTo(() => fnpRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(1);
            attacker.Attacks = 20;
            for (int i = 0; i < 20; i++)
            {
                diceList.Add(new Dice(unitProfile.MinimumSave, random));
                fnpDiceList.Add(new Dice(unitProfile.FeelNoPainTarget, fnpRandom));
            }
            sim.ArmorDice = diceList;
            sim.FeelNoPainDice = fnpDiceList;
            //act
            var res = service.GenerateWinnerMessage(unitProfile, attacker, sim);
            //assert
            res.Should().Contain("0 of 20 wounds blocked by Feel No Pain.");
            res.Should().Contain("20 wounds inflicted to defender.");
            res.Should().Contain("The entire unit was destroyed");
        }
        [Test]
        public void WinnerMessageFNPBlocksOne()
        {
            //arrange
            A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
            unitProfile.FeelNoPain = true;
            A.CallTo(() => fnpRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(1);
            A.CallTo(()=> fnpRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).ReturnsNextFromSequence(6);
            attacker.Attacks = 20;
            for (int i = 0; i < 20; i++)
            {
                diceList.Add(new Dice(unitProfile.MinimumSave, random));
                fnpDiceList.Add(new Dice(unitProfile.MinimumSave, fnpRandom));
            }
            sim.ArmorDice = diceList;
            sim.FeelNoPainDice = fnpDiceList;
            //act
            var res = service.GenerateWinnerMessage(unitProfile, attacker, sim);
            //assert
            res.Should().Contain("20 out of 20 attacks broke through armor.");
            res.Should().Contain("1 of 20 wounds blocked by Feel No Pain.");
            res.Should().Contain("19 wounds inflicted to defender.");
        }
        [Test]
        public void WinnerMessageFNPBlocksAll()
        {
            //arrange
            A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
            unitProfile.FeelNoPain = true;
            A.CallTo(() => fnpRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(6);
            attacker.Attacks = 20;
            for (int i = 0; i < 20; i++)
            {
                diceList.Add(new Dice(unitProfile.MinimumSave, random));
                fnpDiceList.Add(new Dice(unitProfile.FeelNoPainTarget, fnpRandom));
            }
            sim.ArmorDice = diceList;
            sim.FeelNoPainDice = fnpDiceList;
            //act
            var res = service.GenerateWinnerMessage(unitProfile, attacker, sim);
            //assert
            res.Should().Contain("20 out of 20 attacks broke through armor.");
            res.Should().Contain("All wounds blocked by feel no pain. ");
            res.Should().NotContain("20 wounds inflicted to defender.");
            res.Should().NotContain("The entire unit was destroyed");
        }
        [Test]
        public void WinnerMessageUnitKilled()
        {
            //arrange
            A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
            attacker.Attacks = 20;
            for (int i = 0; i < 20; i++)
            {
                diceList.Add(new Dice(unitProfile.MinimumSave, random));
            }
            sim.ArmorDice = diceList;
            //act
            var res = service.GenerateWinnerMessage(unitProfile, attacker, sim);
            //assert
            res.Should().Contain("20 out of 20 attacks broke through armor.");
            res.Should().Contain("20 wounds inflicted to defender.");
            res.Should().Contain("The entire unit was destroyed");
        }
        [Test]
        public void WinnerMessageHalfUnit()
        {
            //arrange
            var sequence = new List<int>();
            for( int i =0; i<10; i++)
            {
                sequence.Add(2);
                sequence.Add(3);
            }
            A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).ReturnsNextFromSequence(sequence.ToArray());
            attacker.Attacks = 20;
            for (int i = 0; i < 20; i++)
            {
                diceList.Add(new Dice(unitProfile.MinimumSave, random));
            }
            sim.ArmorDice = diceList;
            //act
            var res = service.GenerateWinnerMessage(unitProfile, attacker, sim);
            //assert
            res.Should().Contain("10 out of 20 attacks broke through armor.");
            res.Should().Contain("10 wounds inflicted to defender.");
            res.Should().Contain("5 out of 10 models were destroyed");
        }
        [Test]
        public void WinnerMessageDamageButNoModelDestroyed()
        {
            //arrange
            
            A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(3);
            A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).ReturnsNextFromSequence(1);
            attacker.Attacks = 20;
            for (int i = 0; i < 20; i++)
            {
                diceList.Add(new Dice(unitProfile.MinimumSave, random));
            }
            sim.ArmorDice = diceList;
            //act
            var res = service.GenerateWinnerMessage(unitProfile, attacker,  sim);
            //assert
            res.Should().Contain("1 out of 20 attacks broke through armor.");
            res.Should().Contain("1 wounds inflicted to defender.");
            res.Should().Contain("0 out of 10 models were destroyed");
            res.Should().Contain("A remaining model was inflicted 1 wounds, leaving it with 1 remaining");
        }
        [Test]
        public void WinnerMessageDamagedSingleModel ()
        {
            //arrange

            A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
            attacker.Attacks = 1;
            unitProfile.ModelCount = 1;
            diceList.Add(new Dice(unitProfile.MinimumSave, random));
            sim.ArmorDice = diceList;
            //act
            var res = service.GenerateWinnerMessage(unitProfile, attacker, sim);
            //assert
            res.Should().Contain("1 out of 1 attacks broke through armor.");
            res.Should().Contain("1 wounds inflicted to defender.");
            res.Should().Contain("The model has 1 wound(s) remaining");
        }
        [Test]
        public void WinnerMessageDestroyedSingleModel ()
        {
            //arrange
            A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(1);
            attacker.Attacks = 20;
            unitProfile.ModelCount = 1;
            for (int i = 0; i < 20; i++)
            {
                diceList.Add(new Dice(unitProfile.MinimumSave, random));
            }
            sim.ArmorDice = diceList;
            //act
            var res = service.GenerateWinnerMessage(unitProfile, attacker, sim);
            //assert
            res.Should().Contain("20 out of 20 attacks broke through armor.");
            res.Should().Contain("20 wounds inflicted to defender.");
            res.Should().Contain("The entire unit was destroyed");
        }
        [Test]
        public void WinnerMessage0Damage()
        {
            //arrange
            A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1),A<int>.That.IsEqualTo(7))).Returns(3);
            for (int i = 0; i < 2; i++)
            {
                diceList.Add(new Dice(unitProfile.MinimumSave, random));
            }
            sim.ArmorDice = diceList;
            //act
            var res = service.GenerateWinnerMessage(unitProfile, attacker, sim);
            //assert
            res.Should().Contain("0 out of 2 attacks broke through armor.");

        }
    }
}