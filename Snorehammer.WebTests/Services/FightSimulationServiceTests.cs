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
            throw new NotImplementedException(); 
        }
        [Test]
        public void WinnerMessageHalfUnit()
        {
            throw new NotImplementedException(); 
        }
        [Test]
        public void WinnerMessageDamageButNoModelDestroyed()
        {
            throw new NotImplementedException(); 
        }
        [Test]
        public void WinnerMessageDamagedSingleModel ()
        {
            throw new NotImplementedException(); 
        }
        [Test]
        public void WinnerMessageDestroyedSingleModel ()
        {
            throw new NotImplementedException(); 
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