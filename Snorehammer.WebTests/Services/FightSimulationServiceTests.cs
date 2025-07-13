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
        private Random random = A.Fake<Random>();
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
        }
        [Test]
        public void WinnerMessageUnitKilled()
        {
            //arrange
            A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(6))).Returns(2);
            attacker.Attacks = 20;
            for (int i = 0; i < 20; i++)
            {
                diceList.Add(new Dice(unitProfile.MinimumSave, random));
            }
            //act
            var res = service.GenerateWinnerMessage(unitProfile, attacker, diceList);
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
            A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(6))).ReturnsNextFromSequence(sequence.ToArray());
            attacker.Attacks = 20;
            for (int i = 0; i < 20; i++)
            {
                diceList.Add(new Dice(unitProfile.MinimumSave, random));
            }
            //act
            var res = service.GenerateWinnerMessage(unitProfile, attacker, diceList);
            //assert
            res.Should().Contain("10 out of 20 attacks broke through armor.");
            res.Should().Contain("10 wounds inflicted to defender.");
            res.Should().Contain("5 out of 10 models were destroyed");
        }
        [Test]
        public void WinnerMessageDamageButNoModelDestroyed()
        {
            //arrange
            
            A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(6))).Returns(3);
            A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(6))).ReturnsNextFromSequence(1);
            attacker.Attacks = 20;
            for (int i = 0; i < 20; i++)
            {
                diceList.Add(new Dice(unitProfile.MinimumSave, random));
            }
            //act
            var res = service.GenerateWinnerMessage(unitProfile, attacker, diceList);
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

            A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(6))).Returns(2);
            attacker.Attacks = 1;
            unitProfile.ModelCount = 1;
            diceList.Add(new Dice(unitProfile.MinimumSave, random));
            //act
            var res = service.GenerateWinnerMessage(unitProfile, attacker, diceList);
            //assert
            res.Should().Contain("1 out of 1 attacks broke through armor.");
            res.Should().Contain("1 wounds inflicted to defender.");
            res.Should().Contain("The model has 1 wound(s) remaining");
        }
        [Test]
        public void WinnerMessageDestroyedSingleModel ()
        {
            //arrange
            A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(6))).Returns(1);
            attacker.Attacks = 20;
            unitProfile.ModelCount = 1;
            for (int i = 0; i < 20; i++)
            {
                diceList.Add(new Dice(unitProfile.MinimumSave, random));
            }
            //act
            var res = service.GenerateWinnerMessage(unitProfile, attacker, diceList);
            //assert
            res.Should().Contain("20 out of 20 attacks broke through armor.");
            res.Should().Contain("20 wounds inflicted to defender.");
            res.Should().Contain("The entire unit was destroyed");
        }
        [Test]
        public void WinnerMessage0Damage()
        {
            //arrange
            A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1),A<int>.That.IsEqualTo(6))).Returns(3);
            for (int i = 0; i < 2; i++)
            {
                diceList.Add(new Dice(unitProfile.MinimumSave, random));
            }
            //act
            var res = service.GenerateWinnerMessage(unitProfile, attacker, diceList);
            //assert
            res.Should().Contain("0 out of 2 attacks broke through armor.");

        }
    }
}